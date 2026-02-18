using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;
    public bool useGenerator = false;
    public LevelGenerator levelGenerator;
    private LevelData currentGeneratedLevel;

    [Header("Spacing Settings")]
    public float maxBottleDistance = 0.7f;

    [Header("Bottle Spawning")]
    public BottleController bottlePrefab;
    public Transform bottlesParent;

    [Header("Layout")]
    public float horizontalPadding = 1f;
    public float verticalPadding = 1f;

    [Header("References")]
    public GameController gameController;
    public Camera mainCamera;

    private BottleController[] spawnedBottles;

    void Start() { }

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void LoadLevel(int levelIndex)
    {
        ClearBottles();
        currentLevelIndex = levelIndex;

        if (useGenerator && levelIndex >= levels.Length)
        {
            LoadGeneratedLevel();
            return;
        }

        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"Уровень {levelIndex} не существует!");
            return;
        }

        LevelData level = levels[levelIndex];

        AdjustCamera(level);
        SpawnBottles(level);

        if (gameController != null)
        {
            gameController.allBottles = spawnedBottles;
        }

        Debug.Log($"Загружен ручной уровень {levelIndex + 1}");
    }

    void LoadGeneratedLevel()
    {
        ClearBottles();

        if (currentGeneratedLevel == null)
        {
            currentGeneratedLevel = levelGenerator.GenerateLevel();
        }

        AdjustCamera(currentGeneratedLevel);
        SpawnBottles(currentGeneratedLevel);

        if (gameController != null)
        {
            gameController.allBottles = spawnedBottles;
            gameController.CheckWinCondition();
        }

        Debug.Log($"Загружен сгенерированный уровень {currentLevelIndex + 1}");
    }

    void SpawnBottles(LevelData level)
    {
        int bottleCount = level.bottles.Length;
        spawnedBottles = new BottleController[bottleCount];

        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        int rows = bottleCount <= 6 ? 1 : 2;
        int colsTop = rows == 1 ? bottleCount : (bottleCount + 1) / 2;
        int colsBottom = bottleCount / 2;

        float maxRowWidth = screenWidth * 0.75f;
        float commonSpacing = (colsTop <= 1) ? 0 : Mathf.Min(maxRowWidth / (colsTop - 1), maxBottleDistance);

        float rowOffsetY = screenHeight * 0.3f;
        float startY = (rows == 1) ? 0f : rowOffsetY / 2f;

        int index = 0;
        SpawnRow(colsTop, startY, commonSpacing, level, ref index);

        if (rows == 2)
        {
            SpawnRow(colsBottom, startY - rowOffsetY, commonSpacing, level, ref index);
        }
    }

    void SpawnRow(int count, float y, float spacing, LevelData level, ref int index)
    {
        float totalRowWidth = spacing * (count - 1);
        float startX = -totalRowWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = new Vector3(startX + i * spacing, y, 0);
            CreateBottle(index, pos, level.bottles[index]);
            index++;
        }
    }

    void CreateBottle(int index, Vector3 position, LevelData.BottleConfiguration config)
    {
        BottleController bottle = Instantiate(bottlePrefab, position, Quaternion.identity, bottlesParent);
        bottle.name = $"Bottle_{index}";

        ApplyConfiguration(bottle, config);
        spawnedBottles[index] = bottle;
    }

    void ApplyConfiguration(BottleController bottle, LevelData.BottleConfiguration config)
    {
        if (bottle.bottleColors == null || bottle.bottleColors.Length != 4)
        {
            bottle.bottleColors = new Color[4];
        }

        bottle.numberOfColorsInBottle = config.numberOfColors;

        for (int i = 0; i < 4; i++)
        {
            if (i < config.numberOfColors && config.colors != null && i < config.colors.Length)
            {
                bottle.bottleColors[i] = config.colors[i];
            }
            else
            {
                bottle.bottleColors[i] = Color.clear;
            }
        }

        bottle.UpdateColorsOnShader();
        bottle.UpdateTopColorValues();
        bottle.UpdateBottleState();
        bottle.UpdateCorkVisibility();

        if (bottle.fillAmounts != null && bottle.fillAmounts.Length > bottle.numberOfColorsInBottle)
        {
            bottle.bottleMaskSR.material.SetFloat("_FillAmount", bottle.fillAmounts[bottle.numberOfColorsInBottle]);
        }
        else
        {
            Debug.LogWarning($"У бутылки {bottle.name} не настроен массив fillAmounts!");
        }
    }

    public void ClearBottles()
    {
        if (spawnedBottles == null) return;

        foreach (var bottle in spawnedBottles)
        {
            if (bottle != null)
                Destroy(bottle.gameObject);
        }

        spawnedBottles = null;

        if (gameController != null)
            gameController.allBottles = null;
    }

    public void NextLevel()
    {
        currentLevelIndex++;
        currentGeneratedLevel = null;

        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            LoadLevel(currentLevelIndex);
        }
        else
        {
            LoadGeneratedLevel();
        }
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

    void AdjustCamera(LevelData level)
    {
        int bottleCount = level.bottles.Length;

        float screenHeight = mainCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * mainCamera.aspect;

        int rows = bottleCount <= 6 ? 1 : 2;

        float usableWidth = screenWidth * (1f - horizontalPadding);
        float usableHeight = screenHeight * (1f - verticalPadding);

        float bottleSpacingX = usableWidth / Mathf.Max(1, bottleCount / rows);
        float bottleSpacingY = usableHeight / rows;

        Debug.Log($"Camera adjusted: width={screenWidth}, height={screenHeight}");
    }
}