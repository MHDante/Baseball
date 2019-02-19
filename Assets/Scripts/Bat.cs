using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

/// <summary>
/// Simple script for Synchronizing Rigidbody and TrackedPoseDriver 
/// </summary>
[RequireComponent(typeof(TrackedPoseDriver))]
public class Bat : MonoBehaviour
{
    [FormerlySerializedAs("centerOfMass")]
    public Transform CenterOfMass;
    public TrackedPoseDriver Glove;
    public bool IsRightHanded { get; private set; }

    public KeyCode TriggerKeyCode => IsRightHanded ? KeyCode.JoystickButton15 : KeyCode.JoystickButton14;
    public KeyCode ButtonKeyCode => IsRightHanded ? KeyCode.JoystickButton0: KeyCode.JoystickButton2;

    private TrackedPoseDriver _driver;
    private Rigidbody _rigidbody;
    private Vector3 _lastPos;
    private Quaternion _lastRot = Quaternion.identity;

    void Start()
    {
        _driver = GetComponent<TrackedPoseDriver>();
        _rigidbody = GetComponent<Rigidbody>();
        Application.targetFrameRate = 90;
        _rigidbody.maxAngularVelocity = float.PositiveInfinity;
        _rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton14)) SetRightHanded(false);
        if (Input.GetKeyDown(KeyCode.JoystickButton15)) SetRightHanded(true);
    }

    public void SetRightHanded(bool isRightHanded)
    {
        if(IsRightHanded == isRightHanded) return;
        _driver.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            isRightHanded ? TrackedPoseDriver.TrackedPose.RightPose : TrackedPoseDriver.TrackedPose.LeftPose);

        Glove.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRController,
            isRightHanded ? TrackedPoseDriver.TrackedPose.LeftPose : TrackedPoseDriver.TrackedPose.RightPose);

        var scale = Glove.transform.localScale;
        scale.x = isRightHanded ? 1 : -1;
        Glove.transform.localScale = scale;
        IsRightHanded = isRightHanded;
    }

    private void FixedUpdate()
    {
        var speed = 1 / Time.fixedDeltaTime;

        var pos = transform.position;
        var rot = transform.rotation;

        _rigidbody.position = _lastPos;
        _rigidbody.rotation = _lastRot;

        var posDiff = (pos - _lastPos);
        _rigidbody.velocity = posDiff * speed;

        var rotDiff = rot * Quaternion.Inverse(_lastRot);
        rotDiff.ToAngleAxis(out var angle, out var axis);
        if (angle < -180) angle += 360;
        if (float.IsInfinity(axis.x)) return;
        _rigidbody.angularVelocity = (Mathf.Deg2Rad * angle * speed) * axis.normalized;

        _lastPos = pos;
        _lastRot = rot;
    }

    public void Haptics(Vector3 velocity)
    {
        var dur = (Vector3.Dot(Vector3.forward, velocity) +1) / 2;
        var modifier = Mathf.Clamp01(velocity.magnitude / 30) * dur;
        var force = .5f * (1 + modifier);
        InputDevices.GetDeviceAtXRNode(IsRightHanded ? XRNode.RightHand : XRNode.LeftHand)
            .SendHapticImpulse(0, force);
    }
}
