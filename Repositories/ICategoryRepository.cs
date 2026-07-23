using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

/// <summary>
/// VaultCategory entity'si icin SADECE veritabani erisim islemlerini
/// tanimlar. "Ayni isimde kategori var mi, eklensin mi" gibi kararlar
/// burada DEGIL, CategoryService'te verilir.
/// </summary>
public interface ICategoryRepository
{
    Task<List<VaultCategory>> ListeleAsync(Guid kullaniciId);
    Task<VaultCategory?> GetirAsync(Guid kullaniciId, int id);
    Task<bool> IsimVarMiAsync(Guid kullaniciId, string name);
    Task EkleAsync(VaultCategory kategori);
    Task SilAsync(VaultCategory kategori);
}