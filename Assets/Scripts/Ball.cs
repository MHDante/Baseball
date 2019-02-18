using UnityEngine;

public class Ball : MonoBehaviour
{
    public BallPool pool;
    public Rigidbody RigidBody;
    public SphereCollider Collider;
    public Vector3 magnusForce;
    
    // From http://hyperphysics.phy-astr.gsu.edu/hbase/airfri2.html#c3
    public const float terminalVelocity = 33;
    // Assuming linear drag
    const float Drag = 9.8f / terminalVelocity;

    
    public void ReturnToPool()
    {
        pool.ReturnBall(this);
    }

    private void Awake()
    {
        RigidBody = GetComponent<Rigidbody>();
        RigidBody.drag = 0;
        Collider = GetComponent<SphereCollider>();
    }
    
    private void FixedUpdate()
    {
        if (!RigidBody.isKinematic && RigidBody.IsSleeping())
        {
            ReturnToPool();
            return;
        }
        
        // Custom drag due to: https://forum.unity.com/threads/terminal-velocity.34667/#post-1869897
        var velocity = RigidBody.velocity;
        velocity -= velocity * Drag * Time.fixedDeltaTime;
        RigidBody.velocity = velocity;

        ApplyMagnusForce();


    }

    private void ApplyMagnusForce()
    {
        var B = 0.00041f;
        magnusForce =  B * Vector3.Cross(RigidBody.angularVelocity, RigidBody.velocity);
        RigidBody.AddForce(magnusForce, ForceMode.Force);
    }
}