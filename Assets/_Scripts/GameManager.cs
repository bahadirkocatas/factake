using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button, Image
using TMPro; // TextMeshPro
using UnityEngine.SceneManagement; // Sahne yönetimi
using System.Linq; // OrderBy (CevapDeðiþimi için)

/// <summary>
/// Arena (GameScene) sahnesinin beynidir.
/// (Dairesel Sayaç ve Yeni UI Temasý ile GÜNCELLENDÝ)
/// </summary>
public class GameManager : MonoBehaviour
{
    // === SINGLETON KURULUMU ===
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }
    // === SINGLETON BÝTÝÞÝ ===


    // --- UNITY EDITOR REFERANSLARI ---

    [Header("Ana UI (Yeni Tasarým)")]
    [Tooltip("Soru metnini gösteren ana UI")]
    [SerializeField] private TextMeshProUGUI questionTextUI;
    [Tooltip("Jüri'nin 'KARAR VER' butonu")]
    [SerializeField] private Button startVoteButtonUI;
    [Tooltip("Dairesel sayacýn DOLAN ('Filled') kýsmý")]
    [SerializeField] private Image timerCircleFill_Image;
    [Tooltip("Dairenin ortasýndaki '01:30' dijital metni")]
    [SerializeField] private TextMeshProUGUI digitalTimer_Text;

    [Header("Sayaç Renkleri (G -> Y -> R)")]
    [SerializeField] private Color timerColorGreen = new Color(0.18f, 0.8f, 0.44f); // Yeþil
    [SerializeField] private Color timerColorYellow = new Color(0.94f, 0.76f, 0.05f); // Sarý
    [SerializeField] private Color timerColorRed = new Color(0.9f, 0.3f, 0.23f); // Kýrmýzý

    [Header("Oylama Paneli (Voting Panel)")]
    [SerializeField] private GameObject votingPanelUI;
    [SerializeField] private Transform votingPanelContainer;

    [Header("Bonus Oylama Paneli (Pürüz 2)")]
    [SerializeField] private GameObject improvBonusPanelUI;
    [SerializeField] private TextMeshProUGUI bonusInfoTextUI;
    [SerializeField] private Button safeBetButtonUI;
    [SerializeField] private Button improvButtonUI;

    [Header("Kaos: Hatýrlatýcý UI (Yeni Tasarým)")]
    [SerializeField] private GameObject chaosInfoPanel; // Panel_ChaosInfo (Üst banner)
    [SerializeField] private TextMeshProUGUI chaosInfoText; // Text_ChaosInfo (Banner içindeki metin)

    [Header("Kaos: Hýzlý Atýþ UI")]
    [SerializeField] private GameObject hizliAtisPanelUI;
    [SerializeField] private TextMeshProUGUI hizliAtisInfoTextUI;
    [SerializeField] private TextMeshProUGUI hizliAtisTimerTextUI;

    [Header("Kaos: Cevap Deðiþimi UI")]
    [SerializeField] private GameObject cevapDegisimiPanelUI;
    [SerializeField] private TextMeshProUGUI[] cevapDegisimiTextsUI;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerVoteButtonPrefab;

    // --- ÖZEL OYUN AKIÞ DEÐÝÞKENLERÝ ---

    private float remainingTime;
    private float totalRoundTime = 90f; // Sabit 90 saniye (veya Ayarlar'dan)
    private bool isRoundActive;
    private int winningFakePlayerIndex;

    // --- Kaos: Hýzlý Atýþ Deðiþkenleri ---
    private bool isHizliAtisActive = false;
    private float hizliAtisRemainingTime;
    private int hizliAtisContestantIndex = 0;

    // --- Kaos: Cevap Deðiþimi Deðiþkenleri ---
    private bool cevapDegisimiYapildi = false;


    // === OYUN DÖNGÜSÜ ===

    void Start()
    {
        // (Yeni UI referanslarýný kontrol et)
        if (questionTextUI == null || startVoteButtonUI == null || timerCircleFill_Image == null || digitalTimer_Text == null ||
            chaosInfoPanel == null || chaosInfoText == null)
        {
            Debug.LogError("GameManager'daki ana UI referanslarý EKSÝK! (Yeni Tasarým)");
            return;
        }

        StartRound();
    }

    void Update()
    {
        // === ANA TARTIÞMA SAYACI ===
        if (isRoundActive)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0) { TimeUp(); }
            UpdateTimerUI(); // (Bu fonksiyon artýk dairesel sayacý da güncelliyor)

            // === CEVAP DEÐÝÞÝMÝ TETÝKLEYÝCÝSÝ ===
            if (GameData.CurrentChaosRound == ChaosRoundType.CevapDegisimi &&
                !cevapDegisimiYapildi &&
                remainingTime <= (totalRoundTime / 2f)) // (45. Saniye)
            {
                cevapDegisimiYapildi = true;
                PerformCevapDegisimi();
            }
        }

        // === HIZLI ATIÞ SAYACI ===
        if (isHizliAtisActive)
        {
            hizliAtisRemainingTime -= Time.deltaTime;
            if (hizliAtisRemainingTime <= 0)
            {
                hizliAtisRemainingTime = 0;
                UpdateHizliAtisTimerUI();
                isHizliAtisActive = false;
                Invoke("GoToNextHizliAtisTurn", 2.0f);
            }
            UpdateHizliAtisTimerUI();
        }
    }

    /// <summary>
    /// Turun baþýnda UI'ý kurar ve Kaos Kuralýný uygular.
    /// </summary>
    private void StartRound()
    {
        questionTextUI.text = GameData.CurrentQuestion.soruMetni;

        // (totalRoundTime ileride Ayarlar panelinden okunabilir, þimdilik 90)
        remainingTime = totalRoundTime;
        isRoundActive = true;
        cevapDegisimiYapildi = false;
        UpdateTimerUI(); // (Sayacý ve daireyi 01:30'a ayarlar)

        // Panelleri ayarla (Gizle)
        votingPanelUI.SetActive(false);
        improvBonusPanelUI.SetActive(false);

        // "KARAR VER" butonunu ayarla
        startVoteButtonUI.gameObject.SetActive(true);
        startVoteButtonUI.interactable = true;
        startVoteButtonUI.onClick.RemoveAllListeners();
        startVoteButtonUI.onClick.AddListener(ShowVotingPanel);

        // Kaos Kuralýný Uygula
        ApplyChaosRule(GameData.CurrentChaosRound);
    }

    /// <summary>
    /// (YENÝ) O turun kuralýna göre UI'ý ve mekanikleri deðiþtirir.
    /// </summary>
    private void ApplyChaosRule(ChaosRoundType rule)
    {
        // 1. Önce TÜM özel UI'larý sýfýrla/gizle
        // (Normal tartýþma ekranýný göster)
        timerCircleFill_Image.transform.parent.gameObject.SetActive(true); // Panel_Timer'ý göster
        startVoteButtonUI.gameObject.SetActive(true);
        questionTextUI.gameObject.SetActive(true);

        hizliAtisPanelUI.SetActive(false);
        cevapDegisimiPanelUI.SetActive(false);
        chaosInfoPanel.SetActive(false); // (Kaos banner'ýný gizle)

        // 2. Þimdi yeni kuralý 'switch' ile uygula
        switch (rule)
        {
            case ChaosRoundType.None:
                // Normal tur. Hiçbir panel gösterme.
                Debug.Log("KAOS KURALI UYGULANIYOR: Yok (Normal Tur)");
                break;

            case ChaosRoundType.YasakHarf:
                Debug.Log("KAOS KURALI UYGULANIYOR: Yasak Harf");
                chaosInfoPanel.SetActive(true); // Yeni paneli göster
                chaosInfoText.text = "KAOS TURU: 'A' HARFÝ YASAK!";
                break;

            case ChaosRoundType.ZorunluKelime:
                Debug.Log("KAOS KURALI UYGULANIYOR: Zorunlu Kelime");
                chaosInfoPanel.SetActive(true); // Yeni paneli göster
                chaosInfoText.text = $"KAOS: '{GameData.CurrentMandatoryWord}' DEMEK ZORUNLU!";
                break;

            case ChaosRoundType.CevapDegisimi:
                Debug.Log("KAOS KURALI UYGULANIYOR: Cevap Deðiþimi");
                chaosInfoPanel.SetActive(true); // Yeni paneli göster
                chaosInfoText.text = "KAOS: DEÐÝÞÝM TURU! (45. Saniyede)";
                break;

            case ChaosRoundType.HizliAtis:
                Debug.Log("KAOS KURALI UYGULANIYOR: Hýzlý Atýþ");
                // Hýzlý Atýþ, ana tartýþma ekranýný tamamen gizler.
                timerCircleFill_Image.transform.parent.gameObject.SetActive(false); // Panel_Timer'ý gizle
                startVoteButtonUI.gameObject.SetActive(false);
                questionTextUI.gameObject.SetActive(false);

                StartHizliAtisSequence();
                break;
        }
    }

    /// <summary>
    /// (YENÝ) Zamanlayýcý metnini, dairesel sayacý VE rengini günceller.
    /// </summary>
    private void UpdateTimerUI()
    {
        if (remainingTime < 0) remainingTime = 0;

        // 1. Dijital Metni Güncelle (Dairenin içi)
        float minutes = Mathf.FloorToInt(remainingTime / 60);
        float seconds = Mathf.FloorToInt(remainingTime % 60);
        digitalTimer_Text.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // 2. Dairesel Sayacý (Fill) Güncelle
        float fill = remainingTime / totalRoundTime; // (örn: 45 / 90 = 0.5)
        timerCircleFill_Image.fillAmount = fill;

        // 3. Rengi Güncelle (Yeþil -> Sarý -> Kýrmýzý)
        if (fill > 0.5f) // %50'den fazlaysa
        {
            timerCircleFill_Image.color = timerColorGreen;
        }
        else if (fill > 0.2f) // %20 ile %50 arasý
        {
            timerCircleFill_Image.color = timerColorYellow;
        }
        else // %20'den az (Kritik)
        {
            timerCircleFill_Image.color = timerColorRed;
        }
    }

    /// <summary>
    /// Süre dolduðunda (00:00) çaðrýlýr.
    /// </summary>
    private void TimeUp()
    {
        if (!isRoundActive) return;
        isRoundActive = false;

        // (Eski timerTextUI yerine yeni digitalTimer_Text'i güncelle)
        digitalTimer_Text.text = "SÜRE DOLDU!";
        Debug.Log("Süre doldu! Oylama paneli açýlýyor...");
        ShowVotingPanel();
    }

    /// <summary>
    /// Oylama panelini (isim listesi) gösterir.
    /// </summary>
    private void ShowVotingPanel()
    {
        isRoundActive = false;
        startVoteButtonUI.interactable = false;
        votingPanelUI.SetActive(true);

        // (Paneli temizle)
        foreach (Transform child in votingPanelContainer)
        {
            Destroy(child.gameObject);
        }

        // (Paneli 3 yarýþmacý (Bilen Kiþi/Sahtekârlar) ile doldur)
        foreach (int playerIndex in GameData.ContestantIndices)
        {
            GameObject newVoteButton = Instantiate(playerVoteButtonPrefab, votingPanelContainer);
            PlayerVoteButton buttonScript = newVoteButton.GetComponent<PlayerVoteButton>();
            if (buttonScript != null)
            {
                // (Ýsimleri 'BÝLEN KÝÞÝ' deðil, normal isimleri yaz)
                buttonScript.Setup(GameData.PlayerNames[playerIndex], playerIndex);
            }
        }
    }


    // === KARAR VE PUANLAMA ÝÞLEMLERÝ (Temiz Hali) ===

    /// <summary>
    /// PlayerVoteButton'dan çaðrýlýr. Jüri'nin kararý.
    /// </summary>
    public void OnJuryDecision(int selectedPlayerGlobalIndex)
    {
        foreach (Transform child in votingPanelContainer)
        {
            child.GetComponent<PlayerVoteButton>()?.Lock();
        }

        // === KARAR KONTROLÜ ===
        if (selectedPlayerGlobalIndex == GameData.RealPlayerIndex) // (Bilen Kiþi'yi buldu)
        {
            // === SENARYO 1: JÜRÝ DOÐRU BÝLDÝ ===
            int juryIndex = GameData.CurrentJuryIndex;
            int realPlayerIndex = GameData.RealPlayerIndex;
            GameData.PlayerScores[juryIndex] += 2;
            GameData.PlayerScores[realPlayerIndex] += 2;
            Debug.Log($"JÜRÝ DOÐRU BÝLDÝ! Jüri +2, Bilen Kiþi +2");

            FinishRoundAndLoadNext();
        }
        else
        {
            // === SENARYO 2: JÜRÝ KANDIRILDI (Sahtekârý seçti) ===
            winningFakePlayerIndex = selectedPlayerGlobalIndex;

            // Pürüz 2: Doðaçlama bonus panelini göster
            ShowImprovBonusPanel();
        }
    }

    /// <summary>
    /// (Pürüz 2) Bonus Oylama Panelini gösterir.
    /// </summary>
    private void ShowImprovBonusPanel()
    {
        votingPanelUI.SetActive(false);
        improvBonusPanelUI.SetActive(true);

        string winnerName = GameData.PlayerNames[winningFakePlayerIndex];
        string safeLie1 = GameData.CurrentQuestion.sahteCevap1;
        string safeLie2 = GameData.CurrentQuestion.sahteCevap2;
        bonusInfoTextUI.text = $"{winnerName} (Sahtekâr) kazandý!\n\nHangi yalaný kullandý?";
        safeBetButtonUI.GetComponentInChildren<TextMeshProUGUI>().text = $"Sistem Yalaný (+3 Puan)\n({safeLie1} VEYA {safeLie2})";
        improvButtonUI.GetComponentInChildren<TextMeshProUGUI>().text = "Kendi Doðaçlamasý (+4 Puan)";

        safeBetButtonUI.onClick.RemoveAllListeners();
        safeBetButtonUI.onClick.AddListener(OnBonusVoteSafe);
        improvButtonUI.onClick.RemoveAllListeners();
        improvButtonUI.onClick.AddListener(OnBonusVoteImprov);
    }

    private void OnBonusVoteSafe()
    {
        AwardBonusPoints(3);
    }

    private void OnBonusVoteImprov()
    {
        AwardBonusPoints(4);
    }

    /// <summary>
    /// (Pürüz 2) Puaný verir ve turu bitirir.
    /// </summary>
    private void AwardBonusPoints(int amount)
    {
        improvBonusPanelUI.SetActive(false);
        GameData.PlayerScores[winningFakePlayerIndex] += amount;
        Debug.Log($"JÜRÝ KANDIRILDI! Sahtekâr ({GameData.PlayerNames[winningFakePlayerIndex]}) +{amount} Puan kazandý!");

        FinishRoundAndLoadNext();
    }

    /// <summary>
    /// (Pürüz 4) Turu bitirir, sayacý artýrýr ve sonraki sahneyi yükler.
    /// </summary>
    private void FinishRoundAndLoadNext()
    {
        GameData.CurrentRoundNumber++;
        float waitTime = 5.0f; // Sonucu görmeleri için bekleme

        if (GameData.CurrentRoundNumber >= GameData.TotalRounds)
        {
            Debug.Log("OYUN BÝTTÝ! Puan ekranýna (ScoreScene) gidiliyor...");
            Invoke("LoadScoreScene", waitTime);
        }
        else
        {
            Debug.Log($"Tur {GameData.CurrentRoundNumber + 1} baþlýyor...");
            Invoke("LoadNextRound", waitTime);
        }
    }

    // 'Invoke' ile çaðrýlabilmesi için 'public' olmalý
    public void LoadNextRound()
    {
        SceneManager.LoadScene("RoleRevealScene");
    }

    // 'Invoke' ile çaðrýlabilmesi için 'public' olmalý
    public void LoadScoreScene()
    {
        SceneManager.LoadScene("ScoreScene");
    }


    // === KAOS TURU FONKSÝYONLARI ===

    // --- Hýzlý Atýþ Fonksiyonlarý ---
    private void StartHizliAtisSequence()
    {
        isHizliAtisActive = false;
        isRoundActive = false;

        // (Ana UI'ý gizle - Artýk 'ApplyChaosRule' içinde yapýlýyor)

        hizliAtisPanelUI.SetActive(true);
        hizliAtisContestantIndex = 0;
        GoToNextHizliAtisTurn();
    }
    private void GoToNextHizliAtisTurn()
    {
        if (hizliAtisContestantIndex >= GameData.ContestantIndices.Count)
        {
            Debug.Log("Hýzlý Atýþ bitti. Oylama paneli açýlýyor...");
            hizliAtisPanelUI.SetActive(false);
            questionTextUI.gameObject.SetActive(true); // (Soruyu geri getir)
            ShowVotingPanel();
        }
        else
        {
            int playerIndex = GameData.ContestantIndices[hizliAtisContestantIndex];
            string playerName = GameData.PlayerNames[playerIndex];
            hizliAtisInfoTextUI.text = $"Sýra sende {playerName}!\n\nSavunman için 30 saniyen baþladý!";
            hizliAtisRemainingTime = 30f;
            UpdateHizliAtisTimerUI();
            isHizliAtisActive = true;
            hizliAtisContestantIndex++;
        }
    }
    private void UpdateHizliAtisTimerUI()
    {
        if (hizliAtisRemainingTime < 0) hizliAtisRemainingTime = 0;
        float minutes = Mathf.FloorToInt(hizliAtisRemainingTime / 60);
        float seconds = Mathf.FloorToInt(hizliAtisRemainingTime % 60);
        hizliAtisTimerTextUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // --- Cevap Deðiþimi Fonksiyonlarý ---
    private void PerformCevapDegisimi()
    {
        isRoundActive = false;
        Debug.Log("--- CEVAP DEÐÝÞÝMÝ (RASTGELE) BAÞLADI! ---");

        List<int> contestants = GameData.ContestantIndices;
        Dictionary<int, string> oldAnswers = new Dictionary<int, string>(GameData.PlayerAnswers);
        int oldRealIndex = GameData.RealPlayerIndex; // (Eski Bilen Kiþi)

        List<int> shuffledOldIndices = contestants.OrderBy(a => Random.value).ToList();

        string[] ilanMetinleri = new string[3];
        int newRealIndex = -1;

        for (int i = 0; i < contestants.Count; i++)
        {
            int aliciIndex = contestants[i];
            int vericiIndex = shuffledOldIndices[i];

            GameData.PlayerAnswers[aliciIndex] = oldAnswers[vericiIndex];

            if (vericiIndex == oldRealIndex)
            {
                newRealIndex = aliciIndex; // (Yeni Bilen Kiþi bu oldu)
            }

            ilanMetinleri[i] = $"{GameData.PlayerNames[aliciIndex]} -> {GameData.PlayerNames[vericiIndex]}'nin cevabýný aldý!";
        }

        GameData.RealPlayerIndex = newRealIndex;
        Debug.Log($"ROLLER YENÝDEN DAÐITILDI! YENÝ Bilen Kiþi: {GameData.PlayerNames[GameData.RealPlayerIndex]}");

        StartCoroutine(ShowCevapDegisimiPanel(ilanMetinleri));
    }
    private IEnumerator ShowCevapDegisimiPanel(string[] ilanMetinleri)
    {
        cevapDegisimiPanelUI.SetActive(true);
        if (cevapDegisimiTextsUI.Length >= 3)
        {
            cevapDegisimiTextsUI[0].text = ilanMetinleri[0];
            cevapDegisimiTextsUI[1].text = ilanMetinleri[1];
            cevapDegisimiTextsUI[2].text = ilanMetinleri[2];
        }

        yield return new WaitForSeconds(5.0f);

        cevapDegisimiPanelUI.SetActive(false);
        Debug.Log("Kaos devam ediyor! Kalan 45 saniye baþladý.");
        isRoundActive = true;
    }
}