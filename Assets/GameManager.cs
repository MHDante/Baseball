using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    static bool IsRightHand;

    public Pitcher pitcher;
    public ParticleSystem pitcherParticles;
    public Transform SpeakPosition;
    public Transform PitchPosition;
    public Bat Bat;
    public GameObject StrikeZone;
    public GameObject Sign;
    public Transform SignHidePosition;
    public GameObject Title;
    public Text TutorialText;
    public GameObject Tutorial;
    public LayerMask StrikeZoneLayerMask;

    public Pitch[] pitches;


    readonly CustomYieldInstruction WaitForTrigger = 
        new WaitUntil(TriggerDown);

    public int score;


    private static bool TriggerDown()
    {
        return Input.GetKeyDown(IsRightHand?  KeyCode.JoystickButton14 : KeyCode.JoystickButton15 );
    }
    
    IEnumerator Start()
    {
        Debug.Log("Yo!");
        yield return null;
        yield return WaitForTrigger;
        
        SetSignScreen(SignScreens.Text);

        MatchTransformTo(pitcher.transform, SpeakPosition);
        pitcherParticles.Play();
        
        TutorialText.text = "Welcome to TopTal Baseball! Press the trigger to Continue";
        yield return null;
        yield return WaitForTrigger;

        yield return CalibrateStrikeZone();
        
        TutorialText.text = "That green area above home plate is the strike zone. The pitcher will pitch the ball in that zone.";
        yield return null;
        yield return WaitForTrigger;

        TutorialText.text = "Stand in either one of the boxes beside home plate, and put the tip of your bat inside the strike zone";

        yield return CheckIfInZone();
        TutorialText.text = "Aim for the target in the field. Good Luck!";

        yield return new  WaitForSeconds(2);

        pitcherParticles.Emit(30);
        MatchTransformTo(pitcher.transform, PitchPosition);
        pitcherParticles.Emit(30);
        
        var go = Time.time + 3;

        var pos = Sign.transform.position;
        for (int i = 0; i < 1000; i++)
        {
            Sign.transform.position = Vector3.Lerp(pos, SignHidePosition.position, 1 - (go-Time.time)/3);
            yield return null;
        }

        foreach (var pitch in pitches)
        {
            yield return MakePitch(pitch);
        }

        TutorialText.text = $"Total Score: \n {score}";
        go = Time.time + 1.5f;
        for (int i = 0; i < 1000; i++)
        {
            Sign.transform.position = Vector3.Lerp(SignHidePosition.position,pos,  1 - (go-Time.time)/3);
            yield return null;
        }

        yield return null;
        yield return WaitForTrigger;

        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(0);


    }

    private IEnumerator MakePitch(Pitch pitch)
    {
        pitcher.MakePitch(pitch);
        var ball = pitcher.ball;
        yield return new WaitUntil(() => ball.Launched);
        // Todo: Show speed
        yield return new WaitUntil(() => ball.HitFloor || ball.HitBat);
        var hitScore = 0;
        if (ball.HitBat)
        {
            hitScore += 500;
            if (!ball.HitFloor)
            {
                yield return new WaitUntil(() => ball.HitFloor);
                var p = ball.landPosition;
                if (p.z > 0 && (Mathf.Abs(p.x) < p.z))
                {
                    hitScore += 500; // Not Foul.
                }

                hitScore += (int) (p.magnitude * 100);
            }
        }

        Debug.Log(hitScore);
        score += hitScore;
    }

    private IEnumerator CheckIfInZone()
    {
        var arr = new Collider[10];
        while (true)
        {
            bool inZone = Physics.OverlapSphereNonAlloc(Bat.centerOfMass.position, .1f, arr, StrikeZoneLayerMask,
                              QueryTriggerInteraction.Collide) > 0;

            if (!inZone)
            {
                yield return null;
                continue;
            }

            var batX = Bat.transform.forward.x;
            if (batX < .3 && batX > -.3)
            {
                yield return null;
                continue;
            }

            break;
        }
    }

    private IEnumerator CalibrateStrikeZone()
    {
        while (true)
        {
            TutorialText.text = "Let's start by placing the controller on your shoulder and press the trigger";
            yield return null;
            yield return WaitForTrigger;
            var top = Bat.transform.position.y;


            TutorialText.text = "Put Controller at your waist then press the trigger";
            yield return null;
            yield return WaitForTrigger;
            var middle = Bat.transform.position.y;

            TutorialText.text = "Now Place the Controller at your knee, and press the trigger";
            yield return null;
            yield return WaitForTrigger;
            var bottom = Bat.transform.position.y;
            if (SetStrikeZone(top, middle, bottom)) break;
        }
    }

    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.JoystickButton14)) IsRightHand = true;
        if (Input.GetKeyDown(KeyCode.JoystickButton15)) IsRightHand = false;
            
    }


    public bool SetStrikeZone(float top, float middle, float bottom)
    {

        if (middle < .5f) return false;
        if (top - middle < .3f) top = middle + .3f;
        if (middle - bottom < .3f) bottom = middle - .3f;

        var torso = middle + (top - middle) / 2;
        var s = StrikeZone.transform.localScale;
        s.y = torso - bottom;
        StrikeZone.transform.localScale = s;

        var p = StrikeZone.transform.position;
        p.y = (torso + bottom)/2;
        StrikeZone.transform.position = p;

        StrikeZone.SetActive(true);
        return true;

    }

    public enum SignScreens
    {
        Title,
        Text,
        Menu
    }
    
    public void SetSignScreen(SignScreens screen)
    {
        
        Title.SetActive(false);
        Tutorial.SetActive(false);

        switch (screen)
        {
            case SignScreens.Title:
                Title.SetActive(true);
                break;
            case SignScreens.Text:
                Tutorial.SetActive(true);
                break;
            case SignScreens.Menu:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(screen), screen, null);
        }
    }

    private void MatchTransformTo(Transform source, Transform target)
    {
        source.transform.position = target.transform.position;
        source.transform.rotation = target.transform.rotation;
    }
}
