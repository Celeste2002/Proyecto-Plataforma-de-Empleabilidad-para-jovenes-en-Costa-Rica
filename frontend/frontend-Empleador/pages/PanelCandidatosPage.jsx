import {
  ArrowLeft,
  ChevronDown,
  MailPlus,
  RefreshCw,
  UserRoundCheck,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMisCandidatos, requestInterview, updatePostulacionStatus } from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const POSTULACION_STATUSES = [
  { value: 'En revisión', label: 'En revision' },
  { value: 'Entrevista', label: 'Entrevista programada' },
  { value: 'Finalizada', label: 'Finalizada' },
];

const INTERVIEW_REQUESTED_STATUS = 'Entrevista solicitada';
const INTERVIEW_CONFIRMED_STATUSES = new Set(['Entrevista', 'Entrevista programada']);
const INTERVIEW_LOCKED_STATUSES = new Set([
  INTERVIEW_REQUESTED_STATUS,
  ...INTERVIEW_CONFIRMED_STATUSES,
  'Descartado',
  'Finalizada',
]);

const FILTERS = [
  { value: 'all', label: 'Todas las personas' },
  { value: 'request-not-sent', label: 'Solicitud no enviada' },
  { value: 'request-pending', label: 'Solicitud pendiente de confirmar' },
  { value: 'request-confirmed', label: 'Solicitud confirmada' },
];

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function matchesFilter(postulacion, activeFilter) {
  if (activeFilter === 'request-not-sent') {
    return postulacion.status !== INTERVIEW_REQUESTED_STATUS &&
      !INTERVIEW_CONFIRMED_STATUSES.has(postulacion.status);
  }
  if (activeFilter === 'request-pending') return postulacion.status === INTERVIEW_REQUESTED_STATUS;
  if (activeFilter === 'request-confirmed') return INTERVIEW_CONFIRMED_STATUSES.has(postulacion.status);
  return true;
}

export function PanelCandidatosPage() {
  const { token } = useAuth();

  const [vacantesConCandidatos, setVacantesConCandidatos] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [expandedVacantes, setExpandedVacantes] = useState({});
  const [activeFilter, setActiveFilter] = useState('all');
  const [updatingStatusId, setUpdatingStatusId] = useState(null);
  const [requestingInterviewId, setRequestingInterviewId] = useState(null);
  const [statusResults, setStatusResults] = useState({});

  const allPostulantes = useMemo(
    () => vacantesConCandidatos.flatMap((vacante) => vacante.postulantes),
    [vacantesConCandidatos],
  );

  const filteredTotal = useMemo(
    () => allPostulantes.filter((postulacion) => matchesFilter(postulacion, activeFilter)).length,
    [activeFilter, allPostulantes],
  );

  const filterCounts = useMemo(() => {
    const counts = {};
    FILTERS.forEach((filter) => {
      counts[filter.value] = allPostulantes.filter((postulacion) =>
        matchesFilter(postulacion, filter.value)).length;
    });
    return counts;
  }, [allPostulantes]);

  const loadCandidatos = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const data = await getMisCandidatos(token);
      setVacantesConCandidatos(data);

      const initialExpanded = {};
      data.forEach((vacante) => {
        if (vacante.postulantes?.length > 0) {
          initialExpanded[vacante.vacanteId] = true;
        }
      });
      setExpandedVacantes(initialExpanded);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }, [token]);

  useEffect(() => {
    loadCandidatos();
  }, [loadCandidatos]);

  function toggleVacante(vacanteId) {
    setExpandedVacantes((prev) => ({ ...prev, [vacanteId]: !prev[vacanteId] }));
  }

  function updatePostulante(postulacionId, updater) {
    setVacantesConCandidatos((prev) =>
      prev.map((vacante) => ({
        ...vacante,
        postulantes: vacante.postulantes.map((postulacion) =>
          postulacion.postulacionId === postulacionId ? updater(postulacion) : postulacion,
        ),
      })),
    );
  }

  async function handleStatusChange(postulacionId, newStatus) {
    setUpdatingStatusId(postulacionId);
    setStatusResults((prev) => ({ ...prev, [postulacionId]: null }));

    try {
      await updatePostulacionStatus(token, postulacionId, newStatus);
      updatePostulante(postulacionId, (postulacion) => ({ ...postulacion, status: newStatus }));

      setStatusResults((prev) => ({
        ...prev,
        [postulacionId]: { success: true, message: 'Estado actualizado.' },
      }));
    } catch (error) {
      setStatusResults((prev) => ({
        ...prev,
        [postulacionId]: { success: false, message: error.message },
      }));
    } finally {
      setUpdatingStatusId(null);
    }
  }

  async function handleRequestInterview(postulacionId) {
    setRequestingInterviewId(postulacionId);
    setStatusResults((prev) => ({ ...prev, [postulacionId]: null }));

    try {
      const updatedPostulacion = await requestInterview(token, postulacionId);
      updatePostulante(updatedPostulacion.id, (postulacion) => ({
        ...postulacion,
        status: updatedPostulacion.status,
      }));

      setStatusResults((prev) => ({
        ...prev,
        [updatedPostulacion.id]: {
          success: true,
          message: 'Solicitud enviada por correo.',
        },
      }));
    } catch (error) {
      setStatusResults((prev) => ({
        ...prev,
        [postulacionId]: {
          success: false,
          message: error.validationErrors?.[0] ?? error.message,
        },
      }));
    } finally {
      setRequestingInterviewId(null);
    }
  }

  return (
    <main className="application-shell">
      <header className="top-bar">
        <div className="brand-lockup">
          <img
            alt="Sinergia"
            className="brand-logo"
            onError={(event) => { event.currentTarget.style.display = 'none'; }}
            src="/Logo_Sinergia.png"
          />
          <div>
            <h1>Sinergia</h1>
          </div>
        </div>
        <nav className="dashboard-nav" aria-label="Navegacion del empleador">
          <Link className="secondary-action" to="/empleador">
            <ArrowLeft aria-hidden="true" size={16} />
            Panel principal
          </Link>
        </nav>
      </header>

      <section className="employer-view">
        <div className="section-heading horizontal-heading">
          <div>
            <p className="eyebrow">Gestion de candidatos</p>
            <h2>Panel de candidatos</h2>
            <p className="section-description">
              {filteredTotal} de {allPostulantes.length} personas visibles segun el filtro actual.
            </p>
          </div>
          <button className="secondary-action" onClick={loadCandidatos} type="button">
            <RefreshCw aria-hidden="true" size={18} />
            Actualizar
          </button>
        </div>

        <div className="candidate-filter-bar" aria-label="Filtros de candidatos">
          {FILTERS.map((filter) => (
            <button
              className={`candidate-filter-chip${activeFilter === filter.value ? ' active' : ''}`}
              key={filter.value}
              onClick={() => setActiveFilter(filter.value)}
              type="button"
            >
              <span>{filter.label}</span>
              <strong>{filterCounts[filter.value] ?? 0}</strong>
            </button>
          ))}
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando candidatos...</p>
        ) : vacantesConCandidatos.length === 0 ? (
          <p className="empty-state">Aun no tienes vacantes publicadas.</p>
        ) : filteredTotal === 0 ? (
          <p className="empty-state">No hay personas que coincidan con este filtro.</p>
        ) : (
          <div className="candidatos-panel">
            {vacantesConCandidatos.map((vacante) => {
              const filteredPostulantes = vacante.postulantes.filter((postulacion) =>
                matchesFilter(postulacion, activeFilter));

              if (filteredPostulantes.length === 0) return null;

              return (
                <article key={vacante.vacanteId} className="candidatos-vacante-group">
                  <button
                    aria-expanded={!!expandedVacantes[vacante.vacanteId]}
                    className="candidatos-vacante-group__toggle"
                    onClick={() => toggleVacante(vacante.vacanteId)}
                    type="button"
                  >
                    <div className="candidatos-vacante-group__info">
                      <span className="candidatos-vacante-group__title">{vacante.jobTitle}</span>
                      <span className="candidatos-vacante-group__count">
                        {filteredPostulantes.length}{' '}
                        {filteredPostulantes.length === 1 ? 'persona' : 'personas'}
                      </span>
                    </div>
                    <ChevronDown
                      aria-hidden="true"
                      className={`candidatos-vacante-group__chevron${expandedVacantes[vacante.vacanteId] ? ' candidatos-vacante-group__chevron--open' : ''}`}
                      size={20}
                    />
                  </button>

                  {expandedVacantes[vacante.vacanteId] && (
                    <div className="candidatos-list">
                      {filteredPostulantes.map((postulacion) => {
                        const interviewAlreadyRequested =
                          postulacion.status === INTERVIEW_REQUESTED_STATUS;
                        const interviewRequestDisabled =
                          requestingInterviewId === postulacion.postulacionId ||
                          updatingStatusId === postulacion.postulacionId ||
                          INTERVIEW_LOCKED_STATUSES.has(postulacion.status);

                        return (
                          <article key={postulacion.postulacionId} className="candidato-row">
                            <div className="candidato-row__identity">
                              <div className="candidato-row__avatar" aria-hidden="true">
                                <UserRoundCheck size={20} />
                              </div>
                              <div>
                                <p className="candidato-row__name">{postulacion.candidateFullName}</p>
                                <p className="candidato-row__edu">{postulacion.candidateEducationLevel}</p>
                              </div>
                            </div>

                            <dl className="candidato-row__details">
                              <div>
                                <dt>Provincia</dt>
                                <dd>{postulacion.candidateProvince}</dd>
                              </div>
                              <div>
                                <dt>Edad</dt>
                                <dd>{postulacion.candidateAge}</dd>
                              </div>
                              <div>
                                <dt>Correo</dt>
                                <dd><a href={`mailto:${postulacion.candidateEmail}`}>{postulacion.candidateEmail}</a></dd>
                              </div>
                              <div>
                                <dt>Postulado</dt>
                                <dd>{formatDate(postulacion.appliedAt)}</dd>
                              </div>
                            </dl>

                            <div className="candidato-row__status-control">
                              <label className="candidato-row__status-label">
                                Estado
                                <select
                                  className="candidato-row__status-select"
                                  disabled={updatingStatusId === postulacion.postulacionId}
                                  onChange={(event) =>
                                    handleStatusChange(postulacion.postulacionId, event.target.value)
                                  }
                                  value={postulacion.status}
                                >
                                  {!POSTULACION_STATUSES.some(
                                    (status) => status.value === postulacion.status,
                                  ) && (
                                    <option value={postulacion.status} disabled>
                                      {postulacion.status}
                                    </option>
                                  )}
                                  {POSTULACION_STATUSES.map((status) => (
                                    <option key={status.value} value={status.value}>
                                      {status.label}
                                    </option>
                                  ))}
                                </select>
                              </label>
                              {statusResults[postulacion.postulacionId] && (
                                <span
                                  className={`candidato-row__status-feedback candidato-row__status-feedback--${statusResults[postulacion.postulacionId].success ? 'ok' : 'err'}`}
                                >
                                  {statusResults[postulacion.postulacionId].message}
                                </span>
                              )}
                            </div>

                            <div className="candidato-row__actions">
                              <button
                                className="secondary-action"
                                disabled={interviewRequestDisabled}
                                onClick={() => handleRequestInterview(postulacion.postulacionId)}
                                type="button"
                              >
                                <MailPlus aria-hidden="true" size={15} />
                                {requestingInterviewId === postulacion.postulacionId
                                  ? 'Enviando...'
                                  : interviewAlreadyRequested
                                    ? 'Entrevista solicitada'
                                    : 'Solicitar entrevista'}
                              </button>
                            </div>
                          </article>
                        );
                      })}
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
