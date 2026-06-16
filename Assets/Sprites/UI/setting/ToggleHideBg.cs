using UnityEngine;
using UnityEngine.UI;

public class ToggleHideBg : MonoBehaviour
{
    public Image bg;
    Toggle tg;

    void Start()
    {
        tg = GetComponent<Toggle>();
        tg.onValueChanged.AddListener(SetBgAlpha);
        SetBgAlpha(tg.isOn);
    }

    void SetBgAlpha(bool isOn)
    {
        bg.color = new Color(1, 1, 1, isOn ? 0 : 1);
    }
}