import { useCallback, useEffect, useState } from 'react';
import { ArrowLeft, Search, Send, UserRoundCheck } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import {
  getCandidateAppliedVacanteIds,
  getCandidateFullProfile,
  getMyVacantes,
  searchCandidates,
  sendSugerenciaPostulacion,
} from '../api/employerApi.js';
import { costaRicaProvinces, educationLevels } from '../constants/vacanteCatalogs.js';

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function CandidatosDisponiblesPage() {
  const { token } = useAuth();

  const [filters, setFilters] = useState({
    skillKeyword: '',
    province: '',
    educationLevel: '',
    minExperienceYears: '',
    isAvailableForContact: '',
  });

  const [candidates, setCandidates] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  const [expandedCandidateId, setExpandedCandidateId] = useState(null);
  const [profilesById, setProfilesById] = useState({});
  const [appliedVacanteIdsById, setAppliedVacanteIdsById] = useState({});
  const [profileErrorById, setProfileErrorById] = useState({});
  const [loadingProfileId, setLoadingProfileId] = useState(null);

  const [myVacantes, setMyVacantes] = useState([]);
  const [sugerenciaForm, setSugerenciaForm] = useState({});
  const [sendingId, setSendingId] = useState(null);
  const [sugerenciaResults, setSugerenciaResults] = useState({});

  const runSearch = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setLoading(true);
    }
    setErrorMsg('');

    try {
      const results = await searchCandidates(token, filters);
      setCandidates(results);
    } catch (err) {
      setErrorMsg(err.message);
    } finally {
      if (showLoading) {
        setLoading(false);
      }
    }
  }, [token, filters]);

  useEffect(() => {
    runSearch(true);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    getMyVacantes(token).then(setMyVacantes).catch(() => {});
  }, [token]);

  function handleFilterChange(e) {
    const { name, value } = e.target;
    setFilters((prev) => ({ ...prev, [name]: value }));
  }

  function handleSearchSubmit(e) {
    e.preventDefault();
    runSearch(true);
  }

  async function loadCandidateDetail(candidateId) {
    setLoadingProfileId(candidateId);
    setProfileErrorById((prev) => ({ ...prev, [candidateId]: '' }));
    try {
      const [profile, appliedVacanteIds] = await Promise.all([
        getCandidateFullProfile(token, candidateId),
        getCandidateAppliedVacanteIds(token, candidateId),
      ]);
      setProfilesById((prev) => ({ ...prev, [candidateId]: profile }));
      setAppliedVacanteIdsById((prev) => ({ ...prev, [candidateId]: appliedVacanteIds }));
    } catch (err) {
      setProfileErrorById((prev) => ({ ...prev, [candidateId]: err.message }));
    } finally {
      setLoadingProfileId(null);
    }
  }

  async function toggleCandidate(candidateId) {
    if (expandedCandidateId === candidateId) {
      setExpandedCandidateId(null);
      return;
    }

    setExpandedCandidateId(candidateId);

    if (profilesById[candidateId]) {
      return;
    }

    await loadCandidateDetail(candidateId);
  }

  function handleSugerenciaFieldChange(candidateId, field, value) {
    setSugerenciaForm((prev) => ({
      ...prev,
      [candidateId]: { ...prev[candidateId], [field]: value },
    }));
  }

  async function handleSendSugerencia(candidateId) {
    const form = sugerenciaForm[candidateId] ?? {};

    if (!form.vacanteId) {
      setSugerenciaResults((prev) => ({
        ...prev,
        [candidateId]: { success: false, message: 'Selecciona una vacante.' },
      }));
      return;
    }

    setSendingId(candidateId);
    try {
      await sendSugerenciaPostulacion(token, {
        candidateProfileId: candidateId,
        vacanteId: form.vacanteId,
        message: form.message || null,
      });
      setSugerenciaResults((prev) => ({
        ...prev,
        [candidateId]: { success: true, message: 'Sugerencia enviada correctamente.' },
      }));
      setExpandedCandidateId(null);
    } catch (err) {
      setSugerenciaResults((prev) => ({
        ...prev,
        [candidateId]: { success: false, message: err.validationErrors?.[0] ?? err.message },
      }));
    } finally {
      setSendingId(null);
    }
  }

  const activeVacantes = myVacantes.filter((v) => v.isActive);

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink to="/empleador" />
        <nav className="dashboard-nav" aria-label="Navegación del empleador">
          <Link className="secondary-action" to="/empleador">
            <ArrowLeft aria-hidden="true" size={16} />
            Panel principal
          </Link>
        </nav>
      </header>

      <section className="employer-view">
        <div className="section-heading horizontal-heading">
          <div>
            <p className="eyebrow">Empleadores aliados</p>
            <h2>Candidatos disponibles</h2>
          </div>
        </div>

        <form className="candidate-search-filters" onSubmit={handleSearchSubmit}>
          <div className="candidate-search-filters__grid">
            <label>
              Habilidad
              <input
                name="skillKeyword"
                onChange={handleFilterChange}
                placeholder="Ej. Excel, atención al cliente..."
                type="text"
                value={filters.skillKeyword}
              />
            </label>
            <label>
              Provincia
              <select name="province" onChange={handleFilterChange} value={filters.province}>
                <option value="">Todas las provincias</option>
                {costaRicaProvinces.map((p) => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </label>
            <label>
              Nivel educativo
              <select name="educationLevel" onChange={handleFilterChange} value={filters.educationLevel}>
                <option value="">Todos los niveles</option>
                {educationLevels.map((e) => (
                  <option key={e} value={e}>{e}</option>
                ))}
              </select>
            </label>
            <label>
              Experiencia mínima (años)
              <input
                min="0"
                name="minExperienceYears"
                onChange={handleFilterChange}
                step="0.5"
                type="number"
                value={filters.minExperienceYears}
              />
            </label>
            <label>
              Disponibilidad
              <select name="isAvailableForContact" onChange={handleFilterChange} value={filters.isAvailableForContact}>
                <option value="">Todos</option>
                <option value="true">Disponible para contacto</option>
                <option value="false">No disponible</option>
              </select>
            </label>
          </div>
          <button className="primary-action" type="submit">
            <Search aria-hidden="true" size={16} />
            Buscar
          </button>
        </form>

        <StatusMessage message={errorMsg} tone="error" />

        {loading && <p className="empty-state">Cargando candidatos...</p>}

        {!loading && !errorMsg && candidates.length === 0 && (
          <p className="empty-state">No hay candidatos que coincidan con los filtros seleccionados.</p>
        )}

        {!loading && !errorMsg && candidates.length > 0 && (
          <div className="candidate-list">
            {candidates.map((candidate) => {
              const profile = profilesById[candidate.id];
              const profileError = profileErrorById[candidate.id];
              const form = sugerenciaForm[candidate.id] ?? {};
              const result = sugerenciaResults[candidate.id];

              return (
                <article className="candidate-card candidate-card--expandable" key={candidate.id}>
                  <button
                    className="candidate-card__toggle"
                    onClick={() => toggleCandidate(candidate.id)}
                    type="button"
                  >
                    <div className="candidate-avatar" aria-hidden="true">
                      <UserRoundCheck size={24} />
                    </div>
                    <div>
                      <h3>{candidate.fullName}</h3>
                      <p>{candidate.educationLevel}</p>
                      {candidate.hasAppliedToYourVacantes && (
                        <p className="candidate-applied-badge">Ya aplicó a alguna(s) de tus vacantes</p>
                      )}
                    </div>
                    <dl>
                      <div>
                        <dt>Edad</dt>
                        <dd>{candidate.age}</dd>
                      </div>
                      <div>
                        <dt>Provincia</dt>
                        <dd>{candidate.province}</dd>
                      </div>
                      <div>
                        <dt>Correo</dt>
                        <dd>{candidate.email}</dd>
                      </div>
                      <div>
                        <dt>Experiencia</dt>
                        <dd>{candidate.experienceYears} años</dd>
                      </div>
                      <div>
                        <dt>Disponibilidad</dt>
                        <dd>{candidate.isAvailableForContact ? 'Disponible' : 'No disponible'}</dd>
                      </div>
                    </dl>
                  </button>

                  {expandedCandidateId === candidate.id && (
                    <div className="candidate-detail-panel">
                      {loadingProfileId === candidate.id && (
                        <p className="empty-state">Cargando ficha...</p>
                      )}

                      {profileError && loadingProfileId !== candidate.id && (
                        <div className="field-error" role="alert">
                          <p>No se pudo cargar la ficha del candidato: {profileError}</p>
                          <button
                            className="secondary-action"
                            onClick={() => loadCandidateDetail(candidate.id)}
                            type="button"
                          >
                            Reintentar
                          </button>
                        </div>
                      )}

                      {profile && (
                        <>
                          <div className="candidate-detail-panel__section">
                            <h4>Habilidades</h4>
                            {profile.habilidades.length === 0 ? (
                              <p className="empty-state-small">Sin habilidades registradas.</p>
                            ) : (
                              <ul className="candidate-skill-tags">
                                {profile.habilidades.map((h) => (
                                  <li key={h.id}>{h.nombre}</li>
                                ))}
                              </ul>
                            )}
                          </div>

                          <div className="candidate-detail-panel__section">
                            <h4>Experiencia laboral</h4>
                            {profile.experiencias.length === 0 ? (
                              <p className="empty-state-small">Sin experiencia registrada.</p>
                            ) : (
                              profile.experiencias.map((e) => (
                                <div className="candidate-experience-item" key={e.id}>
                                  <p><strong>{e.cargo}</strong> — {e.empresa}</p>
                                  <p>
                                    {formatDate(e.fechaInicio)} - {e.esTrabajoActual ? 'Actual' : formatDate(e.fechaFin)}
                                  </p>
                                  {e.descripcion && <p>{e.descripcion}</p>}
                                </div>
                              ))
                            )}
                          </div>

                          <div className="candidate-detail-panel__section">
                            <h4>Cursos completados</h4>
                            {profile.cursos.length === 0 ? (
                              <p className="empty-state-small">Sin cursos registrados.</p>
                            ) : (
                              profile.cursos.map((c) => (
                                <p key={c.id}>
                                  {c.nombreCurso} — {c.institucion} ({formatDate(c.fechaCompletado)})
                                </p>
                              ))
                            )}
                          </div>

                          <div className="candidate-detail-panel__sugerencia">
                            <h4>Enviar sugerencia de postulación</h4>
                            <label>
                              Vacante
                              <select
                                onChange={(e) => handleSugerenciaFieldChange(candidate.id, 'vacanteId', e.target.value)}
                                value={form.vacanteId ?? ''}
                              >
                                <option value="">Selecciona una vacante activa</option>
                                {activeVacantes.map((v) => {
                                  const alreadyAppliedToThisVacante =
                                    (appliedVacanteIdsById[candidate.id] ?? []).includes(v.id);
                                  return (
                                    <option disabled={alreadyAppliedToThisVacante} key={v.id} value={v.id}>
                                      {v.jobTitle}{alreadyAppliedToThisVacante ? ' (ya aplicó)' : ''}
                                    </option>
                                  );
                                })}
                              </select>
                            </label>
                            <label>
                              Mensaje (opcional)
                              <textarea
                                maxLength={500}
                                onChange={(e) => handleSugerenciaFieldChange(candidate.id, 'message', e.target.value)}
                                rows={3}
                                value={form.message ?? ''}
                              />
                            </label>
                            {result && (
                              <p
                                className={`vacante-card__result ${result.success ? 'vacante-card__result--success' : 'vacante-card__result--error'}`}
                                role="status"
                              >
                                {result.message}
                              </p>
                            )}
                            <button
                              className="primary-action"
                              disabled={sendingId === candidate.id || activeVacantes.length === 0 || result?.success === true}
                              onClick={() => handleSendSugerencia(candidate.id)}
                              type="button"
                            >
                              <Send aria-hidden="true" size={15} />
                              {result?.success === true
                                ? 'Sugerencia enviada'
                                : sendingId === candidate.id ? 'Enviando...' : 'Enviar sugerencia de postulación'}
                            </button>
                            {activeVacantes.length === 0 && (
                              <p className="empty-state-small">Necesitas al menos una vacante activa para enviar sugerencias.</p>
                            )}
                          </div>
                        </>
                      )}
                    </div>
                  )}
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
