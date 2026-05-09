using TMPro;
using UnityEngine;

public sealed class MainMenuProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bestScoreText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (bestScoreText != null)
            bestScoreText.text = $"Puntaje máximo: {PlayerProfile.BestScore}";
    }
}