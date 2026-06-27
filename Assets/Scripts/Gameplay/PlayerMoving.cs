using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMoving : MonoBehaviour
{
    [SerializeField] private bool isAbleToMove = true;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    public float WalkSpeed { get => walkSpeed; set => walkSpeed = value; }
    public float RunSpeed { get => runSpeed; set => runSpeed = value; }
    private Vector3 inputDir;
    private bool isRunning;
    private Rigidbody rb;

    [Header("References")]
    [SerializeField] private Animator anim;
    private PlayerAudio playerAudio; 

    [Header("Footstep Settings")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    private float footstepTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    public void SetIsAbleToMove(bool isAbleToMoveNew)
    {
        isAbleToMove = isAbleToMoveNew;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        inputDir = new Vector3(-h, 0, -v).normalized;
        isRunning = Input.GetKey(KeyCode.LeftShift);

        UpdateAnimation();
        HandleFootsteps();
    }

    private void UpdateAnimation()
    {
        if (!isAbleToMove) return;

        if (inputDir.magnitude > 0.1f)
        {
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

    private void HandleFootsteps()
    {
        if (!isAbleToMove) return;

        if (inputDir.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (playerAudio != null) playerAudio.PlayFootstep();

                footstepTimer = isRunning ? runStepInterval : walkStepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!isAbleToMove) return;
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = inputDir * speed;
    }
}