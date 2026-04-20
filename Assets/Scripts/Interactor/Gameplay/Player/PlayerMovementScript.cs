using System.Collections;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trail;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingDuration = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private KeyCode dashingKeyCode = KeyCode.LeftShift;
    [SerializeField] private AnimationCurve dashingCurve;

    private Vector3 moveDirection;

    private bool canDash = true;
    private bool isDashing;

    private void Awake()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (!trail)
        {
            trail = GetComponent<TrailRenderer>();
        }
    }

    private void Update()
    {
        HandleInput();

        if (!isDashing)
        {
            Move();
        }
    }

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection != Vector3.zero)
        {
            Quaternion rotation = Quaternion.Euler(0f, -90f, 0f);
            moveDirection = rotation * inputDirection;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        if (Input.GetKeyDown(dashingKeyCode) && canDash && moveDirection != Vector3.zero)
        {
            StartCoroutine(Dash());
        }
    }

    private void Move()
    {
        if (isDashing)
        {
            return;
        }
        rb.linearVelocity = new Vector3(moveDirection.x*moveSpeed,0, moveDirection.z * moveSpeed); 
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        trail.emitting = true;

        float elapsed = 0f;
        Vector3 dashDirection = moveDirection;

        while (elapsed < dashingDuration)
        {
            float t = elapsed / dashingDuration;
            float power = dashingCurve.Evaluate(t) * dashingPower;


            rb.linearVelocity = dashDirection * power;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        trail.emitting = false;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
