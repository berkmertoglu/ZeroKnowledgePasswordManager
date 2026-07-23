using System.ComponentModel.DataAnnotations;

namespace SifreYoneticiAPI.DTOs;

/// <summary>
/// POST /api/vault icin istek govdesi. EncryptedPassword ve EncryptedNotes,
/// frontend'de kullanicinin KENDI Public Key'i ile RSA-OAEP kullanilarak
/// onceden sifrelenmis olarak gelir. CategoryId artik sabit bir enum
/// degil, kullanicinin kendi olusturdugu VaultCategory tablosuna FK'dir
/// (null = "Kategorisiz").
/// </summary>
public record VaultItemCreateDto(
    [Required, MaxLength(200)] string AppName,
    [MaxLength(200)] string? Username,
    [Required] string EncryptedPassword,
    string? EncryptedNotes,
    int? CategoryId,
    [MaxLength(500)] string? Url,
    bool IsFavorite
);

/// <summary>PUT /api/vault/{id} icin istek govdesi (tam guncelleme).</summary>
public record VaultItemUpdateDto(
    [MaxLength(200)] string? Username,
    [Required] string EncryptedPassword,
    string? EncryptedNotes,
    int? CategoryId,
    [MaxLength(500)] string? Url,
    bool IsFavorite
);

/// <summary>PATCH /api/vault/{id}/favori icin istek govdesi (tek alan guncelleme).</summary>
public record VaultItemFavoriDto(bool IsFavorite);

/// <summary>
/// GET /api/vault yanitinda donen DTO. CategoryName, frontend'in ayrica
/// kategori listesiyle eslestirme yapmasina gerek kalmadan direkt
/// gosterebilmesi icin buraya (Include ile) dahil edilir.
/// </summary>
public record VaultItemResponseDto(
    Guid Id,
    string AppName,
    string? Username,
    string EncryptedPassword,
    string? EncryptedNotes,
    int? CategoryId,
    string? CategoryName,
    string? Url,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime UpdatedAt
);