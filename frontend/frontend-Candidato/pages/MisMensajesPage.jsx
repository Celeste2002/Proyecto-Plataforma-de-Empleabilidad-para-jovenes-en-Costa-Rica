import { ArrowLeft, MessageSquare } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import { getMisMensajes } from '../api/candidatesApi.js';

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function MisMensajesPage() {
  const { token } = useAuth();

  const [mensajes, setMensajes] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');

  useEffect(() => {
    getMisMensajes(token)
      .then((data) => setMensajes(data))
      .catch((err) => setErrorMsg(err.message))
      .finally(() => setIsLoading(false));
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
            <MessageSquare aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Comunicaciones</p>
            <h2>Mis mensajes</h2>
            <p className="section-description">
              Mensajes enviados por empleadores sobre tus postulaciones.
            </p>
          </div>
        </div>

        {isLoading && (
          <p className="empty-state">Cargando mensajes...</p>
        )}

        {!isLoading && errorMsg && (
          <div className="field-error" role="alert">
            <p>{errorMsg}</p>
          </div>
        )}

        {!isLoading && !errorMsg && mensajes.length === 0 && (
          <p className="empty-state">
            Aún no has recibido mensajes de empleadores.
          </p>
        )}

        {!isLoading && !errorMsg && mensajes.length > 0 && (
          <div className="mensaje-list">
            <p className="vacantes-count">
              {mensajes.length}{' '}
              {mensajes.length === 1 ? 'mensaje recibido' : 'mensajes recibidos'}
            </p>
            {mensajes.map((mensaje) => (
              <article key={mensaje.id} className="mensaje-card">
                <div className="mensaje-card__header">
                  <div className="mensaje-card__sender">
                    <p className="mensaje-card__company">{mensaje.senderCompanyName}</p>
                    <p className="mensaje-card__vacante">{mensaje.jobTitle}</p>
                  </div>
                  <time className="mensaje-card__date" dateTime={mensaje.sentAtUtc}>
                    {formatDate(mensaje.sentAtUtc)}
                  </time>
                </div>
                <p className="mensaje-card__body">{mensaje.body}</p>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
