using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour
{
    public Transform target;
    public Transform centerOfMass;
    private new Rigidbody rigidbody;


    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        Application.targetFrameRate = 90;
        rigidbody.maxAngularVelocity = float.PositiveInfinity;
        rigidbody.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
    }

    private void FixedUpdate()
    {
        var speed = 1 / Time.fixedDeltaTime;
        rigidbody.velocity = (target.transform.position - rigidbody.position) * speed;

        var delta = target.transform.rotation * Quaternion.Inverse(rigidbody.rotation);

        delta.ToAngleAxis(out var angle, out var axis);

        if (angle < -180) angle += 360;
        if (float.IsInfinity(axis.x)) return;

        rigidbody.angularVelocity = (Mathf.Deg2Rad * angle * speed) * axis.normalized;


    }

    private void Update()
    {   
        Time.fixedDeltaTime = 1f / (Application.targetFrameRate / Time.timeScale);
    }
}
