const API = "/api";
const $ = (selector, root = document) => root.querySelector(selector);
const content = $("#content");
const message = $("#message");
const routeNames = {
  dashboard: "Dashboard",
  transactions: "Transactions",
  budgets: "Budgets",
  categories: "Categories",
  reports: "Reports",
  goals: "Savings goals",
  settings: "Settings"
};
const navItems = Object.entries(routeNames);
let token = localStorage.getItem("pw_token");
let route = "dashboard";
let categories = [];
let preferences = { currency: "EGP" };
let settingsTab = "profile";
let editingTransactionId = null;
let editingBudgetId = null;
let editingGoalId = null;
let editingCategoryId = null;

const escapeHtml = value => String(value ?? "").replace(/[&<>"']/g, char => ({
  "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
})[char]);
const money = value => new Intl.NumberFormat(undefined, {
  style: "currency", currency: preferences.currency || "EGP", maximumFractionDigits: 2
}).format(Number(value || 0));
const dateOnly = value => value ? new Date(value).toLocaleDateString() : "—";
const isoDate = value => value ? new Date(value).toISOString().slice(0, 10) : "";

async function request(path, { method = "GET", body } = {}) {
  const headers = { Accept: "application/json" };
  if (body !== undefined) headers["Content-Type"] = "application/json";
  if (token) headers.Authorization = `Bearer ${token}`;
  const response = await fetch(`${API}${path}`, {
    method, headers, body: body === undefined ? undefined : JSON.stringify(body)
  });
  if (response.status === 401) {
    localStorage.removeItem("pw_token");
    token = null;
    location.replace("/login");
    throw new Error("Your session has expired. Please sign in again.");
  }
  if (response.status === 204) return null;
  const text = await response.text();
  let data = null;
  try { data = text ? JSON.parse(text) : null; } catch { data = text; }
  if (!response.ok) {
    const validation = data?.errors && Object.values(data.errors).flat().join(" ");
    throw new Error(validation || data?.title || data?.message || `Request failed (${response.status}).`);
  }
  return data;
}

function notify(text, isError = false) {
  message.className = text ? `notice${isError ? " error" : ""}` : "";
  message.textContent = text || "";
}

async function run(action) {
  try { notify(""); await action(); }
  catch (error) { notify(error.message || "The request could not be completed.", true); }
}

function renderNav() {
  $("#nav").innerHTML = navItems.map(([key, label]) =>
    `<a class="nav-link${key === route ? " active" : ""}" href="#${key}" ${key === route ? 'aria-current="page"' : ""}>${label}</a>`
  ).join("");
}

function renderTable(transactions, showActions = true) {
  if (!transactions.length) return '<p class="empty">No transactions recorded yet.</p>';
  return `<div class="table-wrap"><table><thead><tr><th>Date</th><th>Details</th><th>Category</th><th>Amount</th>${showActions ? "<th>Actions</th>" : ""}</tr></thead><tbody>
    ${transactions.map(item => `<tr>
      <td>${escapeHtml(dateOnly(item.date))}</td><td>${escapeHtml(item.note || "Transaction")}</td>
      <td>${escapeHtml(item.categoryName)}</td>
      <td class="amount ${item.type}">${item.type === "income" ? "+" : "−"}${escapeHtml(money(item.amount))}</td>
      ${showActions ? `<td><button class="button small secondary" data-edit-transaction="${item.id}">Edit</button>
      <button class="button small danger" data-delete-transaction="${item.id}">Delete</button></td>` : ""}
    </tr>`).join("")}
  </tbody></table></div>`;
}

async function renderDashboard() {
  const [dashboard, budgets] = await Promise.all([request("/dashboard"), request("/budgets")]);
  content.innerHTML = `<div class="grid">
    <article class="card metric"><small>Income</small><strong>${escapeHtml(money(dashboard.income))}</strong></article>
    <article class="card metric"><small>Expenses</small><strong>${escapeHtml(money(dashboard.expenses))}</strong></article>
    <article class="card metric"><small>Balance</small><strong>${escapeHtml(money(dashboard.balance))}</strong></article></div>
    <div class="columns"><section class="card"><div class="toolbar"><h2>Recent transactions</h2><a href="#transactions">View all</a></div>
    ${renderTable(dashboard.recentTransactions || [], false)}</section>
    <section class="card"><div class="toolbar"><h2>Monthly budgets</h2><a href="#budgets">Manage budgets</a></div>
    ${budgets.length ? budgets.map(item => {
      const percent = Math.min(100, Number(item.spent) / Number(item.limit) * 100);
      return `<div class="row"><div><strong>${escapeHtml(item.name)}</strong>
        <div class="muted">${escapeHtml(money(item.spent))} of ${escapeHtml(money(item.limit))}</div>
        <div class="bar"><i style="width:${percent}%"></i></div></div></div>`;
    }).join("") : '<p class="empty">No budgets yet.</p>'}</section></div>`;
}

function transactionForm(item = null) {
  const options = categories.filter(category => category.type === (item?.type || "expense")).map(category =>
    `<option value="${category.id}" ${item?.categoryId === category.id ? "selected" : ""}>${escapeHtml(category.name)}</option>`
  ).join("");
  return `<section class="card"><h2>${item ? "Edit transaction" : "Add transaction"}</h2>
    <form id="transaction-form" class="forms">
      <div class="field"><label for="transaction-type">Type</label><select id="transaction-type" required>
        <option value="expense" ${item?.type !== "income" ? "selected" : ""}>Expense</option>
        <option value="income" ${item?.type === "income" ? "selected" : ""}>Income</option></select></div>
      <div class="field"><label for="transaction-category">Category</label><select id="transaction-category" required>${options}</select></div>
      <div class="field"><label for="transaction-amount">Amount</label><input id="transaction-amount" type="number" min="0.01" step="0.01" required value="${item ? escapeHtml(item.amount) : ""}"></div>
      <div class="field"><label for="transaction-date">Date</label><input id="transaction-date" type="date" required value="${item ? isoDate(item.date) : isoDate(new Date())}"></div>
      <div class="field full"><label for="transaction-note">Note</label><input id="transaction-note" maxlength="500" value="${escapeHtml(item?.note || "")}"></div>
      <div class="actions field full"><button class="button">${item ? "Save transaction" : "Add transaction"}</button>
        ${item ? '<button class="button secondary" type="button" id="cancel-transaction">Cancel</button>' : ""}</div>
    </form></section>`;
}

async function renderTransactions(item = null) {
  const transactions = await request("/transactions");
  content.innerHTML = `<div class="stack">${transactionForm(item)}
    <section class="card"><div class="toolbar"><h2>Transactions</h2><span class="muted">${transactions.length} records</span></div>${renderTable(transactions)}</section></div>`;
  $("#transaction-type").addEventListener("change", filterCategoryOptions);
  $("#cancel-transaction")?.addEventListener("click", () => { editingTransactionId = null; void showRoute(); });
  $("#transaction-form").addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      const data = {
        type: $("#transaction-type").value,
        categoryId: Number($("#transaction-category").value),
        amount: Number($("#transaction-amount").value),
        date: new Date(`${$("#transaction-date").value}T12:00:00Z`).toISOString(),
        note: $("#transaction-note").value.trim() || null
      };
      await request(editingTransactionId ? `/transactions/${editingTransactionId}` : "/transactions", {
        method: editingTransactionId ? "PUT" : "POST", body: data
      });
      editingTransactionId = null;
      notify("Transaction saved.");
      await showRoute();
    });
  });
  content.querySelectorAll("[data-edit-transaction]").forEach(button => button.addEventListener("click", () => {
    void run(async () => {
      const selected = await request(`/transactions/${button.dataset.editTransaction}`);
      editingTransactionId = selected.id;
      await showRoute(selected);
      window.scrollTo({ top: 0, behavior: "smooth" });
    });
  }));
  content.querySelectorAll("[data-delete-transaction]").forEach(button => button.addEventListener("click", () => {
    if (confirm("Delete this transaction?")) void run(async () => {
      await request(`/transactions/${button.dataset.deleteTransaction}`, { method: "DELETE" });
      notify("Transaction deleted.");
      await showRoute();
    });
  }));
}

function filterCategoryOptions() {
  const type = $("#transaction-type").value;
  const select = $("#transaction-category");
  select.innerHTML = categories.filter(category => category.type === type).map(category =>
    `<option value="${category.id}">${escapeHtml(category.name)}</option>`
  ).join("");
}

async function renderBudgets() {
  const [budgets, expenseCategories] = await Promise.all([
    request("/budgets"), request("/categories?type=expense")
  ]);
  const choices = expenseCategories.map(category =>
    `<option value="${category.id}">${escapeHtml(category.name)}</option>`
  ).join("");
  content.innerHTML = `<div class="columns"><section class="card"><h2>Budgets this month</h2>
    ${budgets.length ? budgets.map(item => {
      const percent = Math.min(100, Number(item.spent) / Number(item.limit) * 100);
      return `<article class="row"><div class="grow"><strong>${escapeHtml(item.name)}</strong>
        <div class="muted">${escapeHtml(money(item.spent))} spent of ${escapeHtml(money(item.limit))}</div>
        <div class="bar"><i style="width:${percent}%"></i></div>
        <form class="inline expense-form" data-budget="${item.id}" style="margin-top:10px">
          <input type="number" min="0.01" step="0.01" aria-label="Expense amount for ${escapeHtml(item.name)}" placeholder="Record expense" required>
          <button class="button small secondary">Add expense</button></form></div>
        <div class="actions"><button class="button small secondary" data-edit-budget="${item.id}" data-category="${item.categoryId}" data-limit="${item.limit}">Edit</button>
        <button class="button small danger" data-delete-budget="${item.id}">Delete</button></div></article>`;
    }).join("") : '<p class="empty">Create a budget to track spending from recorded transactions.</p>'}
    </section><form class="card" id="budget-form"><h2 id="budget-form-title">New budget</h2>
      <div class="field"><label for="budget-category">Expense category</label><select id="budget-category" required>${choices}</select></div>
      <div class="field" style="margin-top:12px"><label for="budget-limit">Monthly limit</label><input id="budget-limit" type="number" min="0.01" step="0.01" required></div>
      <div class="actions"><button class="button">Save budget</button><button class="button secondary" id="cancel-budget" type="button" hidden>Cancel</button></div>
    </form></div>`;
  $("#budget-form").addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      await request(editingBudgetId ? `/budgets/${editingBudgetId}` : "/budgets", {
        method: editingBudgetId ? "PUT" : "POST",
        body: { categoryId: Number($("#budget-category").value), limit: Number($("#budget-limit").value) }
      });
      editingBudgetId = null;
      notify("Budget saved.");
      await showRoute();
    });
  });
  $("#cancel-budget").addEventListener("click", () => { editingBudgetId = null; void showRoute(); });
  content.querySelectorAll("[data-edit-budget]").forEach(button => button.addEventListener("click", () => {
    editingBudgetId = Number(button.dataset.editBudget);
    $("#budget-category").value = button.dataset.category;
    $("#budget-limit").value = button.dataset.limit;
    $("#budget-form-title").textContent = "Edit budget";
    $("#cancel-budget").hidden = false;
  }));
  content.querySelectorAll("[data-delete-budget]").forEach(button => button.addEventListener("click", () => {
    if (confirm("Delete this budget? Recorded transactions will remain.")) void run(async () => {
      await request(`/budgets/${button.dataset.deleteBudget}`, { method: "DELETE" });
      notify("Budget deleted.");
      await showRoute();
    });
  }));
  content.querySelectorAll(".expense-form").forEach(form => form.addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      await request(`/budgets/${form.dataset.budget}/expenses`, {
        method: "POST", body: { amount: Number($("input", form).value) }
      });
      notify("Expense recorded as a transaction.");
      await showRoute();
    });
  }));
}

async function renderGoals() {
  const goals = await request("/goals");
  content.innerHTML = `<div class="columns"><section class="stack">
    ${goals.length ? goals.map(goal => {
      const percent = Math.min(100, Number(goal.saved) / Number(goal.target) * 100);
      return `<article class="card goal-card"><div class="toolbar"><h2>${escapeHtml(goal.name)}</h2>
        <span class="progress">${Math.round(percent)}%${goal.date ? ` · ${escapeHtml(dateOnly(goal.date))}` : ""}</span></div>
        <p class="muted">${escapeHtml(money(goal.saved))} saved of ${escapeHtml(money(goal.target))}</p>
        <div class="bar"><i style="width:${percent}%"></i></div><div class="actions">
          <form class="inline contribution-form" data-goal="${goal.id}">
            <input type="number" min="0.01" step="0.01" placeholder="Contribution" aria-label="Contribution for ${escapeHtml(goal.name)}" required>
            <button class="button small">Add savings</button></form>
          <button class="button small secondary" data-edit-goal="${goal.id}" data-name="${escapeHtml(goal.name)}" data-target="${goal.target}" data-date="${isoDate(goal.date)}">Edit</button>
          <button class="button small danger" data-delete-goal="${goal.id}">Delete</button></div></article>`;
    }).join("") : '<p class="card empty">No savings goals yet.</p>'}
    </section><form class="card" id="goal-form"><h2 id="goal-form-title">New savings goal</h2>
      <div class="field"><label for="goal-name">Goal name</label><input id="goal-name" maxlength="80" required></div>
      <div class="field" style="margin-top:12px"><label for="goal-target">Target amount</label><input id="goal-target" type="number" min="0.01" step="0.01" required></div>
      <div class="field" style="margin-top:12px"><label for="goal-date">Target date</label><input id="goal-date" type="date"></div>
      <div class="actions"><button class="button">Save goal</button><button class="button secondary" id="cancel-goal" type="button" hidden>Cancel</button></div>
    </form></div>`;
  $("#goal-form").addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      await request(editingGoalId ? `/goals/${editingGoalId}` : "/goals", {
        method: editingGoalId ? "PUT" : "POST",
        body: { name: $("#goal-name").value.trim(), target: Number($("#goal-target").value), date: $("#goal-date").value || null }
      });
      editingGoalId = null;
      notify("Savings goal saved.");
      await showRoute();
    });
  });
  $("#cancel-goal").addEventListener("click", () => { editingGoalId = null; void showRoute(); });
  content.querySelectorAll("[data-edit-goal]").forEach(button => button.addEventListener("click", () => {
    editingGoalId = Number(button.dataset.editGoal);
    $("#goal-name").value = button.dataset.name;
    $("#goal-target").value = button.dataset.target;
    $("#goal-date").value = button.dataset.date;
    $("#goal-form-title").textContent = "Edit savings goal";
    $("#cancel-goal").hidden = false;
  }));
  content.querySelectorAll("[data-delete-goal]").forEach(button => button.addEventListener("click", () => {
    if (confirm("Delete this savings goal?")) void run(async () => {
      await request(`/goals/${button.dataset.deleteGoal}`, { method: "DELETE" });
      notify("Savings goal deleted.");
      await showRoute();
    });
  }));
  content.querySelectorAll(".contribution-form").forEach(form => form.addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      await request(`/goals/${form.dataset.goal}/contributions`, {
        method: "POST", body: { amount: Number($("input", form).value) }
      });
      notify("Savings contribution recorded.");
      await showRoute();
    });
  }));
}

async function renderCategories() {
  categories = await request("/categories");
  content.innerHTML = `<div class="columns"><section class="card"><h2>Your categories</h2>
    ${categories.length ? categories.map(category => `<div class="row"><div><strong>${escapeHtml(category.name)}</strong>
      <div class="muted">${escapeHtml(category.type)} · ${category.transactionCount} transactions</div></div>
      <div class="actions"><button class="button small secondary" data-edit-category="${category.id}" data-name="${escapeHtml(category.name)}" data-type="${category.type}" data-icon="${escapeHtml(category.icon)}" data-color="${category.colorIndex}">Edit</button>
      <button class="button small danger" data-delete-category="${category.id}">Delete</button></div></div>`).join("") : '<p class="empty">No categories yet.</p>'}
    </section><form class="card" id="category-form"><h2 id="category-form-title">New category</h2>
      <div class="field"><label for="category-name">Name</label><input id="category-name" maxlength="30" required></div>
      <div class="field" style="margin-top:12px"><label for="category-type">Type</label><select id="category-type"><option value="expense">Expense</option><option value="income">Income</option></select></div>
      <div class="field" style="margin-top:12px"><label for="category-icon">Icon key</label><input id="category-icon" value="tags" maxlength="40" pattern="[a-z0-9\-]+" required></div>
      <div class="field" style="margin-top:12px"><label for="category-color">Color</label><input id="category-color" type="number" min="0" max="6" value="0" required></div>
      <div class="actions"><button class="button">Save category</button><button class="button secondary" id="cancel-category" type="button" hidden>Cancel</button></div>
    </form></div>`;
  $("#category-form").addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      const body = {
        name: $("#category-name").value.trim(), type: $("#category-type").value,
        icon: $("#category-icon").value.trim(), colorIndex: Number($("#category-color").value)
      };
      await request(editingCategoryId ? `/categories/${editingCategoryId}` : "/categories", {
        method: editingCategoryId ? "PUT" : "POST", body
      });
      editingCategoryId = null;
      notify("Category saved.");
      await showRoute();
    });
  });
  $("#cancel-category").addEventListener("click", () => { editingCategoryId = null; void showRoute(); });
  content.querySelectorAll("[data-edit-category]").forEach(button => button.addEventListener("click", () => {
    editingCategoryId = Number(button.dataset.editCategory);
    $("#category-name").value = button.dataset.name;
    $("#category-type").value = button.dataset.type;
    $("#category-icon").value = button.dataset.icon;
    $("#category-color").value = button.dataset.color;
    $("#category-form-title").textContent = "Edit category";
    $("#cancel-category").hidden = false;
  }));
  content.querySelectorAll("[data-delete-category]").forEach(button => button.addEventListener("click", () => {
    if (confirm("Delete this category? Categories with transactions cannot be removed.")) void run(async () => {
      await request(`/categories/${button.dataset.deleteCategory}`, { method: "DELETE" });
      notify("Category deleted.");
      await showRoute();
    });
  }));
}

async function renderReports() {
  const reports = await request("/reports");
  const max = Math.max(1, ...reports.flatMap(report => [Number(report.income), Number(report.expenses)]));
  const totalIncome = reports.reduce((sum, item) => sum + Number(item.income), 0);
  const totalExpenses = reports.reduce((sum, item) => sum + Number(item.expenses), 0);
  content.innerHTML = `<div class="grid">
    <article class="card metric"><small>Income · last 12 months</small><strong>${escapeHtml(money(totalIncome))}</strong></article>
    <article class="card metric"><small>Expenses · last 12 months</small><strong>${escapeHtml(money(totalExpenses))}</strong></article>
    <article class="card metric"><small>Net · last 12 months</small><strong>${escapeHtml(money(totalIncome - totalExpenses))}</strong></article></div>
    <section class="card"><h2>Income and expenses by month</h2><div class="report-bars">
      ${reports.map(report => {
        const label = new Date(report.year, report.month - 1).toLocaleDateString(undefined, { month: "short" });
        const incomeHeight = Math.max(2, Number(report.income) / max * 100);
        const expenseHeight = Math.max(2, Number(report.expenses) / max * 100);
        return `<div class="month" title="${escapeHtml(label)} ${report.year}: income ${escapeHtml(money(report.income))}, expenses ${escapeHtml(money(report.expenses))}">
          <span>${escapeHtml(label)}</span><i class="month-bar" style="height:${incomeHeight}%"></i>
          <i class="month-bar expense" style="height:${expenseHeight}%"></i></div>`;
      }).join("")}</div>
      <div class="row"><span>Income</span><strong>${escapeHtml(money(totalIncome))}</strong></div>
      <div class="row"><span>Expenses</span><strong>${escapeHtml(money(totalExpenses))}</strong></div></section>`;
}

async function renderSettings() {
  const [profile, prefs] = await Promise.all([request("/settings/profile"), request("/settings/preferences")]);
  preferences = prefs;
  $("#user-name").textContent = profile.fullName;
  $("#avatar").textContent = profile.fullName.split(/\s+/).slice(0, 2).map(word => word[0]).join("").toUpperCase();
  content.innerHTML = `<div class="tabs">
    ${[["profile", "Profile"], ["preferences", "Preferences"], ["password", "Password"]].map(([key, label]) =>
      `<button class="tab${settingsTab === key ? " active" : ""}" data-settings-tab="${key}">${label}</button>`
    ).join("")}</div>${settingsTab === "profile" ? `<form class="card auth-card" id="profile-form">
      <h2>Profile</h2><div class="forms">
      <div class="field"><label for="profile-name">Full name</label><input id="profile-name" value="${escapeHtml(profile.fullName)}" minlength="2" maxlength="80" required></div>
      <div class="field"><label for="profile-email">Email</label><input id="profile-email" type="email" value="${escapeHtml(profile.email)}" maxlength="120" required></div>
      <div class="field"><label for="profile-phone">Phone</label><input id="profile-phone" value="${escapeHtml(profile.phone || "")}"></div>
      <div class="field"><label for="profile-country">Country</label><input id="profile-country" value="${escapeHtml(profile.country || "")}" maxlength="80" required></div></div>
      <div class="actions"><button class="button">Save profile</button></div></form>` :
    settingsTab === "preferences" ? `<form class="card auth-card" id="preferences-form"><h2>Preferences</h2><div class="forms">
      <div class="field"><label for="pref-currency">Currency</label><select id="pref-currency">${["EGP", "USD", "EUR", "GBP"].map(value => `<option ${prefs.currency === value ? "selected" : ""}>${value}</option>`).join("")}</select></div>
      <div class="field"><label for="pref-language">Language</label><select id="pref-language">${["English", "Arabic"].map(value => `<option ${prefs.language === value ? "selected" : ""}>${value}</option>`).join("")}</select></div>
      <div class="field"><label for="pref-theme">Theme</label><select id="pref-theme">${["Light", "Dark"].map(value => `<option ${prefs.theme === value ? "selected" : ""}>${value}</option>`).join("")}</select></div>
      <div class="field"><label for="pref-date">Date format</label><select id="pref-date">${["DD/MM/YYYY", "MM/DD/YYYY", "YYYY-MM-DD"].map(value => `<option ${prefs.dateFormat === value ? "selected" : ""}>${value}</option>`).join("")}</select></div>
      ${[["emailNotifications", "Email notifications"], ["budgetAlerts", "Budget alerts"], ["monthlySummary", "Monthly summary"]].map(([key, label]) => `<label class="row"><span>${label}</span><input type="checkbox" id="pref-${key}" ${prefs[key] ? "checked" : ""}></label>`).join("")}
      </div><div class="actions"><button class="button">Save preferences</button></div></form>` :
    `<form class="card auth-card" id="password-form"><h2>Change password</h2>
      <div class="field"><label for="current-password">Current password</label><input id="current-password" type="password" autocomplete="current-password" required></div>
      <div class="field" style="margin-top:12px"><label for="new-password">New password</label><input id="new-password" type="password" minlength="8" autocomplete="new-password" required></div>
      <div class="field" style="margin-top:12px"><label for="confirm-password">Confirm new password</label><input id="confirm-password" type="password" minlength="8" autocomplete="new-password" required></div>
      <div class="actions"><button class="button">Change password</button></div></form>`}`;

  content.querySelectorAll("[data-settings-tab]").forEach(button => button.addEventListener("click", () => {
    settingsTab = button.dataset.settingsTab;
    void showRoute();
  }));
  $("#profile-form")?.addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      const result = await request("/settings/profile", { method: "PUT", body: {
        fullName: $("#profile-name").value.trim(), email: $("#profile-email").value.trim(),
        phone: $("#profile-phone").value.trim() || null, country: $("#profile-country").value.trim()
      } });
      $("#user-name").textContent = result.fullName;
      notify("Profile saved.");
    });
  });
  $("#preferences-form")?.addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      preferences = await request("/settings/preferences", { method: "PUT", body: {
        currency: $("#pref-currency").value, language: $("#pref-language").value,
        theme: $("#pref-theme").value, dateFormat: $("#pref-date").value,
        emailNotifications: $("#pref-emailNotifications").checked,
        budgetAlerts: $("#pref-budgetAlerts").checked,
        monthlySummary: $("#pref-monthlySummary").checked
      } });
      notify("Preferences saved.");
      await showRoute();
    });
  });
  $("#password-form")?.addEventListener("submit", event => {
    event.preventDefault();
    void run(async () => {
      await request("/settings/password", { method: "POST", body: {
        currentPassword: $("#current-password").value,
        newPassword: $("#new-password").value,
        confirmPassword: $("#confirm-password").value
      } });
      event.target.reset();
      notify("Password changed. Your current session remains active.");
    });
  });
}

async function showRoute(transactionToEdit = null) {
  if (!token) { location.replace("/login"); return; }
  const candidate = location.hash.slice(1);
  route = Object.hasOwn(routeNames, candidate) ? candidate : "dashboard";
  if (location.hash !== `#${route}`) history.replaceState(null, "", `#${route}`);
  renderNav();
  $("#page-title").textContent = routeNames[route];
  if (route === "settings") return renderSettings();
  if (route === "categories" || route === "transactions") categories = await request("/categories");
  if (route === "dashboard") return renderDashboard();
  if (route === "transactions") return renderTransactions(transactionToEdit);
  if (route === "budgets") return renderBudgets();
  if (route === "categories") return renderCategories();
  if (route === "goals") return renderGoals();
  if (route === "reports") return renderReports();
}

$("#nav").addEventListener("click", event => {
  if (event.target.closest("a")) notify("");
});
$("#logout").addEventListener("click", () => {
  void run(async () => {
    try { await request("/auth/logout", { method: "POST" }); }
    finally { localStorage.removeItem("pw_token"); token = null; location.replace("/"); }
  });
});
window.addEventListener("hashchange", () => { void run(() => showRoute()); });

void run(async () => {
  const [profile, prefs] = await Promise.all([
    request("/settings/profile"),
    request("/settings/preferences")
  ]);
  preferences = prefs;
  $("#user-name").textContent = profile.fullName;
  $("#avatar").textContent = profile.fullName.split(/\s+/).slice(0, 2).map(word => word[0]).join("").toUpperCase();
  await showRoute();
});