using Microsoft.EntityFrameworkCore;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Data;

/// <summary>
/// EF Core DbContext. Baslangicta SQLite ile calisir; UseSqlite(...) yerine
/// ileride UseNpgsql(...) yazilarak PostgreSQL'e gecis yapilabilecek sekilde
/// tasarlanmistir.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<VaultItem> VaultItems => Set<VaultItem>();
    public DbSet<VaultCategory> VaultCategories => Set<VaultCategory>();
    public DbSet<SecurityLog> SecurityLogs => Set<SecurityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------------- USER ----------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();

            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.MasterHash).IsRequired();
            entity.Property(u => u.Salt).IsRequired();
            entity.Property(u => u.PublicKeyPem).IsRequired();
            entity.Property(u => u.EncryptedPrivateKeyPem).IsRequired();
        });

        // ---------------- VAULT CATEGORY (YENI) ----------------
        modelBuilder.Entity<VaultCategory>(entity =>
        {
            // Kullanici silinirse kategorileri de silinir (User zaten "kok" varlik)
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(50);

            // Ayni kullanici ayni isimde iki kasa olusturamaz
            entity.HasIndex(c => new { c.UserId, c.Name }).IsUnique();
        });

        // ---------------- VAULT ITEM ----------------
        modelBuilder.Entity<VaultItem>(entity =>
        {
            entity.HasOne(v => v.User)
                  .WithMany(u => u.VaultItems)
                  .HasForeignKey(v => v.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // KRITIK KURAL: Bir kategori silinirse, ona ait VaultItem'lar
            // SILINMEZ -- sadece CategoryId'leri otomatik NULL olur
            // (SetNull). Boylece kullanici yanlislikla bir kasayi silse
            // bile hicbir sifre kaybolmaz, sadece "Kategorisiz"e duser.
            entity.HasOne(v => v.Category)
                  .WithMany(c => c.VaultItems)
                  .HasForeignKey(v => v.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(v => v.AppName).IsRequired().HasMaxLength(200);
            entity.Property(v => v.EncryptedPassword).IsRequired();

            entity.HasIndex(v => new { v.UserId, v.AppName }).IsUnique();
        });

        // ---------------- SECURITY LOG (YENI) ----------------
        modelBuilder.Entity<SecurityLog>(entity =>
        {
            // Kullanici silinse BILE audit kayitlari SILINMEZ -- sadece
            // UserId'leri NULL olur (SetNull). Guvenlik kayitlari, hesabin
            // kendisinden BAGIMSIZ olarak saklanmaya devam etmelidir.
            entity.HasOne(l => l.User)
                  .WithMany()
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Enum'u okunabilir string olarak sakla (VaultCategory'de de
            // kullandigimiz PostgreSQL-dostu, elle inceleme-dostu desen)
            entity.Property(l => l.ActionType).HasConversion<string>().HasMaxLength(30);

            // Bir kullanicinin kayitlarini tarihe gore hizlica cekebilmek icin
            entity.HasIndex(l => new { l.UserId, l.Timestamp });
        });
    }
}