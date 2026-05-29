import { ArrowLeft, Save, UserRoundCog } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  getMyCandidateProfile,
  updateMyCandidatePassword,
  updateMyCandidateProfile,
} from '../api/candidatesApi.js';
import { costaRicaProvinces, educationLevels } from '../constants/candidateCatalogs.js';
import { FieldError } from '../../shared/components/FieldError.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const emptyProfileValues = {
  fullName: '',
  dateOfBirth: '',
  province: costaRicaProvinces[0],
  educationLevel: educationLevels[1],
};

const emptyPasswordValues = {
  currentPassword: '',
  newPassword: '',
  confirmNewPassword: '',
};

export function CandidateProfileUpdatePage() {
  const { token } = useAuth();
  const [formValues, setFormValues] = useState(emptyProfileValues);
  const [touchedFields, setTouchedFields] = useState({});
  const [submitAttempted, setSubmitAttempted] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [passwordValues, setPasswordValues] = useState(emptyPasswordValues);
  const [passwordTouchedFields, setPasswordTouchedFields] = useState({});
  const [passwordSubmitAttempted, setPasswordSubmitAttempted] = useState(false);
  const [isPasswordSubmitting, setIsPasswordSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [passwordSuccessMessage, setPasswordSuccessMessage] = useState('');
  const [errors, setErrors] = useState([]);
  const [passwordErrors, setPasswordErrors] = useState([]);

  const fieldValidation = useMemo(() => validateProfileFields(formValues), [formValues]);
  const localValidationErrors = useMemo(
    () => Object.values(fieldValidation).flat(),
    [fieldValidation],
  );
  const passwordValidation = useMemo(() => validatePasswordFields(passwordValues), [passwordValues]);
  const localPasswordValidationErrors = useMemo(
    () => Object.values(passwordValidation).flat(),
    [passwordValidation],
  );

  useEffect(() => {
    async function loadProfile() {
      setIsLoading(true);
      setErrors([]);

      try {
        const profile = await getMyCandidateProfile(token);
        setFormValues({
          fullName: profile.fullName ?? '',
          dateOfBirth: profile.dateOfBirth ?? '',
          province: profile.province ?? costaRicaProvinces[0],
          educationLevel: profile.educationLevel ?? educationLevels[1],
        });
      } catch (error) {
        setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
      } finally {
        setIsLoading(false);
      }
    }

    loadProfile();
  }, [token]);

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

  function updatePasswordField(fieldName, fieldValue) {
    setPasswordValues((currentPasswordValues) => ({
      ...currentPasswordValues,
      [fieldName]: fieldValue,
    }));
  }

  function markPasswordFieldAsTouched(fieldName) {
    setPasswordTouchedFields((currentTouchedFields) => ({
      ...currentTouchedFields,
      [fieldName]: true,
    }));
  }

  function getPasswordFieldErrors(fieldName) {
    const fieldHasValue = Boolean(String(passwordValues[fieldName] ?? '').trim());
    const shouldShowErrors =
      passwordSubmitAttempted || passwordTouchedFields[fieldName] || fieldHasValue;
    return shouldShowErrors ? passwordValidation[fieldName] ?? [] : [];
  }

  function getPasswordFieldClassName(fieldName) {
    return getPasswordFieldErrors(fieldName).length > 0 ? 'input-invalid' : undefined;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setSubmitAttempted(true);
    setSuccessMessage('');
    setErrors([]);

    if (localValidationErrors.length > 0) {
      setErrors(localValidationErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      await updateMyCandidateProfile(token, formValues);
      setSuccessMessage('Registro actualizado correctamente.');
      setTouchedFields({});
      setSubmitAttempted(false);
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handlePasswordSubmit(event) {
    event.preventDefault();
    setPasswordSubmitAttempted(true);
    setPasswordSuccessMessage('');
    setPasswordErrors([]);

    if (localPasswordValidationErrors.length > 0) {
      setPasswordErrors(localPasswordValidationErrors);
      return;
    }

    setIsPasswordSubmitting(true);

    try {
      const response = await updateMyCandidatePassword(token, {
        currentPassword: passwordValues.currentPassword,
        newPassword: passwordValues.newPassword,
      });
      setPasswordSuccessMessage(response.message ?? 'Contraseña actualizada correctamente.');
      setPasswordValues(emptyPasswordValues);
      setPasswordTouchedFields({});
      setPasswordSubmitAttempted(false);
    } catch (error) {
      setPasswordErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsPasswordSubmitting(false);
    }
  }

  return (
    <main className="application-shell">
      <header className="top-bar">
        <div className="brand-lockup">
          <img
            alt="Sinergia"
            className="brand-logo"
            onError={(event) => { event.currentTarget.style.display = 'none'; }}
            src="/Logo_Sinergia.png"
          />
          <div>
            <h1>Sinergia</h1>
          </div>
        </div>
        <Link className="secondary-action" to="/candidato">
          <ArrowLeft aria-hidden="true" size={16} />
          Volver
        </Link>
      </header>

      <section className="employer-view">
        <div className="section-heading">
          <p className="eyebrow">Mi perfil</p>
          <h2>Actualizar registro</h2>
          <p className="section-description">
            Mantén tus datos al día. La edad se calcula automáticamente desde tu fecha de nacimiento.
          </p>
        </div>

        {isLoading ? (
          <p className="empty-state">Cargando perfil...</p>
        ) : (
          <CandidateProfileUpdateForm
            errors={errors}
            formValues={formValues}
            getFieldClassName={getFieldClassName}
            getFieldErrors={getFieldErrors}
            isSubmitting={isSubmitting}
            markFieldAsTouched={markFieldAsTouched}
            onSubmit={handleSubmit}
            successMessage={successMessage}
            updateField={updateField}
          />
        )}
      </section>

      <section className="employer-view profile-security-section">
        <div className="section-heading">
          <p className="eyebrow">Seguridad</p>
          <h2>Cambiar contraseña</h2>
          <p className="section-description">
            Ingresa tu contraseña actual para confirmar el cambio.
          </p>
        </div>

        <CandidatePasswordUpdateForm
          errors={passwordErrors}
          formValues={passwordValues}
          getFieldClassName={getPasswordFieldClassName}
          getFieldErrors={getPasswordFieldErrors}
          isSubmitting={isPasswordSubmitting}
          markFieldAsTouched={markPasswordFieldAsTouched}
          onSubmit={handlePasswordSubmit}
          successMessage={passwordSuccessMessage}
          updateField={updatePasswordField}
        />
      </section>
    </main>
  );
}

function CandidateProfileUpdateForm({
  errors,
  formValues,
  getFieldClassName,
  getFieldErrors,
  isSubmitting,
  markFieldAsTouched,
  onSubmit,
  successMessage,
  updateField,
}) {
  return (
    <form className="profile-update-form" noValidate onSubmit={onSubmit}>
      <div className="dashboard-card-icon">
        <UserRoundCog aria-hidden="true" size={28} />
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
      </div>

      <FieldError errors={errors} />
      <StatusMessage message={successMessage} />

      <button className="primary-action" disabled={isSubmitting} type="submit">
        <Save aria-hidden="true" size={18} />
        {isSubmitting ? 'Guardando...' : 'Guardar cambios'}
      </button>
    </form>
  );
}

function CandidatePasswordUpdateForm({
  errors,
  formValues,
  getFieldClassName,
  getFieldErrors,
  isSubmitting,
  markFieldAsTouched,
  onSubmit,
  successMessage,
  updateField,
}) {
  return (
    <form className="profile-update-form" noValidate onSubmit={onSubmit}>
      <div className="form-grid">
        <label className="wide-field">
          Contraseña actual
          <input
            autoComplete="current-password"
            className={getFieldClassName('currentPassword')}
            name="currentPassword"
            onBlur={() => markFieldAsTouched('currentPassword')}
            onChange={(event) => updateField('currentPassword', event.target.value)}
            placeholder="Tu contraseña actual"
            required
            type="password"
            value={formValues.currentPassword}
          />
          <FieldHelp errors={getFieldErrors('currentPassword')} />
        </label>

        <label>
          Nueva contraseña
          <input
            autoComplete="new-password"
            className={getFieldClassName('newPassword')}
            minLength="8"
            name="newPassword"
            onBlur={() => markFieldAsTouched('newPassword')}
            onChange={(event) => updateField('newPassword', event.target.value)}
            placeholder="Mínimo 8 caracteres"
            required
            type="password"
            value={formValues.newPassword}
          />
          <FieldHelp errors={getFieldErrors('newPassword')} />
        </label>

        <label>
          Confirmar nueva contraseña
          <input
            autoComplete="new-password"
            className={getFieldClassName('confirmNewPassword')}
            minLength="8"
            name="confirmNewPassword"
            onBlur={() => markFieldAsTouched('confirmNewPassword')}
            onChange={(event) => updateField('confirmNewPassword', event.target.value)}
            placeholder="Repite la nueva contraseña"
            required
            type="password"
            value={formValues.confirmNewPassword}
          />
          <FieldHelp errors={getFieldErrors('confirmNewPassword')} />
        </label>
      </div>

      <FieldError errors={errors} />
      <StatusMessage message={successMessage} />

      <button className="primary-action" disabled={isSubmitting} type="submit">
        <Save aria-hidden="true" size={18} />
        {isSubmitting ? 'Actualizando...' : 'Cambiar contraseña'}
      </button>
    </form>
  );
}

function FieldHelp({ errors }) {
  if (errors.length === 0) {
    return null;
  }

  return <span className="form-hint form-hint--error">{errors.join(' ')}</span>;
}

function validateProfileFields(formValues) {
  const candidateAge = calculateAge(formValues.dateOfBirth);

  return {
    fullName: !formValues.fullName.trim() ? ['El nombre es obligatorio.'] : [],
    dateOfBirth: !formValues.dateOfBirth || candidateAge < 18 || candidateAge > 30
      ? ['La fecha de nacimiento debe corresponder a una edad entre 18 y 30 años.']
      : [],
  };
}

function validatePasswordFields(formValues) {
  const missingPasswordCharacters = Math.max(0, 8 - formValues.newPassword.length);

  return {
    currentPassword: !formValues.currentPassword ? ['La contraseña actual es obligatoria.'] : [],
    newPassword: missingPasswordCharacters > 0
      ? [`Faltan ${missingPasswordCharacters} caracteres para el mínimo de 8.`]
      : [],
    confirmNewPassword: !formValues.confirmNewPassword
      ? ['Confirma la nueva contraseña.']
      : formValues.newPassword !== formValues.confirmNewPassword
        ? ['Las contraseñas deben coincidir.']
        : [],
  };
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
