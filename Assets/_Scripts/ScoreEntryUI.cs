using UnityEngine;
using TMPro;

/// <summary>
/// 'ScoreEntry_Prefab'ýn üzerindeki metinleri
/// güvenli bir þekilde güncelleyen küçük bir script.
/// </summary>
public class ScoreEntryUI : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void Setup(string name, int score)
    {
        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = $"{score} Puan";
    }
}