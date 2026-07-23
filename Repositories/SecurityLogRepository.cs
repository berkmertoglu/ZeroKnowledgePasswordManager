using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Data;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

public class SecurityLogRepository : ISecurityLogRepository
{
    private readonly ApplicationDbContext _db;

    public SecurityLogRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task EkleAsync(SecurityLog kayit)
    {
        _db.SecurityLogs.Add(kayit);
        await _db.SaveChangesAsync();
    }

    public Task<List<SecurityLog>> SonKayitlariGetirAsync(Guid kullaniciId, int adet) =>
        _db.SecurityLogs
            .AsNoTracking()
            .Where(l => l.UserId == kullaniciId)
            .OrderByDescending(l => l.Timestamp)
            .Take(adet)
            .ToListAsync();
}