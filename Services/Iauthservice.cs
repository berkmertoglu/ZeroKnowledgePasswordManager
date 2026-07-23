using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Kayit/Giris IS MANTIGINI tanimlayan arayuz. AuthController bu arayuzu
/// DI ile alir; boylece controller, ApplicationDbContext'i veya hash/JWT
/// detaylarini hic bilmez, sadece "kayit ol", "giris yap" der.
/// </summary>
public interface IAuthService
{
    Task<ServisSonucu<LoginResponseDto>> KayitOlAsync(RegisterRequestDto dto);
    Task<ServisSonucu<LoginResponseDto>> GirisYapAsync(LoginRequestDto dto, string? ipAdresi);
    Task<string> SaltGetirAsync(string username);
}