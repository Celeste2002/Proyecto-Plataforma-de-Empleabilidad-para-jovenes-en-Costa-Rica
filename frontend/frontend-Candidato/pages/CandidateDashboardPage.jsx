import { BriefcaseBusiness, ClipboardList, KeyRound, LogOut, MessageSquare, UserRound } from 'lucide-react';
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
            <h3>Buscar vacantes</h3>
            <p>Explora ofertas de empleo y filtra por provincia, sector, modalidad y experiencia.</p>
          </article>

          <article className="dashboard-card">
            <div className="dashboard-card-icon">
              <ClipboardList aria-hidden="true" size={28} />
            </div>
            <h3>Mis postulaciones</h3>
            <p>Consulta el estado de todas tus postulaciones: enviada, en revisión, entrevista o finalizada.</p>
          </article>

          <article className="dashboard-card">
            <div className="dashboard-card-icon">
              <MessageSquare aria-hidden="true" size={28} />
            </div>
            <h3>Mis mensajes</h3>
            <p>Lee los mensajes que los empleadores te han enviado sobre tus postulaciones.</p>
          </article>
        </div>

        <div className="dashboard-actions">
          <Link className="secondary-action" to="/candidato/vacantes">
            <BriefcaseBusiness aria-hidden="true" size={16} />
            Ver vacantes
          </Link>
          <Link className="secondary-action" to="/candidato/postulaciones">
            <ClipboardList aria-hidden="true" size={16} />
            Mis postulaciones
          </Link>
          <Link className="secondary-action" to="/candidato/mensajes">
            <MessageSquare aria-hidden="true" size={16} />
            Mis mensajes
          </Link>
          <Link className="secondary-action" to="/candidato/actualizar-registro">
            Actualizar registro
          </Link>
        </div>
      </section>
    </main>
  );
}
