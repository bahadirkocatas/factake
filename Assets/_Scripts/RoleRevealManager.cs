using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Rol Daðýtým (RoleRevealScene) sahnesinin beynidir.
/// (YENÝ ÝSÝMLER ve YENÝ UI TASARIMI ile güncellendi)
/// </summary>
public class RoleRevealManager : MonoBehaviour
{
    [Header("UI Elemanlarý (Yeni Tasarým)")]
    [SerializeField] private Image iconImageUI; // Image_Icon
    [SerializeField] private TextMeshProUGUI roleNameTextUI; // Text_RoleName
    [SerializeField] private TextMeshProUGUI questionTextUI; // Text_Question
    [SerializeField] private TextMeshProUGUI answerTextUI; // Text_Answer

    [SerializeField] private Button continueButtonUI;
    [SerializeField] private TextMeshProUGUI continueButtonTextUI;

    [Header("Rol Renkleri")]
    [SerializeField] private Color juryColor = new Color(0.94f, 0.76f, 0.05f); // Sarý (#FFDD51)
    [SerializeField] private Color realPlayerColor = new Color(0.18f, 0.8f, 0.44f); // Yeþil (#2ECC71)
    [SerializeField] private Color fakePlayerColor = new Color(0.9f, 0.3f, 0.23f); // Kýrmýzý (#E74C3C)
    [SerializeField] private Color defaultColor = new Color(0.92f, 0.94f, 0.94f); // Beyaz (#ECF0F1)

    // (Eðer ikonlarý (Sprite) deðiþtirmek istersek buraya ekleriz)
    // [SerializeField] private Sprite juryIcon;
    // [SerializeField] private Sprite realIcon;
    // [SerializeField] private Sprite fakeIcon;

    private List<int> revealOrder = new List<int>();
    private int currentPlayerIndexInOrder = 0;
    private bool isShowingRole = false;


    void Start()
    {
        // (Tüm yeni UI referanslarýný kontrol et)
        if (iconImageUI == null || roleNameTextUI == null || questionTextUI == null || answerTextUI == null || continueButtonUI == null || continueButtonTextUI == null)
        {
            Debug.LogError("RoleRevealManager UI referanslarý EKSÝK! Lütfen Inspector'dan sürükleyin.");
            return;
        }

        continueButtonUI.onClick.AddListener(OnContinuePressed);

        // (Test verisi yükleme)
        if (GameData.PlayerNames.Count == 0)
        {
            // (Test verisi yükleme kodlarý...)
        }

        SetupNewRound();
        StartRoleRevealSequence();
    }

    /// <summary>
    /// (Bu fonksiyon 'ZorunluKelime' filtrelemesi dahil ayný kaldý)
    /// </summary>
    private void SetupNewRound()
    {
        // === 1. JÜRÝ'YÝ SEÇ ===
        int playerCount = GameData.PlayerNames.Count;
        int juryOrderIndex = GameData.CurrentRoundNumber % playerCount;
        GameData.CurrentJuryIndex = GameData.JuryOrderList[juryOrderIndex];

        // === 2. YARIÞMACILARI AYIR ===
        GameData.ContestantIndices.Clear();
        for (int i = 0; i < playerCount; i++)
        {
            if (i != GameData.CurrentJuryIndex) { GameData.ContestantIndices.Add(i); }
        }

        // === 3. KAOS KURALINI ATA ===
        GameData.CurrentChaosRound = GameData.RoundRules[GameData.CurrentRoundNumber];

        // === 4. SORUYU SEÇ (Zorunlu Kelime Filtreleme Mantýðý) ===
        List<QuestionData> questionPool = GameData.RemainingQuestionsList;
        List<QuestionData> filteredPool;

        if (GameData.CurrentChaosRound == ChaosRoundType.ZorunluKelime)
            filteredPool = questionPool.Where(q => !string.IsNullOrEmpty(q.zorunluKelime)).ToList();
        else
            filteredPool = questionPool.Where(q => string.IsNullOrEmpty(q.zorunluKelime)).ToList();

        if (filteredPool.Count == 0)
        {
            // (Hata yönetimi: Ana listeyi yeniden doldur ve tekrar filtrele...)
            GameData.RemainingQuestionsList = new List<QuestionData>(GameData.AllQuestions.questionList);
            questionPool = GameData.RemainingQuestionsList;
            if (GameData.CurrentChaosRound == ChaosRoundType.ZorunluKelime)
                filteredPool = questionPool.Where(q => !string.IsNullOrEmpty(q.zorunluKelime)).ToList();
            else
                filteredPool = questionPool.Where(q => string.IsNullOrEmpty(q.zorunluKelime)).ToList();
            if (filteredPool.Count == 0)
            {
                Debug.LogError($"KRÝTÝK HATA: Veritabanýnda '{GameData.CurrentChaosRound}' tipine uygun HÝÇ soru bulunamadý! Normal bir soru seçiliyor.");
                filteredPool = questionPool;
            }
        }
        int randomIndex = Random.Range(0, filteredPool.Count);
        GameData.CurrentQuestion = filteredPool[randomIndex];
        GameData.RemainingQuestionsList.Remove(GameData.CurrentQuestion);
        if (GameData.CurrentChaosRound == ChaosRoundType.ZorunluKelime)
        {
            GameData.CurrentMandatoryWord = GameData.CurrentQuestion.zorunluKelime;
        }

        // === 5. BÝLEN KÝÞÝ'YÝ SEÇ ===
        int randomContestantIndex = Random.Range(0, GameData.ContestantIndices.Count);
        GameData.RealPlayerIndex = GameData.ContestantIndices[randomContestantIndex];

        // === 6. CEVAPLARI HAZIRLA VE ROLLERÝ ATA ===
        GameData.PlayerAnswers.Clear();
        List<string> fakeAnswers = new List<string> { GameData.CurrentQuestion.sahteCevap1, GameData.CurrentQuestion.sahteCevap2 };
        fakeAnswers = fakeAnswers.OrderBy(a => Random.value).ToList();
        int fakeAnswerCounter = 0;
        foreach (int playerIndex in GameData.ContestantIndices)
        {
            if (playerIndex == GameData.RealPlayerIndex)
                GameData.PlayerAnswers[playerIndex] = GameData.CurrentQuestion.gercekCevap;
            else
            {
                GameData.PlayerAnswers[playerIndex] = fakeAnswers[fakeAnswerCounter];
                fakeAnswerCounter++;
            }
        }

        Debug.Log($"--- YENÝ TUR KURULDU ---");
        Debug.Log($"KAOS KURALI: {GameData.CurrentChaosRound}");
        Debug.Log($"Jüri: {GameData.PlayerNames[GameData.CurrentJuryIndex]}");
        Debug.Log($"Bilen Kiþi: {GameData.PlayerNames[GameData.RealPlayerIndex]}");
    }

    /// <summary>
    /// "Paslaþma" arayüzünü baþlatan ana fonksiyondur.
    /// </summary>
    private void StartRoleRevealSequence()
    {
        revealOrder.Clear();
        revealOrder.Add(GameData.CurrentJuryIndex);
        revealOrder.AddRange(GameData.ContestantIndices);

        currentPlayerIndexInOrder = 0;
        isShowingRole = false;
        ShowInstruction();
    }

    /// <summary>
    /// Butona her basýldýðýnda bu fonksiyon çalýþýr.
    /// </summary>
    private void OnContinuePressed()
    {
        if (isShowingRole)
        {
            currentPlayerIndexInOrder++;
            if (currentPlayerIndexInOrder >= revealOrder.Count)
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                ShowInstruction();
            }
        }
        else
        {
            ShowRole();
        }
        isShowingRole = !isShowingRole;
    }

    /// <summary>
    /// YENÝ UI: "Telefonu [Oyuncu Adý]'na verin" ekranýný gösterir.
    /// (Bu ekran için tüm UI elemanlarýný gizleriz)
    /// </summary>
    private void ShowInstruction()
    {
        int globalPlayerIndex = revealOrder[currentPlayerIndexInOrder];
        string playerName = GameData.PlayerNames[globalPlayerIndex];
        int currentScore = GameData.PlayerScores[globalPlayerIndex];

        // 1. Ana rol UI'ýný gizle
        iconImageUI.gameObject.SetActive(false);
        roleNameTextUI.gameObject.SetActive(false);
        questionTextUI.gameObject.SetActive(false);

        // 2. Talimatlarý 'answerTextUI' (en büyük metin alaný) içinde göster
        answerTextUI.gameObject.SetActive(true);
        answerTextUI.alignment = TextAlignmentOptions.Center;
        answerTextUI.color = defaultColor; // Rengi normale döndür
        answerTextUI.text = $"Telefonu {playerName}'a verin.\n\nMevcut Puan: {currentScore}\n\n";

        // 3. Butonu ayarla
        continueButtonTextUI.text = "ROLÜMÜ GÖRMEK ÝÇÝN TIKLA";
        continueButtonUI.GetComponent<Image>().color = juryColor; // Sarý buton
    }

    /// <summary>
    /// YENÝ UI: O oyuncuya ait (Jüri, Bilen Kiþi, Sahtekâr) rol ekranýný gösterir.
    /// </summary>
    private void ShowRole()
    {
        // 1. Tüm UI elemanlarýný görünür yap
        iconImageUI.gameObject.SetActive(true);
        roleNameTextUI.gameObject.SetActive(true);
        questionTextUI.gameObject.SetActive(true);
        answerTextUI.gameObject.SetActive(true);
        answerTextUI.alignment = TextAlignmentOptions.Top; // Hizalamayý düzelt

        // 2. Metinleri hazýrla
        int globalPlayerIndex = revealOrder[currentPlayerIndexInOrder];
        string questionText = $"'{GameData.CurrentQuestion.soruMetni}'";
        string chaosWarning = ""; // Kaos Kuralý Bildirimi

        switch (GameData.CurrentChaosRound)
        {
            case ChaosRoundType.YasakHarf: chaosWarning = "KAOS: 'A' HARFÝ YASAK!\n"; break;
            case ChaosRoundType.ZorunluKelime: chaosWarning = $"KAOS: '{GameData.CurrentMandatoryWord}' DEMEK ZORUNLU!\n"; break;
        }

        // 3. Rolü belirle ve UI'ý ona göre RENKLENDÝR
        if (globalPlayerIndex == GameData.CurrentJuryIndex)
        {
            // === BU KÝÞÝ JÜRÝ ===
            roleNameTextUI.text = $"{chaosWarning}JÜRÝSÝN!";
            questionTextUI.text = questionText;
            answerTextUI.text = "Görevin, 'Sahiciyi Sahtekarlardan ayýrmak.";

            // Renk Kodlamasý (Sarý)
            iconImageUI.color = juryColor;
            roleNameTextUI.color = juryColor;
        }
        else
        {
            // === BU KÝÞÝ YARIÞMACI ===
            string answer = GameData.PlayerAnswers[globalPlayerIndex];

            if (globalPlayerIndex == GameData.RealPlayerIndex)
            {
                // === BÝLEN KÝÞÝ (Gerçek) ===
                roleNameTextUI.text = $"{chaosWarning}SAHÝCÝSÝN!";
                questionTextUI.text = questionText;
                answerTextUI.text = $"Cevabýn:\n'{answer}'\n\nBu gerçeði savun!";

                // Renk Kodlamasý (Yeþil)
                iconImageUI.color = realPlayerColor;
                roleNameTextUI.color = realPlayerColor;
            }
            else
            {
                // === SAHTEKÂR (Sahte) ===
                roleNameTextUI.text = $"{chaosWarning}SAHTEKARSIN!";
                questionTextUI.text = questionText;

                // Senin istediðin o "yalan üret" metni:
                answerTextUI.text = $"Kendin yalan üret (+4p) \n\n '{answer} (+3p)'";

                // Renk Kodlamasý (Kýrmýzý)
                iconImageUI.color = fakePlayerColor;
                roleNameTextUI.color = fakePlayerColor;
            }
        }

        // 4. Butonu ayarla
        continueButtonTextUI.text = "ANLADIM";
        continueButtonUI.GetComponent<Image>().color = realPlayerColor; // Yeþil buton (Onay)
    }
}