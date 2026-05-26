import { RefreshCw, UserRoundCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { getVisibleCandidateProfiles } from '../api/candidatesApi.js';
import { StatusMessage } from '../components/StatusMessage.jsx';

export function PartnerEmployerCandidatesPage() {
  const [candidateProfiles, setCandidateProfiles] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');

  async function loadCandidateProfiles() {
    setIsLoading(true);
    setErrorMessage('');

    try {
      const visibleCandidateProfiles = await getVisibleCandidateProfiles();
      setCandidateProfiles(visibleCandidateProfiles);
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadCandidateProfiles();
  }, []);

  return (
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
  );
}
