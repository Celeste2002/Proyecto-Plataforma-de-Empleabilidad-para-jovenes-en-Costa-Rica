const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function sendJsonRequest(url, options) {
  try {
    const response = await fetch(url, options);
    return readApiResponse(response);
  } catch (error) {
    if (error instanceof TypeError) {
      throw new Error(
        `No se pudo conectar con el servidor. Verifica que el backend esté ejecutándose en ${apiBaseUrl}.`,
      );
    }

    throw error;
  }
}

async function readApiResponse(response) {
  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    if (response.status === 401) {
      window.dispatchEvent(new Event('auth:unauthorized'));
    }
    const error = new Error(body.message ?? 'Error inesperado del servidor.');
    error.status = response.status;
    error.validationErrors = body.errors ?? [];
    throw error;
  }
  return body;
}

export async function login(email, password) {
  return sendJsonRequest(`${apiBaseUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
}

export async function forgotPassword(email) {
  return sendJsonRequest(`${apiBaseUrl}/api/auth/forgot-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  });
}

export async function resetPassword(token, newPassword) {
  return sendJsonRequest(`${apiBaseUrl}/api/auth/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token, newPassword }),
  });
}
