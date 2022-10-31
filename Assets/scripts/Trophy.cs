using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Spin the object around the target at 20 degrees/second.
        //  transform.RotateAround(transform.position, Vector3.up, 20 * Time.deltaTime);
        transform.eulerAngles = new Vector3(-90, transform.eulerAngles.y + Time.deltaTime * 10, 0);
        transform.position = new Vector3(transform.position.x, 2+ Mathf.Sin(Time.time), transform.position.z);
    }
}
