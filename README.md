# 💰 Expense & Budget Tracker

**A Personal Finance Management Web Application**

Expense & Budget Tracker is a web application designed to help users manage their personal finances, track income and expenses, organize financial transactions, and gain a clearer understanding of their spending habits.

The project brings together frontend development, backend services, authentication, and financial management features in a modular application architecture.

🔗 **Repository:** [Expense-and-Budget-Tracker](https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker)

---

## 📌 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Technology Stack](#-technology-stack)
- [Project Modules](#-project-modules)
- [Project Structure](#-project-structure)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
- [Configuration](#-configuration)
- [Team Collaboration](#-team-collaboration)
- [Contributing](#-contributing)
- [Security](#-security)
- [License](#-license)

---

## 🎯 Overview

Managing personal finances can be challenging without a clear view of income, expenses, and financial goals.

Expense & Budget Tracker aims to simplify this process by providing a centralized platform for organizing financial information and monitoring spending.

The application is organized into dedicated modules to support maintainability, collaboration, and future development.

### Project Goals

- Simplify personal income and expense tracking.
- Help users organize and monitor their financial activities.
- Provide a foundation for budget planning and financial analysis.
- Offer a user-friendly and organized interface.
- Apply software development best practices through modular architecture and version control.

---

## ✨ Features

### 🔐 Authentication & User Access

- User registration interface.
- User login interface.
- Backend authentication services.
- Input validation and authentication-related error handling.
- Organized authentication controllers, services, and repositories.

### 📊 Dashboard

Designed to provide a central overview of personal financial information, including income, expenses, and available balance.

### 💳 Transaction Management

A dedicated module for organizing financial transactions and distinguishing between income and expenses.

### 📅 Budget Management

A dedicated section for planning budgets and monitoring spending against financial limits.

### 🗂️ Category Management

Organize financial activities into categories to make transactions easier to understand and analyze.

### 📈 Reports & Analytics

A reporting section designed to present financial summaries and visual insights into spending patterns.

### 🎯 Saving Goals

A dedicated module for organizing and tracking personal savings goals.

### ⚙️ Profile & Settings

A dedicated area for managing user profile information and application preferences.

> **Note:** The modules above describe the project's overall scope. The availability and integration of individual features depend on the current implementation and the progress of each development branch.

---

## 🛠️ Technology Stack

| Technology | Purpose |
|---|---|
| C# | Backend application logic |
| ASP.NET Core | Backend services and HTTP endpoints |
| HTML5 | Frontend structure |
| CSS3 | Styling and responsive layouts |
| JavaScript | Frontend interactions and API communication |
| SQL / SQLite | Relational data storage where configured |
| Git | Version control |
| GitHub | Source code hosting and team collaboration |
| Visual Studio / Visual Studio Code | Development tools |

The authentication module uses a C# backend with separated controllers, interfaces, models, repositories, security components, and services.

The frontend uses HTML, CSS, and JavaScript to provide the user interface.

---

## 🧩 Project Modules

The application is organized around the following functional areas:

| Module | Responsibility |
|---|---|
| Authentication | Registration and login |
| Dashboard | Overview of financial activity |
| Transactions | Income and expense records |
| Budgets | Budget planning and monitoring |
| Categories | Financial transaction classification |
| Reports & Analytics | Financial summaries and insights |
| Saving Goals | Personal savings tracking |
| Profile & Settings | User preferences and profile management |

Each module can be developed and reviewed independently before being integrated into the main application.

---

## 📂 Project Structure

The repository contains the main application and its supporting files. The authentication module follows a structure similar to the one below:

```text
Expense-and-Budget-Tracker/
│
├── Login & Register/
│   ├── Backend/
│   │   ├── Controllers/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   ├── Repositories/
│   │   ├── Routes/
│   │   ├── Security/
│   │   ├── Services/
│   │   ├── Utils/
│   │   ├── PennywiseAuth.csproj
│   │   └── Program.cs
│   │
│   ├── Frontend/
│   │   ├── index.html
│   │   ├── login.html
│   │   ├── register.html
│   │   ├── dashboard.html
│   │   ├── CSS/
│   │   ├── JS/
│   │   └── assets/
│   │
│   ├── PennywiseAuth.sln
│   └── README.md
│
└── README.md
```

*The structure is illustrative; directory names and locations may differ as team modules are integrated.*

---

## ⚙️ Prerequisites

Before running the application, make sure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/downloads)
- [Visual Studio](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- A modern web browser

Use the .NET SDK version required by the project's configuration.

Verify your installation:

```bash
dotnet --version
git --version
```

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker.git
```

### 2. Navigate to the Project

```bash
cd Expense-and-Budget-Tracker
```

### 3. Switch to the Appropriate Branch

To access the login and registration work, use the dedicated branch:

```bash
git fetch origin
git switch --track origin/login--register
```

If you already have the branch locally, use:

```bash
git switch login--register
```

### 4. Restore Backend Dependencies

Navigate to the authentication backend directory:

```bash
cd "Login & Register/Backend"
dotnet restore
```

### 5. Run the Backend

From the backend directory, run:

```bash
dotnet run
```

Follow the terminal output to identify the address and port on which the backend is listening.

If your local folder or project file is located elsewhere, adjust the path accordingly.

### 6. Open the Frontend

Open the appropriate frontend HTML page in your browser, or serve the frontend through the development setup configured for the project.

If the frontend communicates with the backend, make sure its API base URL matches the backend's configured address and port.

> **Important:** The backend and frontend must use compatible API URLs and configuration. A page opened directly from the filesystem may not work correctly if the application requires a web server, authentication cookies, or configured API routes.

---

## 🔧 Configuration

Before running the application, review the project's configuration files.

Depending on the current implementation, configuration may include:

- Backend listening address and port.
- Database connection settings.
- Frontend API base URL.
- Authentication settings.
- Environment-specific application settings.

Keep credentials, connection strings containing passwords, API keys, and other secrets out of source control.

Use local configuration or environment variables for sensitive values, and provide safe example settings when documentation requires them.

---

## 🤝 Team Collaboration

This project uses Git and GitHub to support collaborative development.

### Branching Workflow

Each contributor should work on a dedicated feature branch.

Example:

```bash
git switch -c feature/your-feature-name
```

After completing the work:

```bash
git add .
git commit -m "Describe your changes"
git push -u origin feature/your-feature-name
```

### Pull Request Workflow

1. Push your feature branch to GitHub.
2. Open a Pull Request targeting the branch agreed upon by the team.
3. Describe the changes and affected modules.
4. Allow teammates to review the code.
5. Address review comments and requested changes.
6. Merge the Pull Request after approval and when the team agrees the changes are ready.

Avoid pushing unfinished work directly to the main branch unless the team explicitly agrees otherwise.

---

## 🧑‍💻 Contributing

Contributions and improvements are welcome from project team members.

When contributing:

- Follow the existing project structure and coding conventions.
- Keep changes focused on the relevant module.
- Test your changes before opening a Pull Request.
- Avoid committing generated build files, local databases, or secrets.
- Write clear commit messages.
- Document new configuration requirements and setup steps.
- Coordinate API changes with teammates working on dependent modules.

---

## 🔒 Security

Security is particularly important for applications that handle financial information.

Recommended practices include:

- Validate user input on the backend.
- Use secure password-handling mechanisms.
- Protect authenticated endpoints.
- Avoid exposing sensitive information in error messages.
- Keep secrets out of the repository.
- Use HTTPS in production deployments.
- Review authentication and authorization behavior before release.

These practices are recommendations and should not be interpreted as confirmation that every security control has already been implemented.

---

## 📄 License

No license has been specified in this README. Add a `LICENSE` file and update this section once the project owners decide how the source code may be used, modified, and distributed.

---

## 👥 Project Team

Developed collaboratively by the project contributors.

**Repository:** [GhofranMohamed2006/Expense-and-Budget-Tracker](https://github.com/GhofranMohamed2006/Expense-and-Budget-Tracker)

---

⭐ If you find this project useful, consider giving the repository a star!
