using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Extensions;
using SifreYoneticiAPI.Services;

namespace SifreYoneticiAPI.Controllers;

/// <summary>
/// Kullanicinin KENDI guvenlik kayitlarini (audit log) goruntulemesini
/// saglar. [Authorize] + User.KullaniciId() filtresi sayesinde bir
/// kullanici baska bir kullanicinin loglarini asla goremez.
/// </summary>
[ApiController]
[Route("api/securitylog")]
[Authorize]
public class SecurityLogController : ControllerBase
{
    private readonly ISecurityLogService _securityLogService;

    public SecurityLogController(ISecurityLogService securityLogService)
    {
        _securityLogService = securityLogService;
    }

    // =========================================================
    // GET /api/securitylog -> son 50 guvenlik kaydi
    // =========================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SecurityLogResponseDto>>> Listele()
    {
        var kayitlar = await _securityLogService.SonKayitlariGetirAsync(User.KullaniciId());
        return Ok(kayitlar);
    }
}