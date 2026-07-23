using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SifreYoneticiAPI.Models;

namespace SifreYoneticiAPI.Services;

/// <summary>
/// JWT uretimini soyutlayan arayuz. AuthController bunu DI ile alir,
/// dogrudan JwtSecurityTokenHandler ile ugrasmaz.
/// </summary>
public interface IJwtTokenService
{
    string TokenUret(User kullanici);
}

/// <summary>
/// appsettings.json'daki "Jwt" bolumunden (Key, Issuer, Audience) okuyarak
/// imzali bir JWT uretir. Token icinde SADECE kullanici Id ve Username
/// tasinir -- Ana Sifre, Private Key gibi HICBIR hassas veri token'a girmez.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string TokenUret(User kullanici)
    {
        var anahtarMetni = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("appsettings.json icinde 'Jwt:Key' tanimli degil.");

        var imzalamaAnahtari = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(anahtarMetni));
        var imzalamaBilgisi = new SigningCredentials(imzalamaAnahtari, SecurityAlgorithms.HmacSha256);

        var claimler = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, kullanici.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new Claim(ClaimTypes.Name, kullanici.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claimler,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: imzalamaBilgisi
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}