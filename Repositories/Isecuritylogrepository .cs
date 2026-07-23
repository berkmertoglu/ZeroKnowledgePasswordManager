using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

/// <summary>
/// SecurityLog tablosu icin SADECE veritabani erisim islemlerini tanimlar.
/// </summary>
public interface ISecurityLogRepository
{
    Task EkleAsync(SecurityLog kayit);

    /// <summary>Bir kullanicinin en son N guvenlik kaydini, en yeniden en eskiye siralar.</summary>
    Task<List<SecurityLog>> SonKayitlariGetirAsync(Guid kullaniciId, int adet);
}