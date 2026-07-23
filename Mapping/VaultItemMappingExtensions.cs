using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Mapping;

/// <summary>
/// Entity -> DTO donusumu. CategoryName, kayit.Category navigation
/// property'sinden okunur -- bu yuzden repository katmaninda MUTLAKA
/// Include(v => v.Category) yapilmis olmasi gerekir, aksi halde
/// kayit.Category null gelir ve CategoryName de null doner (kategori
/// atanmis olsa bile).
/// </summary>
public static class VaultItemMappingExtensions
{
    public static VaultItemResponseDto ToResponseDto(this VaultItem kayit) => new(
        kayit.Id,
        kayit.AppName,
        kayit.Username,
        kayit.EncryptedPassword,
        kayit.EncryptedNotes,
        kayit.CategoryId,
        kayit.Category?.Name,
        kayit.Url,
        kayit.IsFavorite,
        kayit.CreatedAt,
        kayit.UpdatedAt
    );
}