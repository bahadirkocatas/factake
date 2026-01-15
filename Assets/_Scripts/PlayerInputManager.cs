using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button, Slider, Toggle
using TMPro; // TextMeshPro
using UnityEngine.SceneManagement; // Sahne yönetimi
using System.Linq; // OrderBy (karıştırma)

/// <summary>
/// 'PlayerInputScene' (Oyun Kurulum) sahnesinin beynidir.
/// (AYARLAR PANELİ ENTEGRE EDİLMİŞ HALİ)
/// </summary>
public class PlayerInputManager : MonoBehaviour
{
    [Header("Veritabanı")]
    [SerializeField] private TextAsset questionDatabaseJSON;

    [Header("Ana UI Referansları")]
    [SerializeField] private Transform playerInputsContainer;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button gameSettingsButton;

    [Header("Oyuncu Prefab")]
    [SerializeField] private GameObject playerInputPrefab;

    [Header("Ayarlar Paneli UI")]
    [SerializeField] private GameObject gameSettingsPopup;
    [SerializeField] private Button saveSettingsButton;
    [SerializeField] private Button defaultSettingsButton;

    [Header("Ayarlar: Oyuncu Sayısı")]
    [SerializeField] private Slider playerCountSlider;
    [SerializeField] private TextMeshProUGUI playerCountSliderText;
    [SerializeField] private TextMeshProUGUI premiumInfoText;

    [Header("Ayarlar: Kaos Turları")]
    [SerializeField] private Toggle chaosHizliAtisToggle;
    [SerializeField] private Toggle chaosYasakHarfToggle;
    [SerializeField] private Toggle chaosZorunluKelimeToggle;
    [SerializeField] private Toggle chaosCevapDegisimiToggle;

    [Header("Ayarlar: Genel")]
    [SerializeField] private Slider roundCountSlider;
    [SerializeField] private TextMeshProUGUI roundCountSliderText;
    [SerializeField] private Slider roundTimeSlider;
    [SerializeField] private TextMeshProUGUI roundTimeSliderText;

    // Ayarların mevcut (kayıtlı) değerlerini tutan değişkenler
    private int playerCount = 4;
    private int totalRounds = 8;
    private int roundTime = 90;
    private List<ChaosRoundType> enabledChaosTypes = new List<ChaosRoundType> {
        ChaosRoundType.HizliAtis, ChaosRoundType.YasakHarf,
        ChaosRoundType.ZorunluKelime, ChaosRoundType.CevapDegisimi
    };

    // Toplanan isimleri ve input field'ları tutan listeler
    private List<string> playerNames = new List<string>();
    private List<TMP_InputField> playerInputFields = new List<TMP_InputField>();

    void Start()
    {
        // 1. Gerekli referansları kontrol et
        // (Tüm yeni slider'lar ve toggle'lar için)
        if (questionDatabaseJSON == null || playerInputsContainer == null || startGameButton == null || backButton == null || gameSettingsButton == null || playerInputPrefab == null || gameSettingsPopup == null ||
            playerCountSlider == null || premiumInfoText == null || chaosHizliAtisToggle == null || roundCountSlider == null)
        {
            Debug.LogError("PlayerInputManager'da referanslar EKSİK! Lütfen Inspector'u kontrol edin.");
            return;
        }

        // 2. Ana Butonları ayarla
        startGameButton.onClick.AddListener(OnStartGame);
        backButton.onClick.AddListener(OnBack);
        gameSettingsButton.onClick.AddListener(ShowSettingsPanel);

        // 3. Ayarlar Paneli Butonlarını ayarla
        saveSettingsButton.onClick.AddListener(OnSaveSettings);
        defaultSettingsButton.onClick.AddListener(OnDefaultSettings);

        // 4. Ayarlar Paneli Slider'larını ayarla (Değiştikçe metni güncellesinler)
        playerCountSlider.onValueChanged.AddListener(OnPlayerCountSliderChanged);
        roundCountSlider.onValueChanged.AddListener(OnRoundCountSliderChanged);
        roundTimeSlider.onValueChanged.AddListener(OnRoundTimeSliderChanged);

        // 5. Başlangıçta panelleri ve UI'ı kur
        gameSettingsPopup.SetActive(false); // Ayarlar panelini gizle
        LoadSettingsToUI(); // Ayarları UI'a yükle (Varsayılanları ayarla)
        SetupPlayerInputFields(playerCount); // 4 oyuncuyla başla
    }

    /// <summary>
    /// Varsayılan ayarları yükler ve UI'ı günceller.
    /// </summary>
    private void LoadSettingsToUI()
    {
        // (Bu fonksiyon 'playerCount', 'totalRounds' gibi değişkenlerden
        // ayarları okur ve UI'daki slider/toggle'ları ayarlar)

        playerCountSlider.value = playerCount;
        OnPlayerCountSliderChanged(playerCount); // (Bu, '4 Kişi' metnini yazar)

        roundCountSlider.value = totalRounds;
        OnRoundCountSliderChanged(totalRounds); // ('8 Tur' metnini yazar)

        roundTimeSlider.value = roundTime;
        OnRoundTimeSliderChanged(roundTime); // ('90 Saniye' metnini yazar)

        chaosHizliAtisToggle.isOn = enabledChaosTypes.Contains(ChaosRoundType.HizliAtis);
        chaosYasakHarfToggle.isOn = enabledChaosTypes.Contains(ChaosRoundType.YasakHarf);
        chaosZorunluKelimeToggle.isOn = enabledChaosTypes.Contains(ChaosRoundType.ZorunluKelime);
        chaosCevapDegisimiToggle.isOn = enabledChaosTypes.Contains(ChaosRoundType.CevapDegisimi);

        premiumInfoText.gameObject.SetActive(false);
        startGameButton.interactable = true;
    }

    /// <summary>
    /// Verilen oyuncu sayısı kadar 'PlayerInput_Prefab'ı ekrana yaratır.
    /// </summary>
    private void SetupPlayerInputFields(int count)
    {
        foreach (Transform child in playerInputsContainer) { Destroy(child.gameObject); }
        playerInputFields.Clear();
        for (int i = 0; i < count; i++)
        {
            GameObject newField = Instantiate(playerInputPrefab, playerInputsContainer);
            TMP_InputField inputField = newField.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                playerInputFields.Add(inputField);
                // Eğer 5. veya 6. (Premium) oyuncuysa, alanı kilitle
                if (i >= 4)
                {
                    inputField.interactable = false;
                    // (Placeholder'ı "Premium Gerekli" yapabiliriz)
                }
            }
        }
    }

    #region Ayarlar Paneli Fonksiyonları

    private void ShowSettingsPanel()
    {
        // Ayarlar panelini açmadan önce, UI'ın mevcut kayıtlı ayarları
        // gösterdiğinden emin ol (eğer "Kaydet"e basmadan kapattıysa)
        LoadSettingsToUI();
        gameSettingsPopup.SetActive(true);
    }

    /// <summary>
    /// Ayarlar panelindeki "KAYDET" butonuna basıldığında çağrılır.
    /// </summary>
    public void OnSaveSettings()
    {
        // 1. Değerleri UI'dan oku ve kaydet
        playerCount = (int)playerCountSlider.value;
        totalRounds = (int)roundCountSlider.value;
        roundTime = (int)roundTimeSlider.value;

        // 2. Kaos listesini güncelle
        enabledChaosTypes.Clear();
        if (chaosHizliAtisToggle.isOn) enabledChaosTypes.Add(ChaosRoundType.HizliAtis);
        if (chaosYasakHarfToggle.isOn) enabledChaosTypes.Add(ChaosRoundType.YasakHarf);
        if (chaosZorunluKelimeToggle.isOn) enabledChaosTypes.Add(ChaosRoundType.ZorunluKelime);
        if (chaosCevapDegisimiToggle.isOn) enabledChaosTypes.Add(ChaosRoundType.CevapDegisimi);

        // 3. İsim giriş alanlarını yeni oyuncu sayısına göre güncelle
        SetupPlayerInputFields(playerCount);

        // 4. Paneli kapat
        gameSettingsPopup.SetActive(false);
    }

    /// <summary>
    /// Ayarlar panelindeki "VARSAYILANA DÖN" butonuna basıldığında çağrılır.
    /// </summary>
    public void OnDefaultSettings()
    {
        // 1. Ayarları varsayılana sıfırla
        playerCount = 4;
        totalRounds = 8;
        roundTime = 90;
        enabledChaosTypes = new List<ChaosRoundType> {
            ChaosRoundType.HizliAtis, ChaosRoundType.YasakHarf,
            ChaosRoundType.ZorunluKelime, ChaosRoundType.CevapDegisimi
        };

        // 2. UI'ı da sıfırla
        LoadSettingsToUI();
    }

    /// <summary>
    /// Oyuncu Sayısı slider'ı değiştiğinde çağrılır.
    /// </summary>
    public void OnPlayerCountSliderChanged(float value)
    {
        int count = (int)value;
        playerCountSliderText.text = $"Oyuncu Sayısı: {count}";

        // Premium Kilidi
        if (count > 4)
        {
            premiumInfoText.gameObject.SetActive(true);
            startGameButton.interactable = false; // Ana "BAŞLAT" butonunu kilitle
        }
        else
        {
            premiumInfoText.gameObject.SetActive(false);
            startGameButton.interactable = true; // "BAŞLAT" butonunu aç
        }

        // (NOT: 'SetupPlayerInputFields'i burada çağırmıyoruz,
        // o sadece "Kaydet"e basınca çalışsın ki daha stabil olsun)
    }

    /// <summary>
    /// Tur Sayısı slider'ı değiştiğinde çağrılır.
    /// </summary>
    public void OnRoundCountSliderChanged(float value)
    {
        roundCountSliderText.text = $"Toplam Tur Sayısı: {(int)value}";
    }

    /// <summary>
    /// Tur Süresi slider'ı değiştiğinde çağrılır.
    /// </summary>
    public void OnRoundTimeSliderChanged(float value)
    {
        roundTimeSliderText.text = $"Tur Süresi: {(int)value} Saniye";
    }

    #endregion

    /// <summary>
    /// "GERİ" butonuna basıldığında çağrılır.
    /// </summary>
    private void OnBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// "BAŞLAT" butonuna basıldığında çağrılır.
    /// GameData'yı artık bu paneldeki *kayıtlı ayarlara* göre kurar.
    /// </summary>
    private void OnStartGame()
    {
        // 1. İsimleri Topla
        playerNames.Clear();
        foreach (TMP_InputField inputField in playerInputFields)
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                playerNames.Add(inputField.text.Trim());
            }
        }

        // 2. Oyuncu Sayısını Kontrol Et (Mevcut 'playerCount' ayarına göre)
        if (playerNames.Count < playerCount)
        {
            Debug.LogWarning($"Devam etmek için {playerCount} oyuncu ismi gerekli!");
            return;
        }

        Debug.Log("Oyun Kuruluyor... GameData dolduruluyor...");

        // 3. GAMEDATA'YI KUR: Oyuncu Bilgileri
        GameData.PlayerNames = playerNames;

        // 4. GAMEDATA'YI KUR: Puanlar
        GameData.PlayerScores.Clear();
        for (int i = 0; i < playerCount; i++) { GameData.PlayerScores[i] = 0; }

        // 5. GAMEDATA'YI KUR: Tur ve Jüri Rotasyonu (AYARLARDAN)
        GameData.TotalRounds = this.totalRounds; // (Artık 8 değil, slider'dan)
        GameData.CurrentRoundNumber = 0;
        GameData.JuryOrderList.Clear();
        List<int> initialOrder = new List<int>();
        for (int i = 0; i < playerCount; i++) { initialOrder.Add(i); }
        GameData.JuryOrderList = initialOrder.OrderBy(a => Random.value).ToList();

        // 6. GAMEDATA'YI KUR: Soru Havuzu
        GameData.AllQuestions = JsonUtility.FromJson<QuestionDatabase>(questionDatabaseJSON.text);
        GameData.RemainingQuestionsList.Clear();
        GameData.RemainingQuestionsList = new List<QuestionData>(GameData.AllQuestions.questionList);

        // 7. GAMEDATA'YI KUR: Kaos Turları (AYARLARDAN)
        GameData.RoundRules.Clear();

        // Kaos havuzunu, Ayarlar'da 'isOn' olanlardan oluştur
        List<ChaosRoundType> chaosPool = new List<ChaosRoundType>(this.enabledChaosTypes);

        // (Eğer oyuncu HİÇ kaos turu seçmediyse, 'None' ile doldur)
        if (chaosPool.Count == 0) chaosPool.Add(ChaosRoundType.None);

        // 8 turluk oyunda (varsayılan) 1'i Normal, 7'si potansiyel kaos
        // (Eğer 'totalRounds' 8 ise, 4 kaos, 3 normal)
        int normalTurSayisi = (GameData.TotalRounds / 2) - 1; // (8/2)-1 = 3
        int kaosTurSayisi = GameData.TotalRounds - normalTurSayisi - 1; // 8 - 3 - 1 = 4

        List<ChaosRoundType> remainingRounds = new List<ChaosRoundType>();
        for (int i = 0; i < normalTurSayisi; i++)
        {
            remainingRounds.Add(ChaosRoundType.None);
        }
        for (int i = 0; i < kaosTurSayisi; i++)
        {
            // Havuzdan rastgele seç (havuz biterse tekrar 'None' ekle)
            remainingRounds.Add(chaosPool[i % chaosPool.Count]);
        }

        remainingRounds = remainingRounds.OrderBy(a => Random.value).ToList();

        GameData.RoundRules.Add(ChaosRoundType.None); // İlk tur her zaman normal
        GameData.RoundRules.AddRange(remainingRounds);

        Debug.Log($"KURULUM BİTTİ! Toplam Tur: {GameData.TotalRounds}. Kaos Kuralları: " + string.Join(", ", GameData.RoundRules));

        // 8. Sonraki Sahneye Geç
        SceneManager.LoadScene("RoleRevealScene");
    }
}