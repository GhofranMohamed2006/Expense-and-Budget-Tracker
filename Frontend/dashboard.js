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

// Support both PascalCase and camelCase JSON properties.
function getValue(object, property) {
if (!object) return undefined;

const pascalCase =
  property.charAt(0).toUpperCase() + property.slice(1);

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

toastTimer = setTimeout(() => {
  toast.classList.remove("show");
}, duration);

}

async function fetchAPI(endpoint, options = {}) {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    });

    const text = await response.text();

    let data = null;

    if (text) {
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }
    }

    if (!response.ok) {
        const message =
            data && typeof data === "object"
                ? data.message || data.Message
                : null;

        throw new Error(message || `Request failed: ${response.status}`);
    }

    return data;
}

// =========================
// Lucide Icons
// =========================
if (window.lucide) {
lucide.createIcons();
}

// =========================
// Mobile Sidebar / Hamburger
// =========================
menuToggle?.addEventListener("click", () => {
sidebar?.classList.toggle("open");
});

document.addEventListener("click", (event) => {
if (
window.innerWidth <= 760 &&
sidebar?.classList.contains("open") &&
!sidebar.contains(event.target) &&
!menuToggle?.contains(event.target)
) {
sidebar.classList.remove("open");
}
});

document.querySelectorAll(".nav-link").forEach((link) => {
link.addEventListener("click", () => {
document.querySelectorAll(".nav-link").forEach((item) => {
item.classList.remove("active");
});

  link.classList.add("active");
  sidebar?.classList.remove("open");
});

});

// =========================
// Dashboard Summary
// =========================
async function loadDashboardSummary() {
try {
const data = await fetchAPI(
`/dashboard/summary?userId=${USER_ID}`
);

  const totalIncome = Number(getValue(data, "totalIncome") || 0);
  const totalExpenses = Number(getValue(data, "totalExpenses") || 0);
  const totalBalance = Number(
    getValue(data, "totalBalance") ?? totalIncome - totalExpenses
  );

  const balanceElement = document.getElementById("totalBalance");
  const incomeElement = document.getElementById("totalIncome");
  const expensesElement = document.getElementById("totalExpenses");
  const balanceIncomeElement = document.getElementById("balanceIncome");
  const balanceExpensesElement = document.getElementById("balanceExpenses");

  if (balanceElement) {
    balanceElement.textContent = formatMoney(totalBalance);
  }

  if (incomeElement) {
    incomeElement.textContent = formatMoney(totalIncome);
  }

  if (expensesElement) {
    expensesElement.textContent = formatMoney(totalExpenses);
  }

  if (balanceIncomeElement) {
    balanceIncomeElement.textContent = formatMoney(totalIncome);
  }

  if (balanceExpensesElement) {
    balanceExpensesElement.textContent = formatMoney(totalExpenses);
  }

  // Used by the category chart.
  const categoryTotal = document.getElementById("categoryTotal");

  if (categoryTotal) {
    categoryTotal.textContent = formatMoney(totalExpenses);
  }
} catch (error) {
  console.error("Could not load dashboard summary:", error);
  showToast("Could not load dashboard summary. Check the API server.");
}

}

// =========================
// Recent Transactions
// =========================
async function loadRecentTransactions() {
    const transactionList = document.getElementById("transactionList");

    if (!transactionList) return;

    try {
        const transactions = await fetchAPI(
            `/transactions/recent?userId=${USER_ID}`
        );

        if (!Array.isArray(transactions) || transactions.length === 0) {
            transactionList.innerHTML = `
                <p class="empty-state">No transactions found yet.</p>
            `;
            return;
        }

        transactionList.innerHTML = transactions.map(transaction => {
            const id = Number(getValue(transaction, "transactionID"));
            const category = getValue(transaction, "category") || "Other";
            const amount = Number(getValue(transaction, "amount") || 0);
            const type = getValue(transaction, "type") || "Expense";
            const description = getValue(transaction, "description") || "";
            const transactionDate = getValue(transaction, "transactionDate");

            const date = transactionDate
                ? new Date(transactionDate).toLocaleDateString("en-GB", {
                    day: "2-digit",
                    month: "short",
                    year: "numeric"
                })
                : "";

            const isIncome = type.toLowerCase() === "income";

            const icon = isIncome
                ? "wallet"
                : category.toLowerCase().includes("food")
                    ? "utensils"
                    : category.toLowerCase().includes("transport")
                        ? "car"
                        : "shopping-bag";

            return `
                <div class="transaction-item">
                    <div class="transaction-icon">
                        <i data-lucide="${icon}"></i>
                    </div>

                    <div class="transaction-details">
                        <strong>${escapeHTML(description || category)}</strong>
                        <span>${escapeHTML(category)} · ${escapeHTML(date)}</span>
                    </div>

                    <div class="transaction-amount ${isIncome ? "positive" : "negative"}">
                        ${isIncome ? "+" : "-"}${formatMoney(amount)}
                    </div>

                    <div class="transaction-actions">
                        <button
                            type="button"
                            class="edit-transaction"
                            data-id="${id}"
                            title="Edit transaction"
                            aria-label="Edit transaction">
                            <i data-lucide="pencil"></i>
                        </button>

                        <button
                            type="button"
                            class="delete-transaction"
                            data-id="${id}"
                            title="Delete transaction"
                            aria-label="Delete transaction">
                            <i data-lucide="trash-2"></i>
                        </button>
                    </div>
                </div>
            `;
        }).join("");

        if (window.lucide) {
            lucide.createIcons();
        }

        // Edit Transaction
        transactionList.querySelectorAll(".edit-transaction").forEach(button => {
            button.addEventListener("click", async () => {
                const id = Number(button.dataset.id);

                try {
                    const transaction = await fetchAPI(
                        `/transactions/${id}?userId=${USER_ID}`
                    );

                    if (!transaction) {
                        showToast("Transaction not found.");
                        return;
                    }

                    // Open the existing modal
                    openTransactionModal(
                        getValue(transaction, "type") || "Expense"
                    );

                    // Set the editing ID AFTER opening the modal
                    editingTransactionId = id;

                    document.getElementById("transactionCategory").value =
                        getValue(transaction, "category") || "";

                    document.getElementById("transactionAmount").value =
                        getValue(transaction, "amount") || "";

                    document.getElementById("transactionType").value =
                        getValue(transaction, "type") || "Expense";

                    document.getElementById("transactionDescription").value =
                        getValue(transaction, "description") || "";

                    const dateValue = getValue(transaction, "transactionDate");

                    document.getElementById("transactionDate").value =
                        dateValue ? String(dateValue).slice(0, 10) : getTodayDate();

                    document.getElementById("modalTitle").textContent =
                        "Edit Transaction";

                    document.getElementById("saveTransactionBtn").textContent =
                        "Update Transaction";

                } catch (error) {
                    console.error("Edit transaction error:", error);
                    showToast(error.message || "Could not load transaction.");
                }
            });
        });

        // Delete Transaction
        transactionList.querySelectorAll(".delete-transaction").forEach(button => {
            button.addEventListener("click", async () => {
                const id = Number(button.dataset.id);

                if (!confirm("Are you sure you want to delete this transaction?")) {
                    return;
                }

                button.disabled = true;

                try {
                    await fetchAPI(
                        `/transactions/${id}?userId=${USER_ID}`,
                        { method: "DELETE" }
                    );

                    showToast("Transaction deleted successfully!");

                    await Promise.allSettled([
                        loadDashboardSummary(),
                        loadRecentTransactions(),
                        loadSpendingChart(),
                        loadCategoryChart(),
                        loadMonthlyBudget(),
                        loadBudgets()
                    ]);

                } catch (error) {
                    console.error("Delete transaction error:", error);
                    showToast(error.message || "Could not delete transaction.");
                    button.disabled = false;
                }
            });
        });

    } catch (error) {
        console.error("Could not load recent transactions:", error);

        transactionList.innerHTML = `
            <p class="empty-state">
                Could not load transactions. Please try again.
            </p>
        `;
    }
}

// Prevent database text from being interpreted as HTML.
function escapeHTML(value) {
  const entities = {
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;"
  };

  return String(value).replace(/[&<>"']/g, character => {
    return entities[character];
  });
}

// =========================
// Spending Overview Chart
// =========================
async function loadSpendingChart() {
const canvas = document.getElementById("spendingChart");

if (!canvas || !window.Chart) return;

try {
  const monthlyData = await fetchAPI(
    `/dashboard/monthly-expenses?userId=${USER_ID}`
  );

  const rows = Array.isArray(monthlyData) ? monthlyData : [];

  // Show the last six calendar months, including months with no expenses.
  const now = new Date();
  const months = [];

  for (let offset = 5; offset >= 0; offset--) {
    const date = new Date(
      now.getFullYear(),
      now.getMonth() - offset,
      1
    );

    const year = date.getFullYear();
    const month = date.getMonth() + 1;

    const matchingRow = rows.find((row) =>
      Number(getValue(row, "year")) === year &&
      Number(getValue(row, "month")) === month
    );

    months.push({
      label: date.toLocaleDateString("en-US", {
        month: "short",
        year: "2-digit"
      }),
      amount: matchingRow
        ? Number(getValue(matchingRow, "totalExpenses") || 0)
        : 0
    });
  }

  const ctx = canvas.getContext("2d");
  const gradient = ctx.createLinearGradient(0, 0, 0, 230);

  gradient.addColorStop(0, "rgba(58, 91, 143, .30)");
  gradient.addColorStop(1, "rgba(58, 91, 143, .02)");

  const values = months.map((month) => month.amount);
  const highestValue = Math.max(...values, 0);
  const suggestedMax = Math.max(1000, Math.ceil(highestValue / 250) * 250);

  if (spendingChart) {
    spendingChart.destroy();
  }

  spendingChart = new Chart(ctx, {
    type: "line",

    data: {
      labels: months.map((month) => month.label),

      datasets: [{
        label: "Monthly Expenses",
        data: values,
        borderColor: "#315991",
        backgroundColor: gradient,
        fill: true,
        tension: 0.42,
        borderWidth: 2.5,
        pointRadius: 4,
        pointHoverRadius: 6,
        pointBackgroundColor: "#315991"
      }]
    },

    options: {
      responsive: true,
      maintainAspectRatio: false,

      plugins: {
        legend: {
          display: false
        },

        tooltip: {
          backgroundColor: "#284875",
          displayColors: false,

          callbacks: {
            label: (item) => formatMoney(item.raw)
          }
        }
      },

      scales: {
        x: {
          grid: {
            display: false
          },

          ticks: {
            color: "#355786",
            maxTicksLimit: 6,
            font: {
              family: "Georgia",
              size: 11
            },
            maxRotation: 0
          },

          border: {
            color: "#d8dee5"
          }
        },

        y: {
          beginAtZero: true,
          suggestedMax,

          ticks: {
            color: "#355786",

            callback: (value) => {
              return Number(value).toLocaleString("en-US");
            },

            font: {
              family: "Georgia",
              size: 11
            }
          },

          grid: {
            color: "#e0e4e7"
          },

          border: {
            color: "#d8dee5"
          }
        }
      }
    }
  });
} catch (error) {
  console.error("Could not load spending chart:", error);
}

}

// =========================
// Category Doughnut Chart
// =========================
async function loadCategoryChart() {
const canvas = document.getElementById("categoryChart");

if (!canvas || !window.Chart) return;

try {
  const categories = await fetchAPI(
    `/dashboard/category-expenses?userId=${USER_ID}`
  );

  const rows = Array.isArray(categories) ? categories : [];

  const labels = rows.map((row) =>
    String(getValue(row, "category") || "Other")
  );

  const values = rows.map((row) =>
    Number(getValue(row, "totalExpenses") || 0)
  );

  const colors = [
    "#304f82",
    "#398b77",
    "#c9ba91",
    "#fa9586",
    "#78a2d5",
    "#aaa0df",
    "#d19a66",
    "#658c9e"
  ];

  const total = values.reduce((sum, value) => sum + value, 0);

  const categoryTotal = document.getElementById("categoryTotal");

  if (categoryTotal) {
    categoryTotal.textContent = formatMoney(total);
  }

  const legend = document.getElementById("categoryLegend");

  if (legend) {
    legend.innerHTML = rows.length
      ? rows.map((row, index) => {
          const category = String(
            getValue(row, "category") || "Other"
          );

          const amount = Number(
            getValue(row, "totalExpenses") || 0
          );

          const percentage = total > 0
            ? (amount / total) * 100
            : 0;

          return `
            <li class="legend-item">
              <span
                class="legend-color"
                style="background-color: ${colors[index % colors.length]}"
              ></span>

              <span class="legend-label">
                ${escapeHTML(category)}
              </span>

              <span class="legend-value">
                ${percentage.toFixed(1)}%
              </span>
            </li>
          `;
        }).join("")
      : `<li class="empty-state">No expense data available.</li>`;
  }

  if (categoryChart) {
    categoryChart.destroy();
  }

  categoryChart = new Chart(canvas, {
    type: "doughnut",

    data: {
      labels,
      datasets: [{
        data: values,

        backgroundColor: labels.map(
          (_, index) => colors[index % colors.length]
        ),

        borderColor: "#f7f6f1",
        borderWidth: 2,
        hoverOffset: 5
      }]
    },

    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: "57%",

      plugins: {
        legend: {
          display: false
        },

        tooltip: {
          callbacks: {
            label: (item) =>
              `${item.label}: ${formatMoney(item.raw)}`
          }
        }
      }
    }
  });

  if (window.lucide) {
    lucide.createIcons();
  }
} catch (error) {
  console.error("Could not load category chart:", error);
}

}

// =========================
// Notifications
// =========================
document.querySelector(".notification-button")?.addEventListener(
"click",
() => {
showToast("You're all caught up!", 2200);
}
);

// =========================
// Period Selector
// =========================
document.getElementById("periodSelect")?.addEventListener(
"change",
(event) => {
showToast(`Showing spending overview: ${event.target.value}`);

  // The current API returns the last six calendar months.
  // Different period options need corresponding backend support.
}

);

async function loadMonthlyBudget() {
    try {
        const budgets = await fetchAPI(`/budgets?userId=${USER_ID}`);

        const now = new Date();

        const currentBudgets = budgets.filter(budget => {
            const date = new Date(getValue(budget, "budgetMonth"));

            return date.getFullYear() === now.getFullYear()
                && date.getMonth() === now.getMonth();
        });

        const totalBudget = currentBudgets.reduce(
            (sum, budget) =>
                sum + Number(getValue(budget, "budgetAmount") || 0),
            0
        );

        const totalSpent = currentBudgets.reduce(
            (sum, budget) =>
                sum + Number(getValue(budget, "spentAmount") || 0),
            0
        );

        const percentage = totalBudget > 0
            ? (totalSpent / totalBudget) * 100
            : 0;

        document.getElementById("monthlyBudget").textContent =
            formatMoney(totalBudget);

        document.getElementById("monthlyBudgetProgress").style.width =
            `${Math.min(percentage, 100)}%`;

        document.getElementById("monthlyBudgetUsed").textContent =
            `${percentage.toFixed(1)}% used`;

    } catch (error) {
        console.error("Monthly budget error:", error);
    }
}

async function loadBudgets() {
    const budgetList = document.getElementById("budgetList");

    if (!budgetList) return;

    try {
        const budgets = await fetchAPI(`/budgets?userId=${USER_ID}`);

        const now = new Date();

        const currentBudgets = budgets.filter(budget => {
            const date = new Date(getValue(budget, "budgetMonth"));

            return date.getFullYear() === now.getFullYear()
                && date.getMonth() === now.getMonth();
        });

        if (currentBudgets.length === 0) {
            budgetList.innerHTML = "<p>No budgets found this month.</p>";
            return;
        }

        budgetList.innerHTML = currentBudgets.map(budget => {
            const id = Number(getValue(budget, "budgetID"));
            const category = getValue(budget, "category") || "Other";
            const amount = Number(getValue(budget, "budgetAmount") || 0);
            const spent = Number(getValue(budget, "spentAmount") || 0);

            const remaining = Number(
                getValue(budget, "remainingAmount") ?? amount - spent
            );

            const percentage = amount > 0 ? (spent / amount) * 100 : 0;
            const width = Math.min(Math.max(percentage, 0), 100);

            const color = percentage >= 100
                ? "#dc6262"
                : percentage >= 80
                    ? "#d8a34a"
                    : "#398b77";

            return `
                <div class="budget-item">
                    <div class="budget-info">
                        <h3>${escapeHTML(category)}</h3>
                        <span>
                            ${formatMoney(spent)} / ${formatMoney(amount)}
                        </span>
                    </div>

                    <div class="progress-line">
                        <span style="width:${width}%; background:${color}"></span>
                    </div>

                    <div class="budget-footer">
                        <small>${percentage.toFixed(1)}% used</small>
                        <small>${formatMoney(remaining)} remaining</small>
                    </div>

                    <div class="budget-actions">
                        <button
                            type="button"
                            class="edit-budget"
                            data-id="${id}"
                        >
                            Edit
                        </button>

                        <button
                            type="button"
                            class="delete-budget"
                            data-id="${id}"
                        >
                            Delete
                        </button>
                    </div>
                </div>
            `;
        }).join("");

        // Edit Budget
        budgetList.querySelectorAll(".edit-budget").forEach(button => {
            button.addEventListener("click", async () => {
                const id = Number(button.dataset.id);

                try {
                    const budget = currentBudgets.find(item =>
                        Number(getValue(item, "budgetID")) === id
                    );

                    if (budget) {
                        openBudgetModal(budget);
                    } else {
                        showToast("Budget not found.");
                    }
                } catch (error) {
                    console.error(error);
                    showToast("Could not open budget.");
                }
            });
        });

        // Delete Budget
        budgetList.querySelectorAll(".delete-budget").forEach(button => {
            button.addEventListener("click", async () => {
                const id = Number(button.dataset.id);

                const confirmed = confirm(
                    "Are you sure you want to delete this budget?"
                );

                if (!confirmed) return;

                button.disabled = true;

                try {
                    await fetchAPI(
                        `/budgets/${id}?userId=${USER_ID}`,
                        { method: "DELETE" }
                    );

                    showToast("Budget deleted successfully!");

                    await Promise.allSettled([
                        loadMonthlyBudget(),
                        loadBudgets()
                    ]);

                } catch (error) {
                    console.error("Delete budget error:", error);
                    showToast(error.message || "Could not delete budget.");
                    button.disabled = false;
                }
            });
        });

    } catch (error) {
        console.error("Budgets error:", error);
        budgetList.innerHTML = "<p>Could not load budgets.</p>";
    }
}

// =========================
// Load Dashboard Data
// =========================
async function initializeDashboard() {
  await Promise.allSettled([
    loadDashboardSummary(),
    loadRecentTransactions(),
    loadSpendingChart(),
    loadCategoryChart(),
    loadMonthlyBudget(),
    loadBudgets()
  ]);
}
// =====================================
// TRANSACTION MODAL
// =====================================

const transactionModal = document.getElementById("transactionModal");
const transactionForm = document.getElementById("transactionForm");

const transactionType = document.getElementById("transactionType");
const transactionCategory = document.getElementById("transactionCategory");
const transactionAmount = document.getElementById("transactionAmount");
const transactionDate = document.getElementById("transactionDate");
const transactionDescription = document.getElementById("transactionDescription");

const modalTitle = document.getElementById("modalTitle");
const saveTransactionBtn = document.getElementById("saveTransactionBtn");

let editingTransactionId = null;


// =====================================
// OPEN MODAL
// =====================================

function openTransactionModal(type = "Expense") {
    transactionForm.reset();

    editingTransactionId = null;

    modalTitle.textContent =
        type === "Income" ? "Add Income" : "Add Expense";

    transactionType.value = type;

    transactionDate.value = getTodayDate();

    saveTransactionBtn.textContent = "Save Transaction";

    transactionModal.classList.add("active");
}


// =====================================
// CLOSE MODAL
// =====================================

function closeTransactionModal() {
    transactionModal.classList.remove("active");

    transactionForm.reset();

    editingTransactionId = null;
}


// =====================================
// TODAY'S DATE
// =====================================

function getTodayDate() {
    const now = new Date();

    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, "0");
    const day = String(now.getDate()).padStart(2, "0");

    return `${year}-${month}-${day}`;
}


// =====================================
// CONNECT EXISTING QUICK ACTION
// =====================================

document.querySelectorAll(".action-button").forEach(button => {
    button.addEventListener("click", () => {
        const action = button.dataset.action;

        switch (action) {
            case "Add Transaction":
                openTransactionModal("Expense");
                break;

            case "Set Budget":
                openBudgetModal();
                break;

            case "View Reports":
                document.getElementById("reports")?.scrollIntoView({
                    behavior: "smooth"
                });
                loadReports();
                break;

            case "Manage Categories":
                showToast(
                    "Categories are currently managed through transactions."
                );
                break;
        }
    });
});


// =====================================
// CLOSE BUTTONS
// =====================================

document.getElementById("closeModalBtn")
    .addEventListener("click", closeTransactionModal);

document.getElementById("cancelTransactionBtn")
    .addEventListener("click", closeTransactionModal);

transactionModal.addEventListener("click", event => {
    if (event.target === transactionModal) {
        closeTransactionModal();
    }
});

document.addEventListener("keydown", event => {
    if (event.key === "Escape" &&
        transactionModal.classList.contains("active")) {
        closeTransactionModal();
    }
});


// =====================================
// SAVE TRANSACTION
// =====================================

transactionForm.addEventListener("submit", async event => {
    event.preventDefault();

    const amount = Number(transactionAmount.value);
    const category = transactionCategory.value;
    const date = transactionDate.value;

    if (
        !Number.isFinite(amount) ||
        amount <= 0 ||
        !category ||
        !date
    ) {
        showToast("Please enter valid transaction details.");
        return;
    }

    const transaction = {
        category: category,
        amount: amount,
        type: transactionType.value,
        description: transactionDescription.value.trim() || null,
        transactionDate: date
    };

    const isEditing = editingTransactionId !== null;

    const endpoint = isEditing
        ? `/transactions/${editingTransactionId}`
        : "/transactions";

    const method = isEditing ? "PUT" : "POST";

    saveTransactionBtn.disabled = true;
    saveTransactionBtn.textContent = isEditing
        ? "Updating..."
        : "Saving...";

    try {
        await fetchAPI(endpoint, {
            method: method,
            body: JSON.stringify(transaction)
        });

        closeTransactionModal();

        showToast(
            isEditing
                ? "Transaction updated successfully!"
                : "Transaction added successfully!"
        );

        await Promise.allSettled([
            loadDashboardSummary(),
            loadRecentTransactions(),
            loadSpendingChart(),
            loadCategoryChart(),
            loadMonthlyBudget(),
            loadBudgets()
        ]);

    } catch (error) {
        console.error("Save transaction error:", error);

        showToast(
            error.message || "Could not save transaction."
        );

    } finally {
        saveTransactionBtn.disabled = false;
        saveTransactionBtn.textContent = "Save Transaction";
    }
});

// =========================
// Budget Modal
// =========================

let editingBudgetId = null;

function createBudgetModal() {
    if (document.getElementById("budgetModal")) return;

    const modal = document.createElement("div");

    modal.id = "budgetModal";
    modal.className = "modal-overlay";

    modal.innerHTML = `
        <div class="transaction-modal">
            <div class="modal-header">
                <h2 id="budgetModalTitle">Set Budget</h2>

                <button
                    type="button"
                    id="closeBudgetModal"
                    class="modal-close"
                    aria-label="Close"
                >&times;</button>
            </div>

            <form id="budgetForm">
                <div class="form-group">
                    <label for="budgetCategory">Category</label>

                    <input
                        id="budgetCategory"
                        type="text"
                        maxlength="100"
                        required
                        placeholder="e.g. Food"
                    >
                </div>

                <div class="form-group">
                    <label for="budgetAmount">Monthly Budget (EGP)</label>

                    <input
                        id="budgetAmount"
                        type="number"
                        min="0.01"
                        step="0.01"
                        required
                        placeholder="Enter amount"
                    >
                </div>

                <div class="form-group">
                    <label for="budgetMonth">Month</label>

                    <input
                        id="budgetMonth"
                        type="month"
                        required
                    >
                </div>

                <div class="modal-actions">
                    <button
                        type="button"
                        id="cancelBudgetModal"
                    >Cancel</button>

                    <button
                        type="submit"
                        id="saveBudgetBtn"
                    >Save Budget</button>
                </div>
            </form>
        </div>
    `;

    document.body.appendChild(modal);

    document.getElementById("closeBudgetModal")
        .addEventListener("click", closeBudgetModal);

    document.getElementById("cancelBudgetModal")
        .addEventListener("click", closeBudgetModal);

    modal.addEventListener("click", event => {
        if (event.target === modal) {
            closeBudgetModal();
        }
    });

    document.getElementById("budgetForm")
        .addEventListener("submit", saveBudget);
}

function openBudgetModal(budget = null) {
    createBudgetModal();

    const modal = document.getElementById("budgetModal");
    const form = document.getElementById("budgetForm");

    form.reset();

    editingBudgetId = budget
        ? Number(getValue(budget, "budgetID"))
        : null;

    document.getElementById("budgetModalTitle").textContent =
        budget ? "Edit Budget" : "Set Budget";

    document.getElementById("budgetCategory").value =
        budget ? getValue(budget, "category") || "" : "";

    document.getElementById("budgetAmount").value =
        budget ? getValue(budget, "budgetAmount") || "" : "";

    const month = budget
        ? String(getValue(budget, "budgetMonth") || "").slice(0, 7)
        : getTodayDate().slice(0, 7);

    document.getElementById("budgetMonth").value = month;

    modal.classList.add("active");
}

function closeBudgetModal() {
    document.getElementById("budgetModal")
        ?.classList.remove("active");

    editingBudgetId = null;
}

async function saveBudget(event) {
    event.preventDefault();

    const category = document
        .getElementById("budgetCategory")
        .value.trim();

    const amount = Number(
        document.getElementById("budgetAmount").value
    );

    const month = document
        .getElementById("budgetMonth")
        .value;

    if (!category || category.length > 100 || amount <= 0 || !month) {
        showToast("Please enter valid budget information.");
        return;
    }

    const payload = {
        category,
        budgetAmount: amount,
        budgetMonth: `${month}-01T00:00:00`
    };

    const saveButton = document.getElementById("saveBudgetBtn");

    saveButton.disabled = true;
    saveButton.textContent = "Saving...";

    try {
        const endpoint = editingBudgetId
            ? `/budgets/${editingBudgetId}`
            : "/budgets";

        await fetchAPI(endpoint, {
            method: editingBudgetId ? "PUT" : "POST",
            body: JSON.stringify(payload)
        });

        const wasEditing = Boolean(editingBudgetId);
        closeBudgetModal();
        showToast(
            wasEditing
                ? "Budget updated successfully!"
                : "Budget added successfully!"
        );

        await Promise.allSettled([
            loadMonthlyBudget(),
            loadBudgets()
        ]);
    } catch (error) {
        console.error("Save budget error:", error);
        showToast(error.message || "Could not save budget.");
    } finally {
        saveButton.disabled = false;
        saveButton.textContent = "Save Budget";
    }
}

async function loadReports() {
    try {
        const summary = await fetchAPI(
            `/dashboard/summary?userId=${USER_ID}`
        );

        const income = Number(
            getValue(summary, "totalIncome") || 0
        );

        const expenses = Number(
            getValue(summary, "totalExpenses") || 0
        );

        const balance = Number(
            getValue(summary, "totalBalance") ?? income - expenses
        );

        document.getElementById("reportIncome").textContent =
            formatMoney(income);

        document.getElementById("reportExpenses").textContent =
            formatMoney(expenses);

        document.getElementById("reportBalance").textContent =
            formatMoney(balance);

        document.getElementById("reports").scrollIntoView({
            behavior: "smooth",
            block: "start"
        });

    } catch (error) {
        console.error("Reports error:", error);
        showToast("Could not load financial reports.");
    }
}

initializeDashboard();
});
