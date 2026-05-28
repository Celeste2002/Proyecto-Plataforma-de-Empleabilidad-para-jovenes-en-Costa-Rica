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

export async function getUsers(token) {
  const response = await fetch(`${apiBaseUrl}/api/admin/users`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return readApiResponse(response);
}

export async function updateUserRole(userId, newRole, token) {
  const response = await fetch(`${apiBaseUrl}/api/admin/users/${userId}/role`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ newRole }),
  });

  if (response.status === 204) return;
  return readApiResponse(response);
}
