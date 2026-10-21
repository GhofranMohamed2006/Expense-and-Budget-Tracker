document.addEventListener("DOMContentLoaded", () => {
    // =========================
    // Configuration
    // =========================
    const API_BASE_URL = "http://localhost:5050/api";
    const USER_ID = 1;

    // =========================
    // Elements
    // =========================
    const sidebar = document.getElementById("sidebar");
    const menuToggle = document.getElementById("menuToggle");
    const toast = document.getElementById("toast");

    let toastTimer;
    let spendingChart = null;
    let categoryChart = null;

    // =========================
    // Helper Functions
    // =========================
    function getValue(object, property) {
        if (!object) return undefined;
        const pascalCase = property.charAt(0).toUpperCase() + property.slice(1);
        return object[property] ?? object[pascalCase];
    }

    function formatMoney(amount) {
        return `${Number(amount || 0).toLocaleString("en-US", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        })} EGP`;
    }

    function showToast(message, duration = 2500) {
        if (!toast) return;
        toast.textContent = message;
        toast.classList.add("show");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toast.classList.remove("show"), duration);
    }

    async function fetchAPI(endpoint, options = {}) {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            ...options,
            headers: { "Content-Type": "application/json", ...(options.headers || {}) }
        });
        const text = await response.text();
        let data = null;
        if (text) {
            try { data = JSON.parse(text); } catch { data = text; }
        }
        if (!response.ok) {
            const message = data && typeof data === "object" ? data.message || data.Message : null;
            throw new Error(message || `Request failed: ${response.status}`);
        }
        return data;
    }

    function escapeHTML(value) {
        const entities = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" };
        return String(value).replace(/[&<>"']/g, (char) => entities[char]);
    }

    if (window.lucide) lucide.createIcons();

    // =========================
    // Mobile Sidebar
    // =========================
    menuToggle?.addEventListener("click", () => sidebar?.classList.toggle("open"));
    document.addEventListener("click", (event) => {
        if (window.innerWidth <= 760 && sidebar?.classList.contains("open") &&
            !sidebar.contains(event.target) && !menuToggle?.contains(event.target)) {
            sidebar.classList.remove("open");
        }
    });

    document.querySelectorAll(".nav-link").forEach((link) => {
        link.addEventListener("click", () => {
            document.querySelectorAll(".nav-link").forEach((item) => item.classList.remove("active"));
            link.classList.add("active");
            sidebar?.classList.remove("open");
        });
    });

    // =========================
    // Dashboard Summary
    // =========================
    async function loadDashboardSummary() {
        try {
            const data = await fetchAPI(`/dashboard/summary?userId=${USER_ID}`);
            const totalIncome = Number(getValue(data, "totalIncome") || 0);
            const totalExpenses = Number(getValue(data, "totalExpenses") || 0);
            const totalBalance = Number(getValue(data, "totalBalance") ?? totalIncome - totalExpenses);

            document.getElementById("totalBalance").textContent = formatMoney(totalBalance);
            document.getElementById("totalIncome").textContent = formatMoney(totalIncome);
            document.getElementById("totalExpenses").textContent = formatMoney(totalExpenses);
            document.getElementById("balanceIncome").textContent = formatMoney(totalIncome);
            document.getElementById("balanceExpenses").textContent = formatMoney(totalExpenses);
            document.getElementById("categoryTotal").textContent = formatMoney(totalExpenses);
        } catch (error) {
            console.error("Summary error:", error);
            showToast("Could not load dashboard summary.");
        }
    }

    // =========================
    // Recent Transactions
    // =========================
    async function loadRecentTransactions() {
        const transactionList = document.getElementById("transactionList");
        if (!transactionList) return;

        try {
            const transactions = await fetchAPI(`/transactions/recent?userId=${USER_ID}`);

            if (!Array.isArray(transactions) || transactions.length === 0) {
                transactionList.innerHTML = `
          <div class="empty-state">
            <i data-lucide="inbox"></i>
            <span>No transactions recorded yet.</span>
          </div>
        `;
                if (window.lucide) lucide.createIcons();
                return;
            }

            transactionList.innerHTML = transactions.map((transaction) => {
                const id = Number(getValue(transaction, "transactionID"));
                const category = getValue(transaction, "category") || "Other";
                const amount = Number(getValue(transaction, "amount") || 0);
                const type = getValue(transaction, "type") || "Expense";
                const description = getValue(transaction, "description") || "";
                const transactionDate = getValue(transaction, "transactionDate");

                const date = transactionDate ? new Date(transactionDate).toLocaleDateString("en-GB", {
                    day: "2-digit", month: "short", year: "numeric"
                }) : "";

                const isIncome = type.toLowerCase() === "income";
                const icon = isIncome ? "wallet" :
                    category.toLowerCase().includes("food") ? "utensils" :
                        category.toLowerCase().includes("transport") ? "car" : "shopping-bag";

                return `
          <div class="transaction-item">
            <div class="transaction-icon"><i data-lucide="${icon}"></i></div>
            <div class="transaction-details">
              <strong>${escapeHTML(description || category)}</strong>
              <span>${escapeHTML(category)} · ${escapeHTML(date)}</span>
            </div>
            <div class="transaction-amount ${isIncome ? "positive" : "negative"}">
              ${isIncome ? "+" : "-"}${formatMoney(amount)}
            </div>
            <div class="transaction-actions">
              <button class="edit-transaction" data-id="${id}"><i data-lucide="pencil"></i></button>
              <button class="delete-transaction" data-id="${id}"><i data-lucide="trash-2"></i></button>
            </div>
          </div>
        `;
            }).join("");

            if (window.lucide) lucide.createIcons();

            // Edit & Delete Listeners
            transactionList.querySelectorAll(".edit-transaction").forEach((button) => {
                button.addEventListener("click", async () => {
                    const id = Number(button.dataset.id);
                    try {
                        const transaction = await fetchAPI(`/transactions/${id}?userId=${USER_ID}`);
                        if (!transaction) return showToast("Transaction not found.");
                        openTransactionModal(getValue(transaction, "type") || "Expense");
                        editingTransactionId = id;
                        document.getElementById("transactionCategory").value = getValue(transaction, "category") || "";
                        document.getElementById("transactionAmount").value = getValue(transaction, "amount") || "";
                        document.getElementById("transactionType").value = getValue(transaction, "type") || "Expense";
                        document.getElementById("transactionDescription").value = getValue(transaction, "description") || "";
                        const dateValue = getValue(transaction, "transactionDate");
                        document.getElementById("transactionDate").value = dateValue ? String(dateValue).slice(0, 10) : getTodayDate();
                        document.getElementById("modalTitle").textContent = "Edit Transaction";
                        document.getElementById("saveTransactionBtn").textContent = "Update Transaction";
                    } catch (error) { showToast("Could not load transaction."); }
                });
            });

            transactionList.querySelectorAll(".delete-transaction").forEach((button) => {
                button.addEventListener("click", async () => {
                    const id = Number(button.dataset.id);
                    if (!confirm("Delete this transaction?")) return;
                    button.disabled = true;
                    try {
                        await fetchAPI(`/transactions/${id}?userId=${USER_ID}`, { method: "DELETE" });
                        showToast("Transaction deleted!");
                        await Promise.allSettled([loadDashboardSummary(), loadRecentTransactions(), loadSpendingChart(), loadCategoryChart(), loadMonthlyBudget(), loadBudgets()]);
                    } catch (error) { showToast("Could not delete transaction."); button.disabled = false; }
                });
            });
        } catch (error) {
            transactionList.innerHTML = `<div class="empty-state" style="color:#d32f2f; border-color:#d32f2f;"><i data-lucide="alert-circle"></i><span>Could not load transactions.</span></div>`;
            if (window.lucide) lucide.createIcons();
        }
    }

    // =========================
    // Charts (Spending & Category)
    // =========================
    async function loadSpendingChart() {
        const canvas = document.getElementById("spendingChart");
        if (!canvas || !window.Chart) return;
        try {
            const monthlyData = await fetchAPI(`/dashboard/monthly-expenses?userId=${USER_ID}`);
            const rows = Array.isArray(monthlyData) ? monthlyData : [];
            const now = new Date();
            const months = [];
            for (let offset = 5; offset >= 0; offset--) {
                const date = new Date(now.getFullYear(), now.getMonth() - offset, 1);
                const matchingRow = rows.find(r => Number(getValue(r, "year")) === date.getFullYear() && Number(getValue(r, "month")) === date.getMonth() + 1);
                months.push({ label: date.toLocaleDateString("en-US", { month: "short", year: "2-digit" }), amount: matchingRow ? Number(getValue(matchingRow, "totalExpenses") || 0) : 0 });
            }
            if (spendingChart) spendingChart.destroy();
            spendingChart = new Chart(canvas.getContext("2d"), {
                type: "line",
                data: { labels: months.map(m => m.label), datasets: [{ label: "Monthly Expenses", data: months.map(m => m.amount), borderColor: "#1F5F34", backgroundColor: "#E8F8F0", fill: true, tension: 0.42, borderWidth: 2.5, pointRadius: 4 }] },
                options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false }, tooltip: { backgroundColor: "#1F5F34", displayColors: false, callbacks: { label: (item) => formatMoney(item.raw) } } }, scales: { x: { grid: { display: false }, ticks: { color: "#464B4C" } }, y: { beginAtZero: true, ticks: { color: "#464B4C" }, grid: { color: "#D1D5DB" } } } }
            });
        } catch (error) { console.error(error); }
    }

    async function loadCategoryChart() {
        const canvas = document.getElementById("categoryChart");
        if (!canvas || !window.Chart) return;
        try {
            const categories = await fetchAPI(`/dashboard/category-expenses?userId=${USER_ID}`);
            const rows = Array.isArray(categories) ? categories : [];
            const labels = rows.map(r => String(getValue(r, "category") || "Other"));
            const values = rows.map(r => Number(getValue(r, "totalExpenses") || 0));
            const colors = ["#1F5F34", "#EBEFFC", "#464B4C", "#B4BCB9", "#E8F8F0", "#F5F5F7"];
            const total = values.reduce((s, v) => s + v, 0);
            document.getElementById("categoryTotal").textContent = formatMoney(total);

            const legend = document.getElementById("categoryLegend");
            if (legend) {
                legend.innerHTML = rows.length ? rows.map((r, i) => {
                    const cat = String(getValue(r, "category") || "Other");
                    const amt = Number(getValue(r, "totalExpenses") || 0);
                    const pct = total > 0 ? (amt / total) * 100 : 0;
                    return `<li class="legend-item"><span class="legend-color" style="background:${colors[i % colors.length]}"></span><span class="legend-label">${escapeHTML(cat)}</span><span class="legend-value">${pct.toFixed(1)}%</span></li>`;
                }).join("") : `<li class="empty-state">No data</li>`;
            }

            if (categoryChart) categoryChart.destroy();
            categoryChart = new Chart(canvas, {
                type: "doughnut",
                data: { labels, datasets: [{ data: values, backgroundColor: labels.map((_, i) => colors[i % colors.length]), borderColor: "#F5F5F7", borderWidth: 2, hoverOffset: 5 }] },
                options: { responsive: true, maintainAspectRatio: false, cutout: "57%", plugins: { legend: { display: false }, tooltip: { callbacks: { label: (item) => `${item.label}: ${formatMoney(item.raw)}` } } } }
            });
        } catch (error) { console.error(error); }
    }

    // =========================
    // Budgets
    // =========================
    async function loadMonthlyBudget() {
        try {
            const budgets = await fetchAPI(`/budgets?userId=${USER_ID}`);
            const now = new Date();
            const current = budgets.filter(b => { const d = new Date(getValue(b, "budgetMonth")); return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth(); });
            const totalBudget = current.reduce((s, b) => s + Number(getValue(b, "budgetAmount") || 0), 0);
            const totalSpent = current.reduce((s, b) => s + Number(getValue(b, "spentAmount") || 0), 0);
            const pct = totalBudget > 0 ? (totalSpent / totalBudget) * 100 : 0;
            document.getElementById("monthlyBudget").textContent = formatMoney(totalBudget);
            document.getElementById("monthlyBudgetProgress").style.width = `${Math.min(pct, 100)}%`;
            document.getElementById("monthlyBudgetUsed").textContent = `${pct.toFixed(1)}% used`;
        } catch (error) { console.error(error); }
    }

    async function loadBudgets() {
        const budgetList = document.getElementById("budgetList");
        if (!budgetList) return;
        try {
            const budgets = await fetchAPI(`/budgets?userId=${USER_ID}`);
            const now = new Date();
            const current = budgets.filter(b => { const d = new Date(getValue(b, "budgetMonth")); return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth(); });

            if (current.length === 0) {
                budgetList.innerHTML = `<div class="empty-state"><i data-lucide="pie-chart"></i><span>No budgets yet.</span></div>`;
                if (window.lucide) lucide.createIcons();
                return;
            }

            budgetList.innerHTML = current.map(b => {
                const id = Number(getValue(b, "budgetID"));
                const cat = getValue(b, "category") || "Other";
                const amt = Number(getValue(b, "budgetAmount") || 0);
                const spent = Number(getValue(b, "spentAmount") || 0);
                const rem = Number(getValue(b, "remainingAmount") ?? amt - spent);
                const pct = amt > 0 ? (spent / amt) * 100 : 0;
                const color = pct >= 100 ? "#464B4C" : pct >= 80 ? "#EBEFFC" : "#1F5F34";
                return `
          <div class="budget-item">
            <div class="budget-info"><h3>${escapeHTML(cat)}</h3><span>${formatMoney(spent)} / ${formatMoney(amt)}</span></div>
            <div class="progress-line"><span style="width:${Math.min(pct, 100)}%; background:${color}"></span></div>
            <div class="budget-footer"><small>${pct.toFixed(1)}% used</small><small>${formatMoney(rem)} left</small></div>
            <div class="budget-actions"><button class="edit-budget" data-id="${id}">Edit</button><button class="delete-budget" data-id="${id}">Delete</button></div>
          </div>
        `;
            }).join("");

            budgetList.querySelectorAll(".edit-budget").forEach(btn => btn.addEventListener("click", () => {
                const b = current.find(x => Number(getValue(x, "budgetID")) === Number(btn.dataset.id));
                if (b) openBudgetModal(b);
            }));

            budgetList.querySelectorAll(".delete-budget").forEach(btn => btn.addEventListener("click", async () => {
                if (!confirm("Delete this budget?")) return;
                btn.disabled = true;
                try {
                    await fetchAPI(`/budgets/${btn.dataset.id}?userId=${USER_ID}`, { method: "DELETE" });
                    showToast("Budget deleted!");
                    await Promise.allSettled([loadMonthlyBudget(), loadBudgets()]);
                } catch (e) { showToast("Could not delete budget."); btn.disabled = false; }
            }));
        } catch (error) {
            budgetList.innerHTML = `<div class="empty-state" style="color:#d32f2f; border-color:#d32f2f;"><i data-lucide="alert-circle"></i><span>Could not load budgets.</span></div>`;
            if (window.lucide) lucide.createIcons();
        }
    }

    // =========================
    // Transaction Modal
    // =========================
    const transactionModal = document.getElementById("transactionModal");
    const transactionForm = document.getElementById("transactionForm");
    let editingTransactionId = null;

    function openTransactionModal(type = "Expense") {
        transactionForm.reset();
        editingTransactionId = null;
        document.getElementById("modalTitle").textContent = type === "Income" ? "Add Income" : "Add Expense";
        document.getElementById("transactionType").value = type;
        document.getElementById("transactionDate").value = getTodayDate();
        document.getElementById("saveTransactionBtn").textContent = "Save Transaction";
        transactionModal.classList.add("active");
    }

    function closeTransactionModal() {
        transactionModal.classList.remove("active");
        transactionForm.reset();
        editingTransactionId = null;
    }

    function getTodayDate() {
        const d = new Date();
        return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
    }

    document.querySelectorAll(".action-button").forEach(btn => {
        btn.addEventListener("click", () => {
            const action = btn.dataset.action;
            if (action === "Add Transaction") openTransactionModal("Expense");
            if (action === "Set Budget") openBudgetModal();
            if (action === "View Reports") document.getElementById("reports")?.scrollIntoView({ behavior: "smooth" });
            if (action === "Manage Categories") showToast("Manage categories via transactions.");
        });
    });

    document.getElementById("closeModalBtn").addEventListener("click", closeTransactionModal);
    document.getElementById("cancelTransactionBtn").addEventListener("click", closeTransactionModal);
    transactionModal.addEventListener("click", (e) => { if (e.target === transactionModal) closeTransactionModal(); });

    transactionForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const amount = Number(document.getElementById("transactionAmount").value);
        const category = document.getElementById("transactionCategory").value;
        const date = document.getElementById("transactionDate").value;
        if (!amount || amount <= 0 || !category || !date) return showToast("Please enter valid details.");

        const isEditing = editingTransactionId !== null;
        const endpoint = isEditing ? `/transactions/${editingTransactionId}` : "/transactions";
        const method = isEditing ? "PUT" : "POST";
        const btn = document.getElementById("saveTransactionBtn");
        btn.disabled = true; btn.textContent = isEditing ? "Updating..." : "Saving...";

        try {
            await fetchAPI(endpoint, {
                method, body: JSON.stringify({
                    category, amount, type: document.getElementById("transactionType").value,
                    description: document.getElementById("transactionDescription").value.trim() || null, transactionDate: date
                })
            });
            closeTransactionModal();
            showToast(isEditing ? "Transaction updated!" : "Transaction added!");
            await Promise.allSettled([loadDashboardSummary(), loadRecentTransactions(), loadSpendingChart(), loadCategoryChart(), loadMonthlyBudget(), loadBudgets()]);
        } catch (error) {
            showToast(error.message || "Could not save transaction.");
        } finally {
            btn.disabled = false; btn.textContent = "Save Transaction";
        }
    });

    // =========================
    // Budget Modal
    // =========================
    let editingBudgetId = null;

    function createBudgetModal() {
        if (document.getElementById("budgetModal")) return;
        const modal = document.createElement("div");
        modal.id = "budgetModal"; modal.className = "modal-overlay";
        modal.innerHTML = `
      <div class="modal-card">
        <div class="modal-header"><h2 id="budgetModalTitle">Set Budget</h2><button class="icon-button" id="closeBudgetModal">&times;</button></div>
        <form id="budgetForm">
          <div class="form-group"><label>Category</label><input id="budgetCategory" type="text" required placeholder="e.g. Food"></div>
          <div class="form-group"><label>Monthly Budget (EGP)</label><input id="budgetAmount" type="number" min="0.01" step="0.01" required placeholder="Amount"></div>
          <div class="form-group"><label>Month</label><input id="budgetMonth" type="month" required></div>
          <div class="modal-actions"><button type="button" class="cancel-button" id="cancelBudgetModal">Cancel</button><button type="submit" class="save-button" id="saveBudgetBtn">Save Budget</button></div>
        </form>
      </div>`;
        document.body.appendChild(modal);
        document.getElementById("closeBudgetModal").addEventListener("click", closeBudgetModal);
        document.getElementById("cancelBudgetModal").addEventListener("click", closeBudgetModal);
        modal.addEventListener("click", (e) => { if (e.target === modal) closeBudgetModal(); });
        document.getElementById("budgetForm").addEventListener("submit", saveBudget);
    }

    function openBudgetModal(budget = null) {
        createBudgetModal();
        const modal = document.getElementById("budgetModal");
        document.getElementById("budgetForm").reset();
        editingBudgetId = budget ? Number(getValue(budget, "budgetID")) : null;
        document.getElementById("budgetModalTitle").textContent = budget ? "Edit Budget" : "Set Budget";
        document.getElementById("budgetCategory").value = budget ? getValue(budget, "category") || "" : "";
        document.getElementById("budgetAmount").value = budget ? getValue(budget, "budgetAmount") || "" : "";
        document.getElementById("budgetMonth").value = budget ? String(getValue(budget, "budgetMonth") || "").slice(0, 7) : getTodayDate().slice(0, 7);
        modal.classList.add("active");
    }

    function closeBudgetModal() {
        document.getElementById("budgetModal")?.classList.remove("active");
        editingBudgetId = null;
    }

    async function saveBudget(e) {
        e.preventDefault();
        const cat = document.getElementById("budgetCategory").value.trim();
        const amt = Number(document.getElementById("budgetAmount").value);
        const month = document.getElementById("budgetMonth").value;
        if (!cat || amt <= 0 || !month) return showToast("Please enter valid info.");

        const btn = document.getElementById("saveBudgetBtn");
        btn.disabled = true; btn.textContent = "Saving...";
        try {
            await fetchAPI(editingBudgetId ? `/budgets/${editingBudgetId}` : "/budgets", {
                method: editingBudgetId ? "PUT" : "POST",
                body: JSON.stringify({ category: cat, budgetAmount: amt, budgetMonth: `${month}-01T00:00:00` })
            });
            closeBudgetModal(); showToast(editingBudgetId ? "Budget updated!" : "Budget added!");
            await Promise.allSettled([loadMonthlyBudget(), loadBudgets()]);
        } catch (error) { showToast("Could not save budget."); }
        finally { btn.disabled = false; btn.textContent = "Save Budget"; }
    }

    // Initialize
    initializeDashboard();
});