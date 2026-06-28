import { useEffect, useState } from 'react';
import {
  ArrowLeft,
  CalendarDays,
  CheckCircle2,
  GraduationCap,
  Mail,
  MailPlus,
  MapPin,
  UserRound,
} from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import {
  getPostulacionDetail,
  requestInterview,
  updatePostulacionStatus,
} from '../api/employerApi.js';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const STATUS_CONFIG = {
  Enviada: { className: 'postulacion-status--enviada', label: 'Enviada' },
  Vista: { className: 'postulacion-status--vista', label: 'Vista' },
  'En revision': { className: 'postulacion-status--revision', label: 'En revision' },
  'En revisión': { className: 'postulacion-status--revision', label: 'En revision' },
  'Entrevista solicitada': { className: 'postulacion-status--entrevista', label: 'Entrevista solicitada' },
  Entrevista: { className: 'postulacion-status--entrevista', label: 'Entrevista programada' },
  'Entrevista programada': { className: 'postulacion-status--entrevista', label: 'Entrevista programada' },
  Descartado: { className: 'postulacion-status--finalizada', label: 'Descartado' },
  Finalizada: { className: 'postulacion-status--finalizada', label: 'Finalizada' },
};

const EMPLOYER_STATUSES = [
  { value: 'En revisión', label: 'En revision' },
  { value: 'Entrevista', label: 'Entrevista programada' },
  { value: 'Finalizada', label: 'Finalizada' },
];

const INTERVIEW_LOCKED_STATUSES = new Set([
  'Entrevista solicitada',
  'Entrevista',
  'Entrevista programada',
  'Descartado',
  'Finalizada',
]);

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

function formatDateOnly(dateOnlyString) {
  const [year, month, day] = dateOnlyString.split('-');
  return new Date(Number(year), Number(month) - 1, Number(day)).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export function PostulacionDetailPage() {
  const { postulacionId } = useParams();
  const { token } = useAuth();

  const [detail, setDetail] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isRequestingInterview, setIsRequestingInterview] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState('');

  async function loadDetail() {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const data = await getPostulacionDetail(token, postulacionId);
      setDetail(data);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadDetail();
  }, [postulacionId]);

  async function handleUpdateStatus(newStatus) {
    setIsUpdating(true);
    setErrorMessage('');
    setSuccessMessage('');

    try {
      await updatePostulacionStatus(token, postulacionId, newStatus);
      setDetail((prev) => ({ ...prev, status: newStatus, updatedAtUtc: new Date().toISOString() }));
      setSuccessMessage(`Estado actualizado a "${newStatus}" correctamente.`);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsUpdating(false);
    }
  }

  async function handleRequestInterview() {
    setIsRequestingInterview(true);
    setErrorMessage('');
    setSuccessMessage('');

    try {
      const updatedPostulacion = await requestInterview(token, postulacionId);
      setDetail((prev) => ({
        ...prev,
        status: updatedPostulacion.status,
        updatedAtUtc: updatedPostulacion.updatedAtUtc,
      }));
      setSuccessMessage('Solicitud de entrevista enviada por correo al candidato.');
    } catch (error) {
      setErrorMessage(error.validationErrors?.[0] ?? error.message);
    } finally {
      setIsRequestingInterview(false);
    }
  }

  const interviewAlreadyRequested = detail?.status === 'Entrevista solicitada';
  const interviewRequestDisabled =
    !detail ||
    isRequestingInterview ||
    isUpdating ||
    INTERVIEW_LOCKED_STATUSES.has(detail.status);

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink to="/empleador" />
        <nav className="dashboard-nav" aria-label="Navegacion del empleador">
          {detail && (
            <Link
              className="secondary-action"
              to={`/empleador/vacantes/${detail.vacanteId}/postulaciones`}
            >
              <ArrowLeft aria-hidden="true" size={16} />
              Postulaciones de la vacante
            </Link>
          )}
          {!detail && (
            <Link className="secondary-action" to="/empleador/vacantes">
              <ArrowLeft aria-hidden="true" size={16} />
              Mis vacantes
            </Link>
          )}
        </nav>
      </header>

      <section className="employer-view">
        {isLoading && (
          <p className="empty-state">Cargando informacion del candidato...</p>
        )}

        <StatusMessage message={errorMessage} tone="error" />
        <StatusMessage message={successMessage} tone="success" />

        {!isLoading && detail && (
          <>
            <div className="section-heading">
              <div>
                <p className="eyebrow">Perfil del candidato</p>
                <h2>{detail.candidateFullName}</h2>
                {STATUS_CONFIG[detail.status] && (
                  <span className={`postulacion-status ${STATUS_CONFIG[detail.status].className}`}>
                    {STATUS_CONFIG[detail.status].label}
                  </span>
                )}
              </div>
            </div>

            <div className="status-actions">
              <button
                className="secondary-action"
                disabled={interviewRequestDisabled}
                onClick={handleRequestInterview}
                type="button"
              >
                <MailPlus aria-hidden="true" size={15} />
                {isRequestingInterview
                  ? 'Enviando...'
                  : interviewAlreadyRequested
                    ? 'Entrevista solicitada'
                    : 'Solicitar entrevista por correo'}
              </button>
            </div>

            <div className="status-actions">
              <span className="status-actions__label">Gestionar estado:</span>
              {EMPLOYER_STATUSES.map(({ value, label }) => (
                <button
                  key={value}
                  className={`secondary-action${detail.status === value ? ' status-btn--active' : ''}`}
                  disabled={isUpdating || detail.status === value}
                  onClick={() => handleUpdateStatus(value)}
                  type="button"
                >
                  {detail.status === value && <CheckCircle2 aria-hidden="true" size={14} />}
                  {label}
                </button>
              ))}
            </div>

            <div className="candidate-profile-card">
              <div className="candidate-profile-card__avatar">
                <UserRound aria-hidden="true" size={40} />
              </div>

              <dl className="candidate-profile-card__data">
                <div>
                  <dt>
                    <Mail aria-hidden="true" size={15} />
                    Correo electronico
                  </dt>
                  <dd><a href={`mailto:${detail.candidateEmail}`}>{detail.candidateEmail}</a></dd>
                </div>
                <div>
                  <dt>
                    <MapPin aria-hidden="true" size={15} />
                    Provincia
                  </dt>
                  <dd>{detail.candidateProvince}</dd>
                </div>
                <div>
                  <dt>
                    <GraduationCap aria-hidden="true" size={15} />
                    Nivel educativo
                  </dt>
                  <dd>{detail.candidateEducationLevel}</dd>
                </div>
                <div>
                  <dt>
                    <CalendarDays aria-hidden="true" size={15} />
                    Fecha de nacimiento
                  </dt>
                  <dd>
                    {formatDateOnly(detail.candidateDateOfBirth)}{' '}
                    <span className="candidate-age">({detail.candidateAge} anos)</span>
                  </dd>
                </div>
              </dl>
            </div>

            <div className="postulacion-meta">
              <p className="eyebrow">Informacion de la postulacion</p>
              <dl className="postulacion-card__details">
                <div>
                  <dt>Vacante</dt>
                  <dd>{detail.jobTitle}</dd>
                </div>
                <div>
                  <dt>Fecha de postulacion</dt>
                  <dd>{formatDate(detail.appliedAt)}</dd>
                </div>
                <div>
                  <dt>Ultima actualizacion</dt>
                  <dd>{formatDate(detail.updatedAtUtc)}</dd>
                </div>
              </dl>
            </div>
          </>
        )}
      </section>
    </main>
  );
}
