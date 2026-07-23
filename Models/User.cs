using System.ComponentModel.DataAnnotations;

namespace SifreYoneticiAPI.Models;

/// <summary>
/// Kullanici entity'si. Python'daki "Kullanicilar" tablosunun EF Core karsiligi.
///
/// ZERO-KNOWLEDGE NOTU:
/// - PublicKeyPem: duz metin saklanir, gizli DEGILDIR (sadece sifreleme icin kullanilir).
/// - EncryptedPrivateKeyPem: Frontend'de, kullanicinin Ana Sifresinden turetilen bir
///   AES anahtari ile ZATEN sifrelenmis olarak buraya gelir. Backend bu alanin
///   icerigini ASLA cozemez, cozmeye calismaz; sadece saklar ve login sonrasi
///   oldugu gibi geri dondurur. Cozme islemi SADECE tarayicida yapilir.
/// - MasterHash: Backend, kullanicinin gercek ana sifresini HICBIR ZAMAN gormez.
///   Frontend SHA-256(salt + anaSifre) hesaplayip bu degeri gonderir, backend
///   sadece bu hash'i saklar/karsilastirir.
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Frontend tarafinda SHA-256(salt + anaSifre) olarak hesaplanip gonderilir.
    /// GUVENLIK NOTU: Bu deger dogrudan "parola esdegeri" (password-equivalent)
    /// oldugu icin, ileride bu alanin backend tarafinda AYRICA bir maliyetli
    /// hash (ornegin BCrypt/Argon2) ile sarilarak saklanmasi onerilir; aksi halde
    /// veritabani sizintisinda bu deger "pass-the-hash" tarzi tekrar oynatma
    /// (replay) saldirisina acik olabilir. Bu, sonraki adimlarda ele alinabilir.
    /// </summary>
    [Required]
    public string MasterHash { get; set; } = string.Empty;

    [Required]
    public string Salt { get; set; } = string.Empty;

    [Required]
    public string PublicKeyPem { get; set; } = string.Empty;

    [Required]
    public string EncryptedPrivateKeyPem { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// BRUTE-FORCE KORUMASI: Ust uste yanlis Ana Sifre denemesi sayaci.
    /// Basarili giriste 0'a sifirlanir. 5'e ulasinca hesap kilitlenir.
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// NULL ise hesap kilitli DEGILDIR. Dolu ve gelecekteki bir tarihse,
    /// bu tarihe kadar dogru sifre girilse bile giris REDDEDILIR.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }

    // ---- Navigation property: bir kullanicinin birden fazla kasa kaydi olabilir ----
    public ICollection<VaultItem> VaultItems { get; set; } = new List<VaultItem>();
}