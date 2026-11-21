const API_ROOT = '/api';

export function saveSession(accessToken, refreshToken, email) {
  localStorage.setItem('mercato.accessToken', accessToken);
  localStorage.setItem('mercato.refreshToken', refreshToken);
  if (email) localStorage.setItem('mercato.email', email);
}

export function clearSession() {
  localStorage.removeItem('mercato.accessToken');
  localStorage.removeItem('mercato.refreshToken');
  localStorage.removeItem('mercato.email');
}

async function decodeError(response) {
  const raw = await response.text();
  if (!raw) return `Sorğu uğursuz oldu (${response.status}).`;
  try {
    const body = JSON.parse(raw);
    if (typeof body === 'string') return body;
    if (body.message) return body.message;
    if (body.title) return body.title;
    if (body.errors) return Object.values(body.errors).flat().join(' ');
  } catch {
    return raw.length < 260 ? raw : `Sorğu uğursuz oldu (${response.status}).`;
  }
  return `Sorğu uğursuz oldu (${response.status}).`;
}

async function rawRequest(path, options = {}, accessToken) {
  const headers = new Headers(options.headers || {});
  const isFormData = options.body instanceof FormData;
  if (options.body && !isFormData && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }
  if (accessToken) headers.set('Authorization', `Bearer ${accessToken}`);

  const response = await fetch(`${API_ROOT}${path}`, {
    ...options,
    headers,
    body: options.body && !isFormData && typeof options.body !== 'string'
      ? JSON.stringify(options.body)
      : options.body,
  });

  if (!response.ok) {
    const error = new Error(await decodeError(response));
    error.status = response.status;
    throw error;
  }
  if (response.status === 204) return null;
  const type = response.headers.get('content-type') || '';
  return type.includes('application/json') ? response.json() : response.text();
}

export async function request(path, options = {}) {
  const token = localStorage.getItem('mercato.accessToken');
  try {
    return await rawRequest(path, options, token);
  } catch (error) {
    if (!options._retried && token && error.status === 401) {
      const refreshToken = localStorage.getItem('mercato.refreshToken');
      if (refreshToken) {
        let renewed;
        try {
          renewed = await rawRequest('/Auth/refresh', {
            method: 'POST',
            body: { refreshToken },
          });
        } catch {
          clearSession();
          window.dispatchEvent(new Event('mercato:session-expired'));
          throw error;
        }
        saveSession(renewed.accessToken, renewed.refreshToken, localStorage.getItem('mercato.email'));
        try {
          return await request(path, { ...options, _retried: true });
        } catch (retryError) {
          if (retryError.status === 401) {
            clearSession();
            window.dispatchEvent(new Event('mercato:session-expired'));
          }
          throw retryError;
        }
      } else {
        clearSession();
        window.dispatchEvent(new Event('mercato:session-expired'));
      }
    }
    throw error;
  }
}
