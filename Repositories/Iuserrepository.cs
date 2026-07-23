using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

/// <summary>
/// User entity'si icin SADECE veritabani erisim islemlerini tanimlar.
/// Hicbir IS KURALI icermez (ornegin "kullanici adi zaten var" kontrolu
/// burada DEGIL, AuthService'te -- bu sinif sadece "var mi diye sor" der,
/// "varsa ne olacagina" karar vermez).
/// </summary>
public interface IUserRepository
{
    Task<bool> KullaniciAdiVarMiAsync(string username);
    Task<User?> KullaniciGetirAsync(string username);

    /// <summary>Yeni kullaniciyi ekler ve kaydeder.</summary>
    Task EkleAsync(User kullanici);

    /// <summary>
    /// KullaniciGetirAsync ile alinmis (TAKIPLI/tracked) bir kullanici
    /// nesnesi uzerinde yapilan degisiklikleri kaydeder. Brute-force
    /// koruma sayaclarini (FailedLoginAttempts, LockoutEnd) guncellemek
    /// icin kullanilir.
    /// </summary>
    Task KaydetAsync();
}