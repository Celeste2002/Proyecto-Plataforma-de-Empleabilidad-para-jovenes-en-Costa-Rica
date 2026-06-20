import { BookOpenCheck, BriefcaseBusiness, ClipboardList, KeyRound, LogOut, UserRound, UserCircle } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getMyCandidateProfile } from '../api/candidatesApi.js';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function CandidateDashboardPage() {
  const { user, token, logout } = useAuth();
  const navigate = useNavigate();
  const [candidateName, setCandidateName] = useState('');

  useEffect(() => {
    getMyCandidateProfile(token)
      .then((profile) => setCandidateName(profile.fullName))
      .catch(() => {});
  }, [token]);

  function handleLogout() {
    logout();
    navigate('/login', { replace: true });
  }

  const displayName = candidateName || user?.email || '';

  return (
    <main className="application-shell">
      <header className="top-bar">
        <BrandHomeLink subtitle="Panel del candidato" to="/candidato" />
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
            <h2>{displayName}</h2>
            <p className="section-description">{user?.email}</p>
          </div>
        </div>

        <div className="dashboard-cards">
          <Link className="dashboard-card dashboard-card-link" to="/candidato/mi-perfil">
            <div className="dashboard-card-icon">
              <UserCircle aria-hidden="true" size={28} />
            </div>
            <h3>Mi perfil público</h3>
            <p>Agrega experiencia laboral, habilidades y cursos para destacar ante los empleadores.</p>
          </Link>

          <Link className="dashboard-card dashboard-card-link" to="/candidato/vacantes">
            <div className="dashboard-card-icon">
              <BriefcaseBusiness aria-hidden="true" size={28} />
            </div>
            <h3>Buscar vacantes</h3>
            <p>Explora ofertas de empleo y filtra por provincia, sector, modalidad y experiencia.</p>
          </Link>

          <Link className="dashboard-card dashboard-card-link" to="/candidato/postulaciones">
            <div className="dashboard-card-icon">
              <ClipboardList aria-hidden="true" size={28} />
            </div>
            <h3>Mis postulaciones</h3>
            <p>Consulta el estado de todas tus postulaciones: enviada, en revisión, entrevista o finalizada.</p>
          </Link>

          <Link className="dashboard-card dashboard-card-link" to="/candidato/microcursos">
            <div className="dashboard-card-icon">
              <BookOpenCheck aria-hidden="true" size={28} />
            </div>
            <h3>Microcursos</h3>
            <p>Explora cursos validados y revisa recomendaciones conectadas con tus habilidades.</p>
          </Link>
        </div>
      </section>
    </main>
  );
}
