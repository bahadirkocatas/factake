

[System.Serializable] // Bu satýr, Unity'nin bu sýnýfý JSON'dan okuyabilmesi için þarttýr.
public class QuestionData
{
    public string soruMetni;
    public string gercekCevap;
    public string sahteCevap1; // Güvenli Að Yalaný 1
    public string sahteCevap2; // Güvenli Að Yalaný 2

    // === YENÝ (ZORUNLU KELÝME TURU ÝÇÝN) ===
    // Eðer bu soru 'ZorunluKelime' turu içinse, buraya "MAVÝ" gibi bir kelime yaz.
    // Normal sorular için bu alaný boþ býrak (veya hiç ekleme).
    public string zorunluKelime;
}