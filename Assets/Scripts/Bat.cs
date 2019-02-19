using UnityEngine;

public class Bat : MonoBehaviour
{
    public Transform centerOfMass;


    private new Rigidbody rigidbody;
    private Vector3 LastPos;
    private Quaternion LastRot;
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

        var pos =  transform.position;
        var rot = transform.rotation;

        rigidbody.position = LastPos;
        rigidbody.rotation = LastRot;

        var posDiff = (pos - LastPos);
        rigidbody.velocity =  posDiff * speed;
        
        var rotDiff = rot * Quaternion.Inverse(LastRot);
        rotDiff.ToAngleAxis(out var angle, out var axis);
        if (angle < -180) angle += 360;
        if (float.IsInfinity(axis.x)) return;
        rigidbody.angularVelocity = (Mathf.Deg2Rad * angle * speed) * axis.normalized;

        LastPos = pos;
        LastRot = rot;


    }

    private void Update()
    {
        Time.fixedDeltaTime = 1f / (Application.targetFrameRate / Time.timeScale);
    }
    
}
