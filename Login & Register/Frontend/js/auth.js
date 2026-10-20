/**
 * Pennywise Authentication Client & Validation Service
 */

const AuthClient = {
  /**
   * Registers a new user account with the C# backend.
   * @param {Object} data { name, email, password, confirmPassword }
   * @returns {Promise<{ status: number, ok: boolean, data: any }>}
   */
  async register(data) {
    try {
      const response = await fetch(`${AppConfig.apiBaseUrl}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify({
          fullName: data.name,
          email: data.email,
          password: data.password
        })
      });

      const result = await response.json().catch(() => ({}));
      const json = {
        ...result,
        success: response.ok,
        message: result.title || result.message || ''
      };

      return {
        status: response.status,
        ok: response.ok,
        data: json
      };
    } catch (err) {
      return {
        status: 0,
        ok: false,
        data: {
          success: false,
          message: 'Unable to connect to the authentication server. Please verify the backend is running.'
        }
      };
    }
  },

  /**
   * Authenticates user credentials with the C# backend.
   * @param {Object} data { email, password }
   * @returns {Promise<{ status: number, ok: boolean, data: any }>}
   */
  async login(data) {
    try {
      const response = await fetch(`${AppConfig.apiBaseUrl}/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json'
        },
        body: JSON.stringify(data)
      });

      const result = await response.json().catch(() => ({}));
      const json = {
        ...result,
        success: response.ok,
        message: result.title || result.message || '',
        user: result.profile ? { ...result.profile, name: result.profile.fullName } : null,
        redirectUrl: AppConfig.dashboardUrl
      };
      if (response.ok && result.token) {
        localStorage.setItem('pw_token', result.token);
      }

      return {
        status: response.status,
        ok: response.ok,
        data: json
      };
    } catch (err) {
      return {
        status: 0,
        ok: false,
        data: {
          success: false,
          message: 'Unable to connect to the authentication server. Please verify the backend is running.'
        }
      };
    }
  },

  /**
   * Checks backend server health.
   */
  async checkHealth() {
    try {
      const response = await fetch('/health', { method: 'GET' });
      return response.ok;
    } catch {
      return false;
    }
  }
};

/**
 * Validation Helpers
 */
const ValidationUtils = {
  EMAIL_REGEX: /^[^@\s]+@[^@\s]+\.[^@\s]+$/,

  isValidEmail(email) {
    if (!email || typeof email !== 'string') return false;
    return this.EMAIL_REGEX.test(email.trim());
  },

  evaluatePassword(password) {
    if (!password) {
      return {
        score: 0,
        label: '',
        length: false,
        hasUpper: false,
        hasLower: false,
        hasNumber: false,
        hasSpecial: false,
        isValid: false
      };
    }

    const length = password.length >= 8;
    const hasUpper = /[A-Z]/.test(password);
    const hasLower = /[a-z]/.test(password);
    const hasNumber = /\d/.test(password);
    const hasSpecial = /[^A-Za-z0-9]/.test(password);

    let passedRules = 0;
    if (length) passedRules++;
    if (hasUpper) passedRules++;
    if (hasLower) passedRules++;
    if (hasNumber) passedRules++;
    if (hasSpecial) passedRules++;

    let score = 0;
    let label = 'Weak';

    if (passedRules >= 5) {
      score = 4;
      label = 'Strong';
    } else if (passedRules >= 4) {
      score = 3;
      label = 'Good';
    } else if (passedRules >= 3) {
      score = 2;
      label = 'Fair';
    } else if (passedRules >= 1) {
      score = 1;
      label = 'Weak';
    }

    return {
      score,
      label,
      length,
      hasUpper,
      hasLower,
      hasNumber,
      hasSpecial,
      isValid: length && hasUpper && hasLower && hasNumber && hasSpecial
    };
  }
};
