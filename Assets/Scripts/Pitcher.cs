using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Pitcher : MonoBehaviour
{
    private const float RPM_2_RADS = 2 * Mathf.PI/60;

    public Transform HandTransform;
    public Transform ThrowTarget;
    public Pool BallPool;
    [NonSerialized] public Ball CurrentBall;

    private CharacterController _cc;
    private ParticleSystem _particles;
    private Animator _animator;
    private Pitch _currentPitch;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _particles = GetComponentInChildren<ParticleSystem>();
        _animator = GetComponent<Animator>();
        _cc.SimpleMove(Vector3.forward * .01f / Time.deltaTime);
    }

    void Update()
    {
        UpdateBall();
    }


    public void MakePitch(Pitch p)
    {
        if (!CurrentBall)
        {
            SetState(PitcherStates.Pitch);
            CurrentBall = BallPool.Get().GetComponent<Ball>();
            _currentPitch = p;
            Physics.IgnoreCollision(_cc, CurrentBall.Collider);
            CurrentBall.RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            CurrentBall.RigidBody.isKinematic = true;
            UpdateBall();
        }
    }
    
    // ReSharper disable once UnusedMember.Local - Called By Animation Event
    void GrabBall()
    {
        // Animation for pitch ended. - Unused.
    }

    // ReSharper disable once UnusedMember.Local - Called By Animation Event
    void OnThrow()
    {
        if (CurrentBall != null)
        {
            try
            {
                Vector3 dir;
                dir = CalculateDragVelocity(CurrentBall.transform.position, ThrowTarget.transform.position);
                CurrentBall.RigidBody.isKinematic = false;
                CurrentBall.RigidBody.velocity = dir;
                var vec = Quaternion.AngleAxis(_currentPitch.CurveAngle, Vector3.forward) * Vector3.up;
                CurrentBall.RigidBody.angularVelocity = vec * (RPM_2_RADS * _currentPitch.MaxCurveSpeedRpm);
                
                CurrentBall.RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                CurrentBall.RigidBody.WakeUp();
                CurrentBall.OnLaunch();
                CurrentBall = null;
            }
            catch (ArithmeticException e)
            {
                Debug.LogError(e.Message);
            }
        }
    }

    void UpdateBall()
    {
        if (CurrentBall != null)
        {
            var bt = CurrentBall.transform;
            bt.position = HandTransform.position;
            bt.rotation = HandTransform.rotation;
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
        const float vt = Ball.TERMINAL_VELOCITY;
        var e = Math.E;
        var t = _currentPitch.FlightTime;
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

    
    // ReSharper disable once UnusedMember.Local - Kept around for reference.
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

    
    public void TeleportTo(Transform target)
    {
        _particles.Emit(30);
        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;
        _particles.Emit(30);
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
