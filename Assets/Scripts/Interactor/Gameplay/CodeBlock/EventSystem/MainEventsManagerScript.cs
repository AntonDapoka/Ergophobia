using UnityEngine;

public class MainEventsManagerScript : MonoBehaviour
{
    private static EventsManagerScript _instance;
    public static EventsManagerScript Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EventsManagerScript();
            return _instance;
        }
    }

    private void Initialize()
    {
        if (_instance == null)
            _instance = new EventsManagerScript();
    }

    private void Awake()
    {
        Initialize();
    }
}