using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{

    [SerializeField] private TrailRenderer tr;
    [SerializeField] private Rigidbody rb;
    private Vector3 moveDirection;
    [SerializeField] private float moveSpeed = 0f;

    private bool canDash = true;
    private bool isDashing;
    
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float dashingCooldown = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInputs();
    }
    void FixedUpdate()
    {
        Move();
    }
    void ProcessInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
       
        moveDirection = new Vector3(moveX,0, moveZ).normalized;
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }
    void Move()
    {
        if (isDashing)
        {
            return;
        }
        rb.velocity = new Vector3(moveDirection.x*moveSpeed,0, moveDirection.z * moveSpeed); 
    }
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        rb.velocity = new Vector3(moveDirection.x* dashingPower,0f, moveDirection.z * dashingPower);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.velocity = new Vector3(0, 0, 0);
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
