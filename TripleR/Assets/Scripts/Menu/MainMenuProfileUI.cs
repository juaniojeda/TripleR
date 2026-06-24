using TMPro;
using UnityEngine;

public sealed class MainMenuProfileUI : MonoBehaviour
{
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text coinsText;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (bestScoreText != null)
            bestScoreText.text = $"Puntaje máximo: {PlayerProfile.BestScore}";

        if (coinsText != null)
            coinsText.text = $"Monedas: {PlayerProfile.Coins}";
    }
}