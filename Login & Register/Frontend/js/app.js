/**
 * Pennywise Global Application Configuration & Utilities
 */

const AppConfig = (() => {
  // Determine API base URL dynamically
  const isSameOriginBackend = window.location.protocol.startsWith('http') && 
    (window.location.port === '5005' || window.location.pathname.startsWith('/api'));

  const apiBase = isSameOriginBackend 
    ? '/api/auth' 
    : 'http://localhost:5005/api/auth';

  // Configurable Dashboard URL (Integration Placeholder)
  const dashboardUrl = 'dashboard.html';

  return {
    apiBaseUrl: apiBase,
    dashboardUrl: dashboardUrl,
    appName: 'Pennywise'
  };
})();

/**
 * Toast Notification Dispatcher
 */
function showToast(message, type = 'info', durationMs = 4000) {
  let container = document.getElementById('toast-container');
  if (!container) {
    container = document.createElement('div');
    container.id = 'toast-container';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;

  const iconSvg = type === 'success'
    ? '<svg class="toast-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>'
    : type === 'error'
    ? '<svg class="toast-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>'
    : '<svg class="toast-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>';

  toast.innerHTML = `${iconSvg}<span>${escapeHtml(message)}</span>`;
  container.appendChild(toast);

  // Trigger animation
  requestAnimationFrame(() => toast.classList.add('show'));

  // Auto remove
  setTimeout(() => {
    toast.classList.remove('show');
    toast.addEventListener('transitionend', () => toast.remove());
  }, durationMs);
}

/**
 * Safe HTML string escaper
 */
function escapeHtml(str) {
  if (!str) return '';
  const div = document.createElement('div');
  div.textContent = str;
  return div.innerHTML;
}
