import { useState } from 'react';
import { ArrowLeft, BriefcaseBusiness, Send } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
import { createVacante } from '../api/employerApi.js';
import { costaRicaProvinces, employerSectors, experienceLevels, vacanteModalities } from '../constants/vacanteCatalogs.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const emptyForm = {
  jobTitle: '',
  province: '',
  sector: '',
  modality: '',
  experienceLevel: '',
  description: '',
  requirements: '',
  salaryRange: '',
};

export function PublicarVacantePage() {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState(emptyForm);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [validationErrors, setValidationErrors] = useState([]);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setIsSubmitting(true);
    setErrorMessage('');
    setValidationErrors([]);

    try {
      await createVacante(token, form);
      navigate('/empleador/vacantes', { state: { created: true } });
    } catch (error) {
      if (error.validationErrors?.length > 0) {
        setValidationErrors(error.validationErrors);
      } else {
        setErrorMessage(error.message);
      }
    } finally {
      setIsSubmitting(false);
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

      <section className="dashboard-layout">
        <div className="dashboard-welcome">
          <div className="dashboard-avatar">
            <BriefcaseBusiness aria-hidden="true" size={40} />
          </div>
          <div>
            <p className="eyebrow">Gestión de vacantes</p>
            <h2>Publicar oferta de trabajo</h2>
            <p className="section-description">
              Completa los datos de la vacante para que los candidatos puedan postularse.
            </p>
          </div>
        </div>

        <form className="profile-form" onSubmit={handleSubmit} noValidate>
          <StatusMessage message={errorMessage} tone="error" />

          {validationErrors.length > 0 && (
            <ul className="field-error" role="alert">
              {validationErrors.map((err) => (
                <li key={err}>{err}</li>
              ))}
            </ul>
          )}

          <fieldset>
            <legend>Información del puesto</legend>

            <label>
              Título del puesto *
              <input
                maxLength={100}
                name="jobTitle"
                onChange={handleChange}
                placeholder="Ej. Desarrollador Frontend"
                required
                type="text"
                value={form.jobTitle}
              />
            </label>

            <label>
              Provincia *
              <select name="province" onChange={handleChange} required value={form.province}>
                <option value="">Selecciona una provincia</option>
                {costaRicaProvinces.map((p) => (
                  <option key={p} value={p}>{p}</option>
                ))}
              </select>
            </label>

            <label>
              Sector *
              <select name="sector" onChange={handleChange} required value={form.sector}>
                <option value="">Selecciona un sector</option>
                {employerSectors.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </label>

            <label>
              Modalidad *
              <select name="modality" onChange={handleChange} required value={form.modality}>
                <option value="">Selecciona una modalidad</option>
                {vacanteModalities.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </label>

            <label>
              Nivel de experiencia requerido *
              <select name="experienceLevel" onChange={handleChange} required value={form.experienceLevel}>
                <option value="">Selecciona un nivel</option>
                {experienceLevels.map((l) => (
                  <option key={l} value={l}>{l}</option>
                ))}
              </select>
            </label>

            <label>
              Rango salarial
              <input
                maxLength={100}
                name="salaryRange"
                onChange={handleChange}
                placeholder="Ej. ₡500,000 – ₡700,000 mensuales"
                type="text"
                value={form.salaryRange}
              />
            </label>
          </fieldset>

          <fieldset>
            <legend>Descripción y requisitos</legend>

            <label>
              Descripción del puesto
              <textarea
                name="description"
                onChange={handleChange}
                placeholder="Describe las responsabilidades y el entorno de trabajo..."
                rows={4}
                value={form.description}
              />
            </label>

            <label>
              Requisitos del candidato
              <textarea
                name="requirements"
                onChange={handleChange}
                placeholder="Lista los conocimientos, habilidades y cualificaciones necesarias..."
                rows={4}
                value={form.requirements}
              />
            </label>
          </fieldset>

          <div className="form-actions">
            <button className="primary-action" disabled={isSubmitting} type="submit">
              <Send aria-hidden="true" size={16} />
              {isSubmitting ? 'Publicando...' : 'Publicar vacante'}
            </button>
            <Link className="secondary-action" to="/empleador/vacantes">
              Cancelar
            </Link>
          </div>
        </form>
      </section>
    </main>
  );
}
