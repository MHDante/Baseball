using UnityEngine;

public class Bat : MonoBehaviour
{
    public Transform target;
    public Transform centerOfMass;
    private new Rigidbody rigidbody;
    private Vector3 LastPos;
    private Quaternion LastRot;
    private PhysicsTracker tracker;
    void Start()
    {
        tracker = new PhysicsTracker();
        tracker.Reset(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
        rigidbody = GetComponent<Rigidbody>();
        Application.targetFrameRate = 90;
        rigidbody.maxAngularVelocity = float.PositiveInfinity;
        rigidbody.centerOfMass = transform.InverseTransformPoint(centerOfMass.position);
    }

    private void FixedUpdate()
    {
        tracker.Update(transform.position, transform.rotation, Time.fixedDeltaTime);
        rigidbody.angularVelocity = tracker.AngularVelocity;
        rigidbody.velocity = tracker.AngularVelocity;

    }

    private void Update()
    {


        Time.fixedDeltaTime = 1f / (Application.targetFrameRate / Time.timeScale);
    }
}
