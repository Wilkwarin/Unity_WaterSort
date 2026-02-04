using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Selected Bottles")]
    public BottleController FirstBottle;
    public BottleController SecondBottle;

    [Header("Level Setup")]
    public BottleController[] allBottles; // Массив всех бутылок на уровне

    [Header("UI References")]
    public GameObject winPanel;

    private bool levelCompleted = false;
    public bool isBusy = false;

    void Start() { }

    void Update()
    {
        if (levelCompleted || isBusy)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.GetComponent<BottleController>() != null)
                {
                    if (FirstBottle == null)
                    {
                        BottleController selectedBottle = hit.collider.GetComponent<BottleController>();

                        if (selectedBottle.currentState != BottleState.InProgress)
                        {
                            return;
                        }

                        FirstBottle = selectedBottle;
                        FirstBottle.Select();
                    }
                    else
                    {
                        if (FirstBottle == hit.collider.GetComponent<BottleController>())
                        {
                            FirstBottle.Deselect();
                            FirstBottle = null;
                        }
                        else
                        {
                            SecondBottle = hit.collider.GetComponent<BottleController>();
                            FirstBottle.bottleControllerRef = SecondBottle;

                            FirstBottle.UpdateTopColorValues();
                            SecondBottle.UpdateTopColorValues();

                            if (SecondBottle.FillBottleCheck(FirstBottle.topColor) == true)
                            {
                                FirstBottle.StartColorTransfer();
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                            else
                            {
                                FirstBottle.Deselect();
                                FirstBottle = null;
                                SecondBottle = null;
                            }
                        }
                    }
                }
            }
        }
    }

    public void CheckWinCondition()
    {
        if (levelCompleted)
        {
            return;
        }

        foreach (BottleController bottle in allBottles)
        {
            if (bottle == null)
            {
                Debug.LogWarning("Пустая ссылка в массиве allBottles");
                continue;
            }

            if (bottle.currentState == BottleState.InProgress)
            {
                return;
            }
        }

        OnLevelComplete();
    }

    void OnLevelComplete()
    {
        levelCompleted = true;
        Debug.Log("Уровень завершён!");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void RestartLevel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        levelCompleted = false;
        isBusy = false;
        FirstBottle = null;
        SecondBottle = null;

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.RestartCurrentLevel();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnNextLevelButtonPressed()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        levelCompleted = false;
        isBusy = false;

        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.NextLevel();
        }
    }

}
