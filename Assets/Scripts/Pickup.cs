using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    //[SerializeField] private string _pickupName;
    [SerializeField] private float torque = 10f;
    [SerializeField] private float speed = 25f;
    [SerializeField] private GameObject _collider;
    private Rigidbody _rigidbody;
    private bool isAttached;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();  
        _rigidbody.isKinematic = true;
        _collider.SetActive(false);
        isAttached = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player walked into Pickup
            _rigidbody.isKinematic = true;
            _collider.SetActive(false);
            isAttached = true;
        }

        if (other.CompareTag("Coots"))
        {
            // Coots Hit Pickup
            // TODO: play pickup death sound
        }

        if (other.GetComponent<BulletTarget>() != null)
        {
            // Hit target
            Debug.Log("Hitit");
        }
    }

    public void Launch(Quaternion launchDir)
    {
        transform.rotation = launchDir;
        _rigidbody.isKinematic = false;
        isAttached = false;
        _collider.SetActive(true);
        _rigidbody.velocity = transform.forward * speed;
        _rigidbody.AddTorque(transform.right * torque);
        StartCoroutine(LaunchRoutine());
    }

    public bool GetAttachState()
    {
        return isAttached;
    }

    private IEnumerator LaunchRoutine()
    {
        yield return new WaitForSeconds(0.15f);
        Vector3 targetScale = transform.localScale * 6.0f;
        float elapsedTime = 0f;
        float growTime = 1f;
        while (elapsedTime < growTime)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, (elapsedTime / growTime));
            elapsedTime += Time.deltaTime;
        }
        yield return null;
    }
}
