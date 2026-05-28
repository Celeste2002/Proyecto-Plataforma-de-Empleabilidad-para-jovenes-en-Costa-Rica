import { MailCheck, SendHorizontal } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { registerCandidate } from '../api/candidatesApi.js';
import { FieldError } from '../../shared/components/FieldError.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { costaRicaProvinces, educationLevels } from '../constants/candidateCatalogs.js';

const initialFormValues = {
  fullName: '',
  age: '',
  province: costaRicaProvinces[0],
  educationLevel: educationLevels[1],
  email: '',
};

export function CandidateRegistrationPage() {
  const [formValues, setFormValues] = useState(initialFormValues);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [apiErrors, setApiErrors] = useState([]);

  const localValidationErrors = useMemo(() => validateCandidateForm(formValues), [formValues]);
  const canSubmit = localValidationErrors.length === 0 && !isSubmitting;

  function updateField(fieldName, fieldValue) {
    setFormValues((currentFormValues) => ({
      ...currentFormValues,
      [fieldName]: fieldValue,
    }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSuccessMessage('');
    setApiErrors([]);

    if (!canSubmit) {
      setApiErrors(localValidationErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      const registrationResponse = await registerCandidate({
        ...formValues,
        age: Number(formValues.age),
      });

      setSuccessMessage(registrationResponse.confirmationMessage);
      setFormValues(initialFormValues);
    } catch (error) {
      setApiErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="registration-layout">
      <form className="registration-form" onSubmit={handleSubmit}>
        <div className="section-heading">
          <p className="eyebrow">Registro de candidato</p>
          <h2>Bienvenido a Sinergia</h2>
          <p className="section-description">
            Completa tu registro para crear un perfil y postularte a empleos formales.
          </p>
        </div>

        <div className="form-grid">
          <label>
            Nombre completo
            <input
              autoComplete="name"
              name="fullName"
              onChange={(event) => updateField('fullName', event.target.value)}
              placeholder="Ej. Maria Fernanda Rojas"
              required
              type="text"
              value={formValues.fullName}
            />
          </label>

          <label>
            Edad
            <input
              inputMode="numeric"
              max="30"
              min="18"
              name="age"
              onChange={(event) => updateField('age', event.target.value)}
              placeholder="18-30"
              required
              type="number"
              value={formValues.age}
            />
          </label>

          <label>
            Provincia
            <select
              name="province"
              onChange={(event) => updateField('province', event.target.value)}
              value={formValues.province}
            >
              {costaRicaProvinces.map((province) => (
                <option key={province} value={province}>
                  {province}
                </option>
              ))}
            </select>
          </label>

          <label>
            Nivel educativo
            <select
              name="educationLevel"
              onChange={(event) => updateField('educationLevel', event.target.value)}
              value={formValues.educationLevel}
            >
              {educationLevels.map((educationLevel) => (
                <option key={educationLevel} value={educationLevel}>
                  {educationLevel}
                </option>
              ))}
            </select>
          </label>

          <label className="wide-field">
            Correo electronico
            <input
              autoComplete="email"
              name="email"
              onChange={(event) => updateField('email', event.target.value)}
              placeholder="tu.correo@ejemplo.com"
              required
              type="email"
              value={formValues.email}
            />
          </label>
        </div>

        <FieldError errors={apiErrors} />
        <StatusMessage message={successMessage} />

        <button className="primary-action" disabled={!canSubmit} type="submit">
          <SendHorizontal aria-hidden="true" size={18} />
          {isSubmitting ? 'Registrando...' : 'Crear perfil'}
        </button>

        <p className="registration-login-link">
          ¿Ya tienes cuenta? <Link to="/login">Inicia sesion</Link>
        </p>
      </form>

      <aside className="profile-preview" aria-label="Resumen del perfil">
        <div className="preview-icon">
          <MailCheck aria-hidden="true" size={32} />
        </div>
        <p className="eyebrow">Perfil visible</p>
        <h2>{formValues.fullName || 'Tu nombre aparecera aqui'}</h2>
        <dl>
          <div>
            <dt>Edad</dt>
            <dd>{formValues.age || '--'} años</dd>
          </div>
          <div>
            <dt>Provincia</dt>
            <dd>{formValues.province}</dd>
          </div>
          <div>
            <dt>Educacion</dt>
            <dd>{formValues.educationLevel}</dd>
          </div>
          <div>
            <dt>Correo</dt>
            <dd>{formValues.email || 'pendiente'}</dd>
          </div>
        </dl>
      </aside>
    </section>
  );
}

function validateCandidateForm(formValues) {
  const validationErrors = [];
  const candidateAge = Number(formValues.age);

  if (!formValues.fullName.trim()) {
    validationErrors.push('El nombre es obligatorio.');
  }

  if (!Number.isInteger(candidateAge) || candidateAge < 18 || candidateAge > 30) {
    validationErrors.push('La edad debe estar entre 18 y 30 años.');
  }

  if (!formValues.email.includes('@')) {
    validationErrors.push('El correo electronico debe tener un formato valido.');
  }

  return validationErrors;
}
