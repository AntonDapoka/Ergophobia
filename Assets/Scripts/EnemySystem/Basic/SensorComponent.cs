using System;
using UnityEngine;

public class SensorComponent : MonoBehaviour
{
    [SerializeField] private LayerMask playerLayer;
    public event Action<Transform> OnPlayerSpotted;
    public event Action OnPlayerLost;

    private Transform currentTarget;
    private float detectionRadius;

    public void Initialize(float radius)
    {
        detectionRadius = radius;
    }

    private void Update()
    {
        //looking for player in the detection radius
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        if (hits.Length > 0)
        {
            if (currentTarget == null)
            {
                currentTarget = hits[0].transform;
                OnPlayerSpotted?.Invoke(currentTarget);
            }
        }
        else
        {
            if (currentTarget != null)
            {
                currentTarget = null;
                OnPlayerLost?.Invoke();
            }
        }
    }

    //Debugging: visualize the detection radius in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}