using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    private const string CURRENT_LEVEL_KEY = "CurrentLevel";
    public static ProgressManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0); // По умолчанию первый уровень (индекс 0)
    }

    public void SaveCurrentLevel(int levelIndex)
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, levelIndex);
        PlayerPrefs.Save();
        Debug.Log($"Сохранён уровень {levelIndex}");
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(CURRENT_LEVEL_KEY);
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен");
    }
#if UNITY_EDITOR
[ContextMenu("Reset Progress")]
private void DebugResetProgress()
{
    ResetProgress();
}
#endif
}