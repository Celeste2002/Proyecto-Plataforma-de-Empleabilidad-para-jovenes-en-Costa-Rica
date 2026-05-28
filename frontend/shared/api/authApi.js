const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function readApiResponse(response) {
  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    const error = new Error(body.message ?? 'Error inesperado del servidor.');
    error.validationErrors = body.errors ?? [];
    throw error;
  }
  return body;
}

export async function login(email, password) {
  const response = await fetch(`${apiBaseUrl}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  return readApiResponse(response);
}

export async function forgotPassword(email) {
  const response = await fetch(`${apiBaseUrl}/api/auth/forgot-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email }),
  });
  return readApiResponse(response);
}

export async function resetPassword(token, newPassword) {
  const response = await fetch(`${apiBaseUrl}/api/auth/reset-password`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ token, newPassword }),
  });
  return readApiResponse(response);
}
