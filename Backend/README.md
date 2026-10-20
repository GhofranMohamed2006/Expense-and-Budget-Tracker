# Pennywise integrated backend

`Pennywise.Api.csproj` is the supported ASP.NET Core 8 application. From the
repository root, provision `Jwt:Key` with .NET user-secrets (see the root README),
then run:

```powershell
dotnet restore Backend\Pennywise.Api.csproj
dotnet run --project Backend\Pennywise.Api.csproj --launch-profile http
```

Open `http://localhost:5248/`; Swagger is at `/swagger` in Development. The app
serves the frontend and legacy login/register assets from the same origin. No CORS
configuration or separate authentication server is used.

All financial endpoints require a bearer token except registration and login:

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | Register and authenticate |
| POST | `/api/auth/logout` | Revoke the current bearer token |
| GET | `/api/dashboard` | Account totals, recent transactions, current budgets |
| GET/POST/PUT/DELETE | `/api/transactions[/{id}]` | User-owned transactions |
| GET/POST/PUT/DELETE | `/api/categories[/{id}]` | User-owned income/expense categories |
| GET/POST/PUT/DELETE | `/api/budgets[/{id}]` | Budgets by expense category; spending reflects this month's transactions |
| POST | `/api/budgets/{id}/expenses` | Record an expense as a transaction |
| GET/POST/PUT/DELETE | `/api/goals[/{id}]` | User-owned savings goals |
| POST | `/api/goals/{id}/contributions` | Add a savings contribution |
| GET | `/api/reports` | Twelve months of persisted income and expenses |
| GET/PUT | `/api/settings/profile`, `/api/settings/preferences` | Profile and preferences |
| POST | `/api/settings/password` | Change password |

Startup applies an additive EF Core migration. The existing unowned
`BudgetCategories` and `Goals` tables are retained as-is; newly created, user-owned
data lives in `UserBudgets` and `UserGoals`. No existing database is deleted or
recreated. Legacy records without an owner are not visible to any user.
