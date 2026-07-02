import { useEffect, useState } from 'react';
import { ArrowLeft, ClipboardList, MapPin, Trash2 } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { ConfirmDialog } from '../../shared/components/ConfirmDialog.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import {
  deleteMyPostulacion,
  getMyPostulaciones,
  markMyPostulacionNotificationsRead,
} from '../api/candidatesApi.js';

const STATUS_CONFIG = {
  'Enviada': { className: 'postulacion-status--enviada', label: 'Enviada' },
  'Vista': { className: 'postulacion-status--vista', label: 'Vista por el empleador' },
  'Entrevista': { className: 'postulacion-status--entrevista', label: 'Entrevista programada' },
  'En revisión': { className: 'postulacion-status--revision', label: 'En revisión' },
  'Entrevista solicitada': { className: 'postulacion-status--entrevista', label: 'Entrevista solicitada' },
  'Entrevista programada': { className: 'postulacion-status--entrevista', label: 'Entrevista programada' },
  'Descartado': { className: 'postulacion-status--descartado', label: 'Descartado' },
  'Finalizada': { className: 'postulacion-status--finalizada', label: 'Finalizada' },
};

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function PostulacionesPage() {
  const { token } = useAuth();

  const [postulaciones, setPostulaciones] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');
  const [deletingId, setDeletingId] = useState(null);
  const [actionMessage, setActionMessage] = useState('');
  const [pendingDeleteId, setPendingDeleteId] = useState(null);

  useEffect(() => {
    getMyPostulaciones(token)
      .then((data) => {
        setPostulaciones(data);
        return markMyPostulacionNotificationsRead(token);
      })
      .catch((err) => setErrorMsg(err.message))
      .finally(() => setLoading(false));
  }, [token]);

  function handleRequestDelete(postulacionId) {
    setPendingDeleteId(postulacionId);
  }

  function handleCancelDelete() {
    setPendingDeleteId(null);
  }

  async function handleConfirmDelete() {
    const postulacionId = pendingDeleteId;
    setPendingDeleteId(null);
    setDeletingId(postulacionId);
    setErrorMsg('');
    setActionMessage('');

    try {
      await deleteMyPostulacion(token, postulacionId);
      setPostulaciones((current) => current.filter((p) => p.id !== postulacionId));
      setActionMessage('Postulación eliminada correctamente.');
    } catch (err) {
      setErrorMsg(err.message);
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink to="/candidato" />
        <nav className="dashboard-nav" aria-label="Navegación del candidato">
          <Link className="secondary-action" to="/candidato">
            <ArrowLeft aria-hidden="true" size={16} />
            Panel principal
          </Link>
        </nav>
      </header>

      <section className="dashboard-layout">
        <div className="dashboard-welcome">
          <div className="dashboard-avatar">
            <ClipboardList aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Mi actividad</p>
            <h2>Mis postulaciones</h2>
            <p className="section-description">
              Consulta el estado actual de todas tus postulaciones a vacantes de empleo.
            </p>
          </div>
        </div>

        <div className="postulaciones-legend">
          {Object.entries(STATUS_CONFIG).map(([key, config]) => (
            <span key={key} className={`postulacion-status ${config.className}`}>
              {config.label}
            </span>
          ))}
        </div>

        {loading && (
          <p className="empty-state">Cargando postulaciones...</p>
        )}

        {!loading && errorMsg && (
          <div className="field-error" role="alert">
            <p>{errorMsg}</p>
          </div>
        )}

        {actionMessage && (
          <p className="vacante-card__result vacante-card__result--success" role="status">
            {actionMessage}
          </p>
        )}

        {!loading && !errorMsg && postulaciones.length === 0 && (
          <p className="empty-state">
            Aún no tienes postulaciones. Explora las{' '}
            <Link to="/candidato/vacantes">vacantes disponibles</Link>.
          </p>
        )}

        {!loading && !errorMsg && postulaciones.length > 0 && (
          <div className="postulacion-list">
            <p className="vacantes-count">
              {postulaciones.length}{' '}
              {postulaciones.length === 1 ? 'postulación' : 'postulaciones'}
            </p>
            {postulaciones.map((p) => {
              const statusConfig = STATUS_CONFIG[p.status] ?? { className: '', label: p.status };
              return (
                <article key={p.id} className="postulacion-card">
                  <div className="postulacion-card__header">
                    <div>
                      <h3 className="postulacion-card__title">{p.jobTitle}</h3>
                      <p className="postulacion-card__company">{p.companyName}</p>
                    </div>
                    <span className={`postulacion-status ${statusConfig.className}`}>
                      {statusConfig.label}
                    </span>
                  </div>
                  <dl className="postulacion-card__details">
                    <div>
                      <dt>Provincia</dt>
                      <dd>
                        <MapPin aria-hidden="true" size={13} />
                        {' '}{p.province}
                      </dd>
                    </div>
                    <div>
                      <dt>Fecha de postulación</dt>
                      <dd>{formatDate(p.appliedAt)}</dd>
                    </div>
                    <div>
                      <dt>Última actualización</dt>
                      <dd>{formatDate(p.updatedAtUtc)}</dd>
                    </div>
                  </dl>
                  <div className="postulante-card__actions">
                    <button
                      className="danger-action vacante-card__apply-btn"
                      disabled={deletingId === p.id}
                      onClick={() => handleRequestDelete(p.id)}
                      type="button"
                    >
                      <Trash2 aria-hidden="true" size={16} />
                      {deletingId === p.id ? 'Eliminando...' : 'Eliminar postulación'}
                    </button>
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </section>

      <ConfirmDialog
        cancelLabel="Cancelar"
        confirmLabel="Eliminar"
        message="Esta acción no se puede deshacer. ¿Deseas eliminar esta postulación?"
        onCancel={handleCancelDelete}
        onConfirm={handleConfirmDelete}
        open={pendingDeleteId !== null}
        title="Eliminar postulación"
        tone="danger"
      />
    </main>
  );
}
