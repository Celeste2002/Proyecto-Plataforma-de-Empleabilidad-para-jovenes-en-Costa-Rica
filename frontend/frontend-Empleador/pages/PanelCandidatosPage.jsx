import {
  ArrowLeft,
  ChevronDown,
  MessageSquare,
  RefreshCw,
  UserRoundCheck,
} from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getMisCandidatos, updatePostulacionStatus } from '../api/employerApi.js';
import { EnviarMensajeModal } from '../components/EnviarMensajeModal.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const POSTULACION_STATUSES = [
  { value: 'En revisión', label: 'En revisión' },
  { value: 'Entrevista programada', label: 'Entrevista programada' },
  { value: 'Descartado', label: 'Descartado' },
];

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function PanelCandidatosPage() {
  const { token } = useAuth();

  const [vacantesConCandidatos, setVacantesConCandidatos] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [expandedVacantes, setExpandedVacantes] = useState({});
  const [updatingStatusId, setUpdatingStatusId] = useState(null);
  const [statusResults, setStatusResults] = useState({});
  const [mensajeModal, setMensajeModal] = useState(null);

  const loadCandidatos = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const data = await getMisCandidatos(token);
      setVacantesConCandidatos(data);

      // Expandir por defecto las vacantes con postulantes
      const initialExpanded = {};
      data.forEach((v) => {
        if (v.postulantes && v.postulantes.length > 0) {
          initialExpanded[v.vacanteId] = true;
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

  async function handleStatusChange(postulacionId, newStatus) {
    setUpdatingStatusId(postulacionId);
    setStatusResults((prev) => ({ ...prev, [postulacionId]: null }));

    try {
      await updatePostulacionStatus(token, postulacionId, newStatus);

      setVacantesConCandidatos((prev) =>
        prev.map((v) => ({
          ...v,
          postulantes: v.postulantes.map((p) =>
            p.postulacionId === postulacionId ? { ...p, status: newStatus } : p,
          ),
        })),
      );

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

  function openMensajeModal(postulacion) {
    setMensajeModal({
      postulacionId: postulacion.postulacionId,
      candidateFullName: postulacion.candidateFullName,
    });
  }

  return (
    <main className="application-shell">
      <header className="top-bar">
        <div className="brand-lockup">
          <img
            alt="Sinergia"
            className="brand-logo"
            onError={(e) => { e.currentTarget.style.display = 'none'; }}
            src="/Logo_Sinergia.png"
          />
          <div>
            <h1>Sinergia</h1>
          </div>
        </div>
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
            <p className="eyebrow">Gestión de candidatos</p>
            <h2>Panel de candidatos</h2>
          </div>
          <button className="secondary-action" onClick={loadCandidatos} type="button">
            <RefreshCw aria-hidden="true" size={18} />
            Actualizar
          </button>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando candidatos...</p>
        ) : vacantesConCandidatos.length === 0 ? (
          <p className="empty-state">Aún no tienes vacantes publicadas.</p>
        ) : (
          <div className="candidatos-panel">
            {vacantesConCandidatos.map((vacante) => (
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
                      {vacante.postulantes.length}{' '}
                      {vacante.postulantes.length === 1 ? 'candidato' : 'candidatos'}
                    </span>
                  </div>
                  <ChevronDown
                    aria-hidden="true"
                    className={`candidatos-vacante-group__chevron${expandedVacantes[vacante.vacanteId] ? ' candidatos-vacante-group__chevron--open' : ''}`}
                    size={20}
                  />
                </button>

                {expandedVacantes[vacante.vacanteId] && (
                  vacante.postulantes.length === 0 ? (
                    <p className="candidatos-vacante-group__empty">
                      Sin postulaciones todavía.
                    </p>
                  ) : (
                    <div className="candidatos-list">
                      {vacante.postulantes.map((postulacion) => (
                        <div key={postulacion.postulacionId} className="candidato-row">
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
                                onChange={(e) =>
                                  handleStatusChange(postulacion.postulacionId, e.target.value)
                                }
                                value={postulacion.status}
                              >
                                {/* Mostrar el estado actual si no está en la lista settable */}
                                {!POSTULACION_STATUSES.some(
                                  (s) => s.value === postulacion.status,
                                ) && (
                                  <option value={postulacion.status} disabled>
                                    {postulacion.status}
                                  </option>
                                )}
                                {POSTULACION_STATUSES.map((s) => (
                                  <option key={s.value} value={s.value}>
                                    {s.label}
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

                          <button
                            className="secondary-action candidato-row__msg-btn"
                            onClick={() => openMensajeModal(postulacion)}
                            type="button"
                          >
                            <MessageSquare aria-hidden="true" size={15} />
                            Enviar mensaje
                          </button>
                        </div>
                      ))}
                    </div>
                  )
                )}
              </article>
            ))}
          </div>
        )}
      </section>

      {mensajeModal && (
        <EnviarMensajeModal
          candidateName={mensajeModal.candidateFullName}
          onClose={() => setMensajeModal(null)}
          postulacionId={mensajeModal.postulacionId}
          token={token}
        />
      )}
    </main>
  );
}
