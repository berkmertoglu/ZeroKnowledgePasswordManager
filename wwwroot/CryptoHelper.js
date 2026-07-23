/* ==============================================================
   CryptoHelper.js
   ==============================================================
   Tarayici tarafinda calisan TUM kriptografik islemler burada.
   Web Crypto API kullanilir (window.crypto.subtle) -- JSEncrypt veya
   node-forge gibi harici bir kutuphaneye ihtiyac YOKTUR, modern her
   tarayicida yerlesik olarak bulunur.

   ZERO-KNOWLEDGE MIMARISI -- BU DOSYA PROJENIN KALBIDIR:
   - RSA anahtar cifti (Public/Private) SADECE burada, tarayicida uretilir.
   - Private Key, backend'e gitmeden ONCE Ana Sifreden turetilen bir AES
     anahtariyla kilitlenir (AES-GCM). Backend bu kilidi ASLA acamaz.
   - Sifreler (VaultItem.EncryptedPassword) Public Key ile burada sifrelenir.
   - Sifre COZME islemi de SADECE burada, cozulmus Private Key (bellekte,
     RAM'de) kullanilarak yapilir. Duz metin sifre HICBIR ZAMAN backend'e
     geri gonderilmez, localStorage'a yazilmaz.

   Python projesindeki SifrelemeYoneticisi sinifinin tarayici karsiligidir.
   ============================================================== */

const CryptoHelper = (function () {

    // ------------------------------------------------------------
    // YARDIMCI: ArrayBuffer <-> Base64 donusumleri
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

    // ------------------------------------------------------------
    // YARDIMCI: Public Key <-> PEM donusumleri
    // (Private Key icin gercek PEM kullanmiyoruz; onu kendi JSON
    //  paketimizle AES-GCM ile sifreliyoruz -- asagida aciklanmistir)
    // ------------------------------------------------------------
    function derToPem(derBuffer, etiket) {
        const base64 = arrayBufferToBase64(derBuffer);
        const satirlar = base64.match(/.{1,64}/g).join("\n");
        return `-----BEGIN ${etiket}-----\n${satirlar}\n-----END ${etiket}-----`;
    }

    function pemToDer(pem) {
        const base64 = pem
            .replace(/-----BEGIN [^-]+-----/, "")
            .replace(/-----END [^-]+-----/, "")
            .replace(/\s+/g, "");
        return base64ToArrayBuffer(base64);
    }

    // ------------------------------------------------------------
    // 1) RSA-OAEP ANAHTAR CIFTI URETIMI (kayit sirasinda bir kere)
    // ------------------------------------------------------------
    async function rsaAnahtarCiftiUret() {
        return await crypto.subtle.generateKey(
            {
                name: "RSA-OAEP",
                modulusLength: 2048,
                publicExponent: new Uint8Array([0x01, 0x00, 0x01]), // 65537
                hash: "SHA-256"
            },
            true, // extractable: PEM/JSON'a cevirebilmek icin sart
            ["encrypt", "decrypt"]
        );
    }

    // Public Key (CryptoKey) -> PEM string (backend'e acikca gonderilecek)
    async function publicKeyiPemYap(publicKey) {
        const der = await crypto.subtle.exportKey("spki", publicKey);
        return derToPem(der, "PUBLIC KEY");
    }

    // Backend'den gelen PEM string -> tekrar kullanilabilir CryptoKey
    async function pemdenPublicKeyYukle(pem) {
        const der = pemToDer(pem);
        return await crypto.subtle.importKey(
            "spki",
            der,
            { name: "RSA-OAEP", hash: "SHA-256" },
            true,
            ["encrypt"]
        );
    }

    // ------------------------------------------------------------
    // 2) ANA SIFREDEN AES ANAHTARI TURETME (PBKDF2)
    // ------------------------------------------------------------
    async function anaSifredenAesAnahtariTuret(anaSifre, saltBytes) {
        const encoder = new TextEncoder();

        const temelAnahtar = await crypto.subtle.importKey(
            "raw",
            encoder.encode(anaSifre),
            { name: "PBKDF2" },
            false,
            ["deriveKey"]
        );

        return await crypto.subtle.deriveKey(
            {
                name: "PBKDF2",
                salt: saltBytes,
                iterations: 210000, // OWASP 2023+ onerisi
                hash: "SHA-256"
            },
            temelAnahtar,
            { name: "AES-GCM", length: 256 },
            false,
            ["encrypt", "decrypt"]
        );
    }

    // ------------------------------------------------------------
    // 3) PRIVATE KEY'I ANA SIFRE ILE KILITLEME (kayit sirasinda)
    // Donen JSON string, oldugu gibi EncryptedPrivateKeyPem alaninda saklanir.
    // ------------------------------------------------------------
    async function privateKeyiKilitle(privateKey, anaSifre) {
        const pkcs8Buffer = await crypto.subtle.exportKey("pkcs8", privateKey);

        const aesSalt = crypto.getRandomValues(new Uint8Array(16));
        const iv = crypto.getRandomValues(new Uint8Array(12));

        const aesAnahtari = await anaSifredenAesAnahtariTuret(anaSifre, aesSalt);

        const sifreliBuffer = await crypto.subtle.encrypt(
            { name: "AES-GCM", iv: iv },
            aesAnahtari,
            pkcs8Buffer
        );

        return JSON.stringify({
            data: arrayBufferToBase64(sifreliBuffer),
            salt: arrayBufferToBase64(aesSalt),
            iv: arrayBufferToBase64(iv)
        });
    }

    // ------------------------------------------------------------
    // 4) PRIVATE KEY'I ANA SIFRE ILE ACMA (giris sirasinda)
    // Ana sifre yanlissa burada exception firlar (AES-GCM dogrulamasi
    // basarisiz olur) -> cagiran kod bunu yakalayip "yanlis sifre" gostermeli.
    // ------------------------------------------------------------
    async function privateKeyiCoz(encryptedPrivateKeyJson, anaSifre) {
        const paket = JSON.parse(encryptedPrivateKeyJson);

        const aesSalt = base64ToArrayBuffer(paket.salt);
        const iv = base64ToArrayBuffer(paket.iv);
        const sifreliBuffer = base64ToArrayBuffer(paket.data);

        const aesAnahtari = await anaSifredenAesAnahtariTuret(anaSifre, aesSalt);

        const pkcs8Buffer = await crypto.subtle.decrypt(
            { name: "AES-GCM", iv: iv },
            aesAnahtari,
            sifreliBuffer
        );

        return await crypto.subtle.importKey(
            "pkcs8",
            pkcs8Buffer,
            { name: "RSA-OAEP", hash: "SHA-256" },
            true,
            ["decrypt"]
        );
    }

    // ------------------------------------------------------------
    // 5) ANA SIFRENIN HASH'INI HESAPLAMA (backend'e SADECE bu gider)
    // ------------------------------------------------------------
    async function anaSifreyiHashle(anaSifre, saltHex) {
        const encoder = new TextEncoder();
        const veri = encoder.encode(saltHex + anaSifre);
        const hashBuffer = await crypto.subtle.digest("SHA-256", veri);

        return Array.from(new Uint8Array(hashBuffer))
            .map(b => b.toString(16).padStart(2, "0"))
            .join("");
    }

    function rastgeleSaltHexUret() {
        const bytes = crypto.getRandomValues(new Uint8Array(16));
        return Array.from(bytes).map(b => b.toString(16).padStart(2, "0")).join("");
    }

    // ------------------------------------------------------------
    // 6) VAULT SIFRELERI ICIN RSA SIFRELEME / COZME
    // ------------------------------------------------------------

    // Public Key ile duz metni sifreler -> base64 (VaultItem.EncryptedPassword)
    async function sifreyiSifrele(publicKey, duzMetin) {
        const encoder = new TextEncoder();
        const sifreliBuffer = await crypto.subtle.encrypt(
            { name: "RSA-OAEP" },
            publicKey,
            encoder.encode(duzMetin)
        );
        return arrayBufferToBase64(sifreliBuffer);
    }

    // Private Key ile base64 sifreli metni cozer -> duz metin
    async function sifreyiCoz(privateKey, sifreliBase64) {
        const sifreliBuffer = base64ToArrayBuffer(sifreliBase64);
        const duzBuffer = await crypto.subtle.decrypt(
            { name: "RSA-OAEP" },
            privateKey,
            sifreliBuffer
        );
        return new TextDecoder().decode(duzBuffer);
    }

    // ------------------------------------------------------------
    // 7) GUVENLI SIFRE URETICI (Python'daki guvenli_sifre_uret karsiligi)
    // ------------------------------------------------------------
    function guvenliSifreUret(uzunluk = 16) {
        const harfler = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const rakamlar = "0123456789";
        const semboller = "!@#$%^&*()-_=+";
        const tumKarakterler = harfler + rakamlar + semboller;

        while (true) {
            let sifre = "";
            const rastgeleDegerler = crypto.getRandomValues(new Uint32Array(uzunluk));
            for (let i = 0; i < uzunluk; i++) {
                sifre += tumKarakterler[rastgeleDegerler[i] % tumKarakterler.length];
            }
            const harfVarMi = [...sifre].some(c => harfler.includes(c));
            const rakamVarMi = [...sifre].some(c => rakamlar.includes(c));
            const sembolVarMi = [...sifre].some(c => semboller.includes(c));
            if (harfVarMi && rakamVarMi && sembolVarMi) {
                return sifre;
            }
            // Sarti saglamazsa dongu tekrar dener (cok nadir gerekir)
        }
    }

    // ------------------------------------------------------------
    // 8) UST DUZEY YARDIMCI: KAYIT ICIN TUM PAKETI TEK SEFERDE HAZIRLA
    // ------------------------------------------------------------
    async function kayitPaketiHazirla(username, anaSifre) {
        const saltHex = rastgeleSaltHexUret();
        const masterHash = await anaSifreyiHashle(anaSifre, saltHex);

        const { publicKey, privateKey } = await rsaAnahtarCiftiUret();

        const publicKeyPem = await publicKeyiPemYap(publicKey);
        const encryptedPrivateKeyPem = await privateKeyiKilitle(privateKey, anaSifre);

        return {
            username,
            masterHash,
            salt: saltHex,
            publicKeyPem,
            encryptedPrivateKeyPem
        };
    }

    // Disariya SADECE gerekli fonksiyonlari aciyoruz
    return {
        rsaAnahtarCiftiUret,
        publicKeyiPemYap,
        pemdenPublicKeyYukle,
        privateKeyiKilitle,
        privateKeyiCoz,
        anaSifreyiHashle,
        rastgeleSaltHexUret,
        sifreyiSifrele,
        sifreyiCoz,
        guvenliSifreUret,
        kayitPaketiHazirla
    };
})();

// Node.js ortaminda test edebilmek icin (tarayicida bu blok calismaz,
// "module" tanimsiz oldugu icin sessizce atlanir)
if (typeof module !== "undefined") {
    module.exports = CryptoHelper;
}