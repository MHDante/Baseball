using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pitcher : MonoBehaviour
{

    public Transform HandTransform;
    public Transform ThrowOrigin;
    private CharacterController _cc;
    private Animator _animator;
    

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cc.SimpleMove(Vector3.forward*.01f / Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.JoystickButton14) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton15))
        {
            SetState(PitcherStates.Pitch);
        }
    }

    void OnThrow()
    {
        ThrowOrigin.position = HandTransform.position;
    }

    void SetState(PitcherStates state)
    {
        _animator.SetInteger(AnimVar_State, (int)state);
        _animator.SetTrigger(AnimVar_Animate);
    }


    // Todo: Keep In Sync with Animator.
    private static readonly int AnimVar_Animate = Animator.StringToHash("Animate");
    private static readonly int AnimVar_State = Animator.StringToHash("State");

    enum PitcherStates
    {
        Idle=0,
        Pitch=1
    }

}
