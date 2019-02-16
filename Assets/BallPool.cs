using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    public GameObject Prefab;
    public int PrewarmAmount;
    private List<Ball> balls = new List<Ball>();
    // Start is called before the first frame update
    void Awake()
    {

        for (int i = 0; i < PrewarmAmount; i++)
        {
            balls.Add(MakeBall());
        }
        
    }

    public Ball GetBall()
    {
        var b =  balls.Count > 0 ? balls[0] : MakeBall();
        b.gameObject.SetActive(true);
        return b;
    }

    public void ReturnBall(Ball b)
    {
        b.gameObject.SetActive(false);
        var t = b.transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
        balls.Add(b);
    }

    private Ball MakeBall()
    {

        var ball = Instantiate(Prefab).AddComponent<Ball>();
        ball.pool = this;
        ball.transform.SetParent(transform);
        ball.gameObject.SetActive(false);
        return ball;
    }
}

public class Ball : MonoBehaviour
{
    public BallPool pool;
    public Rigidbody rigidBody;
    

    public void ReturnToPool()
    {
        pool.ReturnBall(this);
    }

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
}