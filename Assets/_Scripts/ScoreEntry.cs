using UnityEngine;
using TMPro;

/// <summary>
/// 'ScoreEntry_Prefab'ýnýn beynidir.
/// ScoreSceneManager'dan gelen "sýra", "isim" ve "puan" verilerini
/// kendi üzerindeki TextMeshPro objelerine yazar.
/// </summary>
public class ScoreEntry : MonoBehaviour
{
    // --- UNITY EDITOR REFERANSLARI ---
    // (Prefab Düzenleme Modu'nda baðlanacak)
    [SerializeField] private TextMeshProUGUI rankText;  // "1." yazan yer
    [SerializeField] private TextMeshProUGUI nameText;  // "Oyuncu Adý" yazan yer
    [SerializeField] private TextMeshProUGUI scoreText; // "12 PTS" yazan yer

    /// <summary>
    /// ScoreSceneManager bu fonksiyonu çaðýrarak prefab'ýn içini doldurur.
    /// </summary>
    /// <param name="rank">Sýralama (örn: 1)</param>
    /// <param name="name">Oyuncu Adý (örn: "Ahmet")</param>
    /// <param name="score">Puan (örn: 12)</param>
    public void Setup(int rank, string name, int score)
    {
        rankText.text = $"{rank}.";
        nameText.text = name;
        scoreText.text = $"{score} PTS";

        // (Opsiyonel Cila: Eðer 1. sýra ise, rengini Vurgu Rengi yap)
        if (rank == 1)
        {
            // rankText.color = new Color(0, 0.63f, 1f); // #00A2FF
            // nameText.color = new Color(0, 0.63f, 1f);
            // scoreText.color = new Color(0, 0.63f, 1f);
        }
    }
}