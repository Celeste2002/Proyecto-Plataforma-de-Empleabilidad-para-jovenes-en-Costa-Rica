import { BriefcaseBusiness, CircleCheck, CircleOff, KeyRound, LogOut, RefreshCw, UserRoundCheck } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getMyVacantes, getVisibleCandidateProfiles, updateMyVacanteStatus } from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function EmployerDashboardPage() {
  const { user, logout, token } = useAuth();
  const navigate = useNavigate();

  const [candidateProfiles, setCandidateProfiles] = useState([]);
  const [vacantes, setVacantes] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [savingVacanteId, setSavingVacanteId] = useState(null);
  const [savedVacanteId, setSavedVacanteId] = useState(null);

  const loadDashboardData = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setIsLoading(true);
    }

    setErrorMessage('');

    try {
      const [profiles, ownVacantes] = await Promise.all([
        getVisibleCandidateProfiles(token),
        getMyVacantes(token),
      ]);
      setCandidateProfiles(profiles);
      setVacantes(ownVacantes);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      if (showLoading) {
        setIsLoading(false);
      }
    }
  }, [token]);

  useEffect(() => {
    loadDashboardData(true);
    const intervalId = setInterval(() => {
      loadDashboardData(false);
    }, 15000);

    return () => clearInterval(intervalId);
  }, [loadDashboardData]);

  async function handleVacanteStatusChange(vacanteId, isActive) {
    setSavingVacanteId(vacanteId);
    setErrorMessage('');

    try {
      const updatedVacante = await updateMyVacanteStatus(token, vacanteId, isActive);
      setVacantes((prev) => prev.map((vacante) => (
        vacante.id === vacanteId ? updatedVacante : vacante
      )));
      setSavedVacanteId(vacanteId);
      window.setTimeout(() => setSavedVacanteId(null), 1800);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setSavingVacanteId(null);
    }
  }

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
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
          <span className="dashboard-user-email">{user?.email}</span>
          <Link className="secondary-action" to={AUTH_ROUTES.recoverPassword}>
            <KeyRound aria-hidden="true" size={16} />
            Restablecer contraseña
          </Link>
          <button className="secondary-action" onClick={handleLogout} type="button">
            <LogOut aria-hidden="true" size={16} />
            Cerrar sesión
          </button>
        </nav>
      </header>

      <section className="employer-view">
        <div className="section-heading horizontal-heading">
          <div>
            <p className="eyebrow">Empleadores aliados</p>
            <h2>Candidatos visibles</h2>
          </div>
          <button className="secondary-action" onClick={() => loadDashboardData(true)} type="button">
            <RefreshCw aria-hidden="true" size={18} />
            Actualizar
          </button>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando perfiles...</p>
        ) : candidateProfiles.length === 0 ? (
          <p className="empty-state">Aún no hay perfiles registrados.</p>
        ) : (
          <div className="candidate-list">
            {candidateProfiles.map((candidateProfile) => (
              <article className="candidate-card" key={candidateProfile.id}>
                <div className="candidate-avatar" aria-hidden="true">
                  <UserRoundCheck size={24} />
                </div>
                <div>
                  <h3>{candidateProfile.fullName}</h3>
                  <p>{candidateProfile.educationLevel}</p>
                </div>
                <dl>
                  <div>
                    <dt>Edad</dt>
                    <dd>{candidateProfile.age}</dd>
                  </div>
                  <div>
                    <dt>Provincia</dt>
                    <dd>{candidateProfile.province}</dd>
                  </div>
                  <div>
                    <dt>Correo</dt>
                    <dd>{candidateProfile.email}</dd>
                  </div>
                  <div>
                    <dt>Confirmación</dt>
                    <dd>{candidateProfile.emailConfirmationSent ? 'Enviada' : 'Pendiente'}</dd>
                  </div>
                </dl>
              </article>
            ))}
          </div>
        )}

        <div className="section-heading horizontal-heading employer-section-spacing">
          <div>
            <p className="eyebrow">Gestión de vacantes</p>
            <h2>Mis vacantes</h2>
            <p className="section-description">
              Aquí puedes desactivar una vacante para dejar de recibir postulaciones y volver a activarla cuando quieras.
            </p>
          </div>
          <button className="secondary-action" onClick={() => loadDashboardData(true)} type="button">
            <RefreshCw aria-hidden="true" size={18} />
            Actualizar
          </button>
        </div>

        {isLoading ? (
          <p className="empty-state">Cargando vacantes...</p>
        ) : vacantes.length === 0 ? (
          <p className="empty-state">Aún no tienes vacantes registradas.</p>
        ) : (
          <div className="admin-table-wrapper">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Vacante</th>
                  <th>Provincia</th>
                  <th>Estado</th>
                  <th>Publicado</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {vacantes.map((vacante) => {
                  const isSaving = savingVacanteId === vacante.id;
                  const isSaved = savedVacanteId === vacante.id;

                  return (
                    <tr key={vacante.id} className={isSaved ? 'admin-table__row--saved' : ''}>
                      <td>
                        <div className="vacante-table__title">
                          <BriefcaseBusiness size={16} />
                          <div>
                            <strong>{vacante.jobTitle}</strong>
                            <p>{vacante.sector} · {vacante.modality}</p>
                          </div>
                        </div>
                      </td>
                      <td>{vacante.province}</td>
                      <td>
                        <span className={`vacante-badge ${vacante.isActive ? 'vacante-badge--active' : 'vacante-badge--inactive'}`}>
                          {vacante.isActive ? 'Activa' : 'Desactivada'}
                        </span>
                      </td>
                      <td className="admin-table__date">
                        {new Date(vacante.publishedAt).toLocaleDateString('es-CR')}
                      </td>
                      <td>
                        <button
                          className={`admin-save-btn ${vacante.isActive ? 'admin-save-btn--danger' : ''}`}
                          disabled={isSaving}
                          onClick={() => handleVacanteStatusChange(vacante.id, !vacante.isActive)}
                          type="button"
                        >
                          {isSaving ? 'Actualizando...' : vacante.isActive ? (
                            <>
                              <CircleOff size={16} />
                              Desactivar
                            </>
                          ) : (
                            <>
                              <CircleCheck size={16} />
                              Activar
                            </>
                          )}
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  );
}
