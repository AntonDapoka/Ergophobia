using UnityEngine;

public class TreasureScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SphereCollider colliderInteraction;
    [SerializeField] private GameObject hint;

    public void OnPlayerEntered()
    {
        hint.SetActive(true);
    }

    public void OnPlayerExited()
    {
        hint.SetActive(false);
    }
}
