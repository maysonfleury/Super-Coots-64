using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private Transform vfxHit;
    private Rigidbody bulletRigidbody;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();    
    }

    void Start()
    {
        float speed = 55f;
        bulletRigidbody.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<BulletTarget>() != null)
        {
            // Hit Target
            //Instantiate(vfxHit, transform.position, Quaternion.identity);
            Debug.Log("Bullet Hit");
        } else {
            // Hit something else
        }
        Destroy(gameObject);
    }
}
