using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public TextMeshProUGUI continueButtonText;

    [Header("References")]
    public LevelManager levelManager;
    public GameController gameController;

    void Start()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        UpdateContinueButton();
    }

    public void HideMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        GameController gameController = Object.FindFirstObjectByType<GameController>();

        if (gameController != null)
        {
            if (gameController.restartButton != null)
            {
                gameController.restartButton.SetActive(true);
            }
        }
    }

    void UpdateContinueButton()
    {
        int currentLevel = ProgressManager.Instance.GetCurrentLevel();

        if (continueButtonText != null)
        {
            continueButtonText.text = $"{currentLevel + 1}";
        }
    }

    public void OnContinueButtonPressed()
    {
        int currentLevel = ProgressManager.Instance.GetCurrentLevel();

        HideMenu();

        if (levelManager != null)
        {
            levelManager.LoadLevel(currentLevel);
        }

        GameController gc = FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            gc.OnLevelStarted();
        }
    }

    public void OnQuitButtonPressed()
    {
        Debug.Log("Выход из игры");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}