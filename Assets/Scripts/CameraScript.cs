using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform tf,cartf;
    public GameObject car;
    void Start()
    {
        tf = this.GetComponent<Transform>();
        cartf = car.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        tf.eulerAngles = new Vector3(0, cartf.eulerAngles.y, 0);
        tf.position = new Vector3(cartf.position.x, cartf.position.y+2.5f, cartf.position.z);
    }
}
