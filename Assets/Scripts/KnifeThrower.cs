using UnityEngine;
using UnityEngine.InputSystem;

public class KnifeThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform launcherTransform;
    [SerializeField] private InputActionAsset inputActionAsset;
    
    [Header("Throwing Settings")]
    [SerializeField] private float throwSpeed = 10f;
    
    private InputAction attackAction;
    private Knife currentKnife;
    private bool isKnifeInFlight;
    private int maxKnives = 0;
    private int knivesThrown = 0;
    private bool canThrow = true;
    private GameController gameController;
    private GameObject knivesParent;
    
    private void Awake()
    {
        if (inputActionAsset != null)
        {
            attackAction = inputActionAsset.FindAction("Player/Attack");
        }
        
        gameController = FindFirstObjectByType<GameController>();
    }
    
    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.Enable();
            attackAction.performed += OnThrowInput;
        }
    }
    
    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnThrowInput;
            attackAction.Disable();
        }
    }
    
    private void OnThrowInput(InputAction.CallbackContext context)
    {
        if (!canThrow || isKnifeInFlight || knifePrefab == null || launcherTransform == null)
            return;
        
        // Check if we've reached max knives
        if (maxKnives > 0 && knivesThrown >= maxKnives)
        {
            return;
        }
        
        ThrowKnife();
    }
    
    private void ThrowKnife()
    {
        GameObject knifeObject = Instantiate(knifePrefab, launcherTransform.position, launcherTransform.rotation);
        
        // Parent knife to knives parent if available
        if (knivesParent != null)
        {
            knifeObject.transform.SetParent(knivesParent.transform);
        }
        
        currentKnife = knifeObject.GetComponent<Knife>();
        
        if (currentKnife != null)
        {
            currentKnife.Initialize(this, throwSpeed);
            isKnifeInFlight = true;
            knivesThrown++;
            
            // Notify GameController
            if (gameController != null)
            {
                gameController.OnKnifeThrown();
            }
        }
    }
    
    public void OnKnifeResolved()
    {
        isKnifeInFlight = false;
        currentKnife = null;
    }
    
    public void ResetState()
    {
        // Destroy current knife if it exists
        if (currentKnife != null)
        {
            Destroy(currentKnife.gameObject);
            currentKnife = null;
        }
        isKnifeInFlight = false;
        knivesThrown = 0;
        canThrow = true;
    }
    
    public void SetMaxKnives(int max)
    {
        maxKnives = max;
        knivesThrown = 0;
    }
    
    public void SetCanThrow(bool allow)
    {
        canThrow = allow;
    }
    
    public void SetKnivesParent(GameObject parent)
    {
        knivesParent = parent;
    }
}

