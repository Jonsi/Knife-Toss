using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private RotatingTarget rotatingTarget;
    [SerializeField] private KnifeThrower knifeThrower;
    [SerializeField] private GameObject failText;
    [SerializeField] private LevelConfig levelConfig;
    [SerializeField] private GameObject knifePrefab;
    
    [Header("Level Settings")]
    [SerializeField] private int startLevel = 1;
    
    private int currentLevel;
    private LevelData currentLevelData;
    private int knivesStuck;
    private int knivesThrown;
    private bool isLevelComplete;
    private bool isGameOver;
    
    private const string HIGHEST_LEVEL_KEY = "HighestLevel";
    
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
        
        // Load highest level or use start level
        int highestLevel = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, startLevel);
        currentLevel = Mathf.Max(startLevel, highestLevel);
        
        // Initialize level system
        if (levelConfig == null)
        {
            Debug.LogWarning("LevelConfig not assigned! Creating default level data.");
        }
    }
    
    private void Start()
    {
        StartLevel(currentLevel);
    }
    
    private void OnEnable()
    {
        Knife.OnKnifeFailed += HandleGameFail;
        Knife.OnKnifeStuck += HandleKnifeStuck;
    }
    
    private void OnDisable()
    {
        Knife.OnKnifeFailed -= HandleGameFail;
        Knife.OnKnifeStuck -= HandleKnifeStuck;
    }
    
    private void HandleGameFail()
    {
        if (isGameOver || isLevelComplete) return;
        
        isGameOver = true;
        
        if (failText != null)
        {
            failText.SetActive(true);
        }
        
        // Pause the game
        Time.timeScale = 0f;
    }
    
    private void HandleKnifeStuck()
    {
        if (isGameOver || isLevelComplete) return;
        
        knivesStuck++;
        
        Debug.Log($"Knife stuck! Progress: {knivesStuck}/{currentLevelData.requiredKnives}");
        
        // Check if level is complete
        if (knivesStuck >= currentLevelData.requiredKnives)
        {
            CompleteLevel();
        }
        // Check if we've run out of knives and haven't reached the target
        else if (currentLevelData.maxKnives > 0 && knivesThrown >= currentLevelData.maxKnives && knivesStuck < currentLevelData.requiredKnives)
        {
            HandleGameFail();
        }
    }
    
    public void StartLevel(int level)
    {
        currentLevel = level;
        knivesStuck = 0;
        knivesThrown = 0;
        isLevelComplete = false;
        isGameOver = false;
        
        // Load level data
        if (levelConfig != null)
        {
            currentLevelData = levelConfig.GetLevelData(level);
        }
        else
        {
            // Fallback default level
            currentLevelData = new LevelData();
        }
        
        // Reset game state
        ResetGame();
        
        // Apply level settings to target
        if (rotatingTarget != null)
        {
            rotatingTarget.SetRotationSpeed(currentLevelData.rotationSpeed);
            rotatingTarget.SetDirectionChangeInterval(currentLevelData.directionChangeInterval);
            rotatingTarget.SetRandomizeDirection(currentLevelData.randomizeDirection);
        }
        
        // Update knife thrower with level limits
        if (knifeThrower != null)
        {
            knifeThrower.SetMaxKnives(currentLevelData.maxKnives);
        }
        
        // Spawn pre-stuck knives
        SpawnPreStuckKnives();
        
        Debug.Log($"Level {currentLevel} started! Required knives: {currentLevelData.requiredKnives}");
    }
    
    private void SpawnPreStuckKnives()
    {
        if (currentLevelData.preStuckKnives <= 0 || knifePrefab == null || targetTransform == null)
            return;
        
        CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
        float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;
        
        float angleStep = currentLevelData.preStuckKnifeSpacing;
        float startAngle = Random.Range(0f, 360f);
        
        for (int i = 0; i < currentLevelData.preStuckKnives; i++)
        {
            float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = (Vector2)targetTransform.position + direction * targetRadius;
            
            GameObject knifeObj = Instantiate(knifePrefab, position, Quaternion.identity);
            knifeObj.transform.SetParent(targetTransform);
            
            // Rotate knife to point outward
            float knifeAngle = angle * Mathf.Rad2Deg;
            knifeObj.transform.rotation = Quaternion.Euler(0, 0, knifeAngle);
            
            // Mark as stuck and disable movement
            Knife knife = knifeObj.GetComponent<Knife>();
            if (knife != null)
            {
                knife.MarkAsPreStuck();
            }
            
            knivesStuck++;
        }
        
        Debug.Log($"Spawned {currentLevelData.preStuckKnives} pre-stuck knives");
    }
    
    private void CompleteLevel()
    {
        if (isLevelComplete) return;
        
        isLevelComplete = true;
        
        // Save highest level
        int highestLevel = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 1);
        if (currentLevel > highestLevel)
        {
            PlayerPrefs.SetInt(HIGHEST_LEVEL_KEY, currentLevel);
            PlayerPrefs.Save();
        }
        
        Debug.Log($"Level {currentLevel} completed!");
        
        // Pause the game (will be resumed when next level starts)
        Time.timeScale = 0f;
    }
    
    public void NextLevel()
    {
        StartLevel(currentLevel + 1);
    }
    
    public void RetryLevel()
    {
        StartLevel(currentLevel);
    }
    
    public void OnKnifeThrown()
    {
        knivesThrown++;
        
        // Check if we've exceeded max knives
        if (currentLevelData.maxKnives > 0 && knivesThrown >= currentLevelData.maxKnives)
        {
            // Disable throwing
            if (knifeThrower != null)
            {
                knifeThrower.SetCanThrow(false);
            }
        }
    }
    
    // Public getters for UI
    public int GetCurrentLevel() => currentLevel;
    public int GetKnivesStuck() => knivesStuck;
    public int GetRequiredKnives() => currentLevelData != null ? currentLevelData.requiredKnives : 0;
    public int GetKnivesRemaining() => currentLevelData != null ? Mathf.Max(0, currentLevelData.requiredKnives - knivesStuck) : 0;
    public int GetKnivesThrown() => knivesThrown;
    public int GetMaxKnives() => currentLevelData != null ? currentLevelData.maxKnives : 0;
    public bool IsLevelComplete() => isLevelComplete;
    public bool IsGameOver() => isGameOver;
}

