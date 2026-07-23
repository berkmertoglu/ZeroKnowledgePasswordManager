/* ==============================================================
   FRONTEND KRIPTOGRAFI KONSEPTI (Web Crypto API)
   ==============================================================
   Python projesindeki SifrelemeYoneticisi sinifinin tarayici karsiligidir.
   HICBIR ek kutuphane (node-forge vb.) gerekmez; modern tum tarayicilarda
   yerlesik olarak bulunan window.crypto.subtle kullanilir.

   Python <-> JS Karsilik Tablosu:
   ------------------------------------------------------------
   rsa.generate_private_key()        -> crypto.subtle.generateKey("RSA-OAEP")
   hashlib.sha256(salt+sifre)        -> crypto.subtle.digest("SHA-256", ...)
   BestAvailableEncryption(master)   -> PBKDF2 (master sifreden AES anahtari
                                         turet) + AES-GCM ile private key'i sifrele
   public_key.encrypt() / decrypt()  -> crypto.subtle.encrypt/decrypt("RSA-OAEP")
   ==============================================================
   ONEMLI: Bu dosyadaki fonksiyonlar SADECE tarayicida calisir ve Ana Sifre
   ile cozulmus Private Key ASLA backend'e gonderilmez, ASLA localStorage'a
   duz metin yazilmaz. Sadece o an gereken islem icin bellekte (JS degiskeninde)
   tutulup, sayfa yenilendiginde / cikis yapildiginda kaybolmasi hedeflenir.
   ============================================================== */


// ------------------------------------------------------------
// YARDIMCI FONKSIYONLAR: ArrayBuffer <-> Base64 donusumleri
// (Python'daki base64.b64encode / b64decode karsiligi)
// ------------------------------------------------------------
function arrayBufferToBase64(buffer) {
    const bytes = new Uint8Array(buffer);
    let binary = "";
    for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return btoa(binary);
}

function base64ToArrayBuffer(base64) {
    const binary = atob(base64);
    const bytes = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        bytes[i] = binary.charCodeAt(i);
    }
    return bytes.buffer;
}

// SPKI (public key) DER verisini standart PEM formatina sarar.
// Boylece backend'e gonderilen PublicKeyPem, Python'daki
// "-----BEGIN PUBLIC KEY-----" ciktisiyla ayni formatta olur.
function derToPem(derBuffer, etiket) {
    const base64 = arrayBufferToBase64(derBuffer);
    const satirlar = base64.match(/.{1,64}/g).join("\n");
    return `-----BEGIN ${etiket}-----\n${satirlar}\n-----END ${etiket}-----`;
}


// ------------------------------------------------------------
// 1) RSA-OAEP ANAHTAR CIFTI URETIMI
// Python karsiligi: SifrelemeYoneticisi.rsa_anahtar_ciftini_uret()
// ------------------------------------------------------------
async function rsaAnahtarCiftiUret() {
    // Not: extractable=true olmali, aksi halde exportKey() ile
    // PEM/DER formatina cevirip sunucuya gonderemeyiz.
    const anahtarCifti = await crypto.subtle.generateKey(
        {
            name: "RSA-OAEP",
            modulusLength: 2048,           // Python'daki key_size=2048 ile ayni
            publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537, Python ile ayni
            hash: "SHA-256"
        },
        true,                              // extractable
        ["encrypt", "decrypt"]
    );

    return anahtarCifti; // { publicKey, privateKey } -> CryptoKey nesneleri
}


// ------------------------------------------------------------
// 2) ANA SIFREDEN AES ANAHTARI TURETME (PBKDF2)
// Python'da BestAvailableEncryption bunu kutuphane icinde otomatik
// yapiyordu; JS tarafinda bunu ACIKCA kendimiz yapmamiz gerekiyor.
// ------------------------------------------------------------
async function anaSifredenAesAnahtariTuret(anaSifre, saltBytes) {
    const encoder = new TextEncoder();

    // Once ana sifreyi "ham" bir PBKDF2 anahtarina donustur
    const temelAnahtar = await crypto.subtle.importKey(
        "raw",
        encoder.encode(anaSifre),
        { name: "PBKDF2" },
        false,
        ["deriveKey"]
    );

    // Sonra bu temel anahtardan gercek AES-256 anahtarini turet
    return await crypto.subtle.deriveKey(
        {
            name: "PBKDF2",
            salt: saltBytes,
            iterations: 210000,   // OWASP 2023+ onerisi (SHA-256 icin)
            hash: "SHA-256"
        },
        temelAnahtar,
        { name: "AES-GCM", length: 256 },
        false,
        ["encrypt", "decrypt"]
    );
}


// ------------------------------------------------------------
// 3) PRIVATE KEY'I ANA SIFRE ILE KILITLEME (AES-GCM)
// Python karsiligi: SifrelemeYoneticisi.private_key_sifreli_pem()
//   (orada BestAvailableEncryption tek satirda hallediyordu, burada
//    PBKDF2 + AES-GCM adimlarini elle yapiyoruz)
// ------------------------------------------------------------
async function privateKeyiKilitle(privateKey, anaSifre) {
    // Private key'i once "duz" PKCS8 formatina cikar (henuz sifresiz)
    const pkcs8Buffer = await crypto.subtle.exportKey("pkcs8", privateKey);

    // Rastgele salt (AES anahtari turetmek icin) ve IV (GCM icin) uret.
    // NOT: Bu salt, Kullanicilar.Salt (master_hash icin kullanilan salt) ile
    // AYNI DEGILDIR -- bu, sadece bu AES islemine ozel ayri bir salt'tir.
    const aesSalt = crypto.getRandomValues(new Uint8Array(16));
    const iv = crypto.getRandomValues(new Uint8Array(12)); // AES-GCM icin standart 12 byte

    const aesAnahtari = await anaSifredenAesAnahtariTuret(anaSifre, aesSalt);

    const sifreliBuffer = await crypto.subtle.encrypt(
        { name: "AES-GCM", iv: iv },
        aesAnahtari,
        pkcs8Buffer
    );

    // Backend'e tek bir JSON metni olarak gonderilecek "sifreli paket".
    // Cozerken bu ucune de (data + salt + iv) ihtiyac var.
    const paket = {
        data: arrayBufferToBase64(sifreliBuffer),
        salt: arrayBufferToBase64(aesSalt),
        iv: arrayBufferToBase64(iv)
    };

    // EncryptedPrivateKeyPem alanina JSON string olarak yaziyoruz.
    return JSON.stringify(paket);
}


// ------------------------------------------------------------
// 4) PRIVATE KEY'I ANA SIFRE ILE ACMA (login sirasinda kullanilir)
// Python karsiligi: SifrelemeYoneticisi.private_key_coz()
// ------------------------------------------------------------
async function privateKeyiCoz(encryptedPrivateKeyJson, anaSifre) {
    const paket = JSON.parse(encryptedPrivateKeyJson);

    const aesSalt = base64ToArrayBuffer(paket.salt);
    const iv = base64ToArrayBuffer(paket.iv);
    const sifreliBuffer = base64ToArrayBuffer(paket.data);

    const aesAnahtari = await anaSifredenAesAnahtariTuret(anaSifre, aesSalt);

    // ANA SIFRE YANLISSA burada exception firlar (AES-GCM dogrulamasi basarisiz olur)
    // -> Python'daki "yanlis sifre ile private key cozulemez" davranisiyla birebir ayni.
    const pkcs8Buffer = await crypto.subtle.decrypt(
        { name: "AES-GCM", iv: iv },
        aesAnahtari,
        sifreliBuffer
    );

    // Cozulen PKCS8 verisini tekrar kullanilabilir bir CryptoKey nesnesine cevir
    return await crypto.subtle.importKey(
        "pkcs8",
        pkcs8Buffer,
        { name: "RSA-OAEP", hash: "SHA-256" },
        true,
        ["decrypt"]
    );
}


// ------------------------------------------------------------
// 5) ANA SIFRENIN HASH'INI HESAPLAMA (SHA-256 + salt)
// Python karsiligi: SifrelemeYoneticisi.master_hashle()
// Backend'e SADECE bu hash gonderilir, ana sifrenin kendisi ASLA gitmez.
// ------------------------------------------------------------
async function anaSifreyiHashle(anaSifre, saltHex) {
    const encoder = new TextEncoder();
    const veri = encoder.encode(saltHex + anaSifre); // Python'daki (salt + master_sifre) ile ayni siralama
    const hashBuffer = await crypto.subtle.digest("SHA-256", veri);

    // hex string'e cevir (Python'daki hashlib...hexdigest() ile ayni format)
    return Array.from(new Uint8Array(hashBuffer))
        .map(b => b.toString(16).padStart(2, "0"))
        .join("");
}


// ==============================================================
// ORNEK KULLANIM (Kayit / Register akisi)
// ==============================================================
async function ornekKayitAkisi(username, anaSifre) {
    // 1) Rastgele bir salt uret (hex string olarak, Python'daki secrets.token_hex(16) gibi)
    const saltBytes = crypto.getRandomValues(new Uint8Array(16));
    const saltHex = Array.from(saltBytes).map(b => b.toString(16).padStart(2, "0")).join("");

    // 2) Ana sifreyi hashle (backend'e SADECE bu gidecek)
    const masterHash = await anaSifreyiHashle(anaSifre, saltHex);

    // 3) Bu kullaniciya OZEL RSA anahtar cifti uret
    const { publicKey, privateKey } = await rsaAnahtarCiftiUret();

    // 4) Public key'i PEM formatina cevir (duz metin, backend'e acikca gider)
    const publicKeyDer = await crypto.subtle.exportKey("spki", publicKey);
    const publicKeyPem = derToPem(publicKeyDer, "PUBLIC KEY");

    // 5) Private key'i Ana Sifre ile kilitle (backend bunu ASLA cozemez)
    const encryptedPrivateKeyPem = await privateKeyiKilitle(privateKey, anaSifre);

    // 6) Backend'e gonderilecek istek govdesi (RegisterRequestDto ile birebir eslesir)
    const istekGovdesi = {
        username: username,
        masterHash: masterHash,
        salt: saltHex,
        publicKeyPem: publicKeyPem,
        encryptedPrivateKeyPem: encryptedPrivateKeyPem
    };

    console.log("POST /api/auth/register govdesi hazir:", istekGovdesi);

    // Gercek kullanimda:
    // await fetch("/api/auth/register", {
    //     method: "POST",
    //     headers: { "Content-Type": "application/json" },
    //     body: JSON.stringify(istekGovdesi)
    // });

    return istekGovdesi;
}


// Sadece dogrulama/test amacli disariya aciyoruz (Node.js ortaminda calistirmak icin)
if (typeof module !== "undefined") {
    module.exports = {
        rsaAnahtarCiftiUret,
        anaSifredenAesAnahtariTuret,
        privateKeyiKilitle,
        privateKeyiCoz,
        anaSifreyiHashle,
        ornekKayitAkisi
    };
}