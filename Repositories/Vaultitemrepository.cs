using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Data;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

public class VaultItemRepository : IVaultItemRepository
{
    private readonly ApplicationDbContext _db;

    public VaultItemRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<VaultItem>> ListeleAsync(Guid kullaniciId) =>
        _db.VaultItems
            .AsNoTracking()
            .Include(v => v.Category)
            .Where(v => v.UserId == kullaniciId)
            .OrderByDescending(v => v.IsFavorite)
            .ThenBy(v => v.AppName)
            .ToListAsync();

    public Task<VaultItem?> GetirAsync(Guid kullaniciId, Guid id) =>
        _db.VaultItems
            .AsNoTracking()
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == kullaniciId);

    public Task<VaultItem?> GetirTakipliAsync(Guid kullaniciId, Guid id) =>
        _db.VaultItems
            .Include(v => v.Category)
            .FirstOrDefaultAsync(v => v.Id == id && v.UserId == kullaniciId);

    public Task<bool> AyniIsimVarMiAsync(Guid kullaniciId, string appName) =>
        _db.VaultItems.AnyAsync(v => v.UserId == kullaniciId && v.AppName == appName);

    public async Task EkleAsync(VaultItem kayit)
    {
        _db.VaultItems.Add(kayit);
        await _db.SaveChangesAsync();
    }

    public Task KaydetAsync() => _db.SaveChangesAsync();

    public async Task SilAsync(VaultItem kayit)
    {
        _db.VaultItems.Remove(kayit);
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> KategoriBazindaSayilariAsync(Guid kullaniciId)
    {
        return await _db.VaultItems
            .Where(v => v.UserId == kullaniciId && v.CategoryId != null)
            .GroupBy(v => v.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Adet = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Adet);
    }
}