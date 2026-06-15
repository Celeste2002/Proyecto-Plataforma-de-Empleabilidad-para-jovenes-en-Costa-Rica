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
  const responseText = await response.text();
  const responseBody = responseText ? JSON.parse(responseText) : {};

  if (!response.ok) {
    const apiError = new Error(responseBody.message ?? 'No se pudo completar la solicitud.');
    apiError.validationErrors = responseBody.errors ?? [];
    throw apiError;
  }
  return responseBody;
}

export async function registerCandidate(candidateRegistrationRequest) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(candidateRegistrationRequest),
  });
}

export async function getMyCandidateProfile(token) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function updateMyCandidateProfile(token, candidateProfileRequest) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/me`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(candidateProfileRequest),
  });
}

export async function updateMyCandidatePassword(token, passwordRequest) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/me/password`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(passwordRequest),
  });
}

export async function getVacantes(token) {
  return sendApiRequest(`${apiBaseUrl}/api/vacantes`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function applyToVacante(token, vacanteId) {
  return sendApiRequest(`${apiBaseUrl}/api/postulaciones`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ vacanteId }),
  });
}

export async function getMyPostulaciones(token) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/me/postulaciones`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function getMisMensajes(token) {
  return sendApiRequest(`${apiBaseUrl}/api/candidates/me/mensajes`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}
