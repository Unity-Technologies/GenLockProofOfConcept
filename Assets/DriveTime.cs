using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveTime : MonoBehaviour
{
    public float Speed = 1;
    public float Min, Max;

    void Update()
    {
        var transform = GetComponent<RectTransform>();
        transform.position = new Vector3(Min + Mathf.PingPong(Time.time * Speed, Max-Min), transform.position.y, transform.position.z);
    }
}
