using System.Collections;
using UnityEngine;

public class PlayerBehaviourInteractorScript : BehaviourInteractorScript
{
    private GameObject _bullet;

    public new Transform Transform => transform;

    private void Awake() //Rewrite
    {
        foreach (Transform child in transform)
            if (child.name == "Bullet") _bullet = child.gameObject;
    }

    public void Shoot()
    {
        GameObject newBullet = Instantiate(_bullet, _bullet.transform.position, Quaternion.identity);
        newBullet.SetActive(true);
        newBullet.GetComponent<Rigidbody>().AddForce(transform.forward * 1000);
        StartCoroutine(C_DestroyTime(newBullet));
    }
    
    private IEnumerator C_DestroyTime(GameObject go)
    {
        yield return new WaitForSeconds(1f);
        Destroy(go);
    }
}
