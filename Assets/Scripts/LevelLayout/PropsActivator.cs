using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropsActivator : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private Transform holderProps;
    [SerializeField] private int propsPerFrame = 3;

    private List<GameObject> props = new();

    public void Prepare()
    {
        props.Clear();

        foreach (Transform child in holderProps)
        {
            if (child == null) continue;
            props.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
    }

    public IEnumerator ActivateAsync()
    {
        for (int i = 0; i < props.Count; i++)
        {
            if (props[i] != null)
                props[i].SetActive(true);

            if (i % propsPerFrame == propsPerFrame - 1)
                yield return null;
        }
    }

    public void ForceActivate()
    {
        foreach (var prop in props)
        {
            if (prop != null)
                prop.SetActive(true);
        }
    }
}
