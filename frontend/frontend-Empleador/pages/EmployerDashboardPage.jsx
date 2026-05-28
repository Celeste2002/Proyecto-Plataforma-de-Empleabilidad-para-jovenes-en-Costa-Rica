import { LogOut, RefreshCw, UserRoundCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getVisibleCandidateProfiles } from '../api/employerApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function EmployerDashboardPage() {
  const { user, logout, token } = useAuth();
  const navigate = useNavigate();

  const [candidateProfiles, setCandidateProfiles] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  async function loadCandidateProfiles() {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const profiles = await getVisibleCandidateProfiles(token);
      setCandidateProfiles(profiles);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadCandidateProfiles();
  }, []);

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
        <nav className="dashboard-nav" aria-label="Navegacion del empleador">
          <span className="dashboard-user-email">{user?.email}</span>
          <button className="secondary-action" onClick={handleLogout} type="button">
            <LogOut aria-hidden="true" size={16} />
            Cerrar sesion
          </button>
        </nav>
      </header>

      <section className="employer-view">
        <div className="section-heading horizontal-heading">
          <div>
            <p className="eyebrow">Empleadores aliados</p>
            <h2>Candidatos visibles</h2>
          </div>
          <button className="secondary-action" onClick={loadCandidateProfiles} type="button">
            <RefreshCw aria-hidden="true" size={18} />
            Actualizar
          </button>
        </div>

        <StatusMessage message={errorMessage} tone="error" />

        {isLoading ? (
          <p className="empty-state">Cargando perfiles...</p>
        ) : candidateProfiles.length === 0 ? (
          <p className="empty-state">Aun no hay perfiles registrados.</p>
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
                    <dt>Confirmacion</dt>
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
