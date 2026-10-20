# Pennywise.Api (ASP.NET Core 8 + EF Core SQLite + JWT)

## Run
    dotnet restore
    dotnet run
Open Swagger at the URL printed in the console + /swagger (e.g. http://localhost:5xxx/swagger).
The SQLite file `pennywise.db` is created automatically on first run.

## Endpoints (all except /auth/* need `Authorization: Bearer <token>`)
| Method | Route | Purpose |
|---|---|---|
| POST | /api/auth/register | Create account (seeds default categories) |
| POST | /api/auth/login | Get JWT |
| GET / PUT | /api/settings/profile | Profile tab |
| POST | /api/settings/password | Change Password tab |
| GET / PUT | /api/settings/preferences | Preferences tab |
| GET | /api/categories?type=expense\|income | List with transactionCount |
| POST | /api/categories | Add category |
| PUT | /api/categories/{id} | Edit category |
| DELETE | /api/categories/{id} | Delete (409 if it has transactions) |

## Frontend example
    const API = "http://localhost:5000";           // use your port
    const token = localStorage.getItem("token");
    const cats = await fetch(`${API}/api/categories?type=expense`,
      { headers: { Authorization: `Bearer ${token}` } }).then(r => r.json());

## Notes
- Change `Jwt:Key` before deploying (use user-secrets / environment variables).
- Add your frontend origin to `Cors:Origins` in appsettings.json. Opening the HTML via file:// will not work with CORS; serve it (e.g. VS Code Live Server on port 5500).
- `EnsureCreated()` is for development; switch to EF migrations for production.
