import { useCallback, useEffect, useMemo, useState } from 'react';
import { ArrowLeft, BriefcaseBusiness, MapPin, Search, Send } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import { applyToVacante, getMyPostulaciones, getVacantes } from '../api/candidatesApi.js';
import { costaRicaProvinces } from '../constants/candidateCatalogs.js';
import { employerSectors, experienceLevels, vacanteModalities } from '../constants/vacanteCatalogs.js';

function formatDate(dateString) {
  return new Date(dateString).toLocaleDateString('es-CR', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

export function VacantesPage() {
  const { token } = useAuth();

  const [vacantes, setVacantes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [errorMsg, setErrorMsg] = useState('');
  const [applyingId, setApplyingId] = useState(null);
  const [applyResults, setApplyResults] = useState({});

  const [filters, setFilters] = useState({
    province: '',
    sector: '',
    modality: '',
    experienceLevel: '',
  });

  const loadVacantes = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setLoading(true);
    }

    setErrorMsg('');

    try {
      const [vacantesData, postulacionesData] = await Promise.all([
        getVacantes(token),
        getMyPostulaciones(token),
      ]);
      setVacantes(vacantesData);
      const existing = {};
      postulacionesData.forEach((p) => {
        existing[p.vacanteId] = { success: true, message: 'Ya te postulaste a esta vacante.' };
      });
      setApplyResults(existing);
    } catch (err) {
      setErrorMsg(err.message);
    } finally {
      if (showLoading) {
        setLoading(false);
      }
    }
  }, [token]);

  useEffect(() => {
    loadVacantes(true);
    const intervalId = setInterval(() => {
      loadVacantes(false);
    }, 15000);

    return () => clearInterval(intervalId);
  }, [loadVacantes]);

  const filteredVacantes = useMemo(() => {
    return vacantes.filter((v) => {
      if (filters.province && v.province !== filters.province) return false;
      if (filters.sector && v.sector !== filters.sector) return false;
      if (filters.modality && v.modality !== filters.modality) return false;
      if (filters.experienceLevel && v.experienceLevel !== filters.experienceLevel) return false;
      return true;
    });
  }, [vacantes, filters]);

  function handleFilterChange(e) {
    const { name, value } = e.target;
    setFilters((prev) => ({ ...prev, [name]: value }));
  }

  function clearFilters() {
    setFilters({ province: '', sector: '', modality: '', experienceLevel: '' });
  }

  const handleApply = useCallback(async (vacanteId) => {
    setApplyingId(vacanteId);
    setApplyResults((prev) => ({ ...prev, [vacanteId]: null }));
    try {
      await applyToVacante(token, vacanteId);
      setApplyResults((prev) => ({
        ...prev,
        [vacanteId]: { success: true, message: 'Postulación enviada correctamente.' },
      }));
    } catch (err) {
      setApplyResults((prev) => ({
        ...prev,
        [vacanteId]: { success: false, message: err.message },
      }));
    } finally {
      setApplyingId(null);
    }
  }, [token]);

  const hasActiveFilters = Object.values(filters).some(Boolean);

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
            <BriefcaseBusiness aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Empleos disponibles</p>
            <h2>Búsqueda de vacantes</h2>
            <p className="section-description">
              Filtra las ofertas de empleo por provincia, sector, modalidad y nivel de experiencia.
            </p>
          </div>
        </div>

        <div className="vacantes-filters">
          <div className="vacantes-filters__grid">
            <label>
              Provincia
              <select name="province" value={filters.province} onChange={handleFilterChange}>
                <option value="">Todas las provincias</option>
                {costaRicaProvinces.map((p) => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </label>
            <label>
              Sector
              <select name="sector" value={filters.sector} onChange={handleFilterChange}>
                <option value="">Todos los sectores</option>
                {employerSectors.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </label>
            <label>
              Modalidad
              <select name="modality" value={filters.modality} onChange={handleFilterChange}>
                <option value="">Todas las modalidades</option>
                {vacanteModalities.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </label>
            <label>
              Nivel de experiencia
              <select name="experienceLevel" value={filters.experienceLevel} onChange={handleFilterChange}>
                <option value="">Todos los niveles</option>
                {experienceLevels.map((l) => (
                  <option key={l} value={l}>{l}</option>
                ))}
              </select>
            </label>
          </div>
          {hasActiveFilters && (
            <button className="vacantes-filters__clear secondary-action" onClick={clearFilters} type="button">
              Limpiar filtros
            </button>
          )}
        </div>

        {loading && (
          <p className="empty-state">Cargando vacantes...</p>
        )}

        {!loading && errorMsg && (
          <div className="field-error" role="alert">
            <p>{errorMsg}</p>
          </div>
        )}

        {!loading && !errorMsg && filteredVacantes.length === 0 && (
          <p className="empty-state">
            <Search aria-hidden="true" size={20} />
            {' '}
            {hasActiveFilters
              ? 'No hay vacantes que coincidan con los filtros seleccionados.'
              : 'No hay vacantes disponibles en este momento.'}
          </p>
        )}

        {!loading && !errorMsg && filteredVacantes.length > 0 && (
          <div className="vacante-list">
            <p className="vacantes-count">
              {filteredVacantes.length}{' '}
              {filteredVacantes.length === 1 ? 'vacante encontrada' : 'vacantes encontradas'}
            </p>
            {filteredVacantes.map((vacante) => {
              const result = applyResults[vacante.id];
              const alreadyApplied = result?.success === true;
              return (
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
                      <span className={`vacante-badge ${vacante.isActive ? 'vacante-badge--active' : 'vacante-badge--inactive'}`}>
                        {vacante.isActive ? 'Activa' : 'Desactivada'}
                      </span>
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
                    <div>
                      <dt>Publicado</dt>
                      <dd>{formatDate(vacante.publishedAt)}</dd>
                    </div>
                  </dl>
                  {vacante.description && (
                    <p className="vacante-card__description">{vacante.description}</p>
                  )}
                  <div className="vacante-card__footer">
                    {result && (
                      <p
                        className={`vacante-card__result ${result.success ? 'vacante-card__result--success' : 'vacante-card__result--error'}`}
                        role="status"
                      >
                        {result.message}
                      </p>
                    )}
                    <button
                      className="primary-action vacante-card__apply-btn"
                      disabled={applyingId === vacante.id || alreadyApplied}
                      onClick={() => handleApply(vacante.id)}
                      type="button"
                    >
                      <Send aria-hidden="true" size={15} />
                      {alreadyApplied ? 'Postulación enviada' : applyingId === vacante.id ? 'Enviando...' : 'Postularme'}
                    </button>
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
