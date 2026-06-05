using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenInventoryButton : MonoBehaviour
{
    [SerializeField] bool isClickable = true;
    [SerializeField] Button buttonOpen;
    [SerializeField] RectTransform imageInventory;
    [SerializeField] private float moveImageDuration = 2f;
    [SerializeField] private float offset = 500f;
    [Header("UI Curves")]
    [SerializeField] private AnimationCurve moveSettingsCurve;

    private void Start()
    {
        buttonOpen.onClick.AddListener(OnOpenClick);
        isClickable = true;
    }

    private void OnOpenClick()
    {
        if (isClickable)
        {
            buttonOpen.onClick.RemoveAllListeners();
            buttonOpen.onClick.AddListener(OnOpenBackClick);

            StartCoroutine(ChangeInventoryImage(imageInventory, true));

        }

    }

    private void OnOpenBackClick()
    {
        if (isClickable)
        {
            buttonOpen.onClick.RemoveAllListeners();
            buttonOpen.onClick.AddListener(OnOpenClick);

            StartCoroutine(ChangeInventoryImage(imageInventory, false));
        }

    }

    private IEnumerator ChangeInventoryImage(RectTransform image, bool isImageUp)
    {
        //wall.gameObject.SetActive(true);
        isClickable = false;

        if (image != null)
            StartCoroutine(MoveObjectAndUI(image.gameObject, offset * (isImageUp ? -1 : 1), moveImageDuration, true, true));

        float max = moveImageDuration;

        yield return new WaitForSeconds(max);
        //wall.gameObject.SetActive(false);

        isClickable = true;
    }

    private IEnumerator MoveObjectAndUI(GameObject obj, float bias, float duration, bool isChosen, bool isDown)
    {

        if (isChosen) obj.SetActive(true);

        float elapsedTime = 0f;

        int positionMultiplier = isDown ? 1 : -1;

        RectTransform rectTransform = obj.GetComponent<RectTransform>();

        Vector2 initialPos = rectTransform.anchoredPosition;

        while (elapsedTime < duration)
        {
            float curveProgress = moveSettingsCurve.Evaluate(elapsedTime / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(initialPos, initialPos - new Vector2(0, bias * positionMultiplier), curveProgress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = initialPos - new Vector2(0, bias) * positionMultiplier;

        if (!isChosen) obj.SetActive(false);
    }
}
