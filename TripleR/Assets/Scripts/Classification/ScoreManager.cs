using TMPro;
using UnityEngine;

public sealed class ScoreManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private bool clampToZero = true;

    private int currentScore;

    public int CurrentScore => currentScore;

    private void Awake()
    {
        RefreshUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (clampToZero && currentScore < 0)
            currentScore = 0;

        RefreshUI();

        Debug.Log($"Puntaje actual: {currentScore}");
    }

    private void RefreshUI()
    {
        if (scoreText != null)
            scoreText.text = $"Puntos: {currentScore}";
    }
}