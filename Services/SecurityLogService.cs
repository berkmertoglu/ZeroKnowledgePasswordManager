using SifreYoneticiAPI.DTOs;
using SifreYoneticiAPI.Mapping;
using SifreYoneticiAPI.Models;
using SifreYoneticiAPI.Repositories;

namespace SifreYoneticiAPI.Services;

public class SecurityLogService : ISecurityLogService
{
    private readonly ISecurityLogRepository _securityLogRepository;

    public SecurityLogService(ISecurityLogRepository securityLogRepository)
    {
        _securityLogRepository = securityLogRepository;
    }

    public async Task KaydetAsync(Guid? kullaniciId, GuvenlikOlayTuru olayTuru, string? ipAdresi, string? detay)
    {
        var kayit = new SecurityLog
        {
            UserId = kullaniciId,
            ActionType = olayTuru,
            IpAddress = ipAdresi,
            Details = detay,
            Timestamp = DateTime.UtcNow
        };

        await _securityLogRepository.EkleAsync(kayit);
    }

    public async Task<IEnumerable<SecurityLogResponseDto>> SonKayitlariGetirAsync(Guid kullaniciId, int adet = 50)
    {
        var kayitlar = await _securityLogRepository.SonKayitlariGetirAsync(kullaniciId, adet);
        return kayitlar.Select(k => k.ToResponseDto());
    }
}