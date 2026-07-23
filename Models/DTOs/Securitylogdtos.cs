namespace SifreYoneticiAPI.DTOs;

/// <summary>
/// GET /api/securitylog yanitinda donen DTO. Kullanici SADECE kendi
/// guvenlik kayitlarini gorebilir (SecurityLogController bunu garanti eder).
/// </summary>
public record SecurityLogResponseDto(
    long Id,
    string ActionType,
    string? IpAddress,
    DateTime Timestamp,
    string? Details
);