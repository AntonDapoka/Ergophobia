using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaivourScript : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeDuration = 1f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        dir.y = 0f;
        direction = dir.normalized;
        StartCoroutine(SelfDestruct());
    }

    private void Update()
    {
        // Двигае?пулю только ?плоскост?XZ
        Vector3 move = direction * speed * Time.deltaTime;
        transform.position += move;
        //transform.position = new Vector3(transform.position.x, 1f, transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Проверка наличия маркеров на столкнувшемся объект?
        if (collision.gameObject.GetComponent<WallMarker>() != null ||
            collision.gameObject.GetComponent<EnemyMarker>() != null ||
            collision.gameObject.GetComponent<PlayerMarker>() != null)
        {
            // Отражени?направления относительно нормал?
            Vector3 normal = collision.contacts[0].normal;
            normal.y = 0f; // исключае?вертикальный отскок
            direction = Vector3.Reflect(direction, normal).normalized;
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(lifeDuration);
        Destroy(gameObject);
    }
}