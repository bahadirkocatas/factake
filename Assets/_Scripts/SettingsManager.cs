using UnityEngine;
using UnityEngine.Audio; // AudioMixer için
using UnityEngine.UI; // Slider için

/// <summary>
/// Bu script, oyun boyunca yaþar (DontDestroyOnLoad).
/// Ses ayarlarýný yönetir ve cihazýn hafýzasýna (PlayerPrefs) kaydeder.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // === SINGLETON KURULUMU ===
    public static SettingsManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // === SINGLETON BÝTÝÞÝ ===

    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;

    // (Bu, 'Panel_Settings'teki Slider'ý çalýþtýrmak için)
    [Tooltip("Sadece Ayarlar panelindeki Slider'ý baþlatmak için")]
    public float currentMasterVolume; // (0.001f ile 1.0f arasý)

    // PlayerPrefs'te ayarý kaydetmek için kullanacaðýmýz anahtar
    private const string MASTER_VOLUME_KEY = "MasterVolume";

    void Start()
    {
        // Oyuncu oyunu açtýðýnda, kaydedilmiþ sesi yükle
        // (Eðer kayýt yoksa, varsayýlan olarak %100 (1.0f) olsun)
        currentMasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1.0f);

        // Bu yüklenen sesi anýnda AudioMixer'a uygula
        SetMasterVolume(currentMasterVolume);
    }

    /// <summary>
    /// Sesi ayarlar ve hafýzaya kaydeder.
    /// Ayarlar panelindeki SLIDER tarafýndan çaðrýlýr.
    /// </summary>
    /// <param name="volume">Slider'dan gelen 0.001f - 1.0f arasý deðer</param>
    public void SetMasterVolume(float volume)
    {
        if (mainAudioMixer == null) return;

        // Slider 0 olduðunda ses -80dB (sessiz) olur, 1 olduðunda 0dB (tam ses).
        // (Sesi logaritmik olarak ayarlamamýz gerekir)
        float dbVolume = Mathf.Log10(volume) * 20;

        // Slider'ýn 0'a inmesine izin veriyorsak (tamamen sessiz)
        if (volume <= 0.001f)
        {
            dbVolume = -80f; // Minimum (sessiz)
        }

        mainAudioMixer.SetFloat("MasterVolume", dbVolume);

        // Bu ayarý cihaz hafýzasýna kaydet
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);

        // Mevcut deðeri de güncelle
        currentMasterVolume = volume;
    }
}