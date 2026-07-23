using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Mapping;

public static class SecurityLogMappingExtensions
{
    public static SecurityLogResponseDto ToResponseDto(this SecurityLog kayit) => new(
        kayit.Id,
        kayit.ActionType.ToString(),
        kayit.IpAddress,
        kayit.Timestamp,
        kayit.Details
    );
}