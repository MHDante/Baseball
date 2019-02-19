using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public ScoreBoard ScoreBoard;
    public int score;
    public AudioSource Audience;

    public Pitch[] pitches;

    private void Awake()
    {
        Score.GetScores();
    }

    IEnumerator Start()
    {
        SetSignScreen(SignScreens.Title);

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

        yield return new WaitForSeconds(2);

        pitcherParticles.Emit(30);
        MatchTransformTo(pitcher.transform, PitchPosition);
        pitcherParticles.Emit(30);

        var go = Time.time + 3;

        var pos = Sign.transform.position;
        while (Time.time < go)
        {
            Sign.transform.position = Vector3.Lerp(pos, SignHidePosition.position, 1 - (go - Time.time) / 3);
            yield return null;
        }

        foreach (var pitch in pitches)
        {
            yield return MakePitch(pitch);
        }

        var s = new Score()
        {
            DateTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Points = score
        };
        Score.Submit(s);
        
        ScoreBoard.ShowScores(s);
        SetSignScreen(SignScreens.Scores);

        go = Time.time + 1.5f;

        while(Time.time < go)
        {
            Sign.transform.position = Vector3.Lerp(SignHidePosition.position, pos, 1 - (go - Time.time) / 3);
            yield return null;
        }
        
        yield return null;
        yield return WaitForTrigger;


        
        // Todo: Fade to black. We can use the SteamVR Loading Screen.
        SceneManager.LoadScene(0);
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.JoystickButton14)) IsRightHand = true;
        if (Input.GetKeyDown(KeyCode.JoystickButton15)) IsRightHand = false;

    }

    readonly CustomYieldInstruction WaitForTrigger =
        new WaitUntil(TriggerDown);

    private static bool TriggerDown()
    {
        return Input.GetKeyDown(IsRightHand ? KeyCode.JoystickButton14 : KeyCode.JoystickButton15);
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
                var q = ball.hitVelocity;
                if (q.z > 0 && (Mathf.Abs(q.x) < q.z))
                {
                    if (q.magnitude > 30) Audience.Play();
                }

                yield return new WaitUntil(() => ball.HitFloor);
                Audience.Stop();
                var p = ball.landPosition;
                // Not Foul.
                if (p.z > 0 && (Mathf.Abs(p.x) < p.z))
                {
                    if(ball.hitVelocity.magnitude > 30) Audience.Play();
                    hitScore += 500;
                    hitScore += (int)(p.magnitude * 100);
                }

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
        p.y = (torso + bottom) / 2;
        StrikeZone.transform.position = p;

        StrikeZone.SetActive(true);
        return true;

    }

    public enum SignScreens
    {
        Title,
        Text,
        Scores
    }

    public void SetSignScreen(SignScreens screen)
    {

        Title.SetActive(false);
        Tutorial.SetActive(false);
        ScoreBoard.gameObject.SetActive(false);

        switch (screen)
        {
            case SignScreens.Title:
                Title.SetActive(true);
                break;
            case SignScreens.Text:
                Tutorial.SetActive(true);
                break;
            case SignScreens.Scores:
                ScoreBoard.gameObject.SetActive(true);
                break;
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