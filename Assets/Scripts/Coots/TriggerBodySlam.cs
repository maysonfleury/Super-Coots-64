using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBodySlam : MonoBehaviour
{
    [SerializeField] private CootsAI _cootsAI;

    // Start is called before the first frame update
    void Start()
    {
        _cootsAI = gameObject.GetComponentInParent<CootsAI>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _cootsAI.BodySlam();
        }

        if (other.GetComponent<Pickup>() != null)
        {
            var toy = other.GetComponent<Pickup>();
            if (!toy.GetAttachState())
            {
                _cootsAI.BodySlam();
            }
        }
    }
}
