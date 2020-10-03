using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoSingleton<UIController>
{
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private TextMeshProUGUI scoreText;
    private int score;
    
    void Awake()
    {
        score = 0;
        scoreText.text = score.ToString();
        gameOverCanvas.gameObject.SetActive(false);
    }

    public int IncrementAndReturnScore(int _points)
    {
        score += _points;
        scoreText.text = score.ToString();
        return score;
    }

    public void ShowGameOverCanvas()
    {
        gameOverCanvas.gameObject.SetActive(true);
    }
}
