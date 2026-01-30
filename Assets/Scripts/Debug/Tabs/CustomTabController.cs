#if TEST_ENABLED
using System.Collections.Generic;
using SRDebugger.UI.Controls;
using SRF;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Debugging.Tabs
{
    /// <summary>
    /// Controller attached to the CustomTab prefab. Renders content from a DebugTab or OptionsTab.
    /// </summary>
    public class CustomTabController : MonoBehaviour
    {
        [SerializeField] private RectTransform _ContentContainer;

        private DebugTab _DebugTab;
        private InfoBlock _InfoBlockPrefab;

        public void Setup(DebugTab debugTab, InfoBlock infoBlockPrefab)
        {
            _DebugTab = debugTab;
            _InfoBlockPrefab = infoBlockPrefab;
            _DebugTab.RefreshCallback = Refresh;
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_DebugTab == null || _ContentContainer == null)
                return;

            ClearContent();

            if (_DebugTab is OptionsTab optionsTab)
            {
                RenderOptionsTab(optionsTab);
            }
            else
            {
                RenderDebugTab();
            }
        }

        private void RenderDebugTab()
        {
            if (_InfoBlockPrefab == null) return;

            foreach (var section in _DebugTab.GetSections())
            {
                if (section.Entries.Count > 0)
                {
                    var block = SRInstantiate.Instantiate(_InfoBlockPrefab);
                    block.Title.text = section.Title;
                    block.Content.text = DebugTabLoader.FormatSection(section);
                    block.CachedTransform.SetParent(_ContentContainer, false);
                }

                foreach (var action in section.Actions)
                {
                    CreateButton(action.Label, action.Callback);
                }
            }
        }

        private void RenderOptionsTab(OptionsTab optionsTab)
        {
            var options = optionsTab.GetOptions();
            var categories = new Dictionary<string, List<OptionDefinition>>();

            foreach (var opt in options)
            {
                if (!categories.ContainsKey(opt.Category))
                    categories[opt.Category] = new List<OptionDefinition>();
                categories[opt.Category].Add(opt);
            }

            foreach (var kvp in categories)
            {
                CreateCategoryHeader(kvp.Key);

                foreach (var option in kvp.Value)
                {
                    CreateOptionControl(option);
                }
            }
        }

        private void CreateCategoryHeader(string title)
        {
            var header = CreateUIElement($"Header_{title}");
            var layout = header.AddComponent<LayoutElement>();
            layout.minHeight = 40;
            layout.preferredHeight = 40;

            var text = header.AddComponent<Text>();
            text.text = title.ToUpper();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.fontStyle = FontStyle.Bold;
            text.color = new Color(0.9f, 0.9f, 0.9f);
            text.alignment = TextAnchor.MiddleLeft;
        }

        private void CreateOptionControl(OptionDefinition option)
        {
            if (option.IsMethod)
            {
                CreateButton(option.Name, () => option.Invoke());
                return;
            }

            var valueType = option.ValueType;

            if (valueType == typeof(bool))
            {
                CreateToggle(option);
            }
            else if (valueType == typeof(int) || valueType == typeof(float) || valueType == typeof(double))
            {
                CreateNumberInput(option);
            }
            else if (valueType == typeof(string))
            {
                CreateTextInput(option);
            }
            else
            {
                CreateReadOnly(option);
            }
        }

        private void CreateButton(string label, System.Action callback)
        {
            var buttonGo = CreateUIElement($"Button_{label}");
            var layout = buttonGo.AddComponent<LayoutElement>();
            layout.minHeight = 50;
            layout.preferredHeight = 50;

            var image = buttonGo.AddComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.25f);

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => callback?.Invoke());

            var colors = button.colors;
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
            button.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
        }

        private void CreateToggle(OptionDefinition option)
        {
            var row = CreateRow(option.Name);

            var toggleGo = new GameObject("Toggle");
            toggleGo.transform.SetParent(row.transform, false);
            var toggleRect = toggleGo.AddComponent<RectTransform>();
            toggleRect.sizeDelta = new Vector2(30, 30);

            var bgImage = toggleGo.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            var checkGo = new GameObject("Check");
            checkGo.transform.SetParent(toggleGo.transform, false);
            var checkRect = checkGo.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkRect.offsetMin = Vector2.zero;
            checkRect.offsetMax = Vector2.zero;
            var checkImage = checkGo.AddComponent<Image>();
            checkImage.color = new Color(0.4f, 0.8f, 0.4f);

            var toggle = toggleGo.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.graphic = checkImage;
            toggle.isOn = (bool)option.GetValue();
            toggle.onValueChanged.AddListener(val => option.SetValue(val));
        }

        private void CreateNumberInput(OptionDefinition option)
        {
            var row = CreateRow(option.Name);

            var inputGo = new GameObject("Input");
            inputGo.transform.SetParent(row.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(100, 30);

            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = new Color(0.15f, 0.15f, 0.15f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(inputGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.supportRichText = false;

            var input = inputGo.AddComponent<InputField>();
            input.textComponent = text;
            input.text = option.GetValue()?.ToString() ?? "0";

            if (option.ValueType == typeof(int))
                input.contentType = InputField.ContentType.IntegerNumber;
            else
                input.contentType = InputField.ContentType.DecimalNumber;

            if (option.CanWrite)
            {
                input.onEndEdit.AddListener(val =>
                {
                    if (option.ValueType == typeof(int) && int.TryParse(val, out var intVal))
                        option.SetValue(intVal);
                    else if (option.ValueType == typeof(float) && float.TryParse(val, out var floatVal))
                        option.SetValue(floatVal);
                    else if (option.ValueType == typeof(double) && double.TryParse(val, out var doubleVal))
                        option.SetValue(doubleVal);
                });
            }
            else
            {
                input.interactable = false;
            }
        }

        private void CreateTextInput(OptionDefinition option)
        {
            var row = CreateRow(option.Name);

            var inputGo = new GameObject("Input");
            inputGo.transform.SetParent(row.transform, false);
            var inputRect = inputGo.AddComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(150, 30);

            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = new Color(0.15f, 0.15f, 0.15f);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(inputGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);

            var text = textGo.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            var input = inputGo.AddComponent<InputField>();
            input.textComponent = text;
            input.text = option.GetValue()?.ToString() ?? "";

            if (option.CanWrite)
            {
                input.onEndEdit.AddListener(val => option.SetValue(val));
            }
            else
            {
                input.interactable = false;
            }
        }

        private void CreateReadOnly(OptionDefinition option)
        {
            var row = CreateRow(option.Name);

            var valueGo = new GameObject("Value");
            valueGo.transform.SetParent(row.transform, false);

            var text = valueGo.AddComponent<Text>();
            text.text = option.GetValue()?.ToString() ?? "null";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = new Color(0.7f, 0.7f, 0.7f);
            text.alignment = TextAnchor.MiddleRight;
        }

        private GameObject CreateRow(string label)
        {
            var row = CreateUIElement($"Row_{label}");

            var layout = row.AddComponent<LayoutElement>();
            layout.minHeight = 45;
            layout.preferredHeight = 45;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;
            hlg.padding = new RectOffset(10, 10, 5, 5);
            hlg.spacing = 10;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(row.transform, false);
            var labelLayout = labelGo.AddComponent<LayoutElement>();
            labelLayout.flexibleWidth = 1;

            var text = labelGo.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            return row;
        }

        private GameObject CreateUIElement(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_ContentContainer, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private void ClearContent()
        {
            for (var i = _ContentContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_ContentContainer.GetChild(i).gameObject);
            }
        }
    }
}
#endif
