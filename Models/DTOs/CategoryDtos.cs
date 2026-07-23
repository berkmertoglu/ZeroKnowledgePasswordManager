using System.ComponentModel.DataAnnotations;

namespace SifreYoneticiAPI.DTOs;

/// <summary>
/// POST /api/category icin istek govdesi. Icon opsiyoneldir -- bos/null
/// gelirse CategoryService varsayilan bir simge ("📁") atar.
/// </summary>
public record CategoryCreateDto(
    [Required, MaxLength(50)] string Name,
    [MaxLength(16)] string? Icon
);

/// <summary>
/// GET/POST /api/category yanitinda donen DTO. SifreSayisi, bu kategoriye
/// ait kac VaultItem oldugunu gosterir (sidebar'daki sayaclar icin).
/// </summary>
public record CategoryResponseDto(
    int Id,
    string Name,
    string Icon,
    DateTime CreatedAt,
    int SifreSayisi
);