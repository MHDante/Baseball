using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{

    public Transform HandTransform;
    public Transform ThrowTarget;
    public BallPool ballPool;
    public float flightTime = 1;
    public Transform ThrowOrigin;
    public float curveAngle;
    public float curveSpeedRPM = 1800;
    private const float Rpm2Rads = 2 * Mathf.PI/60;

    private CharacterController _cc;
    private Animator _animator;
    private Ball ball = null;


    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cc.SimpleMove(Vector3.forward * .01f / Time.deltaTime);
        GrabBall();
    }

    void Update()
    {

        UpdateBall();

        if (Input.GetKeyDown(KeyCode.JoystickButton14) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton15))
        {
            if (ball) SetState(PitcherStates.Pitch);
        }
    }

    // Called By Animation Event
    void OnThrow()
    {
        if (ball != null)
        {
            try
            {
                Vector3 dir;
                dir = CalculateDragVelocity(ball.transform.position, ThrowTarget.transform.position);
                Debug.Log(dir.magnitude);
                Debug.DrawRay(ball.transform.position, dir, Color.red, 4);
                ball.RigidBody.isKinematic = false;
                ball.RigidBody.velocity = dir;
                var vec = Quaternion.AngleAxis(curveAngle, Vector3.forward) * Vector3.up;
                ball.RigidBody.angularVelocity = vec * (Rpm2Rads * curveSpeedRPM);
                
                ball.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                ball.RigidBody.WakeUp();
                ball = null;
            }
            catch (ArithmeticException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    void UpdateBall()
    {
        if (ball != null)
        {
            var bt = ball.transform;
            bt.position = HandTransform.position;
            bt.rotation = HandTransform.rotation;
        }
    }

    // Called By Animation Event
    void GrabBall()
    {
        if (!ball)
        {
            ball = ballPool.GetBall();
            Physics.IgnoreCollision(_cc, ball.Collider);
            ball.RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            ball.RigidBody.isKinematic = true;
            UpdateBall();
        }
    }
    void SetState(PitcherStates state)
    {
        _animator.SetInteger(AnimVar_State, (int)state);
        _animator.SetTrigger(AnimVar_Animate);
    }

    private Vector3 CalculateDragVelocity(Vector3 origin, Vector3 destination)
    {
        var g = Physics.gravity.magnitude;
        var m = ball.RigidBody.mass;
        const float vt = Ball.terminalVelocity;
        var e = Math.E;
        var t = flightTime;
        var y = destination.y - origin.y;
        destination.y = origin.y;
        var xVec = (destination - origin);
        var x = xVec.magnitude;

        var exp = Math.Pow(e , g * t / vt);
        var vx = g * x * exp/(vt * (exp - 1));
        var vy = -((vt*vt) * exp - g * y * exp - g * vt * t * exp - (vt*vt))/(vt * (exp - 1));

        var dir = xVec.normalized * (float)vx;
        dir += Vector3.up * (float)vy;

        return dir;
    }

    private Vector3 CalculateDraglessTrajectoryVelocity(Vector3 origin, Vector3 destination, float throwSpeed, bool highball = false)
    {
        var g = Physics.gravity.magnitude;
        var v = throwSpeed;
        var y = destination.y - origin.y;
        origin.y = destination.y;
        var xVector = destination - origin;
        var x = xVector.magnitude;
        var det = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);
        if (det < 0) throw new ArithmeticException("Pitcher will not be able to reach target with that speed.");

        var rt = Mathf.Sqrt(det);
        rt = highball ? rt : -rt;
        var theta = Mathf.Atan((v * v + rt) / (g * x));
        var rotVector = Vector3.Cross(Physics.gravity, xVector).normalized;
        var rot = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, rotVector);
        var dir = rot * xVector.normalized * v;

        return dir;
    }

    // Todo: Keep In Sync with Animator.
    private static readonly int AnimVar_Animate = Animator.StringToHash("Animate");
    private static readonly int AnimVar_State = Animator.StringToHash("State");

    public enum PitcherStates
    {
        Idle = 0,
        Pitch = 1
    }

}
