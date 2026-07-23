using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Models;
using SifreYoneticiAPI.Repositories;
using SifreYoneticiAPI.Security;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// Kayit/Giris IS MANTIGI (orkestrasyon). Artik ApplicationDbContext'i
/// DOGRUDAN bilmiyor -- veritabani erisimi IUserRepository'ye, kriptografik
/// islemler IHashKarsilastirici/ISahteSaltUreticisi'ye devredildi. Bu
/// sinifin gorevi SADECE "hangi sirayla, hangi kurala gore" karar vermek:
/// "kullanici adi zaten varsa cakisma dondur", "hash uyusmuyorsa yetkisiz
/// erisim dondur" gibi.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISecurityLogService _securityLogService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IHashKarsilastirici _hashKarsilastirici;
    private readonly ISahteSaltUreticisi _sahteSaltUreticisi;

    // BRUTE-FORCE KORUMASI sabitleri
    private const int MaksimumBasarisizDeneme = 5;
    private const int KilitlenmeSuresiDakika = 15;

    // YENI KULLANICIYA otomatik olarak acilan varsayilan kasalar (isim + simge).
    // NOT: "Diger" burada YOK cunku onun isini zaten "Kategorisiz" sanal
    // klasoru goruyor. Kullanici bunlari istedigi gibi silebilir/yenilerini
    // ekleyebilir, bu SADECE rahat bir baslangic noktasidir.
    private static readonly (string Ad, string Simge)[] VarsayilanKasalar =
    {
        ("Sosyal Medya", "📱"),
        ("Is", "💼"),
        ("Banka", "🏦"),
        ("E-posta", "✉️"),
        ("Oyun", "🎮"),
        ("Alisveris", "🛒")
    };

    public AuthService(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        ISecurityLogService securityLogService,
        IJwtTokenService jwtTokenService,
        IHashKarsilastirici hashKarsilastirici,
        ISahteSaltUreticisi sahteSaltUreticisi)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _securityLogService = securityLogService;
        _jwtTokenService = jwtTokenService;
        _hashKarsilastirici = hashKarsilastirici;
        _sahteSaltUreticisi = sahteSaltUreticisi;
    }

    public async Task<string> SaltGetirAsync(string username)
    {
        var kullanici = await _userRepository.KullaniciGetirAsync(username);

        // Kullanici varsa gercek salt'i, yoksa ISahteSaltUreticisi'nin
        // urettigi tutarli sahte salt'i dondur (enumeration korumasi).
        return kullanici?.Salt ?? _sahteSaltUreticisi.Uret(username);
    }

    public async Task<ServisSonucu<LoginResponseDto>> KayitOlAsync(RegisterRequestDto dto)
    {
        if (await _userRepository.KullaniciAdiVarMiAsync(dto.Username))
        {
            return ServisSonucu<LoginResponseDto>.Cakisma($"'{dto.Username}' kullanici adi zaten alinmis.");
        }

        var yeniKullanici = new User
        {
            Username = dto.Username,
            MasterHash = dto.MasterHash,
            Salt = dto.Salt,
            PublicKeyPem = dto.PublicKeyPem,
            EncryptedPrivateKeyPem = dto.EncryptedPrivateKeyPem,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _userRepository.EkleAsync(yeniKullanici);
        }
        catch (DbUpdateException)
        {
            // Ayni anda iki istek gelip UNIQUE index'e takilirsa (race condition) buraya duser
            return ServisSonucu<LoginResponseDto>.Cakisma($"'{dto.Username}' kullanici adi zaten alinmis.");
        }

        // Kullanici basariyla olusturuldu -> rahat bir baslangic icin
        // varsayilan kasalari (kategorileri) otomatik ac. Bu bir "iş kuralı"
        // (yeni kullaniciya ne olsun) oldugu icin burada, AuthService'te
        // kalmasi dogru -- CategoryService'e tasimadik cunku o, "kullanici
        // kaydi" kavramindan tamamen bagimsiz kalmali.
        await VarsayilanKasalariOlusturAsync(yeniKullanici.Id);

        var token = _jwtTokenService.TokenUret(yeniKullanici);

        var yanit = new LoginResponseDto(
            Token: token,
            UserId: yeniKullanici.Id,
            Username: yeniKullanici.Username,
            Salt: yeniKullanici.Salt,
            PublicKeyPem: yeniKullanici.PublicKeyPem,
            EncryptedPrivateKeyPem: yeniKullanici.EncryptedPrivateKeyPem
        );

        return ServisSonucu<LoginResponseDto>.Basari(yanit);
    }

    /// <summary>
    /// Yeni kullaniciya VarsayilanKasalar listesindeki kasalari acar.
    /// Bu SADECE kayit sirasinda, bir KERE calisir -- kullanici sonradan
    /// bunlari silerse tekrar geri gelmezler (bu, kullanicinin kendi
    /// tercihine saygi duymak icin kasitlidir).
    /// </summary>
    private async Task VarsayilanKasalariOlusturAsync(Guid kullaniciId)
    {
        foreach (var kasa in VarsayilanKasalar)
        {
            await _categoryRepository.EkleAsync(new VaultCategory
            {
                UserId = kullaniciId,
                Name = kasa.Ad,
                Icon = kasa.Simge,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    public async Task<ServisSonucu<LoginResponseDto>> GirisYapAsync(LoginRequestDto dto, string? ipAdresi)
    {
        var kullanici = await _userRepository.KullaniciGetirAsync(dto.Username);

        // ---- 1) HESAP KILITLI Mi? (Brute-force korumasi) ----
        if (kullanici is not null && kullanici.LockoutEnd.HasValue && kullanici.LockoutEnd.Value > DateTime.UtcNow)
        {
            var kalanDakika = (int)Math.Ceiling((kullanici.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);

            await _securityLogService.KaydetAsync(
                kullanici.Id, GuvenlikOlayTuru.GirisBasarisiz, ipAdresi,
                $"Hesap kilitliyken giris denemesi. Kalan sure: {kalanDakika} dk.");

            return ServisSonucu<LoginResponseDto>.YetkisizErisim(
                $"Hesabiniz cok fazla hatali deneme nedeniyle kilitlendi. Lutfen {kalanDakika} dakika sonra tekrar deneyin.");
        }

        // ---- 2) KULLANICI YOK VEYA HASH UYUSMUYOR ----
        // ikisinde de AYNI mesaj (kullanici adi kesif saldirisina karsi).
        if (kullanici is null || !_hashKarsilastirici.Esit(kullanici.MasterHash, dto.MasterHash))
        {
            if (kullanici is not null)
            {
                kullanici.FailedLoginAttempts += 1;

                if (kullanici.FailedLoginAttempts >= MaksimumBasarisizDeneme)
                {
                    kullanici.LockoutEnd = DateTime.UtcNow.AddMinutes(KilitlenmeSuresiDakika);
                    kullanici.FailedLoginAttempts = 0; // sayaci sifirla, kilit zaten devrede

                    await _userRepository.KaydetAsync();

                    await _securityLogService.KaydetAsync(
                        kullanici.Id, GuvenlikOlayTuru.HesapKilitlendi, ipAdresi,
                        $"{MaksimumBasarisizDeneme} basarisiz deneme sonrasi hesap {KilitlenmeSuresiDakika} dakika kilitlendi.");
                }
                else
                {
                    await _userRepository.KaydetAsync();
                }
            }

            await _securityLogService.KaydetAsync(
                kullanici?.Id, GuvenlikOlayTuru.GirisBasarisiz, ipAdresi,
                kullanici is null ? $"Bilinmeyen kullanici adi denemesi: '{dto.Username}'" : "Ana sifre hatali");

            return ServisSonucu<LoginResponseDto>.YetkisizErisim("Kullanici adi veya Ana Sifre hatali.");
        }

        // ---- 3) BASARILI GIRIS -> sayaci ve kilidi sifirla ----
        kullanici.FailedLoginAttempts = 0;
        kullanici.LockoutEnd = null;
        await _userRepository.KaydetAsync();

        await _securityLogService.KaydetAsync(kullanici.Id, GuvenlikOlayTuru.GirisBasarili, ipAdresi, null);

        var token = _jwtTokenService.TokenUret(kullanici);

        var yanit = new LoginResponseDto(
            Token: token,
            UserId: kullanici.Id,
            Username: kullanici.Username,
            Salt: kullanici.Salt,
            PublicKeyPem: kullanici.PublicKeyPem,
            EncryptedPrivateKeyPem: kullanici.EncryptedPrivateKeyPem
        );

        return ServisSonucu<LoginResponseDto>.Basari(yanit);
    }
}