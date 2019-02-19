using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class Score
{

    public struct ScoreList
    {
        public List<Score> Scores;
    }

    public int DateTime;
    public int Points;


    private static string _ScorePath = Path.Combine(Application.persistentDataPath, "Scores.ball");
    private static  ScoreList _Scores;
    public static IReadOnlyList<Score> GetScores()
    {
        if (_Scores.Scores != null) return _Scores.Scores;

        if (File.Exists(_ScorePath))
        {
            var json = File.ReadAllText(_ScorePath);
            _Scores = JsonUtility.FromJson<ScoreList>(json);
            return _Scores.Scores;
        }
        else
        {
            _Scores.Scores = new List<Score>()
            {
                new Score() {DateTime = -481248000, Points = 4000},
                new Score() {DateTime = 689644800, Points = 2000},
                new Score() {DateTime = 688435200, Points = 1000},
            };

            var json = JsonUtility.ToJson(_Scores);
            File.WriteAllText(_ScorePath, json);
            return _Scores.Scores;
        }
    }

    public static void Submit(Score score)
    {
        _Scores.Scores.Add(score);
        _Scores.Scores = _Scores.Scores.OrderByDescending(s => s.Points).ToList();
        var json = JsonUtility.ToJson(_Scores);
        File.WriteAllText(_ScorePath, json);

    }




}
