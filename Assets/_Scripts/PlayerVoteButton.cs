using UnityEngine;
using UnityEngine.UI; // Button için
using TMPro; // TextMeshPro için

/// <summary>
/// 'VotingPanel' (Oylama Paneli) içinde yaratýlan
/// 'PlayerVoteButton_Prefab'ýnýn beynidir.
/// </summary>
public class PlayerVoteButton : MonoBehaviour
{
    // --- UNITY EDITOR REFERANSLARI ---
    // (Bu baðlantýlar Prefab Düzenleme Modu'nda yapýlmalýdýr)

    [Tooltip("Butonun içindeki oyuncu adýný gösteren metin")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    [Tooltip("Prefab'ýn kendi Button bileþeni")]
    [SerializeField] private Button thisButton;

    // Oyuncunun 'GameData.PlayerNames' listesindeki
    // 'Global Index'ini (örn: 0, 1, 2, 3) saklar.
    private int playerGlobalIndex;

    /// <summary>
    /// GameManager, bu butonu yarattýðýnda bu fonksiyonu çaðýrýr.
    /// </summary>
    /// <param name="playerName">Butonda görünecek isim (örn: "Ahmet")</param>
    /// <param name="globalIndex">Bu oyuncunun 'GameData'daki ana index'i</param>
    public void Setup(string playerName, int globalIndex)
    {
        this.playerGlobalIndex = globalIndex;
        playerNameText.text = playerName;

        // Butonun týklanabilir olduðundan emin ol
        if (thisButton != null)
            thisButton.interactable = true;
    }

    /// <summary>
    /// Bu fonksiyon, prefab'daki 'Button' bileþeninin 'On Click ()' olayýna baðlýdýr.
    /// Jüri bu butona týkladýðýnda çalýþýr.
    /// </summary>
    public void OnVoteButtonClicked()
    {
        // Butonu kilitle (Jüri fikrini deðiþtiremesin)
        if (thisButton != null)
            thisButton.interactable = false;

        // GameManager'ýn Singleton'ýna (Instance) eriþ
        // ve ona hangi 'Global Index'in seçildiðini bildir.
        // GameManager.Instance.OnJuryDecision(this.playerGlobalIndex);

        // Güvenlik kontrolü (Eðer GameManager yoksa diye)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnJuryDecision(this.playerGlobalIndex);
        }
        else
        {
            Debug.LogError("GameManager 'Instance' bulunamadý! PlayerVoteButton týklamayý iletemiyor.");
        }
    }

    /// <summary>
    /// GameManager'ýn dýþarýdan çaðýrmasý için
    /// (örn: Jüri baþka birini seçtiðinde veya süre dolduðunda)
    /// </summary>
    public void Lock()
    {
        if (thisButton != null)
            thisButton.interactable = false;
    }
}