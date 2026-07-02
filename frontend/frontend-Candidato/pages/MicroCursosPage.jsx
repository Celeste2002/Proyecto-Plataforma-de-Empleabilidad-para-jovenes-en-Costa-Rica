import { useEffect, useRef, useState } from 'react';
import { ArrowLeft, Award, BookOpenCheck, Building2, Clock3, ExternalLink, Filter, RefreshCw, Search, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';
import {
  getMicroCursoDetail,
  getMicroCursos,
  getRecommendedMicroCursos,
} from '../api/candidatesApi.js';

function formatDuration(hours) {
  return `${hours} ${hours === 1 ? 'hora' : 'horas'}`;
}

function getUniqueAreas(cursos) {
  const areas = cursos
    .map((curso) => curso.area)
    .filter(Boolean);
  return Array.from(new Set(areas)).sort((a, b) => a.localeCompare(b, 'es-CR'));
}

function MicroCursoCard({ curso, isRecommended, onSelect }) {
  return (
    <article className="microcurso-card">
      <div className="microcurso-card__header">
        <div className="microcurso-card__icon">
          {isRecommended ? (
            <Sparkles aria-hidden="true" size={22} />
          ) : (
            <BookOpenCheck aria-hidden="true" size={22} />
          )}
        </div>
        <div>
          <p className="microcurso-card__area">{curso.area}</p>
          <h3 className="microcurso-card__title">{curso.titulo}</h3>
        </div>
      </div>

      <p className="microcurso-card__description">{curso.descripcion}</p>

      <dl className="microcurso-card__details">
        <div>
          <dt>Duracion</dt>
          <dd>
            <Clock3 aria-hidden="true" size={13} />
            {formatDuration(curso.duracionHoras)}
          </dd>
        </div>
        <div>
          <dt>Entidad</dt>
          <dd>
            <Building2 aria-hidden="true" size={13} />
            {curso.entidadProveedora}
          </dd>
        </div>
        <div>
          <dt>Certificacion</dt>
          <dd>
            <Award aria-hidden="true" size={13} />
            {curso.otorgaCertificacion ? 'Si otorga' : 'No otorga'}
          </dd>
        </div>
      </dl>

      <div className="microcurso-card__meta">
        <span className="vacante-badge vacante-badge--experience">
          {curso.tipoProveedor}
        </span>
        <span className="vacante-badge vacante-badge--active">
          {curso.cantidadValidaciones} validaciones
        </span>
        {curso.coincidencias > 0 && (
          <span className="vacante-badge vacante-badge--modality">
            {curso.coincidencias} coincidencia{curso.coincidencias === 1 ? '' : 's'}
          </span>
        )}
      </div>

      <div className="microcurso-card__footer">
        <button className="secondary-action" onClick={() => onSelect(curso)} type="button">
          Ver detalle
        </button>
      </div>
    </article>
  );
}

export function MicroCursosPage() {
  const { token } = useAuth();

  const [activeTab, setActiveTab] = useState('catalogo');
  const [areaFilter, setAreaFilter] = useState('');
  const [areaOptions, setAreaOptions] = useState([]);
  const [catalogo, setCatalogo] = useState([]);
  const [recomendados, setRecomendados] = useState([]);
  const [selectedCurso, setSelectedCurso] = useState(null);
  const [isLoadingCatalogo, setIsLoadingCatalogo] = useState(true);
  const [isLoadingRecomendados, setIsLoadingRecomendados] = useState(true);
  const [isLoadingDetail, setIsLoadingDetail] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const detailSectionRef = useRef(null);

  async function loadCatalogo(selectedArea = areaFilter) {
    setIsLoadingCatalogo(true);
    setErrorMessage('');

    try {
      const data = await getMicroCursos(token, selectedArea);
      setCatalogo(data);
      if (!selectedArea) {
        setAreaOptions(getUniqueAreas(data));
      }
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoadingCatalogo(false);
    }
  }

  async function loadRecomendados() {
    setIsLoadingRecomendados(true);
    setErrorMessage('');

    try {
      const data = await getRecommendedMicroCursos(token);
      setRecomendados(data);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoadingRecomendados(false);
    }
  }

  useEffect(() => {
    loadCatalogo('');
    loadRecomendados();
  }, [token]);

  useEffect(() => {
    if (selectedCurso && !isLoadingDetail) {
      detailSectionRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }, [selectedCurso, isLoadingDetail]);

  async function handleAreaChange(event) {
    const nextArea = event.target.value;
    setAreaFilter(nextArea);
    await loadCatalogo(nextArea);
  }

  async function handleSelectCurso(curso) {
    setIsLoadingDetail(true);
    setErrorMessage('');

    try {
      const detail = await getMicroCursoDetail(token, curso.id);
      setSelectedCurso({
        ...detail,
        coincidencias: curso.coincidencias,
        habilidadesCoincidentes: curso.habilidadesCoincidentes,
      });
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoadingDetail(false);
    }
  }

  const visibleCursos = activeTab === 'catalogo' ? catalogo : recomendados;
  const isLoading = activeTab === 'catalogo' ? isLoadingCatalogo : isLoadingRecomendados;
  const emptyMessage = activeTab === 'catalogo'
    ? 'No hay microcursos disponibles para el area seleccionada.'
    : 'No hay recomendaciones disponibles con las habilidades registradas.';

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink to="/candidato" />
        <nav className="dashboard-nav" aria-label="Navegacion del candidato">
          <Link className="secondary-action" to="/candidato">
            <ArrowLeft aria-hidden="true" size={16} />
            Panel principal
          </Link>
        </nav>
      </header>

      <section className="dashboard-layout">
        <div className="dashboard-welcome">
          <div className="dashboard-avatar">
            <BookOpenCheck aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Formacion</p>
            <h2>Microcursos disponibles</h2>
            <p className="section-description">
              Cursos breves con proveedor, duracion, certificacion y validaciones de empleadores aliados.
            </p>
          </div>
        </div>

        <div className="microcurso-toolbar">
          <div className="segmented-control" aria-label="Vista de microcursos">
            <button
              className={activeTab === 'catalogo' ? 'active' : ''}
              onClick={() => setActiveTab('catalogo')}
              type="button"
            >
              Catalogo
            </button>
            <button
              className={activeTab === 'recomendados' ? 'active' : ''}
              onClick={() => setActiveTab('recomendados')}
              type="button"
            >
              Recomendados
            </button>
          </div>

          <button
            className="secondary-action"
            onClick={activeTab === 'catalogo' ? () => loadCatalogo(areaFilter) : loadRecomendados}
            type="button"
          >
            <RefreshCw aria-hidden="true" size={16} />
            Actualizar
          </button>
        </div>

        {activeTab === 'catalogo' && (
          <div className="vacantes-filters">
            <div className="microcurso-filter-row">
              <label>
                Area
                <select name="area" value={areaFilter} onChange={handleAreaChange}>
                  <option value="">Todas las areas</option>
                  {areaOptions.map((area) => (
                    <option key={area} value={area}>{area}</option>
                  ))}
                </select>
              </label>
              {areaFilter && (
                <button
                  className="secondary-action vacantes-filters__clear"
                  onClick={() => handleAreaChange({ target: { value: '' } })}
                  type="button"
                >
                  <Filter aria-hidden="true" size={16} />
                  Limpiar filtro
                </button>
              )}
            </div>
          </div>
        )}

        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando microcursos...</p>
        ) : visibleCursos.length === 0 ? (
          <p className="empty-state">
            <Search aria-hidden="true" size={20} />
            {' '}
            {emptyMessage}
          </p>
        ) : (
          <div className="microcurso-grid">
            {visibleCursos.map((curso) => (
              <MicroCursoCard
                curso={curso}
                isRecommended={activeTab === 'recomendados'}
                key={curso.id}
                onSelect={handleSelectCurso}
              />
            ))}
          </div>
        )}

        {isLoadingDetail && (
          <p className="empty-state">Cargando detalle del microcurso...</p>
        )}

        {selectedCurso && !isLoadingDetail && (
          <section className="microcurso-detail" aria-label="Detalle del microcurso" ref={detailSectionRef}>
            <div className="microcurso-detail__header">
              <div>
                <p className="eyebrow">{selectedCurso.area}</p>
                <h3>{selectedCurso.titulo}</h3>
              </div>
              <span className="vacante-badge vacante-badge--active">
                {selectedCurso.cantidadValidaciones} validaciones
              </span>
            </div>
            <p className="microcurso-detail__description">{selectedCurso.descripcion}</p>
            <dl className="microcurso-detail__data">
              <div>
                <dt>Duracion</dt>
                <dd>{formatDuration(selectedCurso.duracionHoras)}</dd>
              </div>
              <div>
                <dt>Entidad</dt>
                <dd>{selectedCurso.entidadProveedora}</dd>
              </div>
              <div>
                <dt>Proveedor</dt>
                <dd>{selectedCurso.tipoProveedor}</dd>
              </div>
              <div>
                <dt>Certificacion</dt>
                <dd>{selectedCurso.otorgaCertificacion ? 'Si otorga certificacion' : 'No otorga certificacion'}</dd>
              </div>
            </dl>
            {selectedCurso.enlaceUrl && (
              <a
                className="primary-action microcurso-detail__link"
                href={selectedCurso.enlaceUrl}
                rel="noopener noreferrer"
                target="_blank"
              >
                <ExternalLink aria-hidden="true" size={16} />
                Ir al curso
              </a>
            )}
            {selectedCurso.habilidades?.length > 0 && (
              <div className="microcurso-skill-block">
                <p className="cursos-plataforma-label">Habilidades relacionadas</p>
                <div className="habilidades-tags">
                  {selectedCurso.habilidades.map((habilidad) => (
                    <span className="habilidad-tag" key={habilidad}>{habilidad}</span>
                  ))}
                </div>
              </div>
            )}
            {selectedCurso.habilidadesCoincidentes?.length > 0 && (
              <div className="microcurso-skill-block">
                <p className="cursos-plataforma-label">Coincidencias con mi perfil</p>
                <div className="habilidades-tags">
                  {selectedCurso.habilidadesCoincidentes.map((habilidad) => (
                    <span className="habilidad-tag habilidad-tag--match" key={habilidad}>{habilidad}</span>
                  ))}
                </div>
              </div>
            )}
          </section>
        )}
      </section>
    </main>
  );
}
