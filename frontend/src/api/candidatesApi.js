const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000';

export async function registerCandidate(candidateRegistrationRequest) {
  const response = await fetch(`${apiBaseUrl}/api/candidates/register`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(candidateRegistrationRequest),
  });

  return readApiResponse(response);
}

export async function getVisibleCandidateProfiles() {
  const response = await fetch(`${apiBaseUrl}/api/employers/candidates`);

  return readApiResponse(response);
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
