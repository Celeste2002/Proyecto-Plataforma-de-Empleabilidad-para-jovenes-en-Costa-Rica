import { BriefcaseBusiness, UsersRound } from 'lucide-react';
import { CandidateRegistrationPage } from './pages/CandidateRegistrationPage.jsx';
import { PartnerEmployerCandidatesPage } from './pages/PartnerEmployerCandidatesPage.jsx';
import { useState } from 'react';

const applicationViews = {
  registration: 'registration',
  employers: 'employers',
};

export function App() {
  const [selectedView, setSelectedView] = useState(applicationViews.registration);

  return (
    <main className="application-shell">
      <header className="top-bar">
        <div className="brand-lockup">
          <img
            alt="Sinergia"
            className="brand-logo"
            onError={(event) => {
              event.currentTarget.style.display = 'none';
            }}
            src="/Logo_Sinergia.png"
          />
          <div>
            <h1>Sinergia</h1>
          </div>
        </div>
        <nav className="segmented-control" aria-label="Vistas principales">
          <button
            className={selectedView === applicationViews.registration ? 'active' : ''}
            onClick={() => setSelectedView(applicationViews.registration)}
            type="button"
            title="Registro de candidato"
          >
            <BriefcaseBusiness aria-hidden="true" size={18} />
            Registro
          </button>
          <button
            className={selectedView === applicationViews.employers ? 'active' : ''}
            onClick={() => setSelectedView(applicationViews.employers)}
            type="button"
            title="Candidatos visibles para empleadores"
          >
            <UsersRound aria-hidden="true" size={18} />
            Empleadores
          </button>
        </nav>
      </header>

      {selectedView === applicationViews.registration ? (
        <CandidateRegistrationPage onCandidateRegistered={() => setSelectedView(applicationViews.employers)} />
      ) : (
        <PartnerEmployerCandidatesPage />
      )}
    </main>
  );
}
