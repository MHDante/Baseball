using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    public GameObject Prefab;
    public int PrewarmAmount;
    private List<GameObject> objects = new List<GameObject>();
    void Awake()
    {

        for (int i = 0; i < PrewarmAmount; i++)
        {
            objects.Add(Make());
        }
        
    }

    public GameObject Get()
    {
        GameObject b;
        if (objects.Count > 0)
        {
            b = objects[0];
            objects.RemoveAt(0);
        }
        else
        {
            b = Make();
        }

        b.gameObject.SetActive(true);
        return b;
    }

    public void Return(GameObject b)
    {
        b.gameObject.SetActive(false);
        var t = b.transform;
        t.localPosition = Prefab.transform.localPosition;
        t.localRotation = Prefab.transform.localRotation;
        t.localScale = Prefab.transform.localScale;
        objects.Add(b);
    }

    private GameObject Make()
    {
        var ball = Instantiate(Prefab);
        var comp = ball.GetComponent<Poolable>() ?? ball.AddComponent<Poolable>();
        comp.pool = this;
        ball.transform.SetParent(transform);
        ball.gameObject.SetActive(false);
        return ball;
    }
}

public class Poolable : MonoBehaviour
{
    public Pool pool;
    
    public void ReturnToPool()
    {
        pool.Return(this.gameObject);
    }
}