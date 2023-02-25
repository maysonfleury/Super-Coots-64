using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDistraction : MonoBehaviour
{
    [SerializeField] private CootsAI _cootsAI;

    // Start is called before the first frame update
    void Start()
    {
        _cootsAI = gameObject.GetComponentInParent<CootsAI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var toy = other.GetComponent<Pickup>();
        if (!toy.GetAttachState())
        {
            _cootsAI.SetTarget(transform);
            _cootsAI.DistractCoots();
        }
    }
}
