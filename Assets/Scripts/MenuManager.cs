using UnityEngine;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public TextMeshProUGUI continueButtonText;

    [Header("References")]
    public LevelManager levelManager;

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