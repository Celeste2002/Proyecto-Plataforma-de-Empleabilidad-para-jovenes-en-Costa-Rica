const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

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
  const response = await fetch(`${apiBaseUrl}/api/candidates/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(candidateRegistrationRequest),
  });
  return readApiResponse(response);
}

export async function getMyCandidateProfile(token) {
  const response = await fetch(`${apiBaseUrl}/api/candidates/me`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  return readApiResponse(response);
}

export async function updateMyCandidateProfile(token, candidateProfileRequest) {
  const response = await fetch(`${apiBaseUrl}/api/candidates/me`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(candidateProfileRequest),
  });
  return readApiResponse(response);
}

export async function updateMyCandidatePassword(token, passwordRequest) {
  const response = await fetch(`${apiBaseUrl}/api/candidates/me/password`, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(passwordRequest),
  });
  return readApiResponse(response);
}
