using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    public LevelData[] levels;
    public int currentLevelIndex = 0;

    [Header("Bottle Spawning")]
    public BottleController bottlePrefab;
    public Transform bottlesParent;
    public Vector3 firstBottlePosition = new Vector3(-3, 0, 0);
    public float bottleSpacing = 1.5f;

    [Header("References")]
    public GameController gameController;

    private BottleController[] spawnedBottles;

    void Start()
    {
        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int levelIndex)
    {
        // Проверка корректности индекса
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

        for (int i = 0; i < bottleCount; i++)
        {
            // Вычисляем позицию бутылки
            Vector3 position = firstBottlePosition + Vector3.right * (i * bottleSpacing);

            // Создаём бутылку
            BottleController bottle = Instantiate(bottlePrefab, position, Quaternion.identity);

            // Помещаем в родителя (для чистоты Hierarchy)
            if (bottlesParent != null)
            {
                bottle.transform.SetParent(bottlesParent);
            }

            // Переименовываем
            bottle.name = $"Bottle_{i}";

            // Применяем конфигурацию
            ApplyConfiguration(bottle, level.bottles[i]);

            // Сохраняем ссылку
            spawnedBottles[i] = bottle;
        }

        // Передаём массив бутылок в GameController
        if (gameController != null)
        {
            gameController.allBottles = spawnedBottles;
        }
    }

    void ApplyConfiguration(BottleController bottle, LevelData.BottleConfiguration config)
    {
        // Инициализируем массив цветов
        if (bottle.bottleColors == null || bottle.bottleColors.Length != 4)
        {
            bottle.bottleColors = new Color[4];
        }

        bottle.numberOfColorsInBottle = config.numberOfColors;

        Debug.Log($"Бутылка {bottle.name}: config.colors = {config.colors}, Length = {config.colors?.Length}");

        for (int i = 0; i < 4; i++)
        {
            if (i < config.numberOfColors)
            {
                bottle.bottleColors[i] = config.colors[i];
                Debug.Log($"  i={i}, пытаюсь взять config.colors[{i}]");
            }
            else
            {
                bottle.bottleColors[i] = Color.clear;
            }
        }

        // Обновляем визуал
        bottle.UpdateColorsOnShader();
        bottle.UpdateTopColorValues();
        bottle.UpdateBottleState();
        bottle.UpdateCorkVisibility();

        // ИСПРАВЛЕНО: Проверяем наличие fillAmounts перед использованием
        if (bottle.fillAmounts != null && bottle.fillAmounts.Length > bottle.numberOfColorsInBottle)
        {
            bottle.bottleMaskSR.material.SetFloat("_FillAmount", bottle.fillAmounts[bottle.numberOfColorsInBottle]);
        }
        else
        {
            Debug.LogWarning($"У бутылки {bottle.name} не настроен массив fillAmounts!");
        }
    }

    void ClearBottles()
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
