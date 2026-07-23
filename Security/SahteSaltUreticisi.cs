using System.Security.Cryptography;
using System.Text;

namespace SifreYoneticiAPI.Security;

public class SahteSaltUreticisi : ISahteSaltUreticisi
{
    // NOT: Gercek bir projede bu degeri appsettings.json / ortam degiskenine
    // tasiyin (ornegin IConfiguration ile enjekte edilerek), kod icine
    // gomulu sabit deger olarak birakmayin.
    private const string SahteTuretmeAnahtari = "BURAYA_FARKLI_VE_GIZLI_BIR_SUNUCU_ANAHTARI_YAZIN";

    public string Uret(string username)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SahteTuretmeAnahtari));
        var sahteSaltBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(username));
        return Convert.ToHexString(sahteSaltBytes)[..32].ToLowerInvariant();
    }
}