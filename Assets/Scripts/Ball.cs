using UnityEngine;

public class Ball : MonoBehaviour
{
    public Rigidbody RigidBody;
    public SphereCollider Collider;
    public AudioSource AudioSource;
    
    public AudioClip GrounderClip;
    public AudioClip WhackClip;
    public AudioClip TipClip;
    public AudioClip TapClip;

    private Vector3 magnusForce;


    public bool Launched;
    public bool HitBat;
    public bool HitFloor;

    public Vector3 launchVelocity;
    public Vector3 hitVelocity;
    public Vector3 landPosition;
    

    // From http://hyperphysics.phy-astr.gsu.edu/hbase/airfri2.html#c3
    public const float terminalVelocity = 33;
    // Assuming linear drag
    const float Drag = 9.8f / terminalVelocity;
    
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
            HitFloor = false;
            HitBat = false;
            Launched = false;

            GetComponent<Poolable>().ReturnToPool();
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


    public void OnLaunch()
    {
        Launched = true;
        launchVelocity = RigidBody.velocity;
        HitBat = false;

    }

    private void OnCollisionStay(Collision other)
    {
        if (!HitFloor && other.collider.CompareTag("Floor") && Mathf.Abs(RigidBody.velocity.y) <=.5f)
        {
            HitFloor = true;
            landPosition = RigidBody.position;
        }
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Floor"))
        {
            AudioSource.PlayOneShot(Time.frameCount %2 == 1 ? TapClip : TipClip,.1f);
        }
    }

    private void OnCollisionExit(Collision other)
    {

        if (other.collider.CompareTag("Bat"))
        {
            HitBat = true;
            var bat = other.rigidbody.GetComponent<Bat>();
            hitVelocity = RigidBody.velocity;

            if (hitVelocity.y < 0)
            {
                AudioSource.PlayOneShot(GrounderClip,.5f);
                return;
            }
            AudioSource.PlayOneShot(WhackClip);
            
        }
    }
}