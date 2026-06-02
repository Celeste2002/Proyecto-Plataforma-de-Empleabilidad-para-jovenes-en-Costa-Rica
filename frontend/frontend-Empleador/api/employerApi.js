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
  const responseBody = await response.json();
  if (!response.ok) {
    const apiError = new Error(responseBody.message ?? 'No se pudo completar la solicitud.');
    apiError.validationErrors = responseBody.errors ?? [];
    throw apiError;
  }
  return responseBody;
}

export async function getVisibleCandidateProfiles(token) {
  const headers = token ? { Authorization: `Bearer ${token}` } : {};
  return sendApiRequest(`${apiBaseUrl}/api/employers/candidates`, { headers });
}
