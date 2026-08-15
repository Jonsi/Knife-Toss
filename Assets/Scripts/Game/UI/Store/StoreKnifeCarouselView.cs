using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Feedback.Events;
using Game.Store.Data;
using Game.UI.Common;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI.Store
{
    /// <summary>
    /// Theme-neutral carousel with two slide containers and a bounded six-card view pool.
    /// Catalog entries remain data; only the outgoing and incoming pages own live card views.
    /// </summary>
    public sealed class StoreKnifeCarouselView : MonoBehaviour
    {
        private const int RequiredSlideContainers = 2;
        private const int MaximumCardsPerPage = 3;
        private const int MaximumPooledCards = RequiredSlideContainers * MaximumCardsPerPage;
        private const int MaximumPageDots = 5;

        [Header("Pooled Slide Hierarchy")]
        [SerializeField] private RectTransform _CardPoolRoot;
        [SerializeField] private RectTransform[] _SlideContainers = new RectTransform[RequiredSlideContainers];

        [Header("Controls")]
        [SerializeField] private Button _PreviousButton;
        [SerializeField] private Button _NextButton;
        [SerializeField] private RectTransform _PageDotsRoot;
        [SerializeField] private Image _PageDotTemplate;

        [Header("Motion")]
        [SerializeField, Range(1, MaximumCardsPerPage)] private int _CardsPerPage = MaximumCardsPerPage;
        [SerializeField, Min(0f)] private float _TransitionDuration = 0.34f;
        [SerializeField, Min(1f)] private float _SlideDistance = 820f;
        [SerializeField] private Ease _SlideEase = Ease.OutCubic;
        [SerializeField] private Color _ActiveDotColor = new(0.45f, 1f, 0.08f, 1f);
        [SerializeField] private Color _InactiveDotColor = new(0.969f, 0.894f, 0.745f, 0.72f);

        private readonly List<PurchasableEntry> _Entries = new();
        private readonly List<Image> _PageDots = new();
        private readonly List<VisibleCardBinding> _VisibleCards = new();
        private ObjectPool<KnifeSkinStoreCardView> _CardPool;
        private KnifeSkinStoreCardView _CardPrefab;
        private Action<PurchasableEntry> _OnOfferPressed;
        private Action<KnifeSkinStoreCardView, PurchasableEntry> _ApplyCardState;
        private FeedbackEventChannel _FeedbackChannel;
        private ActionButtonPresenter _PreviousPresenter;
        private ActionButtonPresenter _NextPresenter;
        private int _ActiveContainerIndex;
        private int _CurrentPage;
        private int _CreatedCardSerial;
        private bool _CanInteract = true;
        private bool _IsTransitioning;

        private int CardsPerPage => Mathf.Clamp(_CardsPerPage, 1, MaximumCardsPerPage);

        public int CurrentPage => _CurrentPage;
        public int PageCount => _Entries.Count == 0
            ? 0
            : Mathf.CeilToInt(_Entries.Count / (float)CardsPerPage);
        public int ContainerPoolSize => _SlideContainers?.Length ?? 0;
        public int CreatedCardCount => _CardPool?.CountAll ?? 0;
        public int VisibleCardCount => _VisibleCards.Count;
        public int PoolCapacity => MaximumPooledCards;
        public int EntryCount => _Entries.Count;
        public bool IsTransitioning => _IsTransitioning;

        public void Configure(
            IReadOnlyList<PurchasableEntry> entries,
            KnifeSkinStoreCardView cardPrefab,
            Action<PurchasableEntry> onOfferPressed,
            Action<KnifeSkinStoreCardView, PurchasableEntry> applyCardState,
            FeedbackEventChannel feedbackChannel = null)
        {
            DOTween.Kill(this);
            UnbindButtons();
            ResetContainers();

            if (_CardPrefab != cardPrefab)
            {
                _CardPool?.Clear();
                _CardPool = null;
                _CreatedCardSerial = 0;
            }

            _CardPrefab = cardPrefab;
            _OnOfferPressed = onOfferPressed;
            _ApplyCardState = applyCardState;
            _FeedbackChannel = feedbackChannel;
            _Entries.Clear();

            if (entries != null)
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i] != null)
                    {
                        _Entries.Add(entries[i]);
                    }
                }
            }

            EnsureCardPool();
            BindButtons(feedbackChannel);
            BuildPageDots();
            SetPageImmediate(0);
            RefreshInteractionState();
        }

        public void SetInteractable(bool interactable)
        {
            _CanInteract = interactable;
            RefreshVisibleCards();
            RefreshInteractionState();
        }

        public void RefreshVisibleCards()
        {
            for (var i = 0; i < _VisibleCards.Count; i++)
            {
                var binding = _VisibleCards[i];
                if (binding.Card != null && binding.Entry != null)
                {
                    _ApplyCardState?.Invoke(binding.Card, binding.Entry);
                }
            }
        }

        public KnifeSkinStoreCardView FindVisibleCard(PurchasableEntry entry)
        {
            for (var i = 0; i < _VisibleCards.Count; i++)
            {
                if (ReferenceEquals(_VisibleCards[i].Entry, entry))
                {
                    return _VisibleCards[i].Card;
                }
            }

            return null;
        }

        private void OnDisable()
        {
            DOTween.Kill(this);
            _IsTransitioning = false;
            UnbindButtons();
        }

        private void OnDestroy()
        {
            ResetContainers();
            _CardPool?.Clear();
            _CardPool = null;
        }

        private void OnPreviousClicked()
        {
            SlideToPageAsync(-1).Forget();
        }

        private void OnNextClicked()
        {
            SlideToPageAsync(1).Forget();
        }

        private async UniTaskVoid SlideToPageAsync(int direction)
        {
            if (_IsTransitioning || !_CanInteract || PageCount <= 1 || !HasValidContainerPool())
            {
                return;
            }

            _IsTransitioning = true;
            RefreshInteractionState();

            var normalizedDirection = direction < 0 ? -1 : 1;
            var nextPage = (_CurrentPage + normalizedDirection + PageCount) % PageCount;
            var outgoing = _SlideContainers[_ActiveContainerIndex];
            var incomingIndex = (_ActiveContainerIndex + 1) % RequiredSlideContainers;
            var incoming = _SlideContainers[incomingIndex];
            var distance = Mathf.Max(1f, _SlideDistance);

            PopulateContainer(incoming, nextPage);
            SetContainerX(outgoing, 0f);
            SetContainerX(incoming, normalizedDirection * distance);

            try
            {
                if (_TransitionDuration > 0f)
                {
                    var sequence = DOTween.Sequence()
                        .SetUpdate(true)
                        .SetTarget(this);
                    sequence.Join(outgoing
                        .DOAnchorPosX(-normalizedDirection * distance, _TransitionDuration)
                        .SetEase(_SlideEase));
                    sequence.Join(incoming
                        .DOAnchorPosX(0f, _TransitionDuration)
                        .SetEase(_SlideEase));
                    await sequence.AsyncWaitForCompletion().AsUniTask();
                }
                else
                {
                    SetContainerX(outgoing, -normalizedDirection * distance);
                    SetContainerX(incoming, 0f);
                }

                if (this == null || !isActiveAndEnabled)
                {
                    return;
                }

                ReleaseContainerCards(outgoing);
                outgoing.gameObject.SetActive(false);
                SetContainerX(outgoing, 0f);
                _ActiveContainerIndex = incomingIndex;
                _CurrentPage = nextPage;
                RefreshDots();
            }
            finally
            {
                _IsTransitioning = false;
                if (this != null)
                {
                    RefreshInteractionState();
                }
            }
        }

        private void SetPageImmediate(int page)
        {
            if (!HasValidContainerPool())
            {
                return;
            }

            _CurrentPage = PageCount == 0 ? 0 : Mathf.Clamp(page, 0, PageCount - 1);
            _ActiveContainerIndex = 0;
            for (var i = 0; i < _SlideContainers.Length; i++)
            {
                ReleaseContainerCards(_SlideContainers[i]);
                SetContainerX(_SlideContainers[i], 0f);
                _SlideContainers[i].gameObject.SetActive(i == _ActiveContainerIndex && PageCount > 0);
            }

            if (PageCount > 0)
            {
                PopulateContainer(_SlideContainers[_ActiveContainerIndex], _CurrentPage);
            }

            RefreshDots();
        }

        private void PopulateContainer(RectTransform container, int page)
        {
            if (container == null || _CardPool == null)
            {
                return;
            }

            ReleaseContainerCards(container);
            var firstVisible = page * CardsPerPage;
            var lastVisibleExclusive = Mathf.Min(_Entries.Count, firstVisible + CardsPerPage);
            var visibleCount = lastVisibleExclusive - firstVisible;
            var selectedIndex = visibleCount <= 1
                ? firstVisible
                : firstVisible + Mathf.Min(1, visibleCount - 1);

            for (var i = firstVisible; i < lastVisibleExclusive; i++)
            {
                var entry = _Entries[i];
                var card = _CardPool.Get();
                card.transform.SetParent(container, false);
                card.Setup(entry, _OnOfferPressed, _FeedbackChannel);
                card.gameObject.SetActive(true);
                card.SetCarouselSelected(i == selectedIndex);
                _VisibleCards.Add(new VisibleCardBinding(card, entry));
                _ApplyCardState?.Invoke(card, entry);
            }

            container.gameObject.SetActive(true);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }

        private void EnsureCardPool()
        {
            if (_CardPool != null || _CardPrefab == null || _CardPoolRoot == null)
            {
                return;
            }

            _CardPool = new ObjectPool<KnifeSkinStoreCardView>(
                CreateCard,
                OnTakeCard,
                OnReturnCard,
                OnDestroyCard,
                collectionCheck: true,
                defaultCapacity: MaximumPooledCards,
                maxSize: MaximumPooledCards);
        }

        private KnifeSkinStoreCardView CreateCard()
        {
            var card = Instantiate(_CardPrefab, _CardPoolRoot);
            _CreatedCardSerial++;
            card.name = $"KnifeCardPooled_{_CreatedCardSerial}";
            card.gameObject.SetActive(false);
            return card;
        }

        private static void OnTakeCard(KnifeSkinStoreCardView card)
        {
            if (card != null)
            {
                card.gameObject.SetActive(false);
            }
        }

        private void OnReturnCard(KnifeSkinStoreCardView card)
        {
            if (card == null)
            {
                return;
            }

            card.ResetForReuse();
            card.gameObject.SetActive(false);
            card.transform.SetParent(_CardPoolRoot, false);
        }

        private static void OnDestroyCard(KnifeSkinStoreCardView card)
        {
            if (card != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(card.gameObject);
                }
                else
                {
                    DestroyImmediate(card.gameObject);
                }
            }
        }

        private void ReleaseContainerCards(RectTransform container)
        {
            if (container == null)
            {
                return;
            }

            for (var i = container.childCount - 1; i >= 0; i--)
            {
                var card = container.GetChild(i).GetComponent<KnifeSkinStoreCardView>();
                if (card == null)
                {
                    continue;
                }

                RemoveVisibleBinding(card);
                _CardPool?.Release(card);
            }
        }

        private void RemoveVisibleBinding(KnifeSkinStoreCardView card)
        {
            for (var i = _VisibleCards.Count - 1; i >= 0; i--)
            {
                if (_VisibleCards[i].Card == card)
                {
                    _VisibleCards.RemoveAt(i);
                }
            }
        }

        private void ResetContainers()
        {
            if (_SlideContainers == null)
            {
                return;
            }

            for (var i = 0; i < _SlideContainers.Length; i++)
            {
                var container = _SlideContainers[i];
                if (container == null)
                {
                    continue;
                }

                ReleaseContainerCards(container);
                SetContainerX(container, 0f);
                container.gameObject.SetActive(false);
            }
        }

        private void BindButtons(FeedbackEventChannel feedbackChannel)
        {
            if (_PreviousButton != null)
            {
                _PreviousPresenter = ActionButtonPresenter.GetOrAdd(_PreviousButton, feedbackChannel);
                _PreviousButton.onClick.RemoveListener(OnPreviousClicked);
                _PreviousButton.onClick.AddListener(OnPreviousClicked);
            }

            if (_NextButton != null)
            {
                _NextPresenter = ActionButtonPresenter.GetOrAdd(_NextButton, feedbackChannel);
                _NextButton.onClick.RemoveListener(OnNextClicked);
                _NextButton.onClick.AddListener(OnNextClicked);
            }
        }

        private void BuildPageDots()
        {
            if (_PageDotsRoot == null || _PageDotTemplate == null)
            {
                return;
            }

            _PageDotTemplate.gameObject.SetActive(false);
            var requiredDots = Mathf.Min(PageCount, MaximumPageDots);
            while (_PageDots.Count < requiredDots)
            {
                var dot = Instantiate(_PageDotTemplate, _PageDotsRoot);
                dot.name = $"PageDot_{_PageDots.Count + 1}";
                dot.raycastTarget = false;
                _PageDots.Add(dot);
            }

            for (var i = 0; i < _PageDots.Count; i++)
            {
                _PageDots[i].gameObject.SetActive(i < requiredDots);
            }

            RefreshDots();
        }

        private void RefreshDots()
        {
            var visibleDots = Mathf.Min(PageCount, MaximumPageDots);
            var activeDot = 0;
            if (visibleDots > 1)
            {
                activeDot = PageCount <= visibleDots
                    ? _CurrentPage
                    : Mathf.RoundToInt(_CurrentPage / (float)(PageCount - 1) * (visibleDots - 1));
            }

            for (var i = 0; i < _PageDots.Count; i++)
            {
                if (_PageDots[i] != null && _PageDots[i].gameObject.activeSelf)
                {
                    _PageDots[i].color = i == activeDot ? _ActiveDotColor : _InactiveDotColor;
                }
            }
        }

        private void RefreshInteractionState()
        {
            var arrowsAvailable = _CanInteract && !_IsTransitioning && PageCount > 1 && HasValidContainerPool();
            _PreviousPresenter?.SetAvailable(arrowsAvailable);
            _NextPresenter?.SetAvailable(arrowsAvailable);

            if (_PreviousPresenter == null && _PreviousButton != null)
            {
                _PreviousButton.interactable = arrowsAvailable;
            }

            if (_NextPresenter == null && _NextButton != null)
            {
                _NextButton.interactable = arrowsAvailable;
            }
        }

        private bool HasValidContainerPool()
        {
            return _CardPoolRoot != null &&
                   _SlideContainers != null &&
                   _SlideContainers.Length == RequiredSlideContainers &&
                   _SlideContainers[0] != null &&
                   _SlideContainers[1] != null;
        }

        private static void SetContainerX(RectTransform container, float x)
        {
            if (container == null)
            {
                return;
            }

            var position = container.anchoredPosition;
            position.x = x;
            container.anchoredPosition = position;
        }

        private void UnbindButtons()
        {
            if (_PreviousButton != null)
            {
                _PreviousButton.onClick.RemoveListener(OnPreviousClicked);
            }

            if (_NextButton != null)
            {
                _NextButton.onClick.RemoveListener(OnNextClicked);
            }
        }

        private readonly struct VisibleCardBinding
        {
            public VisibleCardBinding(KnifeSkinStoreCardView card, PurchasableEntry entry)
            {
                Card = card;
                Entry = entry;
            }

            public KnifeSkinStoreCardView Card { get; }
            public PurchasableEntry Entry { get; }
        }
    }
}
