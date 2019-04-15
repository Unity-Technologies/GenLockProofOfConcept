using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWithGameTime : MonoBehaviour
{
    public float Speed = 10;

    void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(-Time.time*Speed, 0, 0);
    }
}
