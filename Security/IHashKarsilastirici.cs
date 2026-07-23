namespace SifreYoneticiAPI.Security;

/// <summary>
/// SADECE hash karsilastirma islemini yapan, tek sorumluluklu bir servis.
/// Neden ayri bir sinif: bu, "kayit ol/giris yap" iş kuralindan tamamen
/// bagimsiz, saf bir kriptografik islemdir -- baska bir yerde (ornegin
/// API anahtari dogrulamasi gibi) tekrar kullanilabilir.
/// </summary>
public interface IHashKarsilastirici
{
    /// <summary>Iki hash degerini SABIT SURE (timing-attack'e karsi guvenli) karsilastirir.</summary>
    bool Esit(string dbHash, string gelenHash);
}