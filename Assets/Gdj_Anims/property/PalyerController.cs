using UnityEngine;

public class PalyerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator anim;
    public Transform aimTarget; 

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
                
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
            }

            
            if (aimTarget) aimTarget.position = hit.point + Vector3.up * 1.5f;
        }
    }

    void HandleMovement()
    {
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.magnitude > 0.1f)
        {
            
            transform.Translate(inputDir * moveSpeed * Time.deltaTime, Space.World);

            
            float angle = Vector3.SignedAngle(transform.forward, inputDir, Vector3.up);

            
            float animX = Mathf.Sin(angle * Mathf.Deg2Rad);
            float animY = Mathf.Cos(angle * Mathf.Deg2Rad);

            
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
