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
    if (response.status === 401) {
      window.dispatchEvent(new Event('auth:unauthorized'));
    }
    const apiError = new Error(responseBody.message ?? 'No se pudo completar la solicitud.');
    apiError.status = response.status;
    apiError.validationErrors = responseBody.errors ?? [];
    throw apiError;
  }
  return responseBody;
}

export async function getVisibleCandidateProfiles(token) {
  const headers = token ? { Authorization: `Bearer ${token}` } : {};
  return sendApiRequest(`${apiBaseUrl}/api/employers/candidates`, { headers });
}

export async function searchCandidates(token, filters) {
  const params = new URLSearchParams();
  if (filters.skillKeyword) params.set('skillKeyword', filters.skillKeyword);
  if (filters.province) params.set('province', filters.province);
  if (filters.educationLevel) params.set('educationLevel', filters.educationLevel);
  if (filters.minExperienceYears) params.set('minExperienceYears', filters.minExperienceYears);
  if (filters.isAvailableForContact !== '' && filters.isAvailableForContact !== undefined) {
    params.set('isAvailableForContact', filters.isAvailableForContact);
  }

  return sendApiRequest(`${apiBaseUrl}/api/employers/candidates/search?${params.toString()}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function getCandidateFullProfile(token, candidateProfileId) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/candidates/${candidateProfileId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function getCandidateAppliedVacanteIds(token, candidateProfileId) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/candidates/${candidateProfileId}/postulaciones-vacantes`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function sendSugerenciaPostulacion(token, data) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/sugerencias-postulacion`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      candidateProfileId: data.candidateProfileId,
      vacanteId: data.vacanteId,
      message: data.message || null,
    }),
  });
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

export async function updateVacanteStatus(token, vacanteId, isActive) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes/${vacanteId}/estado`, {
    method: 'PATCH',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ isActive }),
  });
}

export async function updateMyVacanteStatus(token, vacanteId, isActive) {
  return updateVacanteStatus(token, vacanteId, isActive);
}

export async function updateVacante(token, id, data) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes/${id}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({
      description: data.description || null,
      requirements: data.requirements || null,
      salaryRange: data.salaryRange || null,
    }),
  });
}

export async function getVacantePostulaciones(token, vacanteId) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes/${vacanteId}/postulaciones`, {
    headers: { Authorization: `Bearer ${token}` },
  });
}

export const getPostulacionesByVacante = getVacantePostulaciones;

export async function requestInterview(token, postulacionId) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/postulaciones/${postulacionId}/solicitar-entrevista`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function declinePostulacion(token, postulacionId) {
  return sendApiRequest(`${apiBaseUrl}/api/employers/me/postulaciones/${postulacionId}/declinar`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
  });
}

export async function getPostulacionDetail(token, postulacionId) {
  return sendApiRequest(
    `${apiBaseUrl}/api/employers/me/postulaciones/${postulacionId}`,
    { headers: { Authorization: `Bearer ${token}` } },
  );
}

export async function updatePostulacionStatus(token, postulacionId, newStatus) {
  return sendApiRequest(
    `${apiBaseUrl}/api/employers/me/postulaciones/${postulacionId}/status`,
    {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ status: newStatus }),
    },
  );
}

export async function getNotificaciones(token, vacanteId) {
  const query = vacanteId ? `?vacanteId=${vacanteId}` : '';
  return sendApiRequest(
    `${apiBaseUrl}/api/employers/me/notificaciones${query}`,
    { headers: { Authorization: `Bearer ${token}` } },
  );
}

export async function markNotificacionRead(token, notificacionId) {
  return sendApiRequest(
    `${apiBaseUrl}/api/employers/me/notificaciones/${notificacionId}/read`,
    {
      method: 'PUT',
      headers: { Authorization: `Bearer ${token}` },
    },
  );
}

export async function getUnreadNotificacionCount(token) {
  return sendApiRequest(
    `${apiBaseUrl}/api/employers/me/notificaciones/unread-count`,
    { headers: { Authorization: `Bearer ${token}` } },
  );
}

export async function getMisCandidatos(token) {
  const headers = { Authorization: `Bearer ${token}` };
  const vacantes = await sendApiRequest(`${apiBaseUrl}/api/employers/me/vacantes`, { headers });
  return Promise.all(
    vacantes.map(async (vacante) => {
      const postulaciones = await sendApiRequest(
        `${apiBaseUrl}/api/employers/me/vacantes/${vacante.id}/postulaciones`,
        { headers },
      );
      const postulantes = await Promise.all(
        postulaciones.map(async (p) => {
          const detail = await sendApiRequest(
            `${apiBaseUrl}/api/employers/me/postulaciones/${p.id}`,
            { headers },
          );
          return {
            postulacionId: detail.id,
            candidateFullName: detail.candidateFullName,
            candidateEducationLevel: detail.candidateEducationLevel,
            candidateProvince: detail.candidateProvince,
            candidateAge: detail.candidateAge,
            candidateEmail: detail.candidateEmail,
            appliedAt: detail.appliedAt,
            status: detail.status,
          };
        }),
      );
      return { vacanteId: vacante.id, jobTitle: vacante.jobTitle, postulantes };
    }),
  );
}
