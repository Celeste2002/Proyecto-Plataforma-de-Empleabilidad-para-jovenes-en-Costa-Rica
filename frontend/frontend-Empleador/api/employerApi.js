const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

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
  const response = await fetch(`${apiBaseUrl}/api/employers/candidates`, { headers });
  return readApiResponse(response);
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
