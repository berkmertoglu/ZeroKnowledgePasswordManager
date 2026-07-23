# 🛡️ Zero-Knowledge Password Manager

![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-10.0-239120?logo=csharp)
![JavaScript](https://img.shields.io/badge/Vanilla_JS-F7DF1E?logo=javascript&logoColor=black)
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite)
![Architecture](https://img.shields.io/badge/Architecture-Clean-brightgreen)

A highly secure, self-hosted password manager built with **C# ASP.NET Core** and **Vanilla JavaScript**. 

Unlike standard CRUD applications, this project is engineered with a **Zero-Knowledge Architecture**. The server **never** stores, sees, or has the ability to read the user's plaintext passwords. All encryption and decryption happen strictly within the user's browser memory (Client-Side).

## ✨ Key Features

* **Zero-Knowledge Architecture:** The backend only stores encrypted ciphertext (`AES/RSA` logic) and a hashed master password. Data decryption is mathematically impossible without the user's mind.
* **Clean Architecture:** The backend is rigorously structured into independent layers (`Controllers`, `Services`, `Repositories`, `Security`, `Mapping`) ensuring high maintainability and decoupled business logic.
* **Anti-Timing Attack Measures:** Custom Dummy Hash Checkers (`SahteSaltUreticisi`) are implemented to prevent user enumeration attacks during the login process.
* **JWT Authentication:** Secure API endpoints protected by JSON Web Tokens.
* **In-Memory Crypto Processing:** Decrypted private keys exist only in the browser's volatile RAM and are never persisted to `localStorage` or `cookies`.
* **Dynamic Vault Management:** Securely store, categorize, search, and manage credentials with a modern, dark-themed UI.

## 🧠 How the Zero-Knowledge Crypto Works

1. **Registration:** The browser generates a unique `Public Key` and `Private Key`. The Private Key is heavily encrypted using the user's **Master Password** before being sent to the server. 
2. **Login:** The user inputs their Master Password. The server validates the hash and returns the *Encrypted Private Key*. The browser decrypts it in-memory.
3. **Saving a Password:** The browser encrypts the Netflix/Instagram password using the `Public Key`. The server only receives and stores the ciphertext.
4. **Viewing a Password:** The server sends the ciphertext to the browser. The browser uses the in-memory `Private Key` to decrypt and display the original password.

## 🛠️ Technology Stack

**Backend:**
* C# / ASP.NET Core Web API
* Entity Framework Core
* SQLite (Easily swappable to PostgreSQL/SQL Server due to Repository Pattern)
* BCrypt.Net (for Master Password Hashing)

**Frontend:**
* Vanilla JavaScript (ES6+)
* Web Crypto API
* HTML5 & Modern CSS (CSS Variables, Flexbox/Grid)

## 📂 Architecture Overview

```text
📦 SifreYoneticiAPI
 ┣ 📂 Controllers    # HTTP Endpoints (AuthController, VaultController)
 ┣ 📂 Services       # Business Logic & Orchestration
 ┣ 📂 Repositories   # Data Access Layer (EF Core completely abstracted)
 ┣ 📂 Security       # Hashing, Timing Attack Protections
 ┣ 📂 Mapping        # Entity ↔ DTO Extensions
 ┣ 📂 Models         # Database Entities
 ┣ 📂 DTOs           # Data Transfer Objects
 ┗ 📂 wwwroot        # Frontend Assets (index.html, JS, CSS)


 ## 📸 Screenshots
<table align="center">
  <tr>
    <td align="center"><b>Welcome / Authentication Screen</b></td>
    <td align="center"><b>Main Vault Dashboard</b></td>
  </tr>
  <tr>
    <td align="center"><img src="https://github.com/user-attachments/assets/7f4a47f5-815a-486c-a348-10f1cf76945f" width="400" alt="Giriş Ekranı"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/a13ffe4b-0679-4357-afac-0cd7d10e17e2" width="400" alt="Ana Kasa Paneli"/></td>
  </tr>
  <tr>
    <td align="center"><b>New Password Entry Form</b></td>
    <td align="center"><b>New Vault / Category Creation</b></td>
  </tr>
  <tr>
    <td align="center"><img src="https://github.com/user-attachments/assets/20a50285-08ae-44f1-a6d5-80a6b4ea5e06" width="400" alt="Şifre Ekleme"/></td>
    <td align="center"><img src="https://github.com/user-attachments/assets/216898e1-c142-44ec-ada5-d8a3e9afbdae" width="400" alt="Kasa Ekleme"/></td>
  </tr>
</table>


