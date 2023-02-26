using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEgg : MonoBehaviour
{
    [SerializeField] private Transform FinishPos;
    private bool inPosition;

    // Start is called before the first frame update
    void Start()
    {
        inPosition = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!inPosition)
        {
            UpYouGo();
        }
    }

    public void SummonSlime()
    {
        inPosition = false;
    }

    private void UpYouGo()
    {
        transform.position = transform.position + new Vector3(0, 10f * Time.deltaTime, 0);
        if (transform.position.y >= FinishPos.position.y)
        {
            inPosition = true;
        }
    }
}
