using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Extensions;
using SifreYoneticiAPI.Services;

namespace SifreYoneticiAPI.Controllers;

/// <summary>
/// Kullanicinin kendi kasalarini (kategorilerini) yonetir -- ince (thin)
/// HTTP katmani, tum is mantigi ICategoryService'te.
/// </summary>
[ApiController]
[Route("api/category")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // =========================================================
    // GET /api/category
    // =========================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponseDto>>> Listele()
    {
        var kategoriler = await _categoryService.ListeleAsync(User.KullaniciId());
        return Ok(kategoriler);
    }

    // =========================================================
    // POST /api/category
    // =========================================================
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Ekle([FromBody] CategoryCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var sonuc = await _categoryService.EkleAsync(User.KullaniciId(), dto);

        return sonuc.Tur switch
        {
            SonucTuru.Basarili => StatusCode(StatusCodes.Status201Created, sonuc.Veri),
            SonucTuru.Cakisma => Conflict(new { message = sonuc.Mesaj }),
            SonucTuru.GecersizIstek => BadRequest(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }

    // =========================================================
    // DELETE /api/category/{id}
    // =========================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id)
    {
        var sonuc = await _categoryService.SilAsync(User.KullaniciId(), id);

        return sonuc.Tur switch
        {
            SonucTuru.Basarili => NoContent(),
            SonucTuru.Bulunamadi => NotFound(new { message = sonuc.Mesaj }),
            _ => Problem(sonuc.Mesaj)
        };
    }
}