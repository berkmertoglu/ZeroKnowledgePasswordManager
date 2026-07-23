using SifreYoneticiAPI.Common;
using SifreYoneticiAPI.DTOs;

namespace SifreYoneticiAPI.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> ListeleAsync(Guid kullaniciId);
    Task<ServisSonucu<CategoryResponseDto>> EkleAsync(Guid kullaniciId, CategoryCreateDto dto);
    Task<ServisSonucu> SilAsync(Guid kullaniciId, int id);
}