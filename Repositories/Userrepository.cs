using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Data;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> KullaniciAdiVarMiAsync(string username) =>
        _db.Users.AnyAsync(u => u.Username == username);

    public Task<User?> KullaniciGetirAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task EkleAsync(User kullanici)
    {
        _db.Users.Add(kullanici);
        await _db.SaveChangesAsync();
    }
    public Task KaydetAsync() => _db.SaveChangesAsync();
}