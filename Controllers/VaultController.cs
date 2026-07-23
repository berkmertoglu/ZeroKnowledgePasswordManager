using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Extensions;
using SifreYoneticiAPI.Services;

namespace SifreYoneticiAPI.Controllers;

/// <summary>
/// Kasa (VaultItem) CRUD icin INCE (thin) HTTP katmani.
///
/// DIKKAT: Bu controller artik hicbir is mantigi (sahiplik kontrolu, DTO
/// eslestirme, cakisma kontrolu vb.) ICERMIYOR -- hepsi IVaultService'e
/// tasindi. Bu sinifin TEK gorevi: istegi al, kullanici kimligini JWT'den
/// oku, servise devret, ServisSonucu'nu dogru HTTP durum koduna cevir.
///
/// [Authorize] sayesinde bu controller'daki HICBIR endpoint'e gecerli
/// bir JWT olmadan erisilemez.
/// </summary>
[ApiController]
[Route("api/vault")]
[Authorize]
public class VaultController : ControllerBase
{
    private readonly IVaultService _vaultService;

    public VaultController(IVaultService vaultService)
    {
        _vaultService = vaultService;
    }

    /// <summary>ServisSonucu(Tur) ortak eslestirmesi -- her endpoint'te tekrar yazmamak icin.</summary>
    private ActionResult SonucaGoreYanitVer<T>(ServisSonucu<T> sonuc, Func<T, ActionResult> basariGoster)
    {
        return sonuc.Tur switch
        {
            SonucTuru.Basarili => basariGoster(sonuc.Veri!),
            SonucTuru.Bulunamadi => NotFound(new { message = sonuc.Mesaj }),
            SonucTuru.Cakisma => Conflict(new { message = sonuc.Mesaj }),
            SonucTuru.YetkisizErisim => Unauthorized(new { message = sonuc.Mesaj }),
            SonucTuru.GecersizIstek => BadRequest(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }

    // =========================================================
    // GET /api/vault
    // =========================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VaultItemResponseDto>>> Listele()
    {
        var kayitlar = await _vaultService.ListeleAsync(User.KullaniciId());
        return Ok(kayitlar);
    }

    // =========================================================
    // GET /api/vault/{id}
    // =========================================================
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VaultItemResponseDto>> Getir(Guid id)
    {
        var sonuc = await _vaultService.GetirAsync(User.KullaniciId(), id);
        return SonucaGoreYanitVer(sonuc, veri => Ok(veri));
    }

    // =========================================================
    // POST /api/vault
    // =========================================================
    [HttpPost]
    public async Task<ActionResult<VaultItemResponseDto>> Ekle([FromBody] VaultItemCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var ipAdresi = HttpContext.Connection.RemoteIpAddress?.ToString();
        var sonuc = await _vaultService.EkleAsync(User.KullaniciId(), dto, ipAdresi);
        return SonucaGoreYanitVer(sonuc, veri => CreatedAtAction(nameof(Getir), new { id = veri.Id }, veri));
    }

    // =========================================================
    // PUT /api/vault/{id}
    // =========================================================
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VaultItemResponseDto>> Guncelle(Guid id, [FromBody] VaultItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sonuc = await _vaultService.GuncelleAsync(User.KullaniciId(), id, dto);
        return SonucaGoreYanitVer(sonuc, veri => Ok(veri));
    }

    // =========================================================
    // PATCH /api/vault/{id}/favori
    // =========================================================
    [HttpPatch("{id:guid}/favori")]
    public async Task<ActionResult<VaultItemResponseDto>> FavoriDegistir(Guid id, [FromBody] VaultItemFavoriDto dto)
    {
        var sonuc = await _vaultService.FavoriDegistirAsync(User.KullaniciId(), id, dto.IsFavorite);
        return SonucaGoreYanitVer(sonuc, veri => Ok(veri));
    }

    // =========================================================
    // DELETE /api/vault/{id}
    // =========================================================
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Sil(Guid id)
    {
        var ipAdresi = HttpContext.Connection.RemoteIpAddress?.ToString();
        var sonuc = await _vaultService.SilAsync(User.KullaniciId(), id, ipAdresi);

        return sonuc.Tur switch
        {
            SonucTuru.Basarili => NoContent(),
            SonucTuru.Bulunamadi => NotFound(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }
}