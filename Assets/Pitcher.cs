using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{
    
    public Transform HandTransform;
    public Transform ThrowTarget;
    public BallPool ballPool;
    public float throwSpeedMps = 32;
    public Transform ThrowOrigin;
    
    
    private CharacterController _cc;
    private Animator _animator;
    private Ball ball = null;


    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cc.SimpleMove(Vector3.forward*.01f / Time.deltaTime);
        GrabBall();
    }

    // Update is called once per frame
    void Update()
    {

        UpdateBall();

        if (Input.GetKeyDown(KeyCode.JoystickButton14) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton15))
        {
            if (ball) SetState(PitcherStates.Pitch);
        }
    }

    private void OnDrawGizmosSelected()    {
        var g = Physics.gravity.magnitude;
        var destination = ThrowTarget.position;
        var origin = ThrowOrigin.transform.position;

        var xVector = destination - origin;
        var x = xVector.magnitude;
        var v = throwSpeedMps;
        var y = destination.y - origin.y;
        var det = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);
        if (det < 0) Debug.LogWarning("Pitcher will not be able to reach target with that speed.");

    }
    

    // Called By Animation Event
    void OnThrow()
    {
        if (ball != null)
        {
            var origin = ball.transform.position;
            var destination = ThrowTarget.transform.position;
            var y = destination.y - origin.y;
            origin.y = destination.y;
            var xVector = destination - origin;
            var x = xVector.magnitude;
            var g = Physics.gravity.magnitude;
            var v = throwSpeedMps;
            var det = Mathf.Pow(v, 4) - g * (g * x * x + 2 * y * v * v);

            if (det < 0)
            {
                Debug.LogError("Pitcher cannot reach the target!");
                return;
            }

            var rt = Mathf.Sqrt(det);
            var theta1 = Mathf.Atan((v * v - rt) / (g * x));
            var theta2 = Mathf.Atan((v * v - rt) / (g * x));
           

            var rot = Quaternion.AngleAxis(theta2*Mathf.Rad2Deg, Vector3.Cross(Physics.gravity, xVector).normalized);
            var dir = rot * xVector.normalized;
            Debug.DrawRay(ball.transform.position, dir, Color.red, 4);
            ball.rigidBody.isKinematic = false;
            ball.rigidBody.velocity = dir * v;
            ball.rigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            ball.rigidBody.WakeUp();
            ball = null;

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
            ball.rigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            ball.rigidBody.isKinematic = true;
            UpdateBall();
        }
    }
    void SetState(PitcherStates state)
    {
        _animator.SetInteger(AnimVar_State, (int)state);
        _animator.SetTrigger(AnimVar_Animate);
    }


    // Todo: Keep In Sync with Animator.
    private static readonly int AnimVar_Animate = Animator.StringToHash("Animate");
    private static readonly int AnimVar_State = Animator.StringToHash("State");

    public enum PitcherStates
    {
        Idle=0,
        Pitch=1
    }

}
