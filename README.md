# 🔐 SifreYoneticiAPI — Zero-Knowledge Password Manager

A highly secure, self-hosted password manager built with **C# ASP.NET Core** and **Vanilla JavaScript**.

Unlike standard CRUD applications, this project is engineered with a **Zero-Knowledge Architecture**. The server never stores, sees, or has the ability to read the user's plaintext passwords. All encryption and decryption happen strictly within the user's browser memory (client-side).

![Status](https://img.shields.io/badge/status-active-brightgreen)
![.NET](https://img.shields.io/badge/.NET-8%2F10-512BD4?logo=dotnet)
![SQLite](https://img.shields.io/badge/database-SQLite-003B57?logo=sqlite)
![Security](https://img.shields.io/badge/architecture-Zero--Knowledge-critical)
![License](https://img.shields.io/badge/license-MIT-blue)

---

## 📖 Table of Contents

- [Key Features](#-key-features)
- [How the Zero-Knowledge Crypto Works](#-how-the-zero-knowledge-crypto-works)
- [Technology Stack](#️-technology-stack)
- [Project Structure](#-project-structure)
- [Screenshots](#-screenshots)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Database Schema](#️-database-schema)
- [Roadmap](#-roadmap)
- [License](#-license)

---

## ✨ Key Features

- **Zero-Knowledge Architecture** — The backend only stores encrypted ciphertext (RSA/AES) and a salted hash of the master password. Decrypting the data is mathematically impossible without the user's own Master Password.
- **Clean, Layered Architecture** — The backend is rigorously structured into independent layers (`Controllers`, `Services`, `Repositories`, `Security`, `Mapping`) ensuring high maintainability and fully decoupled business logic.
- **Anti-Timing-Attack & Anti-Enumeration Measures** — Constant-time hash comparison, plus a deterministic dummy-salt generator (`SahteSaltUreticisi`) so that attempting to log in with a non-existent username behaves identically (timing- and response-wise) to a real one, preventing username enumeration.
- **JWT Authentication** — Every protected API endpoint requires a valid JSON Web Token, and every query is automatically scoped to the requesting user.
- **In-Memory Crypto Processing** — Decrypted private keys exist only in the browser's volatile RAM and are never persisted to `localStorage`, `sessionStorage`, or cookies.
- **Brute-Force Protection** — 5 failed login attempts locks the account for 15 minutes.
- **Audit Logging** — Every login (success/failure), lockout, credential add/remove, and master password change is recorded with timestamp, IP address, and event type.
- **Auto-Lock** — After 5 minutes of inactivity, the in-memory private key is wiped and the session is force-locked.
- **Zero-Knowledge Master Password Rotation** — Users can change their Master Password; the private key is re-encrypted with the new password entirely client-side. The server only ever receives password *hashes*, never plaintext, for either the old or new password.
- **Dynamic Vault Management** — Securely store, categorize (with user-defined, emoji-tagged vaults), search, and manage credentials with a modern, dark-themed UI.

---

## 🧠 How the Zero-Knowledge Crypto Works

1. **Registration** — The browser generates a unique RSA `Public Key` / `Private Key` pair. The Private Key is heavily encrypted (AES-256-GCM, key derived via PBKDF2 from the Master Password) *before* it is ever sent to the server.
2. **Login** — The user enters their Master Password. The browser hashes it (SHA-256 + salt) and sends only that hash to the server for verification. The server returns the (still encrypted) Private Key, which the browser decrypts locally, in memory.
3. **Saving a Password** — The browser encrypts the credential (e.g. a Netflix or Instagram password) using the user's own `Public Key`. The server only ever receives and stores the ciphertext.
4. **Viewing a Password** — The server sends the ciphertext to the browser. The browser uses the in-memory `Private Key` to decrypt and display the original password — decryption never happens anywhere near the server.

```
┌───────────────────────────────────────────────────────────┐
│                        BROWSER (Client)                    │
│   CryptoHelper.js                                           │
│   ├─ Generates RSA-2048 key pair                            │
│   ├─ Derives AES key from Master Password (PBKDF2)          │
│   ├─ Encrypts/decrypts the Private Key                      │
│   ├─ Encrypts passwords with the Public Key (RSA-OAEP)       │
│   └─ Decrypts passwords with the Private Key (RAM only)      │
│   Private Key & decrypted passwords: NEVER leave the browser│
└───────────────────────────┬───────────────────────────────┘
                             │  HTTPS (only hashes & ciphertext cross this line)
┌───────────────────────────▼───────────────────────────────┐
│                    ASP.NET CORE WEB API                    │
│  Controllers → Services → Repositories → EF Core → SQLite   │
│  Stores: MasterHash, Salt, EncryptedPrivateKeyPem,           │
│          EncryptedPassword, EncryptedNotes                  │
│  Can NEVER decrypt any of the above.                         │
└───────────────────────────────────────────────────────────┘
```

---

## 🛠️ Technology Stack

**Backend:**
- C# / ASP.NET Core Web API
- Entity Framework Core
- SQLite (easily swappable to PostgreSQL/SQL Server thanks to the Repository pattern)
- JWT Bearer Authentication
- SHA-256 (salted) for Master Password verification hashes — the server never handles a plaintext password

**Frontend:**
- Vanilla JavaScript (ES6+) — no framework, no build step
- Web Crypto API — RSA-OAEP 2048-bit, AES-256-GCM, PBKDF2
- HTML5 & modern CSS (CSS variables, Flexbox/Grid)
- HTML5 Canvas API — custom interactive particle-network background

---

## 📁 Project Structure

```
📦 SifreYoneticiAPI
 ┣ 📂 Controllers    # HTTP Endpoints (AuthController, VaultController, CategoryController, SecurityLogController)
 ┣ 📂 Services       # Business Logic & Orchestration
 ┣ 📂 Repositories   # Data Access Layer (EF Core completely abstracted away)
 ┣ 📂 Security       # Hashing & Timing-Attack Protections
 ┣ 📂 Mapping        # Entity ↔ DTO Extensions
 ┣ 📂 Common         # Shared result-wrapper types (ServisSonucu<T>)
 ┣ 📂 Extensions     # Small reusable helpers (e.g. reading the user ID from JWT claims)
 ┣ 📂 Models         # Database Entities
 ┣ 📂 DTOs           # Data Transfer Objects
 ┣ 📂 Data           # ApplicationDbContext (EF Core)
 ┗ 📂 wwwroot        # Frontend Assets (index.html, CryptoHelper.js)
```

---

## 📸 Screenshots

<table align="center">
<tr>
<td align="center"><b>Welcome / Authentication Screen</b></td>
<td align="center"><b>Main Vault Dashboard</b></td>
</tr>
<tr>
<td><img src="https://github.com/user-attachments/assets/7f4a47f5-815a-486c-a348-10f1cf76945f" width="400" alt="Login Screen"/></td>
<td><img src="https://github.com/user-attachments/assets/a13ffe4b-0679-4357-afac-0cd7d10e17e2" width="400" alt="Main Vault Dashboard"/></td>
</tr>
<tr>
<td align="center"><b>New Password Entry Form</b></td>
<td align="center"><b>New Vault / Category Creation</b></td>
</tr>
<tr>
<td><img src="https://github.com/user-attachments/assets/20a50285-08ae-44f1-a6d5-80a6b4ea5e06" width="400" alt="Add Password"/></td>
<td><img src="https://github.com/user-attachments/assets/216898e1-c142-44ec-ada5-d8a3e9afbdae" width="400" alt="Add Vault"/></td>
</tr>
</table>

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) 8.0 or later
- A modern browser (Chrome, Edge, Firefox) — Web Crypto API support required

### Setup

```bash
# Clone the repository
git clone https://github.com/<your-username>/SifreYoneticiAPI.git
cd SifreYoneticiAPI

# Restore dependencies
dotnet restore

# Apply database migrations (creates the SQLite database)
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API (also serves the frontend from wwwroot)
dotnet run
```

Then open your browser at the URL shown in the terminal (e.g. `http://localhost:5150`) and:

1. Click **"Create New Account"**, choose a username and Master Password.
2. Your browser will generate your personal RSA key pair — this may take a second.
3. You'll be dropped straight into your vault with 6 default categories already set up.
4. Start adding passwords — everything is encrypted before it ever leaves your browser.

### Configuration

`appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=sifre_yoneticisi.db"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS",
    "Issuer": "SifreYoneticisiApi",
    "Audience": "SifreYoneticisiFrontend"
  }
}
```

⚠️ **Never commit a real `Jwt:Key` to source control.** Use user secrets or environment variables in production.

---

## 🔌 API Endpoints

| Method | Endpoint | Auth Required | Description |
|---|---|---|---|
| `GET` | `/api/auth/salt/{username}` | No | Returns the salt needed to compute the login hash |
| `POST` | `/api/auth/register` | No | Registers a new user with client-generated keys |
| `POST` | `/api/auth/login` | No | Authenticates and returns a JWT + encrypted keys |
| `PUT` | `/api/auth/change-password` | Yes | Rotates the Master Password (Zero-Knowledge) |
| `GET` | `/api/vault` | Yes | Lists the current user's encrypted vault items |
| `GET` | `/api/vault/{id}` | Yes | Gets a single encrypted vault item |
| `POST` | `/api/vault` | Yes | Adds a new encrypted vault item |
| `PUT` | `/api/vault/{id}` | Yes | Updates an existing vault item |
| `PATCH` | `/api/vault/{id}/favori` | Yes | Toggles the favorite flag |
| `DELETE` | `/api/vault/{id}` | Yes | Deletes a vault item |
| `GET` | `/api/category` | Yes | Lists the current user's vaults/folders |
| `POST` | `/api/category` | Yes | Creates a new vault/folder |
| `DELETE` | `/api/category/{id}` | Yes | Deletes a vault (items move to "Uncategorized") |
| `GET` | `/api/securitylog` | Yes | Returns the current user's own audit trail |

All authenticated endpoints require an `Authorization: Bearer <token>` header, and every query is automatically scoped to the requesting user — one user can never see or modify another user's data.

---

## 🗃️ Database Schema

```
Users
├── Id (Guid, PK)
├── Username (unique)
├── MasterHash            — SHA-256(salt + password), never the password itself
├── Salt
├── PublicKeyPem           — plaintext, safe to expose
├── EncryptedPrivateKeyPem — AES-GCM encrypted, server cannot decrypt
├── FailedLoginAttempts
└── LockoutEnd

VaultCategories ("Vaults")
├── Id (int, PK)
├── UserId (FK → Users)
├── Name
├── Icon (emoji)
└── CreatedAt

VaultItems
├── Id (Guid, PK)
├── UserId (FK → Users)
├── CategoryId (FK → VaultCategories, nullable — ON DELETE SET NULL)
├── AppName
├── Username
├── EncryptedPassword      — RSA-OAEP encrypted
├── EncryptedNotes         — RSA-OAEP encrypted, optional
├── Url
├── IsFavorite
├── CreatedAt / UpdatedAt

SecurityLogs
├── Id (long, PK)
├── UserId (FK → Users, nullable — SET NULL if user deleted)
├── ActionType (enum: login success/fail, lockout, item added/removed, password changed)
├── IpAddress
├── Timestamp
└── Details
```

---

## 🗺️ Roadmap

- [ ] Migrate from SQLite to PostgreSQL for production deployments
- [ ] Server-side rehashing of `MasterHash` (e.g. with BCrypt/Argon2) as defense-in-depth against pass-the-hash replay
- [ ] IP-based rate limiting middleware (in addition to the existing per-account lockout)
- [ ] Two-Factor Authentication (TOTP)
- [ ] Encrypted export/import of the vault
- [ ] Passkey / WebAuthn support

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgements

Built as an iterative learning project: starting from a single-file Python CLI tool, evolving through a Tkinter desktop GUI, and finally rebuilt as a full-stack web application with a layered ASP.NET Core backend — with a strong emphasis on getting the Zero-Knowledge cryptography **actually correct**, not just theoretically described.
