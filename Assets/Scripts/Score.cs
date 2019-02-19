using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Score
{
    
    public int DateTime;
    public int Points;

    private static string _ScorePath = Path.Combine(Application.persistentDataPath, "Scores.ball");
    private static List<Score> _Scores;
    public static IReadOnlyList<Score> GetScores()
    {
        if (_Scores != null) return _Scores;
        
        if (File.Exists(_ScorePath))
        {
            var json = File.ReadAllText(_ScorePath);
            _Scores = JsonUtility.FromJson<List<Score>>(json);
            return _Scores;
        }

        _Scores = new List<Score>()
        {
            new Score() {DateTime = -481248000, Points = 4000},
            new Score() {DateTime = 689644800, Points = 2000},
            new Score() {DateTime = 688435200, Points = 1000},
        };
        return _Scores;

    }

    public static void Submit(Score score)
    {
        _Scores.Add(score);
        _Scores = _Scores.OrderByDescending(s=>s.Points).ToList();
        var json = JsonUtility.ToJson(_Scores);
        Task.Run(() => File.WriteAllText(_ScorePath, json));

    }

}