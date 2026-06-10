import { useEffect, useState } from 'react';
import { ArrowLeft, BriefcaseBusiness, MapPin, Plus, RefreshCw } from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import { getMyVacantes } from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function MisVacantesPage() {
  const { token } = useAuth();
  const location = useLocation();

  const [vacantes, setVacantes] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState(
    location.state?.created ? 'Vacante publicada correctamente.' : '',
  );

  async function loadVacantes() {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const data = await getMyVacantes(token);
      setVacantes(data);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadVacantes();
  }, []);

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
            <p className="eyebrow">Gestión de vacantes</p>
            <h2>Mis vacantes publicadas</h2>
          </div>
          <div className="section-heading-actions">
            <button className="secondary-action" onClick={loadVacantes} type="button">
              <RefreshCw aria-hidden="true" size={18} />
              Actualizar
            </button>
            <Link className="primary-action" to="/empleador/vacantes/nueva">
              <Plus aria-hidden="true" size={18} />
              Nueva vacante
            </Link>
          </div>
        </div>

        <StatusMessage message={successMessage} tone="success" />
        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando vacantes...</p>
        ) : vacantes.length === 0 ? (
          <div className="empty-state-cta">
            <BriefcaseBusiness aria-hidden="true" size={40} />
            <p>Aún no has publicado ninguna vacante.</p>
            <Link className="primary-action" to="/empleador/vacantes/nueva">
              <Plus aria-hidden="true" size={16} />
              Publicar primera vacante
            </Link>
          </div>
        ) : (
          <div className="vacante-list">
            <p className="vacantes-count">
              {vacantes.length}{' '}
              {vacantes.length === 1 ? 'vacante publicada' : 'vacantes publicadas'}
            </p>
            {vacantes.map((vacante) => (
              <article key={vacante.id} className="vacante-card">
                <div className="vacante-card__header">
                  <div className="vacante-card__icon">
                    <BriefcaseBusiness aria-hidden="true" size={24} />
                  </div>
                  <div className="vacante-card__title-area">
                    <h3 className="vacante-card__title">{vacante.jobTitle}</h3>
                    <p className="vacante-card__company">{vacante.companyName}</p>
                  </div>
                  <div className="vacante-card__meta">
                    <span className="vacante-badge vacante-badge--modality">{vacante.modality}</span>
                    <span className="vacante-badge vacante-badge--experience">{vacante.experienceLevel}</span>
                  </div>
                </div>
                <dl className="vacante-card__details">
                  <div>
                    <dt>Provincia</dt>
                    <dd>
                      <MapPin aria-hidden="true" size={13} />
                      {' '}{vacante.province}
                    </dd>
                  </div>
                  <div>
                    <dt>Sector</dt>
                    <dd>{vacante.sector}</dd>
                  </div>
                  {vacante.salaryRange && (
                    <div>
                      <dt>Salario</dt>
                      <dd>{vacante.salaryRange}</dd>
                    </div>
                  )}
                  <div>
                    <dt>Publicado</dt>
                    <dd>{formatDate(vacante.publishedAt)}</dd>
                  </div>
                </dl>
                {vacante.description && (
                  <p className="vacante-card__description">{vacante.description}</p>
                )}
                {vacante.requirements && (
                  <div className="vacante-card__requirements">
                    <p className="vacante-card__requirements-label">Requisitos</p>
                    <p className="vacante-card__description">{vacante.requirements}</p>
                  </div>
                )}
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
