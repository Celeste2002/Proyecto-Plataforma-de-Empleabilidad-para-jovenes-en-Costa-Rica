const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

async function sendApiRequest(url, options) {
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
    const error = new Error(body.message ?? 'Error inesperado del servidor.');
    error.validationErrors = body.errors ?? [];
    throw error;
  }
  return body;
}

export async function getUsers(token) {
  return sendApiRequest(`${apiBaseUrl}/api/admin/users`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function getVacantes(token) {
  return sendApiRequest(`${apiBaseUrl}/api/admin/vacantes`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function updateUserRole(userId, newRole, token) {
  let response;

  try {
    response = await fetch(`${apiBaseUrl}/api/admin/users/${userId}/role`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ newRole }),
    });
  } catch (error) {
    if (error instanceof TypeError) {
      throw new Error(
        `No se pudo conectar con el servidor. Verifica que el backend esté ejecutándose en ${apiBaseUrl}.`,
      );
    }

    throw error;
  }

  if (response.status === 204) return;
  return readApiResponse(response);
}

export async function updateVacanteStatus(vacanteId, isActive, token) {
  return sendApiRequest(`${apiBaseUrl}/api/admin/vacantes/${vacanteId}/status`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ isActive }),
  });
}
