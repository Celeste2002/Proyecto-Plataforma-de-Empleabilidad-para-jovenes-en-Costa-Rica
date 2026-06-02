import { BriefcaseBusiness, KeyRound, LogOut, UserRound } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function CandidateDashboardPage() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

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
        <nav className="dashboard-nav" aria-label="Navegación del candidato">
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

      <section className="dashboard-layout">
        <div className="dashboard-welcome">
          <div className="dashboard-avatar">
            <UserRound aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Mi cuenta</p>
            <h2>Panel del candidato</h2>
            <p className="section-description">{user?.email}</p>
          </div>
        </div>

        <div className="dashboard-cards">
          <article className="dashboard-card">
            <div className="dashboard-card-icon">
              <BriefcaseBusiness aria-hidden="true" size={28} />
            </div>
            <h3>Mi perfil</h3>
            <p>Tu perfil es visible para los empleadores aliados de la plataforma.</p>
          </article>

          <article className="dashboard-card dashboard-card--info">
            <h3>Próximamente</h3>
            <p>Ofertas de empleo, postulaciones y más funcionalidades estarán disponibles pronto.</p>
          </article>
        </div>

        <div className="dashboard-actions">
          <Link className="secondary-action" to="/candidato/actualizar-registro">
            Actualizar registro
          </Link>
        </div>
      </section>
    </main>
  );
}
