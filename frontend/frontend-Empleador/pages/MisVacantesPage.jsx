import { useEffect, useState } from 'react';
import {
  ArrowLeft,
  BriefcaseBusiness,
  MailPlus, 
  MapPin,
  Pencil,
  Plus,
  RefreshCw,
  Save,
  UsersRound,
  X,
  ClipboardList,
} from 'lucide-react';
import { Link, useLocation } from 'react-router-dom';
import {
  getMyVacantes,
  getVacantePostulaciones,
  requestInterview,
  updateVacante,
} from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const POSTULACION_STATUS_CLASS = {
  Enviada: 'postulacion-status--enviada',
  'En revisión': 'postulacion-status--revision',
  'Entrevista solicitada': 'postulacion-status--entrevista',
  Finalizada: 'postulacion-status--finalizada',
};

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
  const [editingVacanteId, setEditingVacanteId] = useState(null);
  const [editForm, setEditForm] = useState({
    description: '',
    requirements: '',
    salaryRange: '',
  });
  const [editValidationErrors, setEditValidationErrors] = useState([]);
  const [isSavingEdit, setIsSavingEdit] = useState(false);
  const [expandedVacanteId, setExpandedVacanteId] = useState(null);
  const [postulacionesByVacante, setPostulacionesByVacante] = useState({});
  const [loadingPostulacionesId, setLoadingPostulacionesId] = useState(null);
  const [postulacionesErrors, setPostulacionesErrors] = useState({});
  const [requestingInterviewId, setRequestingInterviewId] = useState(null);
  const [interviewMessages, setInterviewMessages] = useState({});

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

  function startEditing(vacante) {
    setErrorMessage('');
    setSuccessMessage('');
    setEditValidationErrors([]);
    setEditingVacanteId(vacante.id);
    setEditForm({
      description: vacante.description ?? '',
      requirements: vacante.requirements ?? '',
      salaryRange: vacante.salaryRange ?? '',
    });
  }

  function cancelEditing() {
    setEditingVacanteId(null);
    setEditValidationErrors([]);
  }

  function handleEditChange(e) {
    const { name, value } = e.target;
    setEditForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleEditSubmit(e, vacanteId) {
    e.preventDefault();
    setIsSavingEdit(true);
    setErrorMessage('');
    setSuccessMessage('');
    setEditValidationErrors([]);

    try {
      const updatedVacante = await updateVacante(token, vacanteId, editForm);
      setVacantes((prev) => prev.map((vacante) => (
        vacante.id === updatedVacante.id ? updatedVacante : vacante
      )));
      setEditingVacanteId(null);
      setSuccessMessage('Vacante actualizada correctamente.');
    } catch (error) {
      if (error.validationErrors?.length > 0) {
        setEditValidationErrors(error.validationErrors);
      } else {
        setErrorMessage(error.message);
      }
    } finally {
      setIsSavingEdit(false);
    }
  }

  async function loadPostulaciones(vacanteId) {
    setExpandedVacanteId(vacanteId);
    setLoadingPostulacionesId(vacanteId);
    setPostulacionesErrors((prev) => ({ ...prev, [vacanteId]: '' }));

    try {
      const postulaciones = await getVacantePostulaciones(token, vacanteId);
      setPostulacionesByVacante((prev) => ({ ...prev, [vacanteId]: postulaciones }));
    } catch (error) {
      setPostulacionesErrors((prev) => ({ ...prev, [vacanteId]: error.message }));
    } finally {
      setLoadingPostulacionesId(null);
    }
  }

  function togglePostulaciones(vacanteId) {
    if (expandedVacanteId === vacanteId) {
      setExpandedVacanteId(null);
      return;
    }

    if (postulacionesByVacante[vacanteId]) {
      setExpandedVacanteId(vacanteId);
      return;
    }

    loadPostulaciones(vacanteId);
  }

  async function handleRequestInterview(postulacionId) {
    setRequestingInterviewId(postulacionId);
    setInterviewMessages((prev) => ({ ...prev, [postulacionId]: null }));

    try {
      const updatedPostulacion = await requestInterview(token, postulacionId);
      setPostulacionesByVacante((prev) => ({
        ...prev,
        [updatedPostulacion.vacanteId]: (prev[updatedPostulacion.vacanteId] ?? []).map((postulacion) => (
          postulacion.id === updatedPostulacion.id ? updatedPostulacion : postulacion
        )),
      }));
      setInterviewMessages((prev) => ({
        ...prev,
        [updatedPostulacion.id]: {
          success: true,
          message: 'Correo enviado y estado actualizado.',
        },
      }));
    } catch (error) {
      setInterviewMessages((prev) => ({
        ...prev,
        [postulacionId]: {
          success: false,
          message: error.validationErrors?.[0] ?? error.message,
        },
      }));
    } finally {
      setRequestingInterviewId(null);
    }
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
                    <span className={vacante.isActive ? 'vacante-badge vacante-badge--active' : 'vacante-badge vacante-badge--inactive'}>
                      {vacante.isActive ? 'Activa' : 'Inactiva'}
                    </span>
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
                {editingVacanteId === vacante.id ? (
                  <form
                    className="vacante-edit-form"
                    noValidate
                    onSubmit={(e) => handleEditSubmit(e, vacante.id)}
                  >
                    {editValidationErrors.length > 0 && (
                      <ul className="field-error" role="alert">
                        {editValidationErrors.map((err) => (
                          <li key={err}>{err}</li>
                        ))}
                      </ul>
                    )}

                    <label>
                      Rango salarial
                      <input
                        maxLength={100}
                        name="salaryRange"
                        onChange={handleEditChange}
                        placeholder="Ej. ₡500,000 – ₡700,000 mensuales"
                        type="text"
                        value={editForm.salaryRange}
                      />
                    </label>

                    <label>
                      Descripción del puesto
                      <textarea
                        name="description"
                        onChange={handleEditChange}
                        placeholder="Describe las responsabilidades y el entorno de trabajo..."
                        rows={4}
                        value={editForm.description}
                      />
                    </label>

                    <label>
                      Requisitos del candidato
                      <textarea
                        name="requirements"
                        onChange={handleEditChange}
                        placeholder="Lista los conocimientos, habilidades y cualificaciones necesarias..."
                        rows={4}
                        value={editForm.requirements}
                      />
                    </label>

                    <div className="vacante-card__edit-actions">
                      <button className="primary-action" disabled={isSavingEdit} type="submit">
                        <Save aria-hidden="true" size={16} />
                        {isSavingEdit ? 'Guardando...' : 'Guardar cambios'}
                      </button>
                      <button
                        className="secondary-action"
                        disabled={isSavingEdit}
                        onClick={cancelEditing}
                        type="button"
                      >
                        <X aria-hidden="true" size={16} />
                        Cancelar
                      </button>
                    </div>
                  </form>
                ) : (
                  <>
                    {vacante.description && (
                      <p className="vacante-card__description">{vacante.description}</p>
                    )}
                    {vacante.requirements && (
                      <div className="vacante-card__requirements">
                        <p className="vacante-card__requirements-label">Requisitos</p>
                        <p className="vacante-card__description">{vacante.requirements}</p>
                      </div>
                    )}
                    <div className="vacante-card__footer">
                      <button
                        className="secondary-action vacante-card__apply-btn"
                        disabled={!vacante.isActive}
                        onClick={() => startEditing(vacante)}
                        type="button"
                      >
                        <Pencil aria-hidden="true" size={16} />
                        Editar
                      </button>
                      <button
                        className="secondary-action vacante-card__apply-btn"
                        onClick={() => togglePostulaciones(vacante.id)}
                        type="button"
                      >
                        <UsersRound aria-hidden="true" size={16} />
                        {expandedVacanteId === vacante.id ? 'Ocultar postulantes' : 'Ver postulantes'}
                      </button>
                    </div>
                    {expandedVacanteId === vacante.id && (
                      <div className="postulantes-panel">
                        <div className="postulantes-panel__header">
                          <h4>Postulantes</h4>
                          <button
                            className="secondary-action"
                            disabled={loadingPostulacionesId === vacante.id}
                            onClick={() => loadPostulaciones(vacante.id)}
                            type="button"
                          >
                            <RefreshCw aria-hidden="true" size={16} />
                            Actualizar
                          </button>
                        </div>

                        {loadingPostulacionesId === vacante.id && (
                          <p className="empty-state">Cargando postulantes...</p>
                        )}

                        {postulacionesErrors[vacante.id] && (
                          <div className="field-error" role="alert">
                            <p>{postulacionesErrors[vacante.id]}</p>
                          </div>
                        )}

                        {loadingPostulacionesId !== vacante.id
                          && !postulacionesErrors[vacante.id]
                          && (postulacionesByVacante[vacante.id]?.length ?? 0) === 0 && (
                            <p className="empty-state">Aún no hay candidatos postulados a esta vacante.</p>
                          )}

                        {loadingPostulacionesId !== vacante.id
                          && !postulacionesErrors[vacante.id]
                          && (postulacionesByVacante[vacante.id]?.length ?? 0) > 0 && (
                            <div className="postulante-list">
                              {postulacionesByVacante[vacante.id].map((postulacion) => {
                                const statusClass = POSTULACION_STATUS_CLASS[postulacion.status] ?? '';
                                const interviewMessage = interviewMessages[postulacion.id];
                                const interviewAlreadyRequested = postulacion.status === 'Entrevista solicitada';
                                const isFinalized = postulacion.status === 'Finalizada';

                                return (
                                  <article className="postulante-card" key={postulacion.id}>
                                    <div className="postulante-card__main">
                                      <div>
                                        <h5>{postulacion.candidateFullName}</h5>
                                        <p>{postulacion.candidateEmail}</p>
                                      </div>
                                      <span className={`postulacion-status ${statusClass}`}>
                                        {postulacion.status}
                                      </span>
                                    </div>
                                    <dl className="postulante-card__details">
                                      <div>
                                        <dt>Provincia</dt>
                                        <dd>{postulacion.candidateProvince}</dd>
                                      </div>
                                      <div>
                                        <dt>Educación</dt>
                                        <dd>{postulacion.candidateEducationLevel}</dd>
                                      </div>
                                      <div>
                                        <dt>Postulación</dt>
                                        <dd>{formatDate(postulacion.appliedAt)}</dd>
                                      </div>
                                    </dl>
                                    <div className="postulante-card__actions">
                                      {interviewMessage && (
                                        <p
                                          className={`vacante-card__result ${interviewMessage.success ? 'vacante-card__result--success' : 'vacante-card__result--error'}`}
                                          role="status"
                                        >
                                          {interviewMessage.message}
                                        </p>
                                      )}
                                      <button
                                        className="primary-action vacante-card__apply-btn"
                                        disabled={
                                          requestingInterviewId === postulacion.id
                                          || interviewAlreadyRequested
                                          || isFinalized
                                        }
                                        onClick={() => handleRequestInterview(postulacion.id)}
                                        type="button"
                                      >
                                        <MailPlus aria-hidden="true" size={16} />
                                        {requestingInterviewId === postulacion.id
                                          ? 'Enviando...'
                                          : interviewAlreadyRequested
                                            ? 'Entrevista solicitada'
                                            : 'Solicitar entrevista'}
                                      </button>
                                    </div>
                                  </article>
                                );
                              })}
                            </div>
                          )}
                      </div>
                    )}
                  </>
                )}
                <div className="vacante-card__footer">
                  <Link
                    className="secondary-action"
                    to={`/empleador/vacantes/${vacante.id}/postulaciones`}
                  >
                    <ClipboardList aria-hidden="true" size={15} />
                    Ver postulaciones
                  </Link>
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </main>
  );
}
