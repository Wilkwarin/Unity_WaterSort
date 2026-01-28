using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    public BottleController FirstBottle;
    public BottleController SecondBottle;

    [Header("Level Setup")]
    public BottleController[] allBottles; // Массив всех бутылок на уровне

    [Header("UI References")]
    public GameObject winPanel;

    private bool levelCompleted = false;

    void Start() { }

    void Update()
    {
        if (levelCompleted)
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
                        FirstBottle = hit.collider.GetComponent<BottleController>();
                    }
                    else
                    {
                        if (FirstBottle == hit.collider.GetComponent<BottleController>())
                        {
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

            if (!bottle.IsBottleComplete()) // Если хотя бы одна бутылка не завершена - победы нет
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

}
