using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Mapping;

public static class CategoryMappingExtensions
{
    /// <summary>
    /// sifreSayisi disaridan parametre olarak verilir cunku bu, VaultItem
    /// tablosundan gelen AYRI bir agregasyon sorgusunun sonucudur.
    /// </summary>
    public static CategoryResponseDto ToResponseDto(this VaultCategory kategori, int sifreSayisi) => new(
        kategori.Id,
        kategori.Name,
        kategori.Icon,
        kategori.CreatedAt,
        sifreSayisi
    );
}