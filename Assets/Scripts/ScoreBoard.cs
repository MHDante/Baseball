using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    public Text YourScore;
    public Text Names;
    public Text Scores;
    
    public void ShowScores(Score yours)
    {
        var scoreList = Score.GetScores();
        YourScore.text = $"Your Score: {yours.Points}";
        var n = new StringBuilder();
        var s = new StringBuilder();

        for (int i = 0; i < scoreList.Count; i++)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(scoreList[i].DateTime).DateTime;
            if (scoreList[i] == yours)
            {
                n.Append("<color=#ffff00ff>");
                s.Append("<color=#ffff00ff>");
            }
            n.Append($"{i}.- {dateTime:d}\n");
            s.Append($"{scoreList[i].Points}\n");

            if (scoreList[i] == yours)
            {
                n.Append("</color>");
                s.Append("</color>");
            }
            
        }

        Names.text = n.ToString();
        Scores.text = s.ToString();
    }
}
