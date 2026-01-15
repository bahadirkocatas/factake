using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button
using TMPro; // TextMeshPro
using System.Linq; // OrderByDescending (Sýralama) için
using UnityEngine.SceneManagement; // Sahne yönetimi

/// <summary>
/// 'ScoreScene' (Puan Ekraný) sahnesinin beynidir.
/// GameData'dan puanlarý okur, sýralar, kazananý gösterir
/// ve "Yeniden Oyna" / "Ana Menü" butonlarýný yönetir.
/// </summary>
public class ScoreSceneManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private TextMeshProUGUI winnerNameTextUI;
    [SerializeField] private Transform scoreListContainer; // Panel_ScoreList

    [Header("Butonlar")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Prefab")]
    [SerializeField] private GameObject scoreEntryPrefab; // ScoreEntry_Prefab

    void Start()
    {
        // 1. Referanslarý kontrol et
        if (winnerNameTextUI == null || scoreListContainer == null || replayButton == null || mainMenuButton == null || scoreEntryPrefab == null)
        {
            Debug.LogError("ScoreSceneManager'da referanslar EKSÝK! Lütfen Inspector'u kontrol edin.");
            return;
        }

        // 2. Butonlarý ayarla
        replayButton.onClick.AddListener(OnReplay);
        mainMenuButton.onClick.AddListener(OnMainMenu);

        // 3. Test verisi (Eðer direkt bu sahneden baþlattýysak)
        if (GameData.PlayerNames.Count == 0)
        {
            Debug.LogWarning("GameData boþ! Test verisi yükleniyor...");
            GameData.PlayerNames = new List<string> { "Test A", "Test B", "Test C", "Test D" };
            GameData.PlayerScores = new Dictionary<int, int> {
                { 0, 10 },
                { 1, 15 },
                { 2, 8 },
                { 3, 15 }
            };
        }

        // 4. Puan ekranýný doldur
        PopulateScores();
    }

    /// <summary>
    /// Puanlarý GameData'dan çeker, sýralar ve UI'ý doldurur.
    /// </summary>
    private void PopulateScores()
    {
        // 1. Puanlarý sýralanabilir bir formata (KeyValuePair) dönüþtür
        // ve puana göre BÜYÜKTEN KÜÇÜÐE doðru sýrala.
        var sortedScores = GameData.PlayerScores.OrderByDescending(pair => pair.Value);

        // 2. Kazananý (veya kazananlarý) bul
        if (sortedScores.Any())
        {
            int maxScore = sortedScores.First().Value;
            // O puana sahip herkesi al (Beraberlik durumu için)
            var winners = sortedScores.Where(pair => pair.Value == maxScore);

            // Ýsimlerini birleþtir
            string winnerNames = string.Join(" & ", winners.Select(pair => GameData.PlayerNames[pair.Key]));

            winnerNameTextUI.text = winnerNames;
        }
        else
        {
            winnerNameTextUI.text = "HATA";
        }

        // 3. Puan listesini temizle
        foreach (Transform child in scoreListContainer)
        {
            Destroy(child.gameObject);
        }

        // 4. Listeyi (Sýralý halde) prefab'larla doldur
        foreach (var entry in sortedScores)
        {
            int playerIndex = entry.Key;
            int playerScore = entry.Value;
            string playerName = GameData.PlayerNames[playerIndex];

            // Prefab'ý yarat
            GameObject newEntry = Instantiate(scoreEntryPrefab, scoreListContainer);

            // YENÝ, GÜVENLÝ YÖNTEM:
            // Prefab'ýn üzerindeki 'ScoreEntryUI' script'ini al
            ScoreEntryUI entryScript = newEntry.GetComponent<ScoreEntryUI>();
            if (entryScript != null)
            {
                // O script'in 'Setup' fonksiyonunu çaðýr
                entryScript.Setup(playerName, playerScore);
            }
            else
            {
                Debug.LogError("ScoreEntry_Prefab üzerinde 'ScoreEntryUI.cs' script'i bulunamadý!");
            }

            // (Eski, güvensiz 'transform.Find' kodlarý silindi)
        }
    }

    /// <summary>
    /// "YENÝDEN OYNA" butonuna týklandýðýnda çalýþýr.
    /// (GameData'yý temizlemez, sadece 'PlayerInputScene'e döner
    /// ki ayarlar ve isimler tekrar girilsin)
    /// </summary>
    private void OnReplay()
    {
        // (GameData, PlayerInputManager'da 'OnStartGame'e basýldýðýnda
        // zaten sýfýrlanýyor (Puanlar, Turlar vb.), bu yüzden burada
        // bir þey yapmamýza gerek yok.)
        SceneManager.LoadScene("PlayerInputScene");
    }

    /// <summary>
    /// "ANA MENÜ" butonuna týklandýðýnda çalýþýr.
    /// </summary>
    private void OnMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}