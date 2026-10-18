/**
 * Pennywise Register Page Controller
 */

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('register-form');
  const nameInput = document.getElementById('name');
  const emailInput = document.getElementById('email');
  const passwordInput = document.getElementById('password');
  const confirmPasswordInput = document.getElementById('confirm-password');
  const termsCheckbox = document.getElementById('terms');
  const submitBtn = document.getElementById('submit-btn');

  const alertBanner = document.getElementById('alert-banner');
  const alertText = document.getElementById('alert-text');

  // Error spans
  const nameError = document.getElementById('name-error');
  const emailError = document.getElementById('email-error');
  const passwordError = document.getElementById('password-error');
  const confirmPasswordError = document.getElementById('confirm-password-error');
  const termsError = document.getElementById('terms-error');

  // Password visibility buttons
  const pwdToggleBtn = document.getElementById('pwd-toggle-btn');
  const pwdToggleIcon = document.getElementById('pwd-toggle-icon');
  const confirmToggleBtn = document.getElementById('confirm-toggle-btn');
  const confirmToggleIcon = document.getElementById('confirm-toggle-icon');

  // Password Strength & Rules elements
  const strengthContainer = document.getElementById('password-strength');
  const strengthText = document.getElementById('strength-text');
  const ruleLength = document.getElementById('rule-length');
  const ruleUpper = document.getElementById('rule-upper');
  const ruleLower = document.getElementById('rule-lower');
  const ruleNumber = document.getElementById('rule-number');
  const ruleSpecial = document.getElementById('rule-special');

  // Terms Modal elements
  const termsLink = document.getElementById('terms-link');
  const termsModal = document.getElementById('terms-modal');
  const closeTermsBtn = document.getElementById('close-terms-btn');
  const cancelTermsBtn = document.getElementById('cancel-terms-btn');
  const acceptTermsBtn = document.getElementById('accept-terms-btn');

  // SVG strings for visibility toggles
  const eyeIconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/></svg>';
  const eyeOffIconSvg = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="M10.73 5.08A10.43 10.43 0 0 1 12 5c7 0 10 7 10 7a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.526 13.526 0 0 0 2 12s3 7 10 7a9.74 9.74 0 0 0 5.39-1.61"/><line x1="2" x2="22" y1="2" y2="22"/></svg>';

  // Password Visibility Toggles
  setupVisibilityToggle(pwdToggleBtn, pwdToggleIcon, passwordInput);
  setupVisibilityToggle(confirmToggleBtn, confirmToggleIcon, confirmPasswordInput);

  function setupVisibilityToggle(btn, iconContainer, input) {
    if (!btn) return;
    btn.addEventListener('click', () => {
      const isPassword = input.getAttribute('type') === 'password';
      input.setAttribute('type', isPassword ? 'text' : 'password');
      btn.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      iconContainer.innerHTML = isPassword ? eyeOffIconSvg : eyeIconSvg;
    });
  }

  // Password Strength Real-Time Evaluation
  passwordInput.addEventListener('input', () => {
    const val = passwordInput.value;
    const evaluation = ValidationUtils.evaluatePassword(val);

    // Update Strength Meter Bar
    strengthContainer.className = 'password-strength-container';
    if (val.length > 0) {
      if (evaluation.score === 1) strengthContainer.classList.add('strength-weak');
      else if (evaluation.score === 2) strengthContainer.classList.add('strength-fair');
      else if (evaluation.score === 3) strengthContainer.classList.add('strength-good');
      else if (evaluation.score >= 4) strengthContainer.classList.add('strength-strong');
    }
    strengthText.textContent = val.length > 0 ? evaluation.label : 'None';

    // Update Rules Checklist
    toggleRule(ruleLength, evaluation.length);
    toggleRule(ruleUpper, evaluation.hasUpper);
    toggleRule(ruleLower, evaluation.hasLower);
    toggleRule(ruleNumber, evaluation.hasNumber);
    toggleRule(ruleSpecial, evaluation.hasSpecial);

    clearFieldError(passwordInput, passwordError);

    // If confirm password has content, recheck match
    if (confirmPasswordInput.value.length > 0) {
      validateConfirmPasswordField();
    }
  });

  function toggleRule(element, isValid) {
    if (!element) return;
    if (isValid) element.classList.add('valid');
    else element.classList.remove('valid');
  }

  // Real-time input clearers & blurs
  nameInput.addEventListener('input', () => clearFieldError(nameInput, nameError));
  emailInput.addEventListener('input', () => clearFieldError(emailInput, emailError));
  confirmPasswordInput.addEventListener('input', () => clearFieldError(confirmPasswordInput, confirmPasswordError));
  termsCheckbox.addEventListener('change', () => clearFieldError(termsCheckbox, termsError));

  nameInput.addEventListener('blur', validateNameField);
  emailInput.addEventListener('blur', validateEmailField);
  passwordInput.addEventListener('blur', validatePasswordField);
  confirmPasswordInput.addEventListener('blur', validateConfirmPasswordField);

  function validateNameField() {
    const val = nameInput.value.trim();
    if (!val) {
      showFieldError(nameInput, nameError, 'Full name is required.');
      return false;
    }
    if (val.length < 2) {
      showFieldError(nameInput, nameError, 'Name must be at least 2 characters.');
      return false;
    }
    clearFieldError(nameInput, nameError);
    return true;
  }

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
    const evalResult = ValidationUtils.evaluatePassword(val);
    if (!evalResult.isValid) {
      showFieldError(passwordInput, passwordError, 'Password does not satisfy all complexity requirements.');
      return false;
    }
    clearFieldError(passwordInput, passwordError);
    return true;
  }

  function validateConfirmPasswordField() {
    const pwdVal = passwordInput.value;
    const confirmVal = confirmPasswordInput.value;
    if (!confirmVal) {
      showFieldError(confirmPasswordInput, confirmPasswordError, 'Password confirmation is required.');
      return false;
    }
    if (pwdVal !== confirmVal) {
      showFieldError(confirmPasswordInput, confirmPasswordError, 'Passwords do not match.');
      return false;
    }
    clearFieldError(confirmPasswordInput, confirmPasswordError);
    return true;
  }

  function validateTerms() {
    if (!termsCheckbox.checked) {
      termsError.textContent = 'You must agree to the Terms & Conditions to register.';
      termsError.classList.add('show');
      return false;
    }
    termsError.textContent = '';
    termsError.classList.remove('show');
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

  // Terms & Conditions Modal Handlers
  if (termsLink && termsModal) {
    termsLink.addEventListener('click', (e) => {
      e.preventDefault();
      termsModal.classList.add('show');
      termsModal.setAttribute('aria-hidden', 'false');
    });

    const closeTermsModal = () => {
      termsModal.classList.remove('show');
      termsModal.setAttribute('aria-hidden', 'true');
    };

    if (closeTermsBtn) closeTermsBtn.addEventListener('click', closeTermsModal);
    if (cancelTermsBtn) cancelTermsBtn.addEventListener('click', closeTermsModal);

    if (acceptTermsBtn) {
      acceptTermsBtn.addEventListener('click', () => {
        termsCheckbox.checked = true;
        clearFieldError(termsCheckbox, termsError);
        closeTermsModal();
      });
    }

    termsModal.addEventListener('click', (e) => {
      if (e.target === termsModal) closeTermsModal();
    });
  }

  // Form Submission
  form.addEventListener('submit', async (e) => {
    e.preventDefault();
    hideAlert();

    const isNameOk = validateNameField();
    const isEmailOk = validateEmailField();
    const isPwdOk = validatePasswordField();
    const isConfirmOk = validateConfirmPasswordField();
    const isTermsOk = validateTerms();

    if (!isNameOk || !isEmailOk || !isPwdOk || !isConfirmOk || !isTermsOk) {
      return;
    }

    setLoading(true);

    const payload = {
      name: nameInput.value.trim(),
      email: emailInput.value.trim(),
      password: passwordInput.value,
      confirmPassword: confirmPasswordInput.value
    };

    const response = await AuthClient.register(payload);
    setLoading(false);

    if (response.ok && response.data.success) {
      showAlert('Account created successfully! Redirecting to login...', 'success');
      showToast('Registration complete. Redirecting to login...', 'success');

      setTimeout(() => {
        const encodedEmail = encodeURIComponent(payload.email);
        window.location.href = `login.html?registered=true&email=${encodedEmail}`;
      }, 1000);
    } else {
      // Handle server error responses (400, 409, 500)
      const errorMsg = response.data?.message || 'Registration failed. Please check the entered data.';
      showAlert(errorMsg, 'error');

      if (response.status === 409) {
        showFieldError(emailInput, emailError, 'This email address is already registered.');
        emailInput.focus();
      } else if (response.data?.errors && Array.isArray(response.data.errors)) {
        showAlert(response.data.errors.join(' '), 'error');
      }
    }
  });

  function setLoading(isLoading) {
    if (isLoading) {
      submitBtn.classList.add('loading');
      submitBtn.disabled = true;
      nameInput.disabled = true;
      emailInput.disabled = true;
      passwordInput.disabled = true;
      confirmPasswordInput.disabled = true;
      termsCheckbox.disabled = true;
    } else {
      submitBtn.classList.remove('loading');
      submitBtn.disabled = false;
      nameInput.disabled = false;
      emailInput.disabled = false;
      passwordInput.disabled = false;
      confirmPasswordInput.disabled = false;
      termsCheckbox.disabled = false;
    }
  }
});
