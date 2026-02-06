using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;

    [Header("Bottle Spawning")]
    public BottleController bottlePrefab;
    public Transform bottlesParent;
    public float bottleSpacing = 1.0f;
    public float rowSpacing = 1.5f;

    [Header("References")]
    public GameController gameController;

    private BottleController[] spawnedBottles;

    void Start(){}

    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"Уровень {levelIndex} не существует!");
            return;
        }

        ClearBottles();

        LevelData level = levels[levelIndex];
        currentLevelIndex = levelIndex;

        Debug.Log($"Загружаем уровень: {level.levelName}");

        SpawnBottles(level);
    }

    void SpawnBottles(LevelData level)
    {
        int bottleCount = level.bottles.Length;
        Debug.Log($"Создаём {bottleCount} бутылок");
        spawnedBottles = new BottleController[bottleCount];

        if (bottleCount <= 6)
        {
            SpawnSingleRow(level, bottleCount);
        }
        else
        {
            SpawnTwoRows(level, bottleCount);
        }

        if (gameController != null)
        {
            gameController.allBottles = spawnedBottles;
        }
    }

    void SpawnSingleRow(LevelData level, int bottleCount)
    {
        float totalWidth = (bottleCount - 1) * bottleSpacing;

        float startX = -totalWidth / 2f;
        float y = 0f;

        for (int i = 0; i < bottleCount; i++)
        {
            Vector3 position = new Vector3(startX + i * bottleSpacing, y, 0);
            CreateBottle(i, position, level.bottles[i]);
        }
    }

    void SpawnTwoRows(LevelData level, int bottleCount)
    {
        int topRowCount = (bottleCount + 1) / 2; // Если нечётное, сверху на 1 больше
        int bottomRowCount = bottleCount / 2;

        float topY = rowSpacing / 2f;
        float topWidth = (topRowCount - 1) * bottleSpacing;
        float topStartX = -topWidth / 2f;

        for (int i = 0; i < topRowCount; i++)
        {
            Vector3 position = new Vector3(topStartX + i * bottleSpacing, topY, 0);
            CreateBottle(i, position, level.bottles[i]);
        }

        float bottomY = -rowSpacing / 2f;
        float bottomWidth = (bottomRowCount - 1) * bottleSpacing;
        float bottomStartX = -bottomWidth / 2f;

        for (int i = 0; i < bottomRowCount; i++)
        {
            int bottleIndex = topRowCount + i;
            Vector3 position = new Vector3(bottomStartX + i * bottleSpacing, bottomY, 0);
            CreateBottle(bottleIndex, position, level.bottles[bottleIndex]);
        }
    }

    void CreateBottle(int index, Vector3 position, LevelData.BottleConfiguration config)
    {
        BottleController bottle = Instantiate(bottlePrefab, position, Quaternion.identity);

        if (bottlesParent != null)
        {
            bottle.transform.SetParent(bottlesParent);
        }

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
        if (spawnedBottles != null)
        {
            foreach (var bottle in spawnedBottles)
            {
                if (bottle != null)
                {
                    Destroy(bottle.gameObject);
                }
            }
            spawnedBottles = null;

            if (gameController != null)
            {
                gameController.allBottles = null;
            }
        }
    }

    public void NextLevel()
    {
        int nextIndex = currentLevelIndex + 1;
        if (nextIndex < levels.Length)
        {
            LoadLevel(nextIndex);
        }
        else
        {
            Debug.Log("Это был последний уровень!");
        }
    }

    public void RestartCurrentLevel()
    {
        LoadLevel(currentLevelIndex);
    }

}