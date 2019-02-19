using System;

[Serializable]
public class Pitch
{
    public PitchTypes name;
    public float curveAngle;
    public float MaxCurveSpeedRPM;
    public float flightTime =.5f;
}

public enum PitchTypes
{
    StraightBall,
    FastBall,
    CurveBall,
    Slider,
    Screwball
}