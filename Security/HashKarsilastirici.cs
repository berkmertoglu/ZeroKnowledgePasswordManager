using System.Security.Cryptography;
using System.Text;

namespace SifreYoneticiAPI.Security;

public class HashKarsilastirici : IHashKarsilastirici
{
    public bool Esit(string dbHash, string gelenHash)
    {
        var dbBytes = Encoding.UTF8.GetBytes(dbHash);
        var gelenBytes = Encoding.UTF8.GetBytes(gelenHash);

        // Farkli uzunluktaysa dogrudan false (FixedTimeEquals esit uzunluk ister).
        // NOT: Bu uzunluk kontrolu teorik olarak cok kucuk bir zamanlama sizintisi
        // olusturabilir, ama SHA-256 hex her zaman sabit (64 karakter) uzunlukta
        // oldugu icin pratikte bir risk tasimaz.
        if (dbBytes.Length != gelenBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(dbBytes, gelenBytes);
    }
}