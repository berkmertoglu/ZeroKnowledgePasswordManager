using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Data;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;

    public CategoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<List<VaultCategory>> ListeleAsync(Guid kullaniciId) =>
        _db.VaultCategories
            .AsNoTracking()
            .Where(c => c.UserId == kullaniciId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public Task<VaultCategory?> GetirAsync(Guid kullaniciId, int id) =>
        _db.VaultCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == kullaniciId);

    public Task<bool> IsimVarMiAsync(Guid kullaniciId, string name) =>
        _db.VaultCategories.AnyAsync(c => c.UserId == kullaniciId && c.Name == name);

    public async Task EkleAsync(VaultCategory kategori)
    {
        _db.VaultCategories.Add(kategori);
        await _db.SaveChangesAsync();
    }

    public async Task SilAsync(VaultCategory kategori)
    {
        _db.VaultCategories.Remove(kategori);
        await _db.SaveChangesAsync();
        // NOT: Bu kategoriye ait VaultItem'larin CategoryId'si, veritabani
        // seviyesinde ON DELETE SET NULL sayesinde OTOMATIK NULL olur --
        // burada elle bir islem yapmamiza GEREK YOK.
    }
}