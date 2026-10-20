# Pennywise API

ASP.NET Core 8 Web API and SQLite backend for the Pennywise Transactions page.

## Run

```bash
cd PennywiseApi
dotnet run
```

Open the URL shown in the console followed by `/swagger` (Development only) to try the endpoints. The database file `pennywise.db` is created on first start and filled with 14 example transactions.

Without a `launchSettings.json` the app starts in Production. Set `ASPNETCORE_ENVIRONMENT=Development` to enable Swagger and open CORS.

## Endpoints

| Method | Route | What it does |
|---|---|---|
| GET | `/api/transactions` | List, newest first |
| GET | `/api/transactions/summary` | Income, expenses, net, counts, saved % |
| GET | `/api/transactions/{id}` | One transaction |
| POST | `/api/transactions` | Create |
| PUT | `/api/transactions/{id}` | Replace |
| DELETE | `/api/transactions/{id}` | Delete |
| GET | `/api/lookups` | Categories and accounts for the dropdowns |
| GET | `/api/reports/overview` | Totals, savings rate, average daily spend, change vs the previous period |
| GET | `/api/reports/monthly` | Income and expenses per calendar month |
| GET | `/api/reports/categories` | Totals per category, largest first |

List and summary take the same optional filters:
`type` (`income` or `expense`), `category`, `account`, `search` (search the description, `q` also works), `from`, `to` (`yyyy-MM-dd`).

Example: `GET /api/transactions?type=expense&category=Dining&search=cafe`

### Reports

| Route | Query | Notes |
|---|---|---|
| `/api/reports/overview` | `from`, `to` (`yyyy-MM-dd`) | With both dates it also returns `previous` (the same number of days right before) and the percent change. `from` after `to` returns `400`. |
| `/api/reports/monthly` | `months` (1 to 24, default 6), `to` | The last `months` calendar months ending at `to` (default today), oldest first. Empty months are zeros. |
| `/api/reports/categories` | `from`, `to`, `type` (`expense` default, or `income`) | `percent` is the share of all transactions of that type in the period. |

Example: `GET /api/reports/overview?from=2026-10-01&to=2026-10-19`

## Request body (POST and PUT)

```json
{
  "description": "Koshary lunch",
  "amount": 74.5,
  "type": "expense",
  "category": "Dining",
  "account": "Cash wallet",
  "date": "2026-10-14"
}
```

Invalid input returns `400` with the field errors. An unknown id returns `404`.

## Configuration

| Setting | Where | Purpose |
|---|---|---|
| `ConnectionStrings:Default` | `appsettings.json` or env `ConnectionStrings__Default` | Database file, default `Data Source=pennywise.db` |
| `Cors:AllowedOrigins` | `appsettings.json` or env `Cors__AllowedOrigins__0`, `__1`, ... | Front-end origins allowed to call the API, for example `http://localhost:5173`. Required outside Development. Development allows any origin. |
| `ASPNETCORE_ENVIRONMENT` | env | `Development` enables `/swagger` and open CORS |

## Not included yet

Login and per-user data (the Register and Login pages), budgets, categories as an editable table, and migrations (the database is created with `EnsureCreated`).
