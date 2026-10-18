# Pennywise API

ASP.NET Core 8 Web API and SQLite backend for the Pennywise Transactions page.

## Run

```bash
cd PennywiseApi
dotnet run
```

Open the URL shown in the console. It serves the Transactions page (`wwwroot/index.html`), already connected to the API, and `/swagger` lists the endpoints. The database file `pennywise.db` is created on first start and filled with the same 14 example rows the page shows.

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

List and summary take the same optional filters:
`type` (`income` or `expense`), `category`, `account`, `search` (search the description, `q` also works), `from`, `to` (`yyyy-MM-dd`).

Example: `GET /api/transactions?type=expense&category=Dining&search=cafe`

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

## The page

`wwwroot/index.html` is the Transactions page. All requests go through one small client in its script (`api.list`, `api.summary`, `api.create`, `api.update`, `api.remove`, `api.lookups`). Each row has a trash button: the first click asks "Delete?", the second deletes. It shows a loading state, an empty state, field errors from the API, and a banner when the API cannot be reached. Open it from the address `dotnet run` prints, not as a local file.

## Configuration

| Setting | Where | Purpose |
|---|---|---|
| `ConnectionStrings:Default` | `appsettings.json` or env `ConnectionStrings__Default` | Database file, default `Data Source=pennywise.db` |
| `Cors:AllowedOrigins` | `appsettings.json` or env `Cors__AllowedOrigins__0`, `__1`, ... | Origins allowed to call the API from another host. Not needed when the API serves the page. Development allows any origin. |
| `apiBaseUrl` | `wwwroot/config.js` | API address for the page. Leave empty when the API serves the page. A static page cannot read server environment variables, so this file is the deploy-time setting. |
| `ASPNETCORE_ENVIRONMENT` | env | `Development` enables `/swagger` and open CORS |

## Not included yet

Login and per-user data (the Register and Login pages), budgets, categories as an editable table, and migrations (the database is created with `EnsureCreated`).
