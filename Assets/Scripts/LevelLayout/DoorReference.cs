using UnityEngine;

public class DoorReference : MonoBehaviour
{
    [SerializeField] private Transform positionPlayerEntry;
    [SerializeField] private DoorScript doorConnected;

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
}
