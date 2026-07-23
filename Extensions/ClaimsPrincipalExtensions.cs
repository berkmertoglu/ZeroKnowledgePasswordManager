using System.Security.Claims;

namespace SifreYoneticiAPI.Extensions;

/// <summary>
/// JWT icindeki kullanici kimligini okumak icin ClaimsPrincipal'a eklenen
/// bir uzanti (extension) metodu. Bu mantik daha once VaultController
/// icinde "GecerliKullaniciId()" olarak tekrar tekrar yaziliyordu; artik
/// tek bir yerde tanimli, "User.KullaniciId()" seklinde herhangi bir
/// [Authorize] kontrolcusunden cagrilabilir.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid KullaniciId(this ClaimsPrincipal principal)
    {
        var idMetni = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(idMetni) || !Guid.TryParse(idMetni, out var kullaniciId))
        {
            // Normalde [Authorize] bu noktaya gelmeden 401 dondurur;
            // bu, sadece beklenmedik/bozuk token durumuna karsi bir guvenlik agi.
            throw new UnauthorizedAccessException("Token icinde gecerli bir kullanici kimligi bulunamadi.");
        }

        return kullaniciId;
    }
}