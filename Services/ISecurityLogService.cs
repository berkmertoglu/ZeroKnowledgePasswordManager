using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Guvenlik olaylarini kaydetmekten ve listelemekten sorumlu servis.
/// AuthService ve VaultService bu servisi cagirarak "bir seyler oldu"
/// bilgisini kalici hale getirir.
/// </summary>
public interface ISecurityLogService
{
    Task KaydetAsync(Guid? kullaniciId, GuvenlikOlayTuru olayTuru, string? ipAdresi, string? detay);
    Task<IEnumerable<SecurityLogResponseDto>> SonKayitlariGetirAsync(Guid kullaniciId, int adet = 50);
}