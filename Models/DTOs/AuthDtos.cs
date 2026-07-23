using System.ComponentModel.DataAnnotations;

namespace SifreYoneticiAPI.DTOs;

/// <summary>
/// POST /api/auth/register icin istek govdesi.
/// TUM kriptografik degerler (PublicKeyPem, EncryptedPrivateKeyPem, MasterHash, Salt)
/// frontend'de uretilip HAZIR olarak buraya gonderilir. Backend bunlarin
/// icerigine karisamaz, sadece dogrulayip kaydeder.
/// </summary>
public record RegisterRequestDto(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required] string MasterHash,
    [Required] string Salt,
    [Required] string PublicKeyPem,
    [Required] string EncryptedPrivateKeyPem
);

/// <summary>
/// POST /api/auth/login icin istek govdesi.
/// Frontend, kullanicinin girdigi Ana Sifreyi ASLA gondermez; sadece
/// SHA-256(salt + anaSifre) hesaplayip MasterHash olarak gonderir.
/// </summary>
public record LoginRequestDto(
    [Required] string Username,
    [Required] string MasterHash
);

/// <summary>
/// Basarili login sonrasi donen yanit. Frontend, PublicKeyPem ve
/// EncryptedPrivateKeyPem'i kullanarak kendi tarayicisinda Private Key'i
/// Ana Sifre ile cozup (sadece RAM'de/bellekte) kullanmaya baslar.
/// </summary>
public record LoginResponseDto(
    string Token,
    Guid UserId,
    string Username,
    string Salt,
    string PublicKeyPem,
    string EncryptedPrivateKeyPem
);