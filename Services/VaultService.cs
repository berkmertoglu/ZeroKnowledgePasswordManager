using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Mapping;
using SifreYoneticiAPI.Models;
using SifreYoneticiAPI.Repositories;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Kasa (VaultItem) CRUD IS MANTIGI.
///
/// KATEGORI DOGRULAMASI: Ekle/Guncelle sirasinda CategoryId verilmisse,
/// bu kategorinin GERCEKTEN o kullaniciya ait olup olmadigi kontrol edilir
/// (ICategoryRepository ile). Bu hem guvenlik (baskasinin kategorisine
/// referans vermeyi engeller) hem de veri butunlugu icin gereklidir.
///
/// NAVIGATION SENKRONIZASYONU: CategoryId'yi degistirdikten sonra, ayni
/// islemde bulunmus VaultCategory nesnesini dogrudan kayit.Category'ye
/// atiyoruz. Bu sayede EF Core'un "sadece FK scalar degisti, navigation
/// hala eski degeri gosteriyor" tuzagina dusmuyoruz -- kayit.ToResponseDto()
/// cagrildiginda CategoryName her zaman GUNCEL bilgiyi yansitir.
/// </summary>
public class VaultService : IVaultService
{
    private readonly IVaultItemRepository _vaultItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISecurityLogService _securityLogService;

    public VaultService(
        IVaultItemRepository vaultItemRepository,
        ICategoryRepository categoryRepository,
        ISecurityLogService securityLogService)
    {
        _vaultItemRepository = vaultItemRepository;
        _categoryRepository = categoryRepository;
        _securityLogService = securityLogService;
    }

    public async Task<IEnumerable<VaultItemResponseDto>> ListeleAsync(Guid kullaniciId)
    {
        var kayitlar = await _vaultItemRepository.ListeleAsync(kullaniciId);
        return kayitlar.Select(k => k.ToResponseDto());
    }

    public async Task<ServisSonucu<VaultItemResponseDto>> GetirAsync(Guid kullaniciId, Guid id)
    {
        var kayit = await _vaultItemRepository.GetirAsync(kullaniciId, id);

        if (kayit is null)
        {
            return ServisSonucu<VaultItemResponseDto>.Bulunamadi();
        }

        return ServisSonucu<VaultItemResponseDto>.Basari(kayit.ToResponseDto());
    }

    public async Task<ServisSonucu<VaultItemResponseDto>> EkleAsync(Guid kullaniciId, VaultItemCreateDto dto, string? ipAdresi)
    {
        if (await _vaultItemRepository.AyniIsimVarMiAsync(kullaniciId, dto.AppName))
        {
            return ServisSonucu<VaultItemResponseDto>.Cakisma(
                $"'{dto.AppName}' zaten kayitli. Guncellemek icin PUT kullanin.");
        }

        var kategoriSonucu = await KategoriyiDogrulaAsync(kullaniciId, dto.CategoryId);
        if (kategoriSonucu.Hata is not null)
        {
            return ServisSonucu<VaultItemResponseDto>.GecersizIstek(kategoriSonucu.Hata);
        }

        var simdi = DateTime.UtcNow;

        var yeniKayit = new VaultItem
        {
            UserId = kullaniciId,
            AppName = dto.AppName,
            Username = dto.Username,
            EncryptedPassword = dto.EncryptedPassword,
            EncryptedNotes = dto.EncryptedNotes,
            CategoryId = dto.CategoryId,
            Category = kategoriSonucu.Kategori,
            Url = dto.Url,
            IsFavorite = dto.IsFavorite,
            CreatedAt = simdi,
            UpdatedAt = simdi
        };

        await _vaultItemRepository.EkleAsync(yeniKayit);

        await _securityLogService.KaydetAsync(
            kullaniciId, GuvenlikOlayTuru.SifreEklendi, ipAdresi, $"'{dto.AppName}' eklendi.");

        return ServisSonucu<VaultItemResponseDto>.Basari(yeniKayit.ToResponseDto());
    }

    public async Task<ServisSonucu<VaultItemResponseDto>> GuncelleAsync(Guid kullaniciId, Guid id, VaultItemUpdateDto dto)
    {
        var kayit = await _vaultItemRepository.GetirTakipliAsync(kullaniciId, id);

        if (kayit is null)
        {
            return ServisSonucu<VaultItemResponseDto>.Bulunamadi();
        }

        var kategoriSonucu = await KategoriyiDogrulaAsync(kullaniciId, dto.CategoryId);
        if (kategoriSonucu.Hata is not null)
        {
            return ServisSonucu<VaultItemResponseDto>.GecersizIstek(kategoriSonucu.Hata);
        }

        kayit.Username = dto.Username;
        kayit.EncryptedPassword = dto.EncryptedPassword;
        kayit.EncryptedNotes = dto.EncryptedNotes;
        kayit.CategoryId = dto.CategoryId;
        kayit.Category = kategoriSonucu.Kategori;
        kayit.Url = dto.Url;
        kayit.IsFavorite = dto.IsFavorite;
        kayit.UpdatedAt = DateTime.UtcNow;

        await _vaultItemRepository.KaydetAsync();

        return ServisSonucu<VaultItemResponseDto>.Basari(kayit.ToResponseDto());
    }

    public async Task<ServisSonucu<VaultItemResponseDto>> FavoriDegistirAsync(Guid kullaniciId, Guid id, bool isFavorite)
    {
        var kayit = await _vaultItemRepository.GetirTakipliAsync(kullaniciId, id);

        if (kayit is null)
        {
            return ServisSonucu<VaultItemResponseDto>.Bulunamadi();
        }

        kayit.IsFavorite = isFavorite;
        kayit.UpdatedAt = DateTime.UtcNow;

        await _vaultItemRepository.KaydetAsync();

        return ServisSonucu<VaultItemResponseDto>.Basari(kayit.ToResponseDto());
    }

    public async Task<ServisSonucu> SilAsync(Guid kullaniciId, Guid id, string? ipAdresi)
    {
        var kayit = await _vaultItemRepository.GetirTakipliAsync(kullaniciId, id);

        if (kayit is null)
        {
            return ServisSonucu.Bulunamadi();
        }

        // Silmeden ONCE adini yakaliyoruz -- kayit silindikten sonra artik erisilemez
        var silinenAppName = kayit.AppName;

        await _vaultItemRepository.SilAsync(kayit);

        await _securityLogService.KaydetAsync(
            kullaniciId, GuvenlikOlayTuru.SifreSilindi, ipAdresi, $"'{silinenAppName}' silindi.");

        return ServisSonucu.Basari();
    }

    /// <summary>
    /// CategoryId verilmisse, bu kategorinin GERCEKTEN bu kullaniciya ait
    /// olup olmadigini dogrular. CategoryId null ise (Kategorisiz secilmis)
    /// dogrulamaya gerek yoktur, direkt basarili sayilir.
    /// </summary>
    private async Task<(VaultCategory? Kategori, string? Hata)> KategoriyiDogrulaAsync(Guid kullaniciId, int? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return (null, null);
        }

        var kategori = await _categoryRepository.GetirAsync(kullaniciId, categoryId.Value);

        if (kategori is null)
        {
            return (null, "Secilen kasa bulunamadi.");
        }

        return (kategori, null);
    }
}