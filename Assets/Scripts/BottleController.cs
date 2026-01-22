using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class BottleController : MonoBehaviour
{
    // массив цветов жидкости (4 слоя)
    public Color[] bottleColors;

    // SpriteRenderer, на котором висит материал с шейдером жидкости
    public SpriteRenderer bottleMaskSR;

    // кривая, управляющая масштабом и поворотом жидкости при наклоне бутылки
    public AnimationCurve ScaleAndRotationMultiplierCurve;

    // кривая, управляющая уровнем заполнения жидкости
    public AnimationCurve FillAmountCurve;

    // кривая, которая меняет скорость поворота бутылки
    public AnimationCurve RotationSpeedMultiplier;

    void Start()
    {
        UpdateColorsOnShader();
    }

    void Update() // для проверки поворота бутылочки клавишей Р
    {
        if (Keyboard.current.pKey.wasReleasedThisFrame)
        {
            StartCoroutine(RotateBottle());
        }
    }

    void UpdateColorsOnShader()
    {
        bottleMaskSR.material.SetColor("_C1", bottleColors[0]);
        bottleMaskSR.material.SetColor("_C2", bottleColors[1]);
        bottleMaskSR.material.SetColor("_C3", bottleColors[2]);
        bottleMaskSR.material.SetColor("_C4", bottleColors[3]);
    }

    // общее время поворота бутылки в одну сторону
    public float timeToRotate = 1.0f;

    IEnumerator RotateBottle()
    {
        float t = 0; // t — текущее время анимации
        float lerpValue; // значение интерполяции (0–1)
        float angleValue; // текущий угол поворота бутылки

        while (t < timeToRotate) // пока не прошло заданное время поворота
        {
            lerpValue = t / timeToRotate; // нормализуем время в диапазон 0–1
            angleValue = Mathf.Lerp(0.0f, 90.0f, lerpValue); // плавно интерполируем угол от 0 до 90 градусов

            transform.eulerAngles = new Vector3(0, 0, angleValue); // применяем поворот к объекту бутылки
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue)); // передаём в шейдер множитель масштаба/поворота жидкости
            bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue)); // передаём в шейдер уровень заполнения жидкости

            t += Time.deltaTime * RotationSpeedMultiplier.Evaluate(angleValue); // увеличиваем время с учётом кривой скорости вращения

            yield return new WaitForEndOfFrame(); // ждём следующий кадр
        }
        angleValue = 90.0f; // гарантированно выставляем конечное положение (90 градусов)
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));
        bottleMaskSR.material.SetFloat("_FillAmount", FillAmountCurve.Evaluate(angleValue));

        StartCoroutine(RotateBottleBack()); // после поворота вперёд запускаем обратный поворот

    }

    IEnumerator RotateBottleBack()
    {
        float t = 0;
        float lerpValue;
        float angleValue;

        while (t < timeToRotate)
        {
            lerpValue = t / timeToRotate;
            angleValue = Mathf.Lerp(90.0f, 0.0f, lerpValue);

            transform.eulerAngles = new Vector3(0, 0, angleValue);
            bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));

            t += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        // гарантированно возвращаем бутылку в исходное положение
        angleValue = 0;
        transform.eulerAngles = new Vector3(0, 0, angleValue);
        bottleMaskSR.material.SetFloat("_SARM", ScaleAndRotationMultiplierCurve.Evaluate(angleValue));


    }
}
