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
    
    private void Awake()
    {
        if (inputActionAsset != null)
        {
            attackAction = inputActionAsset.FindAction("Player/Attack");
        }
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
        if (!isKnifeInFlight && knifePrefab != null && launcherTransform != null)
        {
            ThrowKnife();
        }
    }
    
    private void ThrowKnife()
    {
        GameObject knifeObject = Instantiate(knifePrefab, launcherTransform.position, launcherTransform.rotation);
        currentKnife = knifeObject.GetComponent<Knife>();
        
        if (currentKnife != null)
        {
            currentKnife.Initialize(this, throwSpeed);
            isKnifeInFlight = true;
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
    }
}

