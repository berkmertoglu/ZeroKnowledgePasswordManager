using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Mapping;
using SifreYoneticiAPI.Models;
using SifreYoneticiAPI.Repositories;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Kasa (kategori) IS MANTIGI. Silme islemi icin "icinde sifre var mi"
/// kontrolu KASITLI OLARAK burada YOK -- cunku veritabani seviyesinde
/// ON DELETE SET NULL kurali zaten bu durumu guvenle hallediyor (silinen
/// kategorideki sifreler "Kategorisiz"e duser, hicbiri kaybolmaz).
/// </summary>
public class CategoryService : ICategoryService
{
    private const string VarsayilanSimge = "📁";

    private readonly ICategoryRepository _categoryRepository;
    private readonly IVaultItemRepository _vaultItemRepository;

    public CategoryService(ICategoryRepository categoryRepository, IVaultItemRepository vaultItemRepository)
    {
        _categoryRepository = categoryRepository;
        _vaultItemRepository = vaultItemRepository;
    }

    public async Task<IEnumerable<CategoryResponseDto>> ListeleAsync(Guid kullaniciId)
    {
        var kategoriler = await _categoryRepository.ListeleAsync(kullaniciId);
        var sayilar = await _vaultItemRepository.KategoriBazindaSayilariAsync(kullaniciId);

        return kategoriler.Select(k => k.ToResponseDto(sayilar.GetValueOrDefault(k.Id, 0)));
    }

    public async Task<ServisSonucu<CategoryResponseDto>> EkleAsync(Guid kullaniciId, CategoryCreateDto dto)
    {
        var temizIsim = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(temizIsim))
        {
            return ServisSonucu<CategoryResponseDto>.GecersizIstek("Kasa adi bos olamaz.");
        }

        if (await _categoryRepository.IsimVarMiAsync(kullaniciId, temizIsim))
        {
            return ServisSonucu<CategoryResponseDto>.Cakisma($"'{temizIsim}' adinda bir kasaniz zaten var.");
        }

        // Icon bos/null gelirse varsayilan simgeyi kullan
        var simge = string.IsNullOrWhiteSpace(dto.Icon) ? VarsayilanSimge : dto.Icon.Trim();

        var yeniKategori = new VaultCategory
        {
            UserId = kullaniciId,
            Name = temizIsim,
            Icon = simge,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.EkleAsync(yeniKategori);

        return ServisSonucu<CategoryResponseDto>.Basari(yeniKategori.ToResponseDto(0));
    }

    public async Task<ServisSonucu> SilAsync(Guid kullaniciId, int id)
    {
        var kategori = await _categoryRepository.GetirAsync(kullaniciId, id);

        if (kategori is null)
        {
            return ServisSonucu.Bulunamadi();
        }

        await _categoryRepository.SilAsync(kategori);

        return ServisSonucu.Basari();
    }
}