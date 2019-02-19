using System;
using UnityEngine.Serialization;

[Serializable]
public class Pitch
{
    public PitchTypes Name;
    public float CurveAngle;
    public float MaxCurveSpeedRpm;
    public float FlightTime =.5f;
}

public enum PitchTypes
{
    StraightBall,
    FastBall,
    CurveBall,
    Slider,
    Screwball
}