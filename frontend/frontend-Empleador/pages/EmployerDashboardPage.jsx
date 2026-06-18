import { Bell, BriefcaseBusiness, KeyRound, LogOut, RefreshCw, UserRoundCheck } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getUnreadNotificacionCount, getVisibleCandidateProfiles } from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function EmployerDashboardPage() {
  const { user, logout, token } = useAuth();
  const navigate = useNavigate();

  const [candidateProfiles, setCandidateProfiles] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [unreadCount, setUnreadCount] = useState(0);

  const loadDashboardData = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setIsLoading(true);
    }

    setErrorMessage('');

    try {
      const [profiles, unread] = await Promise.all([
        getVisibleCandidateProfiles(token),
        getUnreadNotificacionCount(token).catch(() => ({ count: 0 })),
      ]);
      setCandidateProfiles(profiles);
      setUnreadCount(unread.count ?? 0);
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
          <Link className="secondary-action" to="/empleador/vacantes">
            <BriefcaseBusiness aria-hidden="true" size={16} />
            Mis vacantes
          </Link>
          <Link className="secondary-action bell-action" to="/empleador/vacantes">
            <Bell aria-hidden="true" size={16} />
            {unreadCount > 0 && (
              <span className="bell-badge" aria-label={`${unreadCount} notificaciones sin leer`}>
                {unreadCount}
              </span>
            )}
            Notificaciones
          </Link>
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
      </section>
    </main>
  );
}
