using System.Collections.Generic;

// TÜM ÖZEL TUR TÝPLERÝMÝZ (ENUM)
// (Ayýklanmýþ Hali)
public enum ChaosRoundType
{
    None, // Normal Tur (Kaos yok)
    YasakHarf,
    ZorunluKelime,
    CevapDegisimi,
    HizliAtis
}

/// <summary>
/// Bu statik sýnýf, sahnede "yaþamaz".
/// Oyunun tüm hafýzasýný sahneler arasýnda (MainMenu'den GameScene'e) taþýr.
/// </summary>
public static class GameData
{
    // === Oyuncu Bilgileri ===
    public static List<string> PlayerNames = new List<string>();
    public static Dictionary<int, int> PlayerScores = new Dictionary<int, int>();


    // === Rotasyon ve Tur Bilgileri ===
    public static List<int> JuryOrderList = new List<int>();
    public static int TotalRounds;
    public static int CurrentRoundNumber = 0;

    // === Kalýcý Soru Havuzu ===
    public static QuestionDatabase AllQuestions = null;
    public static List<QuestionData> RemainingQuestionsList = new List<QuestionData>();


    // === Mevcut Tur Verileri ===
    public static int CurrentJuryIndex;
    public static List<int> ContestantIndices = new List<int>();
    public static QuestionData CurrentQuestion;
    public static int RealPlayerIndex;
    public static Dictionary<int, string> PlayerAnswers = new Dictionary<int, string>();


    // === Kaos Turu Verileri ===
    public static List<ChaosRoundType> RoundRules = new List<ChaosRoundType>();
    public static ChaosRoundType CurrentChaosRound = ChaosRoundType.None;
    // (ZorunluKelime turu için seçilen kelimeyi tutar)
    public static string CurrentMandatoryWord;
}