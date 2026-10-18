/**
 * Pennywise Login Page Controller
 */

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('login-form');
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');
  const rememberCheckbox = document.getElementById('remember-me');
  const submitBtn = document.getElementById('submit-btn');
  const alertBanner = document.getElementById('alert-banner');
  const alertText = document.getElementById('alert-text');
  const pwdToggleBtn = document.getElementById('pwd-toggle-btn');
  const pwdToggleIcon = document.getElementById('pwd-toggle-icon');

  const emailError = document.getElementById('email-error');
  const passwordError = document.getElementById('password-error');

  // SVG strings for password visibility toggle
  const eyeIconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/></svg>';
  const eyeOffIconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><line x1="2" x2="22" y1="2" y2="22"/></svg>';

  // Check URL params for post-registration greeting
  const urlParams = new URLSearchParams(window.location.search);
  if (urlParams.get('registered') === 'true') {
    const regEmail = urlParams.get('email');
    if (regEmail) {
      emailInput.value = regEmail;
    }
    showAlert('Account created successfully! Please log in with your credentials.', 'success');
  } else {
    // Restore remembered email if available
    const savedEmail = localStorage.getItem('pennywise_remembered_email');
    if (savedEmail) {
      emailInput.value = savedEmail;
      rememberCheckbox.checked = true;
      passwordInput.focus();
    }
  }

  // Password Visibility Toggle
  if (pwdToggleBtn) {
    pwdToggleBtn.addEventListener('click', () => {
      const isPassword = passwordInput.getAttribute('type') === 'password';
      passwordInput.setAttribute('type', isPassword ? 'text' : 'password');
      pwdToggleBtn.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      pwdToggleIcon.innerHTML = isPassword ? eyeOffIconSvg : eyeIconSvg;
    });
  }

  // Real-time Input Validation
  emailInput.addEventListener('input', () => {
    clearFieldError(emailInput, emailError);
  });

  passwordInput.addEventListener('input', () => {
    clearFieldError(passwordInput, passwordError);
  });

  emailInput.addEventListener('blur', () => {
    validateEmailField();
  });

  passwordInput.addEventListener('blur', () => {
    validatePasswordField();
  });

  function validateEmailField() {
    const val = emailInput.value.trim();
    if (!val) {
      showFieldError(emailInput, emailError, 'Email address is required.');
      return false;
    }
    if (!ValidationUtils.isValidEmail(val)) {
      showFieldError(emailInput, emailError, 'Please enter a valid email address.');
      return false;
    }
    clearFieldError(emailInput, emailError);
    return true;
  }

  function validatePasswordField() {
    const val = passwordInput.value;
    if (!val) {
      showFieldError(passwordInput, passwordError, 'Password is required.');
      return false;
    }
    clearFieldError(passwordInput, passwordError);
    return true;
  }

  function showFieldError(input, errorSpan, message) {
    input.classList.add('is-invalid');
    input.classList.remove('is-valid');
    errorSpan.textContent = message;
    errorSpan.classList.add('show');
  }

  function clearFieldError(input, errorSpan) {
    input.classList.remove('is-invalid');
    errorSpan.textContent = '';
    errorSpan.classList.remove('show');
  }

  function showAlert(message, type = 'error') {
    alertBanner.className = `alert-banner alert-${type} show`;
    alertText.textContent = message;
  }

  function hideAlert() {
    alertBanner.className = 'alert-banner';
    alertText.textContent = '';
  }

  // Form Submission
  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert();

    const isEmailValid = validateEmailField();
    const isPasswordValid = validatePasswordField();

    if (!isEmailValid || !isPasswordValid) {
      return;
    }

    // Set Loading State
    setLoading(true);

    const payload = {
      email: emailInput.value.trim(),
      password: passwordInput.value
    };

    const response = await AuthClient.login(payload);
    setLoading(false);

    if (response.ok && response.data.success) {
      // Remember me handling
      if (rememberCheckbox.checked) {
        localStorage.setItem('pennywise_remembered_email', payload.email);
      } else {
        localStorage.removeItem('pennywise_remembered_email');
      }

      // Store non-sensitive user metadata in session for local demonstration
      if (response.data.user) {
        sessionStorage.setItem('pennywise_auth_user', JSON.stringify(response.data.user));
      }

      showAlert('Login successful! Redirecting to dashboard...', 'success');
      showToast('Welcome back, ' + (response.data.user?.name || 'User') + '!', 'success');

      // Seamless redirect after short feedback delay
      setTimeout(() => {
        const destination = response.data.redirectUrl || AppConfig.dashboardUrl;
        window.location.href = destination;
      }, 750);
    } else {
      // Handle server error responses (401, 400, 500, network)
      const errorMsg = response.data?.message || 'Login failed. Please check your credentials.';
      showAlert(errorMsg, 'error');

      if (response.status === 401) {
        showFieldError(passwordInput, passwordError, 'Invalid email or password.');
        passwordInput.focus();
      }
    }
  });

  function setLoading(isLoading) {
    if (isLoading) {
      submitBtn.classList.add('loading');
      submitBtn.disabled = true;
      emailInput.disabled = true;
      passwordInput.disabled = true;
    } else {
      submitBtn.classList.remove('loading');
      submitBtn.disabled = false;
      emailInput.disabled = false;
      passwordInput.disabled = false;
    }
  }
});
