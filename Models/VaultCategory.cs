using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SifreYoneticiAPI.Models;

/// <summary>
/// Kullaniciya ozel "sanal kasa" (kategori) tablosu.
///
/// NORMALIZASYON NOTU: Eskiden VaultItem.Category sabit bir enum'du.
/// Artik her kullanici KENDI kategorilerini olusturup silebiliyor.
///
/// Icon alani, kasa olustururken kullanicinin sectigi bir emoji'yi tutar
/// (ornegin "🏦", "📱"). MaxLength 16 olarak ayarlandi cunku bazi emoji'ler
/// (bayraklar, ten rengi varyasyonlari vb.) birden fazla Unicode code point'ten
/// olusabilir ve tek karakterden uzun bayt dizisi tutabilir.
/// </summary>
public class VaultCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(16)]
    public string Icon { get; set; } = "📁";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<VaultItem> VaultItems { get; set; } = new List<VaultItem>();
}