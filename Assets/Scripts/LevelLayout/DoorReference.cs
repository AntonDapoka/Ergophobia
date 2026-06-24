using UnityEngine;

public class DoorReference : MonoBehaviour
{
    [SerializeField] private Transform positionPlayerEntry;
    [SerializeField] private DoorScript doorConnected;
    [SerializeField] private GameObject hint;
    private DoorHint hintComponent;

    public Transform GetPositionPlayerEntry()
    {
        return positionPlayerEntry;
    }

    public void SetConnectedDoor(DoorScript door)
    {
        doorConnected = door;
    }

    public DoorScript GetConnectedDoor()
    {
        if (doorConnected != null) 
            return doorConnected;
        else
        {
            Debug.LogWarning("Missing Door");
            return null;
        }
    }

    public void InitializeHint(Transform cameraTransform)
    {
        if (hint == null) return;

        if (hintComponent == null)
            hintComponent = hint.GetComponent<DoorHint>();

        if (hintComponent == null)
            hintComponent = hint.AddComponent<DoorHint>();

        hintComponent.SetCamera(cameraTransform);
        hint.SetActive(false);
    }

    public void SetHintActive(bool active)
    {
        if (hint != null)
            hint.SetActive(active);
    }
}
