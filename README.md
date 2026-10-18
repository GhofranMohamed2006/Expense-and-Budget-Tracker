# 💰 Pennywise — Expense & Budget Tracker

### Make Every Pound Count.

Pennywise is a personal finance management web application designed to help users track income and expenses, manage monthly budgets, organize transactions, analyze spending habits, and monitor savings goals through one centralized platform.

The project is developed collaboratively, with separate modules integrated into a unified application.

**Repository:** [Expense-and-Budget-Tracker](https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker)

---

## 📑 Table of Contents

- [About the Project](#-about-the-project)
- [Project Objectives](#-project-objectives)
- [Application Features](#-application-features)
- [Technology Stack](#-technology-stack)
- [Application Architecture](#-application-architecture)
- [Project Structure](#-project-structure)
- [Database Overview](#-database-overview)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Team Workflow](#-team-workflow)
- [Development Guidelines](#-development-guidelines)
- [Security Considerations](#-security-considerations)
- [Testing](#-testing)
- [Future Improvements](#-future-improvements)
- [Contributors](#-contributors)
- [License](#-license)

---

## 📌 About the Project

Managing personal finances requires a clear understanding of income, expenses, budgets, and financial goals.

Pennywise aims to make this process easier by providing a structured application where users can organize financial activities, understand their spending patterns, and plan their money more effectively.

The application is designed around a modular architecture, separating frontend interfaces, backend business logic, authentication, and data persistence.

### The Problem

People often struggle to:

- Keep track of daily income and expenses.
- Understand where their money goes.
- Stay within monthly spending limits.
- Compare financial activity over time.
- Set savings goals and monitor progress.

### The Solution

Pennywise brings these activities together in one application, providing a foundation for better financial organization and more informed decisions.

---

## 🎯 Project Objectives

The main objectives are to:

1. Build a centralized personal finance management application.
2. Provide secure user registration and authentication.
3. Organize income and expenses into manageable transactions.
4. Support monthly budgets and spending categories.
5. Present financial summaries and reports.
6. Help users define and monitor savings goals.
7. Build a maintainable application using modular development practices.
8. Enable collaborative development through Git and GitHub.

---

## ✨ Application Features

The following sections describe the project's overall feature scope. Individual features should be considered complete only after their implementation has been integrated and tested.

### 1. 🏠 Landing Page

The public landing page introduces Pennywise and explains its purpose.

Expected functionality:

- Project branding and navigation.
- Overview of the main application features.
- Clear calls to action for registration and login.
- Responsive layout for desktop and mobile devices.

### 2. 🔐 Authentication & User Management

Provides the foundation for user access and account management.

Expected functionality:

- User registration.
- Login and logout.
- Input validation and authentication error handling.
- Secure password storage.
- Session management.
- User-specific access to protected application pages.
- Account recovery and optional third-party authentication when configured.

### 3. 📊 Dashboard

Provides a central overview of a user's financial activity.

Expected information:

- Total income.
- Total expenses.
- Available balance.
- Savings goal progress.
- Recent transactions.
- Budget summaries.
- Financial charts and trends.

Dashboard values should be calculated from the user's actual stored records.

### 4. 💳 Transaction Management

Allows users to organize their financial activity.

Expected functionality:

- Add income and expense transactions.
- View transaction history.
- Edit transaction details.
- Delete transactions with confirmation.
- Assign categories.
- Search and filter transactions.
- Sort records by date, amount, or other supported fields.
- Filter by transaction type and date range.

### 5. 📅 Budget Management

Supports planning and monitoring spending limits.

Expected functionality:

- Create monthly budgets.
- Assign budgets to expense categories.
- Edit and delete budgets.
- Calculate spending against budget limits.
- Display remaining budget amounts.
- Track budget utilization and progress.

### 6. 🗂️ Category Management

Organizes financial transactions into meaningful groups.

Examples include:

**Expense categories**
- Food & Drinks
- Transportation
- Shopping
- Bills
- Healthcare
- Education
- Entertainment
- Housing

**Income categories**
- Salary
- Freelancing
- Investments
- Gifts
- Other Income

Users should be able to manage supported categories without accessing another user's private records.

### 7. 📈 Reports & Analytics

Helps users understand their financial habits through summaries and reports.

Planned capabilities include:

- Income versus expenses.
- Spending by category.
- Monthly financial summaries.
- Spending trends over time.
- Date-range filtering.
- Report export where implemented.

Reports should use consistent date ranges and real database records.

### 8. 🎯 Saving Goals

Helps users organize and monitor personal savings objectives.

Expected functionality:

- Create savings goals.
- Set a target amount.
- Set an optional target date.
- Record savings contributions.
- Track saved amounts and progress.
- Edit and delete goals.
- View contribution history where supported.

### 9. ⚙️ Profile & Settings

Provides a place for users to manage their account preferences.

Potential settings include:

- Profile information.
- Preferred currency.
- Theme preferences.
- Language preferences.
- Account and authentication settings.

Only settings supported by the integrated implementation should be considered available.

---

## 🛠️ Technology Stack

| Technology | Purpose |
|---|---|
| C# | Backend application logic |
| .NET | Backend runtime and project tooling |
| ASP.NET Core | Used by the separate authentication module where configured |
| HTML5 | Frontend page structure |
| CSS3 | Layout, styling, and responsive design |
| Vanilla JavaScript | Frontend interactions and API communication |
| SQLite | Relational database storage where configured |
| Git | Version control |
| GitHub | Repository hosting and collaboration |
| Visual Studio / Visual Studio Code | Development environment |

**Architecture note:** The team should confirm and document the final backend hosting approach during integration. The main application and the separate authentication module may use different server implementations until their integration is completed.

---

## 🏗️ Application Architecture

Pennywise is organized into three main application layers.

### Frontend Layer

Responsible for:

- Rendering pages and forms.
- Handling user interactions.
- Validating inputs before submission.
- Communicating with backend endpoints.
- Displaying financial summaries, tables, and charts.
- Showing loading, empty, success, and error states.

### Backend Layer

Responsible for:

- Handling HTTP requests.
- Validating incoming data.
- Implementing business rules.
- Managing authentication and authorization.
- Processing financial transactions.
- Coordinating database operations.
- Returning structured API responses.

### Data Layer

Responsible for:

- Persisting user accounts and settings.
- Storing transactions, categories, budgets, and goals.
- Enforcing relationships and data integrity.
- Supporting reliable financial calculations.
- Isolating each user's financial records.

### Typical Request Flow

```text
User
  |
  v
Frontend (HTML / CSS / JavaScript)
  |
  v
HTTP API
  |
  v
Backend Services
  |
  v
Repositories / Data Access
  |
  v
SQLite Database
```

---

## 📂 Project Structure

The following is a proposed logical structure for the complete project. Adapt the paths to the actual repository as the team integrates its modules.

```text
Expense-and-Budget-Tracker/
│
├── README.md
├── .gitignore
│
├── docs/
│   ├── project-structure.md
│   ├── api-contracts.md
│   └── testing-checklist.md
│
├── Frontend/
│   ├── index.html
│   ├── login.html
│   ├── register.html
│   ├── dashboard.html
│   ├── assets/
│   ├── css/
│   └── js/
│
├── Backend/
│   ├── Controllers/
│   ├── Routes/
│   ├── Interfaces/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Security/
│   ├── Utils/
│   ├── Program.cs
│   └── Backend.csproj
│
├── Database/
│   ├── schema.sql
│   └── migrations/
│
└── tests/
    ├── api/
    └── manual-test-cases.md
```

**Important:** This tree is a reference for the integrated application, not a claim that every directory or file already exists. Preserve existing working files and agree on shared paths before merging team branches.

---

## 🗄️ Database Overview

The database should support the main application entities.

| Entity | Purpose |
|---|---|
| Users | Account information and authentication identifiers |
| Sessions | Active login sessions, if session-based authentication is used |
| Categories | Income and expense classifications |
| Transactions | Income and expense records |
| Budgets | Monthly spending limits |
| SavingGoals | Savings targets and progress |
| SavingGoalContributions | Individual savings contributions, if implemented |
| UserSettings | Currency and application preferences |

### Data Integrity Principles

- Each user's private records must be associated with the correct user.
- Transactions must reference valid categories.
- Budgets must use valid expense categories.
- Financial amounts must be validated.
- Database operations should use parameterized queries.
- Related records should remain consistent when edited or deleted.
- Monetary calculations should use a consistent numeric representation.

The actual database schema must be documented once the final schema and migrations are confirmed.

---

## 🚀 Getting Started

### Prerequisites

Install the tools required by the final project configuration:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/downloads)
- [Visual Studio Code](https://code.visualstudio.com/) or Visual Studio
- A modern web browser

Check your installations:

```bash
dotnet --version
git --version
```

### 1. Clone the Repository

```bash
git clone https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker.git
cd Expense-and-Budget-Tracker
```

### 2. Fetch the Latest Branches

```bash
git fetch origin
git branch -a
```

Switch to the branch agreed upon by the team. For example, to inspect the login and registration branch:

```bash
git switch --track origin/login--register
```

If that branch already exists locally:

```bash
git switch login--register
```

Use the integration branch selected by the team when running the complete application.

### 3. Locate the Backend Project

Find the actual `.csproj` file before restoring dependencies.

For example, if the project file is located in `Backend`:

```bash
cd Backend
dotnet restore
```

If the project uses a solution file, restore from the directory containing that solution instead:

```bash
dotnet restore YourSolution.sln
```

Replace the example solution name with the actual file name.

### 4. Configure the Application

Review the backend configuration and set the required database path, server port, API settings, and authentication configuration.

Do not commit secrets or production credentials.

### 5. Run the Backend

From the directory containing the backend project:

```bash
dotnet run
```

Follow the terminal output to identify the configured local address and port.

### 6. Start the Frontend

Use the frontend serving method specified by the integrated application.

If the backend serves the frontend files itself, start the backend and open its local address in your browser.

If the frontend is served separately, configure its API base URL to match the backend address and port.

### 7. Verify the Application

After startup, verify that:

- The landing page loads.
- Registration and login behave correctly.
- Protected pages require authentication.
- API requests reach the backend.
- Financial records persist in the configured database.
- Errors are displayed clearly.
- The browser console and backend logs do not show unexpected errors.

Exact startup commands may change when the team finalizes the integrated solution structure.

---

## 🔧 Configuration

Configuration depends on the selected backend and authentication implementation.

Potential configuration values include:

| Setting | Purpose |
|---|---|
| `PORT` | Server listening port, if supported |
| `SQLITE_PATH` | Database file location, if supported |
| `APP_BASE_URL` | Public or local application URL |
| `GOOGLE_CLIENT_ID` | Google authentication configuration, if implemented |
| `GOOGLE_CLIENT_SECRET` | Server-side OAuth configuration, if required |
| SMTP settings | Email delivery for recovery flows, if implemented |

Use environment variables or local configuration for sensitive values. Do not place real credentials in this README.

Provide an example configuration file with placeholder values if the team needs one.

---

## 👥 Team Workflow

The project is developed collaboratively using Git and GitHub.

### Branching Strategy

Each contributor should work on a dedicated feature branch.

Example:

```bash
git switch -c feature/transactions
```

Other possible branch names include:

```text
feature/authentication
feature/dashboard
feature/budgets
feature/categories
feature/reports
feature/saving-goals
feature/profile-settings
```

Use the actual branch names agreed upon by the team.

### Commit Changes

```bash
git add .
git commit -m "Implement transaction management"
git push -u origin feature/transactions
```

### Pull Request Process

1. Complete the assigned module.
2. Test the module locally.
3. Push the feature branch to GitHub.
4. Open a Pull Request against the team's agreed target branch.
5. Describe the changes and any dependencies.
6. Request review from the appropriate teammates.
7. Resolve review comments and conflicts.
8. Merge only after the team agrees that the changes are ready.

### Integration Guidelines

Before merging modules:

- Agree on API routes and request/response formats.
- Use consistent naming for database entities and fields.
- Avoid duplicate authentication or database implementations.
- Coordinate changes to shared files.
- Test dependencies between frontend and backend.
- Confirm that the integrated project starts successfully.

**Do not merge every branch directly into `main` automatically.** Follow the repository's agreed review and integration process.

---

## 🧑‍💻 Development Guidelines

To maintain code quality across the team:

- Follow consistent naming conventions.
- Keep controllers and routes focused on HTTP handling.
- Place reusable business logic in services.
- Separate database access from business logic where appropriate.
- Validate data on the server, not only in the browser.
- Keep frontend API communication consistent.
- Avoid duplicating shared components and utility functions.
- Document API changes.
- Write clear commit messages.
- Keep generated build artifacts out of source control.

---

## 🔒 Security Considerations

Because Pennywise handles personal and financial information, security must be considered throughout development.

Recommended practices include:

- Secure password hashing.
- Proper authentication and authorization.
- User-specific access control for all financial records.
- Parameterized database queries.
- Input validation and safe error handling.
- Secure session handling and cookie configuration.
- Protection against unauthorized access to other users' records.
- No hardcoded secrets or credentials.
- HTTPS for production deployment.
- Safe handling of personal information in logs.

These are security requirements and recommendations, not a certification that every control has already been implemented.

---

## 🧪 Testing

Testing should cover each module independently and the integrated application.

### Authentication

- Register a new account.
- Reject duplicate email addresses.
- Test valid and invalid login attempts.
- Verify logout and session behavior.
- Verify protected endpoints reject unauthorized requests.

### Transactions

- Create, edit, search, filter, and delete transactions.
- Validate amounts and required fields.
- Verify totals update after changes.

### Budgets & Categories

- Create and update budgets.
- Verify budget utilization against transactions.
- Validate category assignments.
- Ensure users cannot access another user's records.

### Saving Goals

- Create a savings goal.
- Record contributions where supported.
- Verify totals and progress calculations.
- Reject invalid amounts.

### Dashboard & Reports

- Verify summaries against stored transactions.
- Check date ranges and category totals.
- Confirm charts use real application data.
- Verify empty states when there are no transactions.

### Integration

- Confirm frontend requests match backend routes.
- Confirm database persistence after restart.
- Check navigation between pages.
- Check browser console and server logs.
- Test the integrated branch after every major merge.

Document the tests that have actually been executed and their results.

---

## 🛣️ Future Improvements

Potential future enhancements include:

- Advanced spending analytics.
- CSV export and import.
- Recurring transactions.
- Notifications for budget limits.
- Improved financial reporting.
- Additional currency and localization support.
- Automated testing and continuous integration.
- Deployment and monitoring improvements.

These enhancements are ideas for future development and are not necessarily implemented in the current version.

---

## 👨‍💻 Contributors

Pennywise is a collaborative software development project.

Repository contributors can be viewed on the [GitHub contributors page](https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker/graphs/contributors).

---

## 📄 License

A license has not yet been specified in this README. The repository owners should select an appropriate license and add a `LICENSE` file before defining the project's distribution terms.

---

### 💚 Pennywise

**Make Every Pound Count.**

A collaborative effort to make personal finance management clearer, more organized, and easier to understand.
