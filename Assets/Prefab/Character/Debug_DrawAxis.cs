using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_DrawAxis : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, transform.rotation * new Vector3(100, 0, 0), Color.red);
        Debug.DrawRay(transform.position, new Vector3(0, 100, 0), Color.yellow);
        Debug.DrawRay(transform.position, transform.rotation * new Vector3(0, 0, 100), Color.blue);
    }
}
