using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SifreYoneticiAPI.Models;

/// <summary>
/// Kasa kaydi entity'si.
///
/// NORMALIZASYON NOTU: Eski "public VaultCategory Category" (enum) alani
/// TAMAMEN KALDIRILDI. Artik CategoryId (nullable FK) ve Category
/// (navigation property, VaultCategory ENTITY'sine isaret eder) var.
///
/// CategoryId NEDEN NULLABLE: Bir kullanicinin henuz hic kategorisi
/// olmayabilir (yeni kayit oldu) ya da secmis oldugu kategori sonradan
/// silinmis olabilir. Her iki durumda da kayit "Kategorisiz" sanal
/// kasasinda gorunmeye devam eder -- hicbir veri kaybi olmaz.
/// </summary>
public class VaultItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string AppName { get; set; } = string.Empty;

    /// <summary>Sitede kullanilan giris e-postasi/kullanici adi (duz metin).</summary>
    [MaxLength(200)]
    public string? Username { get; set; }

    /// <summary>RSA-OAEP ile sifrelenmis, base64 kodlanmis sifre.</summary>
    [Required]
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>RSA-OAEP ile sifrelenmis, base64 kodlanmis guvenli notlar (opsiyonel).</summary>
    public string? EncryptedNotes { get; set; }

    /// <summary>
    /// NULL olabilir. Kategori silinirse (bkz. ApplicationDbContext'teki
    /// ON DELETE SET NULL kurali) bu alan otomatik NULL olur, kayit
    /// SILINMEZ, sadece "Kategorisiz" sanal kasasina duser.
    /// </summary>
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public VaultCategory? Category { get; set; }

    [MaxLength(500)]
    public string? Url { get; set; }

    public bool IsFavorite { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}