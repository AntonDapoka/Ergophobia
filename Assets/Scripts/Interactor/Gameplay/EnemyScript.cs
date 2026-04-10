using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //[SerializeField] private int damage = 10; // Урон, который наносит враг при столкновении

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, есть ли на столкнувшемся объекте скрипт PlayerMarker
        PlayerMarker playerMarker = other.GetComponent<PlayerMarker>();
        if (playerMarker != null)
        {/*
            // Пытаемся найти компонент здоровья у игрока
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }*/

            // Уничтожаем врага
            Destroy(gameObject);
        }
    }
}