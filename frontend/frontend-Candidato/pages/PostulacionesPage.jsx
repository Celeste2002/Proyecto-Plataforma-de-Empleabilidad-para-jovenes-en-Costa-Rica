import { useEffect, useState } from 'react';
import { ArrowLeft, ClipboardList, MapPin } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import { getMyPostulaciones } from '../api/candidatesApi.js';

const STATUS_CONFIG = {
  'Enviada': { className: 'postulacion-status--enviada', label: 'Enviada' },
  'En revisión': { className: 'postulacion-status--revision', label: 'En revisión' },
  'Entrevista solicitada': { className: 'postulacion-status--entrevista', label: 'Entrevista solicitada' },
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

  useEffect(() => {
    getMyPostulaciones(token)
      .then((data) => setPostulaciones(data))
      .catch((err) => setErrorMsg(err.message))
      .finally(() => setLoading(false));
  }, [token]);

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
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
