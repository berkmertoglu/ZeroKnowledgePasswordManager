using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

/// <summary>
/// VaultItem entity'si icin SADECE veritabani erisim islemlerini tanimlar.
/// </summary>
public interface IVaultItemRepository
{
    /// <summary>Listeleme icin salt-okunur (AsNoTracking) sorgu. Category dahil edilir.</summary>
    Task<List<VaultItem>> ListeleAsync(Guid kullaniciId);

    /// <summary>Tekil goruntuleme icin salt-okunur (AsNoTracking) sorgu. Category dahil edilir.</summary>
    Task<VaultItem?> GetirAsync(Guid kullaniciId, Guid id);

    /// <summary>Guncelleme/silme oncesi TAKIPLI (tracked) sorgu. Category dahil edilir.</summary>
    Task<VaultItem?> GetirTakipliAsync(Guid kullaniciId, Guid id);

    Task<bool> AyniIsimVarMiAsync(Guid kullaniciId, string appName);

    Task EkleAsync(VaultItem kayit);

    /// <summary>GetirTakipliAsync ile alinmis, uzerinde degisiklik yapilmis bir kaydi kaydeder.</summary>
    Task KaydetAsync();

    Task SilAsync(VaultItem kayit);

    /// <summary>
    /// Kullanicinin TUM kategorileri icin { CategoryId: adet } seklinde tek
    /// sorguda sayac dondurur (sidebar'daki sayaclar icin, N+1 sorgudan kacinmak amacli).
    /// </summary>
    Task<Dictionary<int, int>> KategoriBazindaSayilariAsync(Guid kullaniciId);
}