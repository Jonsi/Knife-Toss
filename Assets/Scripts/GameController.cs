using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private RotatingTarget rotatingTarget;
    [SerializeField] private KnifeThrower knifeThrower;
    [SerializeField] private GameObject failText;
    
    public void ResetGame()
    {
        // Clear all stuck knives (knives that are children of the target)
        if (targetTransform != null)
        {
            // Destroy all child knives
            for (int i = targetTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = targetTransform.GetChild(i);
                if (child.CompareTag("Knife") || child.GetComponent<Knife>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        // Clear any knives in flight or in the scene
        Knife[] allKnives = FindObjectsByType<Knife>(FindObjectsSortMode.None);
        foreach (Knife knife in allKnives)
        {
            Destroy(knife.gameObject);
        }
        
        // Reset target rotation
        if (rotatingTarget != null)
        {
            rotatingTarget.ResetRotation();
        }
        else if (targetTransform != null)
        {
            targetTransform.rotation = Quaternion.identity;
        }
        
        // Reset knife thrower state (allow throwing again)
        if (knifeThrower != null)
        {
            knifeThrower.ResetState();
        }
        
        // Disable fail text
        if (failText != null)
        {
            failText.SetActive(false);
        }
        
        // Reset time scale
        Time.timeScale = 1f;
        
        Debug.Log("Game reset!");
    }
    
    private void Awake()
    {
        // Auto-find references if not set
        if (targetTransform == null)
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag("Target");
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
            }
        }
        
        if (rotatingTarget == null && targetTransform != null)
        {
            rotatingTarget = targetTransform.GetComponent<RotatingTarget>();
        }
        
        if (knifeThrower == null)
        {
            knifeThrower = FindFirstObjectByType<KnifeThrower>();
        }
        
        if (failText == null)
        {
            GameObject failTextObj = GameObject.Find("FailText");
            if (failTextObj != null)
            {
                failText = failTextObj;
            }
        }
        
        // Disable fail text on start
        if (failText != null)
        {
            failText.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        Knife.OnKnifeFailed += HandleGameFail;
    }
    
    private void OnDisable()
    {
        Knife.OnKnifeFailed -= HandleGameFail;
    }
    
    private void HandleGameFail()
    {
        if (failText != null)
        {
            failText.SetActive(true);
        }
        
        // Pause the game
        Time.timeScale = 0f;
    }
}

