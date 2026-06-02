import { LockKeyhole, MailCheck, SendHorizontal, ShieldCheck, UserRoundPlus } from 'lucide-react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { registerCandidate } from '../api/candidatesApi.js';
import { FieldError } from '../../shared/components/FieldError.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { costaRicaProvinces, educationLevels } from '../constants/candidateCatalogs.js';

const initialFormValues = {
  fullName: '',
  dateOfBirth: '',
  province: costaRicaProvinces[0],
  educationLevel: educationLevels[1],
  email: '',
  password: '',
  confirmPassword: '',
};

export function CandidateRegistrationPage() {
  const [formValues, setFormValues] = useState(initialFormValues);
  const [touchedFields, setTouchedFields] = useState({});
  const [submitAttempted, setSubmitAttempted] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [apiErrors, setApiErrors] = useState([]);

  const fieldValidation = useMemo(() => validateCandidateFields(formValues), [formValues]);
  const localValidationErrors = useMemo(
    () => Object.values(fieldValidation).flat(),
    [fieldValidation],
  );
  const canSubmit = localValidationErrors.length === 0 && !isSubmitting;

  function updateField(fieldName, fieldValue) {
    setFormValues((currentFormValues) => ({
      ...currentFormValues,
      [fieldName]: fieldValue,
    }));
  }

  function markFieldAsTouched(fieldName) {
    setTouchedFields((currentTouchedFields) => ({
      ...currentTouchedFields,
      [fieldName]: true,
    }));
  }

  function getFieldErrors(fieldName) {
    const fieldHasValue = Boolean(String(formValues[fieldName] ?? '').trim());
    const shouldShowErrors = submitAttempted || touchedFields[fieldName] || fieldHasValue;
    return shouldShowErrors ? fieldValidation[fieldName] ?? [] : [];
  }

  function getFieldClassName(fieldName) {
    return getFieldErrors(fieldName).length > 0 ? 'input-invalid' : undefined;
  }

  function getFieldSuccess(fieldName, message) {
    const fieldHasValue = Boolean(String(formValues[fieldName] ?? '').trim());
    return fieldHasValue && getFieldErrors(fieldName).length === 0 ? message : undefined;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSubmitAttempted(true);
    setSuccessMessage('');
    setApiErrors([]);

    if (!canSubmit) {
      setApiErrors(localValidationErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      const registrationResponse = await registerCandidate({
        fullName: formValues.fullName,
        dateOfBirth: formValues.dateOfBirth,
        province: formValues.province,
        educationLevel: formValues.educationLevel,
        email: formValues.email,
        password: formValues.password,
      });

      setSuccessMessage(registrationResponse.confirmationMessage);
      setFormValues(initialFormValues);
      setTouchedFields({});
      setSubmitAttempted(false);
    } catch (error) {
      setApiErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="registration-layout">
      <form className="registration-form" noValidate onSubmit={handleSubmit}>
        <div className="registration-header">
          <div className="registration-header__icon">
            <UserRoundPlus aria-hidden="true" size={28} />
          </div>
          <div className="section-heading">
            <p className="eyebrow">Registro de candidato</p>
            <h2>Bienvenido a Sinergia</h2>
            <p className="section-description">
              Completa tu perfil y crea una contraseña segura para ingresar cuando quieras.
            </p>
          </div>
        </div>

        <div className="form-grid">
          <label>
            Nombre completo
            <input
              autoComplete="name"
              className={getFieldClassName('fullName')}
              name="fullName"
              onBlur={() => markFieldAsTouched('fullName')}
              onChange={(event) => updateField('fullName', event.target.value)}
              placeholder="Ej. Maria Fernanda Rojas"
              required
              type="text"
              value={formValues.fullName}
            />
            <FieldHelp errors={getFieldErrors('fullName')} />
          </label>

          <label>
            Fecha de nacimiento
            <input
              className={getFieldClassName('dateOfBirth')}
              max={getDateForAge(18)}
              min={getDateForAge(30)}
              name="dateOfBirth"
              onBlur={() => markFieldAsTouched('dateOfBirth')}
              onChange={(event) => updateField('dateOfBirth', event.target.value)}
              required
              type="date"
              value={formValues.dateOfBirth}
            />
            <FieldHelp errors={getFieldErrors('dateOfBirth')} />
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
            Correo electrónico
            <input
              autoComplete="email"
              className={getFieldClassName('email')}
              name="email"
              onBlur={() => markFieldAsTouched('email')}
              onChange={(event) => updateField('email', event.target.value)}
              placeholder="tu.correo@gmail.com"
              required
              type="email"
              value={formValues.email}
            />
            <FieldHelp
              errors={getFieldErrors('email')}
              successMessage={getFieldSuccess('email', 'Correo con estructura correcta.')}
            />
          </label>
        </div>

        <div className="form-grid">
          <label>
            Contraseña
            <input
              autoComplete="new-password"
              className={getFieldClassName('password')}
              minLength="8"
              name="password"
              onBlur={() => markFieldAsTouched('password')}
              onChange={(event) => updateField('password', event.target.value)}
              placeholder="Mínimo 8 caracteres"
              required
              type="password"
              value={formValues.password}
            />
            <FieldHelp
              errors={getFieldErrors('password')}
              successMessage={getFieldSuccess('password', 'Contraseña con longitud suficiente.')}
            />
          </label>

          <label>
            Confirmar contraseña
            <input
              autoComplete="new-password"
              className={getFieldClassName('confirmPassword')}
              minLength="8"
              name="confirmPassword"
              onBlur={() => markFieldAsTouched('confirmPassword')}
              onChange={(event) => updateField('confirmPassword', event.target.value)}
              placeholder="Repite tu contraseña"
              required
              type="password"
              value={formValues.confirmPassword}
            />
            <FieldHelp
              errors={getFieldErrors('confirmPassword')}
              successMessage={getFieldSuccess('confirmPassword', 'Las contraseñas coinciden.')}
            />
          </label>
        </div>

        <FieldError errors={apiErrors} />
        <StatusMessage message={successMessage} />

        <button className="primary-action" disabled={isSubmitting} type="submit">
          <SendHorizontal aria-hidden="true" size={18} />
          {isSubmitting ? 'Registrando...' : 'Crear perfil'}
        </button>

        <p className="registration-login-link">
          <span>¿Ya tienes cuenta?</span>
          <Link to={AUTH_ROUTES.login}>Inicia sesión</Link>
          <span className="registration-login-link__separator">|</span>
          <Link to={AUTH_ROUTES.recoverPassword}>Restablecer contraseña</Link>
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
            <dd>{formValues.dateOfBirth ? `${calculateAge(formValues.dateOfBirth)} años` : '--'}</dd>
          </div>
          <div>
            <dt>Provincia</dt>
            <dd>{formValues.province}</dd>
          </div>
          <div>
            <dt>Educación</dt>
            <dd>{formValues.educationLevel}</dd>
          </div>
          <div>
            <dt>Correo</dt>
            <dd>{formValues.email || 'pendiente'}</dd>
          </div>
          <div>
            <dt>Acceso</dt>
            <dd className="preview-secure">
              <ShieldCheck aria-hidden="true" size={16} />
              {formValues.password.length >= 8 ? 'Contraseña lista' : 'Crea tu contraseña'}
            </dd>
          </div>
        </dl>
        <div className="preview-security-note">
          <LockKeyhole aria-hidden="true" size={18} />
          <span>Tu contraseña se guarda protegida con hash.</span>
        </div>
      </aside>
    </section>
  );
}

function FieldHelp({ errors, successMessage }) {
  if (errors.length > 0) {
    return <span className="form-hint form-hint--error">{errors.join(' ')}</span>;
  }

  if (successMessage) {
    return <span className="form-hint form-hint--success">{successMessage}</span>;
  }

  return null;
}

function validateCandidateFields(formValues) {
  const missingPasswordCharacters = Math.max(0, 8 - formValues.password.length);
  const candidateAge = calculateAge(formValues.dateOfBirth);

  return {
    fullName: !formValues.fullName.trim() ? ['El nombre es obligatorio.'] : [],
    dateOfBirth: !formValues.dateOfBirth || candidateAge < 18 || candidateAge > 30
      ? ['La fecha de nacimiento debe corresponder a una edad entre 18 y 30 años.']
      : [],
    email: validateEmailStructure(formValues.email),
    password: missingPasswordCharacters > 0
      ? [`Faltan ${missingPasswordCharacters} caracteres para el mínimo de 8.`]
      : [],
    confirmPassword: !formValues.confirmPassword
      ? ['Confirma tu contraseña.']
      : formValues.password !== formValues.confirmPassword
        ? ['Las contraseñas deben coincidir.']
        : [],
  };
}

function validateEmailStructure(email) {
  const validationErrors = [];
  const trimmedEmail = email.trim();
  const atSymbolCount = (trimmedEmail.match(/@/g) ?? []).length;
  const [localPart = '', domainPart = ''] = trimmedEmail.split('@');

  if (!trimmedEmail) {
    validationErrors.push('El correo electrónico es obligatorio.');
    return validationErrors;
  }

  if (!localPart) {
    validationErrors.push('Falta el nombre antes del @.');
  }

  if (atSymbolCount === 0) {
    validationErrors.push('Falta el @.');
  }

  if (atSymbolCount > 1) {
    validationErrors.push('Solo debe tener un @.');
  }

  if (atSymbolCount === 1 && !domainPart) {
    validationErrors.push('Falta el dominio después del @, por ejemplo gmail.');
  }

  if (domainPart && !domainPart.includes('.')) {
    validationErrors.push('Falta el punto del dominio.');
  }

  if (domainPart && domainPart.includes('.') && !/^[^\s@]+\.[^\s@]{2,}$/.test(domainPart)) {
    validationErrors.push('El dominio debe terminar con una extensión válida, por ejemplo .com, .net o .ac.cr.');
  }

  return validationErrors;
}

function calculateAge(dateOfBirth) {
  if (!dateOfBirth) {
    return 0;
  }

  const birthDate = new Date(`${dateOfBirth}T00:00:00`);
  const today = new Date();
  let age = today.getFullYear() - birthDate.getFullYear();
  const birthdayThisYear = new Date(today.getFullYear(), birthDate.getMonth(), birthDate.getDate());

  if (today < birthdayThisYear) {
    age -= 1;
  }

  return age;
}

function getDateForAge(age) {
  const date = new Date();
  date.setFullYear(date.getFullYear() - age);
  return date.toISOString().slice(0, 10);
}
