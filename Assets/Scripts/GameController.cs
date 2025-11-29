using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private RotatingTarget rotatingTarget;
    [SerializeField] private KnifeThrower knifeThrower;
    [SerializeField] private GameObject failText;
    [SerializeField] private LevelConfig levelConfig;
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI knivesLeftText;
    [SerializeField] private Button nextLevelButton;
    
    [Header("Level Settings")]
    [SerializeField] private int startLevel = 1;
    
    private int currentLevel;
    private LevelData currentLevelData;
    private int knivesStuck;
    private int knivesThrown;
    private bool isLevelComplete;
    private bool isGameOver;
    private GameObject knivesParent;
    
    private const string HIGHEST_LEVEL_KEY = "HighestLevel";
    private const string KNIVES_PARENT_NAME = "Knives";
    
    public void ResetGame()
    {
        // Reset game state flags
        isGameOver = false;
        isLevelComplete = false;
        
        // Reset knife counters
        knivesStuck = 0;
        knivesThrown = 0;
        
        // Destroy the knives parent GameObject (this will destroy all knives)
        DestroyKnivesParent();
        
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
        
        // Disable next level button
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        
        // Update UI text to reflect reset state
        if (currentLevelData != null)
        {
            UpdateKnivesLeftText();
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
        
        // Disable fail text on start
        if (failText != null)
        {
            failText.SetActive(false);
        }
        
        // Disable next level button on start
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
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
        
        // Update knives left text
        UpdateKnivesLeftText();
        
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
        // Destroy any existing knives parent first
        DestroyKnivesParent();
        
        // Create new knives parent for this level
        CreateKnivesParent();
        
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
            knifeThrower.SetKnivesParent(knivesParent);
        }
        
        // Spawn pre-stuck knives
        SpawnPreStuckKnives();
        
        // Update UI text
        UpdateLevelText();
        UpdateKnivesLeftText();
        
        // Disable next level button when starting a new level
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(false);
        }
        
        Debug.Log($"Level {currentLevel} started! Required knives: {currentLevelData.requiredKnives}");
    }
    
    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = $"Level {currentLevel}";
        }
    }
    
    private void UpdateKnivesLeftText()
    {
        if (knivesLeftText != null && currentLevelData != null)
        {
            int remaining = Mathf.Max(0, currentLevelData.requiredKnives - knivesStuck);
            knivesLeftText.text = $"Knives Left: {remaining}";
        }
    }
    
    private void CreateKnivesParent()
    {
        // Destroy any existing knives parent first
        DestroyKnivesParent();
        
        // Create new parent GameObject for all knives in this level
        knivesParent = new GameObject(KNIVES_PARENT_NAME);
        knivesParent.transform.SetParent(transform); // Parent to GameController's transform
    }
    
    private void DestroyKnivesParent()
    {
        // Destroy the knives parent GameObject (this will destroy all child knives)
        if (knivesParent != null)
        {
            Destroy(knivesParent);
            knivesParent = null;
        }
        
        // Also try to find and destroy any orphaned knives parent
        GameObject existingParent = GameObject.Find(KNIVES_PARENT_NAME);
        if (existingParent != null)
        {
            Destroy(existingParent);
        }
        
        // Clear all knives that are children of the target (stuck knives)
        if (targetTransform != null)
        {
            for (int i = targetTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = targetTransform.GetChild(i);
                if (child != null)
                {
                    Knife knife = child.GetComponent<Knife>();
                    if (knife != null || child.CompareTag("Knife"))
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        
        // Final cleanup: find and destroy any remaining knives in the scene
        Knife[] allKnives = FindObjectsByType<Knife>(FindObjectsSortMode.None);
        foreach (Knife knife in allKnives)
        {
            if (knife != null && knife.gameObject != null)
            {
                Destroy(knife.gameObject);
            }
        }
    }
    
    private void SpawnPreStuckKnives()
    {
        if (currentLevelData.preStuckKnives <= 0 || knifePrefab == null || targetTransform == null)
            return;
        
        CircleCollider2D targetCollider = targetTransform.GetComponent<CircleCollider2D>();
        float targetRadius = targetCollider != null ? targetCollider.radius : 0.5f;
        
        float angleStep = currentLevelData.preStuckKnifeSpacing;
        float startAngle = Random.Range(0f, 360f);
        
        // Get embedOffset from knife prefab if available
        Knife sampleKnife = knifePrefab.GetComponent<Knife>();
        float embedOffset = 0.02f; // Default value
        if (sampleKnife != null)
        {
            // Use reflection or a public getter - for now use default
            // The embedOffset is private, so we'll use the default
        }
        
        for (int i = 0; i < currentLevelData.preStuckKnives; i++)
        {
            float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 position = (Vector2)targetTransform.position + direction * (targetRadius + embedOffset);
            
            // Calculate direction from knife position to center BEFORE instantiating
            Vector2 targetCenter = targetTransform.position;
            Vector2 directionToCenter = (targetCenter - position).normalized;
            // Atan2 returns angle where 0° points right (positive X axis)
            // Unity sprites typically point up (positive Y) at 0°, so subtract 90° to align
            float knifeAngle = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg - 90f;
            Quaternion rotation = Quaternion.Euler(0, 0, knifeAngle);
            
            GameObject knifeObj = Instantiate(knifePrefab, position, rotation);
            
            // Parent to target for visual positioning, but also keep reference in knives parent
            knifeObj.transform.SetParent(targetTransform);
            
            // Also ensure it's tracked in the knives parent hierarchy
            if (knivesParent != null)
            {
                // Create a wrapper or just track it - knives stuck to target stay as children of target
                // but we'll destroy the knives parent which will clean up any orphaned knives
            }
            
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
        
        // Enable next level button
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
        }
        
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

