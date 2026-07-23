using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SifreYoneticiAPI.Models;

/// <summary>
/// Loglanan guvenlik olayi turleri. DbContext'te string olarak saklanir
/// (HasConversion&lt;string&gt;()) ki veritabani elle incelendiginde
/// okunabilir olsun.
/// </summary>
public enum GuvenlikOlayTuru
{
    GirisBasarili,
    GirisBasarisiz,
    HesapKilitlendi,
    SifreEklendi,
    SifreSilindi
}

/// <summary>
/// AUDIT LOG (Guvenlik Kaydi) tablosu. Kullanicinin hesabiyla ilgili
/// onemli guvenlik olaylarinin degismez (immutable) kaydini tutar.
///
/// UserId NEDEN NULLABLE: Basarisiz bir giris denemesinde, girilen
/// kullanici adi sistemde HIC OLMAYABILIR (ornegin var olmayan bir
/// kullanici adiyla deneme yapilmis olabilir). Bu durumda gercek bir
/// UserId'ye baglayamayiz, ama yine de olayi (hangi kullanici adi
/// denendi bilgisiyle, Details alaninda) loglamak guvenlik acisindan
/// degerlidir.
///
/// Kullanici hesabi SILINSE bile bu loglar SILINMEZ (bkz. DbContext'teki
/// SetNull kurali) -- audit kayitlari, hesabin kendisinden bagimsiz
/// olarak saklanmaya devam eder.
/// </summary>
public class SecurityLog
{
    [Key]
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    public GuvenlikOlayTuru ActionType { get; set; }

    [MaxLength(45)] // IPv6 adresleri icin yeterli uzunluk
    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Details { get; set; }
}