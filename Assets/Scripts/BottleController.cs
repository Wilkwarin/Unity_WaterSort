using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    [Header("Visual Components")]
    public SpriteRenderer bottleMaskSR;
    public LineRenderer lineRenderer;

    [Header("Bottle Colors")]
    public Color[] bottleColors;
    public Color topColor;
    public int numberOfTopColorLayers = 1;

    [Header("Animation Curves")]
    public AnimationCurve ScaleAndRotationMultiplierCurve;
    public AnimationCurve FillAmountCurve;
    public AnimationCurve RotationSpeedMultiplier;

    [Header("Fill & Rotation Settings")]
    public float[] fillAmounts;
    public float[] rotationValues;
    [Range(0, 4)]
    public int numberOfColorsInBottle = 4;
    private int rotationIndex = 0;
    private int numberOfColorsToTransfer = 0;

    [Header("Animation Timing")]
    public float timeToRotate = 1.0f;
    public float timeToRotateBack = 0.4f;

    [Header("Rotation Points")]
    public Transform leftRotationPoint;
    public Transform rightRotationPoint;
    private Transform chosenRotationPoint;

    [Header("Cork Settings")]
    public GameObject corkPrefab;
    public Transform corkAnchor;
    private GameObject corkInstance;
    private ParticleSystem corkParticles;

    [Header("References")]
    public GameController gameController;
    public BottleController bottleControllerRef;

    [Header("Debug")]
    public bool justThisBottle = false;

    [Header("Selection Effects")]
    public float selectionLiftHeight = 0.2f; // Высота подъёма

    private bool isSelected = false;
    private Vector3 normalPosition;

    private float directionMultiplier = 1.0f;
    private Vector3 originalPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    void Awake()
    {
        if (gameController == null)
        {
            gameController = FindFirstObjectByType<GameController>();
        }
    }

    void Start()
    {
        bottleMaskSR.material.SetFloat("_FillAmount", fillAmounts[numberOfColorsInBottle]); // устанавливаем начальный уровень жидкости из массива fillAmounts

        originalPosition = transform.position;

        UpdateColorsOnShader();

        UpdateTopColorValues();

        if (corkPrefab != null)
        {
            corkInstance = Instantiate(
                corkPrefab,
                corkAnchor
            );

            corkInstance.transform.localPosition = Vector3.zero;
            corkInstance.transform.localRotation = Quaternion.identity;

            corkParticles = corkInstance.GetComponent<ParticleSystem>();
            corkInstance.SetActive(false);
        }

        UpdateCorkVisibility();
    }

    public void Select()
    {
        if (isSelected) return;

        isSelected = true;
        normalPosition = transform.position;

        transform.position = normalPosition + Vector3.up * selectionLiftHeight;
    }

    public void Deselect()
    {
        if (!isSelected) return;

        isSelected = false;

        transform.position = originalPosition;
    }

    void Update() // для проверки поворота бутылочки клавишей Р
    {
        if (Keyboard.current.pKey.wasReleasedThisFrame && justThisBottle == true)
        {
            UpdateTopColorValues();

            // Возьми другую бутылку, найди первый свободный слой и столько раз, сколько можно перелить, запиши туда верхний цвет текущей бутылки

            if (bottleControllerRef.FillBottleCheck(topColor)) // бутылка-реципиент.можно в тебя налить?
            {
                ChoseRotationPointAndDirection();

                numberOfColorsToTransfer = Mathf.Min( // защита от перелива - сколько льём?
                    numberOfTopColorLayers, // сколько одинаковых слоёв сверху
                    4 - bottleControllerRef.numberOfColorsInBottle // сколько места в реципиенте
                    );

                for (int i = 0; i < numberOfColorsToTransfer; i++) // знаем, сколько льём, и льём
                {
                    bottleControllerRef.bottleColors[ // реципиент.его массив цветов
                        bottleControllerRef.numberOfColorsInBottle + i //сколько слоёв есть в бутылке - льём в пустой
                        ] = topColor; // льём мы, собственно, верхний цвет
                }
                bottleControllerRef.UpdateColorsOnShader(); // обновили шейдер реципиента
            }

            CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);
            StartCoroutine(RotateBottle());
        }
    }

    public void StartColorTransfer()
    {
        ChoseRotationPointAndDirection();

        numberOfColorsToTransfer = Mathf.Min( // защита от перелива - сколько льём?
            numberOfTopColorLayers, // сколько одинаковых слоёв сверху
            4 - bottleControllerRef.numberOfColorsInBottle // сколько места в реципиенте
            );

        for (int i = 0; i < numberOfColorsToTransfer; i++) // знаем, сколько льём, и льём
        {
            bottleControllerRef.bottleColors[ // реципиент.его массив цветов
                bottleControllerRef.numberOfColorsInBottle + i //сколько слоёв есть в бутылке - льём в пустой
                ] = topColor; // льём мы, собственно, верхний цвет
        }
        bottleControllerRef.UpdateColorsOnShader();

        CalculateRotationIndex(4 - bottleControllerRef.numberOfColorsInBottle);

        transform.GetComponent<SpriteRenderer>().sortingOrder += 2;
        bottleMaskSR.sortingOrder += 2;

        StartCoroutine(MoveBottle());
    }

    IEnumerator MoveBottle() // бутылка-донор движется к реципиенту
    {
        startPosition = transform.position;
        if (chosenRotationPoint == leftRotationPoint)
        {
            endPosition = bottleControllerRef.rightRotationPoint.position;
        }
        else
        {
            endPosition = bottleControllerRef.leftRotationPoint.position;
        }

        float t = 0;

        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        StartCoroutine(RotateBottle());
    }

    IEnumerator MoveBottleBack() // бутылка-донор движется от реципиента
    {
        startPosition = transform.position;
        endPosition = originalPosition;

        float t = 0;

        while (t <= 1)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            t += Time.deltaTime * 2;

            yield return new WaitForEndOfFrame();
        }

        transform.position = endPosition;

        transform.GetComponent<SpriteRenderer>().sortingOrder -= 2;
        bottleMaskSR.sortingOrder -= 2;

        Deselect();

        if (gameController != null)
        {
            gameController.CheckWinCondition();
        }

    }

    void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }

    IEnumerator RotateBottle()
    {
        float t = 0; // t — текущее время анимации
        float lerpValue; // значение интерполяции (0–1)
        float angleValue; // текущий угол поворота бутылки

        float lastAngleValue = 0;

        while (t < timeToRotate) // пока не прошло заданное время поворота
        {
            lerpValue = t / timeToRotate; // нормализуем время в диапазон 0–1
            angleValue = Mathf.Lerp(0.0f, directionMultiplier * rotationValues[rotationIndex], lerpValue); // плавно интерполируем угол

            // transform.eulerAngles = new Vector3(0, 0, angleValue); // применяем поворот к объекту бутылки

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue)); // передаём в шейдер множитель масштаба/поворота жидкости

            if (fillAmounts[numberOfColorsInBottle] > FillAmountCurve.Evaluate(angleValue) + 0.005f) // проверяем, не превышает ли рассчитанный уровень максимально допустимый
            {
                if (lineRenderer.enabled == false)
                {
                    lineRenderer.startColor = topColor;
                    lineRenderer.endColor = topColor;

                    lineRenderer.SetPosition(0, chosenRotationPoint.position);
                    lineRenderer.SetPosition(1, chosenRotationPoint.position - Vector3.up * 1.45f);

                    lineRenderer.enabled = true;
                }

                bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue)); // обновляем fill amount в шейдере, если он ещё не достиг лимита

                bottleControllerRef.FillUp(FillAmountCurve.Evaluate(lastAngleValue) - FillAmountCurve.Evaluate(angleValue)); // красиво вливаем в реципиента
            }

            t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(angleValue); // увеличиваем время с учётом кривой скорости вращения
            lastAngleValue = angleValue;
            yield return new WaitForEndOfFrame(); // ждём следующий кадр
        }
        angleValue = directionMultiplier * rotationValues[rotationIndex]; // принудительно устанавливаем финальный угол поворота, чтобы избежать накопления ошибки интерполяции
        // transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        numberOfColorsInBottle -= numberOfColorsToTransfer; // уменьшаем количество слоёв в бутылке на число слоёв верхнего цвета, которые были перелиты
        bottleControllerRef.numberOfColorsInBottle += numberOfColorsToTransfer;

        lineRenderer.enabled = false;

        StartCoroutine(RotateBottleBack()); // после поворота вперёд запускаем обратный поворот

    }

    IEnumerator RotateBottleBack()
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        float lastAngleValue = directionMultiplier * rotationValues[rotationIndex];

        while (t < timeToRotateBack)
        {
            lerpValue = t / timeToRotateBack;
            angleValue = Mathf.Lerp(directionMultiplier * rotationValues[rotationIndex], 0.0f, lerpValue); // плавно возвращаем бутылку в исходное положение

            // transform.eulerAngles = new Vector3(0, 0, angleValue);

            transform.RotateAround(chosenRotationPoint.position, Vector3.forward, lastAngleValue - angleValue);

            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            lastAngleValue = angleValue;

            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        UpdateTopColorValues(); // пересчитываем верхний цвет и количество одинаковых верхних слоёв после завершения переливания
        bottleControllerRef.UpdateTopColorValues();

        angleValue = 0; // гарантированно возвращаем бутылку в исходное положение
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

        StartCoroutine(MoveBottleBack());

    }

    public void UpdateTopColorValues() // считает одинаковые слои с верху бутылки
    {
        if (numberOfColorsInBottle != 0)
        {
            numberOfTopColorLayers = 1;

            topColor = bottleColors[numberOfColorsInBottle - 1];

            if (numberOfColorsInBottle == 4)
            {
                if (bottleColors[3].Equals(bottleColors[2]))
                {
                    numberOfTopColorLayers = 2;
                    if (bottleColors[2].Equals(bottleColors[1]))
                    {
                        numberOfTopColorLayers = 3;
                        if (bottleColors[1].Equals(bottleColors[0]))
                        {
                            numberOfTopColorLayers = 4;
                        }
                    }
                }
            }

            else if (numberOfColorsInBottle == 3)
            {
                if (bottleColors[2].Equals(bottleColors[1]))
                {
                    numberOfTopColorLayers = 2;
                    if (bottleColors[1].Equals(bottleColors[0]))
                    {
                        numberOfTopColorLayers = 3;
                    }
                }
            }

            else if (numberOfColorsInBottle == 2)
            {
                if (bottleColors[1].Equals(bottleColors[0]))
                {
                    numberOfTopColorLayers = 2;
                }
            }

            rotationIndex = 3 - (numberOfColorsInBottle - numberOfTopColorLayers); // вычисляем индекс угла поворота от того, сколько слоёв осталось и сколько одинаковых сверху

            UpdateCorkVisibility();
        }
    }

    public bool FillBottleCheck(Color colorToCheck) // можно налить?
    {
        if (numberOfColorsInBottle == 0)
        {
            return true; // на здоровье
        }
        else
        {
            if (numberOfColorsInBottle == 4)
            {
                return false; // некуда лить
            }
            else
            {
                if (topColor.Equals(colorToCheck))
                {
                    return true; // верхний цвет совпадает с цветом перелива - можно
                }
                else
                {
                    return false; // не совпадает - нельзя
                }
            }
        }
    }

    private void CalculateRotationIndex(int numberOfEmptySpacesInSecondBottle)
    {
        rotationIndex = 3 - ( // 3 - это максимальный индекс массива поворота
            numberOfColorsInBottle - // сколько слоёв там, откуда льём
            Mathf.Min(numberOfEmptySpacesInSecondBottle, numberOfTopColorLayers) // минимум из пустых мест реципиента и одинаковых верхних слоёв донора
            ); // пусть в доноре 4 цвета, из них 2 верхних одинаковы. В реципиенте 1 место. Итого 3 - (4 - 1) = 0, это первый индекс - переливаем только 1 слой
    }

    private void FillUp(float fillAmountToAdd)
    {
        bottleMaskSR.material.SetFloat(
            "_FillAmount",
            bottleMaskSR.material.GetFloat("_FillAmount") // берём текущий уровень воды из материала шейдера
            + fillAmountToAdd // прибавляем к текущему значению, сколько надо долить
            ); // и устанавливаем обратно в уже долитом виде
    }

    private void ChoseRotationPointAndDirection()
    {
        if (transform.position.x > bottleControllerRef.transform.position.x)
        {
            chosenRotationPoint = leftRotationPoint;
            directionMultiplier = -1.0f;
        }
        else
        {
            chosenRotationPoint = rightRotationPoint;
            directionMultiplier = 1.0f;
        }
    }

    public bool IsBottleComplete()
    {

        if (numberOfColorsInBottle == 0) // Пустая бутылка считается завершённой
        {
            return true;
        }

        if (numberOfColorsInBottle == 4)
        {
            if (bottleColors[0].Equals(bottleColors[1]) &&
                bottleColors[1].Equals(bottleColors[2]) &&
                bottleColors[2].Equals(bottleColors[3]))
            {
                return true;
            }
        }

        return false; // Во всех остальных случаях бутылка не завершена
    }

    public void UpdateCorkVisibility()
    {
        if (corkInstance == null)
        {
            return;
        }

        bool shouldShowCork = numberOfColorsInBottle == 4 && IsBottleComplete();

        if (shouldShowCork && !corkInstance.activeSelf)
        {
            corkInstance.SetActive(true);

            if (corkParticles != null)
            {
                corkParticles.Play();
            }
        }
    }

}
