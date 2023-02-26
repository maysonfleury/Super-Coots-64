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
        if (other.GetComponent<Pickup>() != null)
        {
            var toy = other.GetComponent<Pickup>();
            // Only target toy if player has thrown it
            if (!toy.GetAttachState() && toy.GetActivatedState())
            {
                _cootsAI.DistractCoots(toy.transform);
            }
        }
    }
}
