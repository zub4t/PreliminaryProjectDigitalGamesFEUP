using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUPandDown : MonoBehaviour
{
    public bool _rotateYawAxis;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!_rotateYawAxis) {
            transform.eulerAngles = new Vector3(-90, transform.eulerAngles.y + Time.deltaTime * 10, 0);
            transform.position = new Vector3(transform.position.x, 2 + Mathf.Sin(Time.time), transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x,  Mathf.Sin(Time.time) -0.5f, transform.position.z);

        }

    }
}
