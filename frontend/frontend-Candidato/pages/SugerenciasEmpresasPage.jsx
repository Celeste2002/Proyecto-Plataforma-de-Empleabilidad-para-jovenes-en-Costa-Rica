import { useEffect, useState } from 'react';
import { ArrowLeft, MapPin, Send } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import { applyToVacante, getSugerenciasRecibidas } from '../api/candidatesApi.js';

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function SugerenciasEmpresasPage() {
  const { token } = useAuth();

  const [sugerencias, setSugerencias] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');
  const [applyingSugerenciaId, setApplyingSugerenciaId] = useState(null);
  const [sugerenciaApplyResults, setSugerenciaApplyResults] = useState({});

  useEffect(() => {
    getSugerenciasRecibidas(token)
      .then((data) => {
        setSugerencias(data);
        window.localStorage.setItem('candidate:sugerencias:lastSeen', String(Date.now()));
      })
      .catch((err) => setErrorMsg(err.message))
      .finally(() => setLoading(false));
  }, [token]);

  async function handleApplyFromSugerencia(sugerencia) {
    setApplyingSugerenciaId(sugerencia.id);
    try {
      await applyToVacante(token, sugerencia.vacanteId);
      setSugerenciaApplyResults((prev) => ({
        ...prev,
        [sugerencia.id]: { success: true, message: 'Postulación enviada correctamente.' },
      }));
    } catch (err) {
      setSugerenciaApplyResults((prev) => ({
        ...prev,
        [sugerencia.id]: { success: false, message: err.validationErrors?.[0] ?? err.message },
      }));
    } finally {
      setApplyingSugerenciaId(null);
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
            <Send aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Mi actividad</p>
            <h2>Postulaciones sugeridas por empresas</h2>
            <p className="section-description">
              Empresas aliadas revisaron tu perfil y te sugieren postularte a estas vacantes.
            </p>
          </div>
        </div>

        {loading && (
          <p className="empty-state">Cargando sugerencias...</p>
        )}

        {!loading && errorMsg && (
          <div className="field-error" role="alert">
            <p>{errorMsg}</p>
          </div>
        )}

        {!loading && !errorMsg && sugerencias.length === 0 && (
          <p className="empty-state">
            Aún no has recibido sugerencias de postulación de ninguna empresa.
          </p>
        )}

        {!loading && !errorMsg && sugerencias.length > 0 && (
          <div className="sugerencias-recibidas">
            {sugerencias.map((s) => {
              const result = sugerenciaApplyResults[s.id];
              const alreadyApplied = s.alreadyApplied || result?.success === true;
              return (
                <article key={s.id} className="postulacion-card sugerencia-card">
                  <div className="postulacion-card__header">
                    <div>
                      <h3 className="postulacion-card__title">{s.jobTitle}</h3>
                      <p className="postulacion-card__company">{s.companyName}</p>
                    </div>
                    {!s.vacanteIsActive && (
                      <span className="vacante-badge vacante-badge--inactive">Vacante desactivada</span>
                    )}
                  </div>
                  {s.message && <p className="sugerencia-card__message">"{s.message}"</p>}
                  <dl className="postulacion-card__details">
                    <div>
                      <dt>Provincia</dt>
                      <dd>
                        <MapPin aria-hidden="true" size={13} />
                        {' '}{s.province}
                      </dd>
                    </div>
                    <div>
                      <dt>Recibida</dt>
                      <dd>{formatDate(s.createdAtUtc)}</dd>
                    </div>
                  </dl>
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
                    disabled={applyingSugerenciaId === s.id || alreadyApplied || !s.vacanteIsActive}
                    onClick={() => handleApplyFromSugerencia(s)}
                    type="button"
                  >
                    <Send aria-hidden="true" size={15} />
                    {alreadyApplied ? 'Ya postulado' : applyingSugerenciaId === s.id ? 'Enviando...' : 'Postularme'}
                  </button>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </main>
  );
}
