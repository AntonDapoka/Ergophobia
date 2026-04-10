using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameTextShakingScript : MonoBehaviour
{
    [Header("Angle Range (degrees)")]
    public Vector2 angleRange = new Vector2(5f, 20f);

    [Header("Speed Range")]
    public Vector2 speedRange = new Vector2(0.5f, 2f);

    [Header("Swing Curve")]
    public AnimationCurve swingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float angle;
    private float speed;
    private float timeOffset;

    [SerializeField] private Camera mainCamera;

    private void Start()
    {
        //mainCamera = Camera.main;

        angle = Random.Range(angleRange.x, angleRange.y);
        speed = Random.Range(speedRange.x, speedRange.y);
        timeOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        if (mainCamera == null) return;

        // Вычисляем значение кривой для покачивания
        float t = Mathf.Repeat(Time.time * speed + timeOffset, 1f);
        float curveValue = swingCurve.Evaluate(t);
        float swing = Mathf.Lerp(-1f, 1f, curveValue);
        float z = swing * angle;

        // Повернуть объект лицом к камере (billboard)
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);

        // Добавить покачивание (локально)
        transform.rotation *= Quaternion.Euler(0, 0, z);
    }
}
