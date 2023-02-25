using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAttack : MonoBehaviour
{
    [SerializeField] private CootsAI _cootsAI;

    // Start is called before the first frame update
    void Start()
    {
        _cootsAI = gameObject.GetComponentInParent<CootsAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _cootsAI.Attack();
        }

        if (other.CompareTag("Tower"))
        {
            _cootsAI.SetTarget(other.transform);
            _cootsAI.Attack();
        }

        if (other.GetComponent<Pickup>() != null)
        {
            var toy = other.GetComponent<Pickup>();
            if (!toy.GetAttachState())
            {
                _cootsAI.Attack();
            }
        }
    }
}
