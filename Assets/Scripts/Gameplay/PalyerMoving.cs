using UnityEngine;

public class PlayerMoving : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;       
    [SerializeField] private float rotationSpeed = 15f;

    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform aimTarget;

    private PlayerAiming aimingScript; 

    void Start()
    {
        aimingScript = GetComponent<PlayerAiming>();
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    void HandleRotation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 lookPos = hit.point;
            lookPos.y = transform.position.y;
            Vector3 direction = lookPos - transform.position;

            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
            }

            if (aimTarget) aimTarget.position = hit.point + Vector3.up * 1.5f;
        }
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && (aimingScript == null || !aimingScript.isAiming);

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        if (inputDir.magnitude > 0.1f)
        {
           
            transform.Translate(inputDir * currentSpeed * Time.deltaTime, Space.World);

       
            float angle = Vector3.SignedAngle(transform.forward, inputDir, Vector3.up);

            float animMagnitude = isRunning ? 2f : 1f;

            float animX = Mathf.Sin(angle * Mathf.Deg2Rad) * animMagnitude;
            float animY = Mathf.Cos(angle * Mathf.Deg2Rad) * animMagnitude;

            anim.SetFloat("Horizontal", animX, 0.1f, Time.deltaTime);
            anim.SetFloat("Vertical", animY, 0.1f, Time.deltaTime);
        }
        else
        {
            anim.SetFloat("Horizontal", 0, 0.1f, Time.deltaTime);
            anim.SetFloat("Vertical", 0, 0.1f, Time.deltaTime);
        }
    }
}