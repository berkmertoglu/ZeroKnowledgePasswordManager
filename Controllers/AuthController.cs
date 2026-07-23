using Microsoft.AspNetCore.Mvc;
using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Services;

namespace SifreYoneticiAPI.Controllers;

/// <summary>
/// Kayit (Register) ve Giris (Login) icin INCE (thin) HTTP katmani.
///
/// DIKKAT: Bu controller artik hicbir is mantigi (hash karsilastirma,
/// enumeration korumasi, JWT uretimi vb.) ICERMIYOR -- hepsi IAuthService'e
/// tasindi. Bu sinifin TEK gorevi: istegi al, servise devret, ServisSonucu'nu
/// dogru HTTP durum koduna cevir.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // =========================================================
    // GET /api/auth/salt/{username}
    // =========================================================
    [HttpGet("salt/{username}")]
    public async Task<ActionResult> SaltGetir(string username)
    {
        var salt = await _authService.SaltGetirAsync(username);
        return Ok(new { salt });
    }

    // =========================================================
    // POST /api/auth/register
    // =========================================================
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sonuc = await _authService.KayitOlAsync(dto);

        return sonuc.Tur switch
        {
            SonucTuru.Basarili => StatusCode(StatusCodes.Status201Created, sonuc.Veri),
            SonucTuru.Cakisma => Conflict(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }

    // =========================================================
    // POST /api/auth/login
    // =========================================================
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var ipAdresi = HttpContext.Connection.RemoteIpAddress?.ToString();
        var sonuc = await _authService.GirisYapAsync(dto, ipAdresi);

        return sonuc.Tur switch
        {
            SonucTuru.Basarili => Ok(sonuc.Veri),
            SonucTuru.YetkisizErisim => Unauthorized(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }
}