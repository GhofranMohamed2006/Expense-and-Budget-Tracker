# Pennywise Authentication Module — Frontend & Backend

A standalone, downloadable Login and Register module engineered for **Pennywise**, a personal finance and expense-tracking application.

Built following senior full-stack standards with **Vanilla HTML5/CSS3/JavaScript** on the frontend, an ultra-lightweight **C# (.NET 8/10)** backend utilizing `HttpListener`, and a decoupled **SQLite development adapter** designed for drop-in replacement with the main Pennywise database.

---

## 📸 Design & Brand Specifications

Matching the official Pennywise visual identity:

| Token | Value | Purpose |
| :--- | :--- | :--- |
| **Primary Green** | `#165E34` | Headings, brand logo, active outlines |
| **Accent Blue** | `#7E88EF` | Input icons, brand accents |
| **Secondary Text** | `#4A607C` | Subtitles, labels, descriptions |
| **Muted Accent** | `#B4BCC9` | Input borders, placeholders |
| **Global Background** | `#E8F8F0` | Mint-green global background |
| **Card Background** | `#FFFFFF` / `#F5F5F7` | Rounded auth card surface |
| **Button Green** | `#15803D` / `#16A34A` | High-conversion action buttons |

### Layout Highlights
- **Split-Screen Layout**: Left pane displays a custom vector SVG illustration of a blue wallet with gold coins and blooming green leaves inside a mint circular backdrop. Right pane houses the rounded authentication card.
- **Login Screen**: Centered *"Welcome back!"* heading, email with mail icon, password with lock icon and visibility toggle, Remember Me checkbox, full-width green Login button, and navigation to Sign Up.
- **Register Screen**: Centered *"Create your account"* heading, Full Name, Email, Password, Confirm Password with visibility toggles, live password strength meter & rules checklist, Terms & Conditions checkbox with functional interactive modal, and navigation to Login.

---

## 📁 Project Structure

```
PennywiseAuth/
├── Frontend/
│   ├── index.html                   # Redirects to login.html
│   ├── login.html                   # Login page
│   ├── register.html                # Register page
│   ├── dashboard.html               # Configurable Dashboard integration placeholder
│   ├── css/
│   │   ├── style.css                # Brand design tokens, custom properties, reset
│   │   ├── auth.css                 # Shared card, split layout, inputs, buttons
│   │   ├── login.css                # Login-specific styles
│   │   └── register.css             # Register-specific styles (strength meter & modal)
│   ├── js/
│   │   ├── app.js                   # Config (API URL, Dashboard URL) & toast system
│   │   ├── auth.js                  # Fetch API client & validation rules
│   │   ├── login.js                 # Login form lifecycle & Remember Me logic
│   │   └── register.js              # Register form lifecycle & strength evaluator
│   └── assets/
│       ├── images/
│       │   ├── pennywise-logo.svg   # Official Pennywise brand logo
│       │   └── wallet-illustration.svg # Custom vector illustration
│       └── icons/
│           ├── mail.svg, lock.svg, user.svg, eye.svg, eye-off.svg, check.svg, alert-circle.svg
├── Backend/
│   ├── PennywiseAuth.csproj         # Target net8.0 with Microsoft.Data.Sqlite
│   ├── Program.cs                   # HttpListener server & port configuration
│   ├── Routes/
│   │   └── AuthRoutes.cs            # API routing & static frontend file delivery
│   ├── Controllers/
│   │   └── AuthController.cs        # Request handling, CORS, status code mapping
│   ├── Services/
│   │   └── AuthService.cs           # Validation & business logic
│   ├── Models/
│   │   ├── AuthUser.cs              # Internal domain entity
│   │   ├── LoginRequest.cs          # Incoming login DTO
│   │   ├── RegisterRequest.cs       # Incoming register DTO
│   │   └── AuthResponse.cs         # Outgoing safe response & UserProfileDto
│   ├── Interfaces/
│   │   └── IUserRepository.cs       # Decoupled database repository contract
│   ├── Repositories/
│   │   └── SqliteUserRepository.cs  # Development-only SQLite adapter
│   ├── Security/
│   │   └── PasswordHasher.cs        # PBKDF2 HMAC-SHA256 hasher (100k iterations)
│   └── Utils/
│       └── JsonHelper.cs            # camelCase JSON serializer
├── PennywiseAuth.sln                # Visual Studio 2022 Solution
├── PennywiseAuth.slnx               # Modern .NET XML Solution
├── run.bat                          # Starts the backend and opens the frontend on Windows
├── run_tests.ps1                    # Automated 12-step integration test suite
├── .replit                          # One-click Replit execution configuration
├── replit.nix                       # Replit environment dependencies
├── .gitignore                       # Clean repository exclusions
└── README.md                        # Documentation
```

---

## 🚀 How to Run the Project

### Option 1: Running on Windows

#### Prerequisites
- .NET 8.0 SDK or .NET 10.0 SDK installed ([Download .NET](https://dotnet.microsoft.com/download)).

#### One-click startup
Double-click `run.bat` in the project root. It starts the backend, waits for its health endpoint, then opens `http://localhost:5005/index.html` in your browser. The index page redirects to the login screen, and the frontend communicates with the backend on the same server.

> Browsers do not allow an `index.html` file opened directly from disk (`file://`) to start a backend process. Use `run.bat` to start both together; keep its console window open while using the app.

#### Manual startup
Open PowerShell or Command Prompt in the `PennywiseAuth/Backend` directory:
```powershell
cd Backend
dotnet run
```
You will see:
```text
=================================================
   Pennywise Authentication Module Server        
=================================================
[INFO] Serving frontend assets from: ...\Frontend
[INFO] Dashboard destination URL: /dashboard.html
[INFO] Initializing SQLite development repository (Data Source=pennywise_dev.db)...
[INFO] Database repository initialized successfully.
[INFO] Server running and listening on: http://localhost:5005/
[INFO] Open in browser: http://localhost:5005/login.html
```

Then open the app in your browser:
👉 **[http://localhost:5005/login.html](http://localhost:5005/login.html)**

> **Zero Configuration Required**: The lightweight C# `HttpListener` server serves both the **API endpoints** (`/api/auth/*`) and the **Frontend assets** (`/login.html`, `/register.html`, `/css/*`, `/js/*`, `/assets/*`). There are no CORS friction or separate server setups needed!

#### Visual Studio 2022
1. Double click `PennywiseAuth.sln`.
2. Press **F5** or click **Start**.
3. Open `http://localhost:5005/login.html`.

---

### Option 2: Running on Replit

1. Import or open the `PennywiseAuth` repository in Replit.
2. Replit automatically recognizes `.replit` and `replit.nix`.
3. Click the **Run** button.
4. The backend binds to port `5005` with wildcard external ingress (`http://*:5005/`), and the Replit webview will automatically open the login interface.

---

## 🔒 Security Architecture

1. **PBKDF2 Password Hashing**:
   - `Rfc2898DeriveBytes.Pbkdf2` using **HMAC-SHA256**.
   - **100,000 iterations** (OWASP recommended standard).
   - Cryptographically random **32-byte salt** (`RandomNumberGenerator.GetBytes(32)`).
   - **Constant-Time Verification**: Uses `CryptographicOperations.FixedTimeEquals` to prevent timing analysis attacks.
2. **Credential Sanitization**: Passwords, hashes, and salts are never returned in HTTP responses.
3. **SQL Injection Defense**: 100% of SQLite database queries use strictly parameterized SQL commands.
4. **Credential Privacy**: Passwords and tokens are never stored in browser `localStorage`.
5. **Safe Error Masking**: 500 server errors never leak stack traces, database schema, or internal exceptions.

---

## 🗄️ Database & Shared Repository Integration

### Current Development Adapter
The current database layer uses `SqliteUserRepository.cs` connected to `pennywise_dev.db`. This is **strictly a development adapter** to enable genuine, working authentication without mocking.

### Replacing with the Main Pennywise Database
The module is architected with a decoupled repository pattern via `IUserRepository`:

```csharp
public interface IUserRepository
{
    Task InitializeAsync();
    Task<AuthUser?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<AuthUser> CreateUserAsync(AuthUser user);
    Task UpdateLastLoginAsync(int userId, DateTime lastLogin);
}
```

To connect to your team's shared database:
1. Implement `IUserRepository` in your data project (e.g., `PostgresUserRepository`, `SqlServerUserRepository`, or EF Core).
2. Map your existing database columns to `AuthUser`.
3. In `Backend/Program.cs`, replace:
   ```csharp
   IUserRepository userRepository = new SqliteUserRepository(dbConnection);
   ```
   with your production repository:
   ```csharp
   IUserRepository userRepository = new ProductionUserRepository(dbConnection);
   ```
4. **Zero changes** are needed in the authentication controller, services, or frontend!

---

## 📊 Dashboard Integration Notice

- The primary Pennywise Dashboard belongs to another team member and is maintained in a separate module.
- After a successful login, the application redirects to the configured dashboard URL.
- By default, it redirects to the included `dashboard.html` placeholder which demonstrates verified session persistence.
- To point to the real team dashboard, configure the environment variable:
  ```powershell
  $env:PENNYWISE_DASHBOARD_URL = "https://app.pennywise.com/dashboard"
  ```
  or update `dashboardUrl` in `Frontend/js/app.js`.

---

## 🧪 Automated Testing

An automated 12-step integration test suite is included in `run_tests.ps1`:
```powershell
powershell -ExecutionPolicy Bypass -File .\run_tests.ps1
```

### Verified Test Cases
1. `GET /api/auth/health` — Service liveness check (200 OK)
2. `POST /api/auth/register` — Empty payload validation (400 Bad Request)
3. `POST /api/auth/register` — Invalid email format rejection (400 Bad Request)
4. `POST /api/auth/register` — Password confirmation mismatch (400 Bad Request)
5. `POST /api/auth/register` — Password complexity enforcement (400 Bad Request)
6. `POST /api/auth/register` — Genuine user creation & credential withholding (201 Created)
7. `POST /api/auth/register` — Duplicate email conflict prevention (409 Conflict)
8. `POST /api/auth/login` — Missing login fields (400 Bad Request)
9. `POST /api/auth/login` — Incorrect password rejection (401 Unauthorized)
10. `POST /api/auth/login` — Nonexistent email rejection (401 Unauthorized)
11. `POST /api/auth/login` — Successful authentication & last login update (200 OK)
12. `GET /login.html` & `GET /assets/*` — Static file and SVG delivery (200 OK)

---

## 📦 How to Download / Export

A clean, self-contained ZIP archive (`PennywiseAuth.zip`) has been generated containing all source code, SVG assets, solutions, and scripts (excluding `bin/`, `obj/`, and temporary `.db` files). You can extract it anywhere on Windows or Linux and run `dotnet run`.
