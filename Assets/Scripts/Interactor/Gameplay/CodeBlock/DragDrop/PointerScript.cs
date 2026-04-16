using UnityEngine;

public class PointerScript : MonoBehaviour
{
    private Transform _transform;
    private Vector3 _mousePos;

    static PointerScript _instance;
    public static PointerScript Instance
    {
        get
        {
            if (!_instance)
                _instance = FindAnyObjectByType<PointerScript>();
            return _instance;
        }
        set => _instance = value;
    }

    private void Awake()
    {
        _transform = transform;
    }

    public void OnUpdate()
    {
        UpdatePointerPosition();
    }

    public void UpdatePointerPosition()
    {
        _mousePos = CodeBlockInputManagerScript.Instance.CanvasPointerPosition;

        _transform.position = new Vector3(_mousePos.x, _mousePos.y, _transform.position.z);
        _transform.localPosition = new Vector3(_transform.localPosition.x, _transform.localPosition.y, 0);
        _transform.localEulerAngles = Vector3.zero;
    }
}