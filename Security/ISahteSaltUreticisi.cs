namespace SifreYoneticiAPI.Security;

/// <summary>
/// SADECE "kullanici adi kesfi" (enumeration) saldirisina karsi sahte salt
/// uretmekten sorumlu, tek amacli bir servis. Var olmayan bir kullanici
/// adi icin bile HER ZAMAN AYNI (tutarli) ama gercek gibi gorunen bir salt
/// dondurur, boylece disaridaki biri "bu salt gercek mi sahte mi" ayrimini
/// yapamaz.
/// </summary>
public interface ISahteSaltUreticisi
{
    string Uret(string username);
}