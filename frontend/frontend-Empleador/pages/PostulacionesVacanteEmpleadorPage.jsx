import { useEffect, useState } from 'react';
import { ArrowLeft, Bell, ClipboardList, MapPin, UserRound } from 'lucide-react';
import { Link, useParams } from 'react-router-dom';
import { getNotificaciones, getPostulacionesByVacante, markNotificacionRead } from '../api/employerApi.js';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const STATUS_CONFIG = {
  Enviada: { className: 'postulacion-status--enviada', label: 'Enviada' },
  Vista: { className: 'postulacion-status--vista', label: 'Vista' },
  Entrevista: { className: 'postulacion-status--entrevista', label: 'Entrevista' },
  'En revisión': { className: 'postulacion-status--revision', label: 'En revisión' },
  'Entrevista solicitada': { className: 'postulacion-status--entrevista', label: 'Entrevista solicitada' },
  Finalizada: { className: 'postulacion-status--finalizada', label: 'Finalizada' },
};

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function formatDateTime(dateString) {
  return new Date(dateString).toLocaleString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export function PostulacionesVacanteEmpleadorPage() {
  const { vacanteId } = useParams();
  const { token } = useAuth();

  const [postulaciones, setPostulaciones] = useState([]);
  const [notificaciones, setNotificaciones] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  async function loadData() {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const posts = await getPostulacionesByVacante(token, vacanteId);
      setPostulaciones(posts);
    } catch (error) {
      setErrorMessage(error.message);
    }

    try {
      const notifs = await getNotificaciones(token, vacanteId);
      setNotificaciones(notifs);
    } catch {
      // notifications failure is non-critical
    }

    setIsLoading(false);
  }

  useEffect(() => {
    loadData();
  }, [vacanteId]);

  async function handleMarkRead(notificacionId) {
    try {
      await markNotificacionRead(token, notificacionId);
      setNotificaciones((prev) =>
        prev.map((n) => (n.id === notificacionId ? { ...n, isRead: true } : n)),
      );
    } catch {
      // silently ignore
    }
  }

  const unreadNotificaciones = notificaciones.filter((n) => !n.isRead);

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink to="/empleador" />
        <nav className="dashboard-nav" aria-label="Navegación del empleador">
          <Link className="secondary-action" to="/empleador/vacantes">
            <ArrowLeft aria-hidden="true" size={16} />
            Mis vacantes
          </Link>
        </nav>
      </header>

      <section className="employer-view">
        <div className="section-heading horizontal-heading">
          <div>
            <p className="eyebrow">Gestión de postulaciones</p>
            <h2>Postulaciones recibidas</h2>
          </div>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        {unreadNotificaciones.length > 0 && (
          <div className="notificaciones-panel" role="region" aria-label="Notificaciones nuevas">
            <div className="notificaciones-panel__header">
              <Bell aria-hidden="true" size={18} />
              <span>
                {unreadNotificaciones.length}{' '}
                {unreadNotificaciones.length === 1 ? 'notificación nueva' : 'notificaciones nuevas'}
              </span>
            </div>
            <ul className="notificaciones-list">
              {unreadNotificaciones.map((n) => (
                <li key={n.id} className="notificacion-item">
                  <span className="notificacion-item__message">
                    <strong>{n.jobTitle}</strong>: {n.message}
                  </span>
                  <span className="notificacion-item__time">{formatDateTime(n.createdAtUtc)}</span>
                  <button
                    className="secondary-action notificacion-item__read-btn"
                    onClick={() => handleMarkRead(n.id)}
                    type="button"
                  >
                    Marcar como leída
                  </button>
                </li>
              ))}
            </ul>
          </div>
        )}

        {isLoading ? (
          <p className="empty-state">Cargando postulaciones...</p>
        ) : postulaciones.length === 0 ? (
          <div className="empty-state-cta">
            <ClipboardList aria-hidden="true" size={40} />
            <p>Aún no hay postulaciones para esta vacante.</p>
          </div>
        ) : (
          <div className="postulacion-list">
            <p className="vacantes-count">
              {postulaciones.length}{' '}
              {postulaciones.length === 1 ? 'postulación recibida' : 'postulaciones recibidas'}
            </p>
            {postulaciones.map((p) => {
              const statusConfig = STATUS_CONFIG[p.status] ?? { className: '', label: p.status };
              return (
                <article key={p.id} className="postulacion-card">
                  <div className="postulacion-card__header">
                    <div className="postulacion-card__icon">
                      <UserRound aria-hidden="true" size={24} />
                    </div>
                    <div>
                      <h3 className="postulacion-card__title">{p.candidateFullName}</h3>
                    </div>
                    <span className={`postulacion-status ${statusConfig.className}`}>
                      {statusConfig.label}
                    </span>
                  </div>
                  <dl className="postulacion-card__details">
                    <div>
                      <dt>Fecha de postulación</dt>
                      <dd>
                        <MapPin aria-hidden="true" size={13} />
                        {' '}{formatDate(p.appliedAt)}
                      </dd>
                    </div>
                    <div>
                      <dt>Última actualización</dt>
                      <dd>{formatDate(p.updatedAtUtc)}</dd>
                    </div>
                  </dl>
                  <div className="vacante-card__footer">
                    <Link
                      className="primary-action"
                      to={`/empleador/postulaciones/${p.id}`}
                    >
                      Ver perfil completo
                    </Link>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
