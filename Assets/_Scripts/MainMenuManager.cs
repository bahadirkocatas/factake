using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Button için
using UnityEngine.SceneManagement; // Sahne yönetimi için

/// <summary>
/// Ana Menü (MainMenuScene) sahnesinin beynidir.
/// (TEMÝZLENMÝÞ HALÝ)
/// Artýk sadece ana butonlarý (Oyna, Ayarlar, Çýkýþ vb.) yönetir
/// ve ilgili panelleri (Nasýl Oynanýr, Ayarlar) açýp kapatýr.
/// Ýsim giriþi ve GameData kurulumu 'PlayerInputScene'e taþýndý.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Ana Menü Butonlarý")]
    [SerializeField] private Button playSingleDeviceButton; // "Oyna - Tek Cihaz"
    [SerializeField] private Button playOnlineButton;       // "Oyna - Online (Yakýnda)"
    [SerializeField] private Button howToPlayButton;        // "Nasýl Oynanýr?"
    [SerializeField] private Button settingsButton;         // "Ayarlar"
    [SerializeField] private Button exitButton;             // "Çýkýþ"

    [Header("Ana Menü Panelleri")]
    [Tooltip("Ana butonlarýn olduðu panel (Panel_MainButtons)")]
    [SerializeField] private GameObject mainButtonsPanel;
    [Tooltip("Nasýl Oynanýr kurallarýnýn olduðu panel (Panel_HowToPlay)")]
    [SerializeField] private GameObject howToPlayPanel;
    [Tooltip("Ses, dil vb. ayarlarýn olduðu panel (Panel_Settings)")]
    [SerializeField] private GameObject settingsPanel;
    [Header("Ayarlar Paneli UI (Ýçerik)")]
    [SerializeField] private Slider masterVolumeSlider; // Slider_MasterVolume


    // (NOT: Panel_HowToPlay ve Panel_Settings'in içine,
    // Unity Inspector'ýndan bu script'in 'ShowMainPanel()'
    // fonksiyonunu çaðýran "GERÝ" butonlarý eklemelisin.)

    void Start()
    {
        // 1. Gerekli referanslarý kontrol et
        if (playSingleDeviceButton == null || howToPlayButton == null || settingsButton == null || exitButton == null || mainButtonsPanel == null || howToPlayPanel == null || settingsPanel == null)
        {
            Debug.LogError("MainMenuManager'daki buton veya panel referanslarý EKSÝK! Lütfen Inspector'dan sürükleyin.");
            return;
        }

        // 2. Butonlara týklandýðýnda ne olacaðýný ayarla
        playSingleDeviceButton.onClick.AddListener(OnPlaySingleDevice);
        howToPlayButton.onClick.AddListener(OnHowToPlay);
        settingsButton.onClick.AddListener(OnSettings);
        exitButton.onClick.AddListener(OnExit);

        // (Online butonu zaten 'Interactable = false' (kilitli) olmalý)
        if (playOnlineButton != null)
        {
            playOnlineButton.onClick.AddListener(() => Debug.Log("Online Mod (Çok Yakýnda!) týklandý."));
        }

        // 3. Baþlangýçta panellerin doðru olduðundan emin ol
        // (Sadece Ana Menü butonlarýný göster)
        ShowPanel(mainButtonsPanel);
    }

    /// <summary>
    /// "OYNA - TEK CÝHAZ" butonuna basýldýðýnda çalýþýr.
    /// </summary>
    private void OnPlaySingleDevice()
    {
        Debug.Log("'Oyna - Tek Cihaz' týklandý. 'PlayerInputScene' yükleniyor...");

        // Oyuncu giriþi/kurulumu için bir sonraki sahneye geç
        // (Bu sahneyi bir sonraki adýmda "PlayerInputScene" adýyla oluþturacaðýz)
        SceneManager.LoadScene("PlayerInputScene");
    }

    /// <summary>
    /// "NASIL OYNANIR?" butonuna basýldýðýnda çalýþýr.
    /// </summary>
    private void OnHowToPlay()
    {
        Debug.Log("'Nasýl Oynanýr' týklandý.");
        ShowPanel(howToPlayPanel);
    }

    /// <summary>
    /// "AYARLAR" butonuna basýldýðýnda çalýþýr.
    /// </summary>
    private void OnSettings()
    {
        Debug.Log("'Ayarlar' týklandý.");

        // 1. Ayarlar panelini açmadan ÖNCE,
        // SettingsManager'dan mevcut sesi oku
        if (SettingsManager.Instance != null && masterVolumeSlider != null)
        {
            // Slider'ýn deðerini hafýzadaki deðere eþitle
            masterVolumeSlider.value = SettingsManager.Instance.currentMasterVolume;
        }

        // 2. Paneli göster
        ShowPanel(settingsPanel);
    }

    /// <summary>
    /// "GERÝ" butonlarý (Ayarlar/Nasýl Oynanýr panellerindeki) bunu çaðýrýr.
    /// BU 'PUBLIC' OLMALIDIR KÝ BUTON INSPECTOR'ÜNDEN SEÇÝLEBÝLSÝN.
    /// </summary>
    public void ShowMainPanel()
    {
        Debug.Log("Ana Menüye dönülüyor...");
        ShowPanel(mainButtonsPanel);
    }

    /// <summary>
    /// "ÇIKIÞ" butonuna basýldýðýnda çalýþýr.
    /// </summary>
    private void OnExit()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        // (Editor'de çalýþmaz, sadece build alýndýðýnda çalýþýr)
        Application.Quit();
    }

    /// <summary>
    //  Panel yönetimini kolaylaþtýran yardýmcý fonksiyon
    /// </summary>
    private void ShowPanel(GameObject panelToShow)
    {
        // Önce tüm ana panelleri gizle
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Sonra sadece istenen paneli göster
        if (panelToShow != null)
        {
            panelToShow.SetActive(true);
        }
    }
}