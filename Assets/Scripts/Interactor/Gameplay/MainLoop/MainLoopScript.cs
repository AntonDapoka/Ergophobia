using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainLoopScript : MonoBehaviour
{
    public Slider slider;
    public float fillDuration = 2f; // Время полного заполнения в секундах

    void Start()
    {
        if (slider != null)
            StartCoroutine(FillAndResetSlider());
    }

    IEnumerator FillAndResetSlider()
    {
        while (true)
        {
            float elapsed = 0f;

            while (elapsed < fillDuration)
            {
                elapsed += Time.deltaTime;
                slider.value = Mathf.Clamp01(elapsed / fillDuration);
                yield return null;
            }

            slider.value = 0f;
            yield return null; // можно добавить паузу перед следующим циклом
        }
    }
}