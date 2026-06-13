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

export async function registerEmployer(data) {
  const response = await fetch(`${apiBaseUrl}/api/employers/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      companyName: data.companyName,
      legalId: data.legalId,
      sector: data.sector,
      contactName: data.contactName,
      contactPhone: data.contactPhone,
      location: data.location,
      email: data.email,
      password: data.password,
    }),
  });
  return readApiResponse(response);
}

export async function getMyEmployerProfile(token) {
  const response = await fetch(`${apiBaseUrl}/api/employers/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return readApiResponse(response);
}

export async function getMyVacantes(token) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function createVacante(token, data) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      jobTitle: data.jobTitle,
      province: data.province,
      sector: data.sector,
      modality: data.modality,
      experienceLevel: data.experienceLevel,
      description: data.description || null,
      requirements: data.requirements || null,
      salaryRange: data.salaryRange || null,
    }),
  });
}
