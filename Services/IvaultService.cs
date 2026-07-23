using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Kasa (VaultItem) CRUD IS MANTIGINI tanimlayan arayuz. VaultController
/// bu arayuzu DI ile alir; ApplicationDbContext'i veya sahiplik (ownership)
/// kontrol detaylarini hic bilmez.
/// </summary>
public interface IVaultService
{
    Task<IEnumerable<VaultItemResponseDto>> ListeleAsync(Guid kullaniciId);
    Task<ServisSonucu<VaultItemResponseDto>> GetirAsync(Guid kullaniciId, Guid id);
    Task<ServisSonucu<VaultItemResponseDto>> EkleAsync(Guid kullaniciId, VaultItemCreateDto dto, string? ipAdresi);
    Task<ServisSonucu<VaultItemResponseDto>> GuncelleAsync(Guid kullaniciId, Guid id, VaultItemUpdateDto dto);
    Task<ServisSonucu<VaultItemResponseDto>> FavoriDegistirAsync(Guid kullaniciId, Guid id, bool isFavorite);
    Task<ServisSonucu> SilAsync(Guid kullaniciId, Guid id, string? ipAdresi);
}