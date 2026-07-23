namespace SifreYoneticiAPI.Common;

/// <summary>
/// Bir servis cagrisinin SONUCUNUN TURUNU belirtir. Controller, bu tur
/// bilgisine bakarak dogru HTTP durum kodunu (200, 401, 404, 409 vb.)
/// secer. Boylece servis katmani HTTP'den tamamen habersiz kalir --
/// servisler "Unauthorized() dondur" gibi HTTP'ye ozgu bir sey bilmez,
/// sadece "bu islem yetkisiz" der, HTTP koduna cevirme isi controller'a aittir.
/// </summary>
public enum SonucTuru
{
    Basarili,
    GecersizIstek,
    YetkisizErisim,
    Bulunamadi,
    Cakisma
}

/// <summary>
/// Veri DONMEYEN islemler icin sonuc tipi (ornegin silme islemi).
/// </summary>
public class ServisSonucu
{
    public SonucTuru Tur { get; }
    public string? Mesaj { get; }

    protected ServisSonucu(SonucTuru tur, string? mesaj)
    {
        Tur = tur;
        Mesaj = mesaj;
    }

    public static ServisSonucu Basari() => new(SonucTuru.Basarili, null);
    public static ServisSonucu Bulunamadi(string mesaj = "Kayit bulunamadi.") => new(SonucTuru.Bulunamadi, mesaj);
    public static ServisSonucu Cakisma(string mesaj) => new(SonucTuru.Cakisma, mesaj);
    public static ServisSonucu YetkisizErisim(string mesaj) => new(SonucTuru.YetkisizErisim, mesaj);
    public static ServisSonucu GecersizIstek(string mesaj) => new(SonucTuru.GecersizIstek, mesaj);
}

/// <summary>
/// Veri DONEN islemler icin sonuc tipi (ornegin kayit ekleme -> eklenen
/// kaydin DTO'sunu geri dondurmek istiyoruz).
/// </summary>
public class ServisSonucu<T> : ServisSonucu
{
    public T? Veri { get; }

    private ServisSonucu(SonucTuru tur, T? veri, string? mesaj) : base(tur, mesaj)
    {
        Veri = veri;
    }

    public static ServisSonucu<T> Basari(T veri) => new(SonucTuru.Basarili, veri, null);
    public static new ServisSonucu<T> Bulunamadi(string mesaj = "Kayit bulunamadi.") => new(SonucTuru.Bulunamadi, default, mesaj);
    public static new ServisSonucu<T> Cakisma(string mesaj) => new(SonucTuru.Cakisma, default, mesaj);
    public static new ServisSonucu<T> YetkisizErisim(string mesaj) => new(SonucTuru.YetkisizErisim, default, mesaj);
    public static new ServisSonucu<T> GecersizIstek(string mesaj) => new(SonucTuru.GecersizIstek, default, mesaj);
}