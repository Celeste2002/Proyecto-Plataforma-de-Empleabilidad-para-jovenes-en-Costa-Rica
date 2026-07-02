import { Bell, BriefcaseBusiness, KeyRound, LogOut, Search, UserRoundCheck } from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getMyEmployerProfile, getUnreadNotificacionCount } from '../api/employerApi.js';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function EmployerDashboardPage() {
  const { user, logout, token } = useAuth();
  const navigate = useNavigate();

  const [errorMessage, setErrorMessage] = useState('');
  const [unreadCount, setUnreadCount] = useState(0);
  const [companyName, setCompanyName] = useState('');

  const loadDashboardData = useCallback(async () => {
    setErrorMessage('');

    try {
      const unread = await getUnreadNotificacionCount(token).catch(() => ({ count: 0 }));
      setUnreadCount(unread.count ?? 0);
    } catch (error) {
      setErrorMessage(error.message);
    }
  }, [token]);

  useEffect(() => {
    loadDashboardData();
    const intervalId = setInterval(() => {
      loadDashboardData();
    }, 15000);

    return () => clearInterval(intervalId);
  }, [loadDashboardData]);

  useEffect(() => {
    getMyEmployerProfile(token)
      .then((profile) => setCompanyName(profile.companyName))
      .catch(() => {});
  }, [token]);

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink subtitle={companyName} subtitleClassName="brand-subtitle--company" to="/empleador" />
        <nav className="dashboard-nav" aria-label="Navegación del empleador">
          <span className="dashboard-user-email">{user?.email}</span>
          <Link className="secondary-action" to="/empleador/vacantes">
            <BriefcaseBusiness aria-hidden="true" size={16} />
            Mis vacantes
          </Link>
          <Link className="secondary-action" to="/empleador/candidatos">
            <UserRoundCheck aria-hidden="true" size={16} />
            Candidatos
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
            <h2>Panel principal</h2>
          </div>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        <div className="dashboard-cta-grid">
          <Link className="dashboard-cta-card" to="/empleador/vacantes">
            <div className="dashboard-cta-card__icon">
              <BriefcaseBusiness aria-hidden="true" size={28} />
            </div>
            <div>
              <h3>Mis vacantes</h3>
              <p>
                Publica nuevas vacantes y revisa las postulaciones que has recibido en cada una de ellas.
              </p>
            </div>
          </Link>

          <Link className="dashboard-cta-card" to="/empleador/candidatos">
            <div className="dashboard-cta-card__icon">
              <UserRoundCheck aria-hidden="true" size={28} />
            </div>
            <div>
              <h3>Panel de candidatos</h3>
              <p>
                Da seguimiento a quienes ya se postularon: cambia el estado de cada postulación y
                solicita entrevistas.
              </p>
            </div>
          </Link>

          <Link className="dashboard-cta-card" to="/empleador/candidatos/buscar">
            <div className="dashboard-cta-card__icon">
              <Search aria-hidden="true" size={28} />
            </div>
            <div>
              <h3>Ver candidatos disponibles</h3>
              <p>
                Filtra por habilidad, provincia, nivel educativo y experiencia. Envía sugerencias de
                postulación directamente desde el perfil del candidato.
              </p>
            </div>
          </Link>
        </div>
      </section>
    </main>
  );
}
