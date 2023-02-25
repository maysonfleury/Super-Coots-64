using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerCombat : MonoBehaviour
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
            _cootsAI.SetTarget(other.transform);
            _cootsAI.StartCombat();
            gameObject.SetActive(false);
        }
    }
}
