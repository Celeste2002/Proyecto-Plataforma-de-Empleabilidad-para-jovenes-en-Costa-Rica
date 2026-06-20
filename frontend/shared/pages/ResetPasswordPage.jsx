import { KeyRound, ShieldCheck } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { resetPassword } from '../api/authApi.js';
import { BrandHomeLink } from '../components/BrandHomeLink.jsx';
import { FieldError } from '../components/FieldError.jsx';
import { StatusMessage } from '../components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../constants/authRoutes.js';

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token') ?? '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [errors, setErrors] = useState([]);

  async function handleSubmit(event) {
    event.preventDefault();
    setErrors([]);
    setSuccessMessage('');

    const localErrors = [];
    if (newPassword.length < 8) {
      localErrors.push('La contraseña debe tener al menos 8 caracteres.');
    }
    if (newPassword !== confirmPassword) {
      localErrors.push('Las contraseñas no coinciden.');
    }
    if (localErrors.length > 0) {
      setErrors(localErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      await resetPassword(token, newPassword);
      setSuccessMessage('Contraseña restablecida. Puedes iniciar sesión ahora.');
      setTimeout(() => navigate(AUTH_ROUTES.login), 2500);
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSubmitting(false);
    }
  }

  if (!token) {
    return (
      <div className="auth-page">
        <div className="auth-card auth-card--recovery">
          <StatusMessage message="El enlace de recuperación no es válido." tone="error" />
          <div className="auth-footer">
            <p><Link to={AUTH_ROUTES.recoverPassword}>Solicitar un nuevo enlace</Link></p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-card auth-card--recovery">
        <BrandHomeLink className="auth-brand auth-brand--spacious" to={AUTH_ROUTES.login} />

        <div className="section-heading auth-heading">
          <p className="eyebrow">Nueva contraseña</p>
          <h2>Restablecer contraseña</h2>
          <p className="section-description">
            Crea una contraseña nueva para volver a ingresar a tu cuenta de Sinergia.
          </p>
        </div>

        <form className="auth-form auth-form--spacious" noValidate onSubmit={handleSubmit}>
          <div className="auth-helper">
            <span className="auth-helper__icon" aria-hidden="true">
              <ShieldCheck size={20} />
            </span>
            <p>
              Usa al menos 8 caracteres. Cuando guardes el cambio, podrás iniciar sesión con tu nueva contraseña.
            </p>
          </div>

          <div className="auth-fields auth-fields--spacious">
            <label>
              Nueva contraseña
              <input
                autoComplete="new-password"
                minLength={8}
                name="newPassword"
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Mínimo 8 caracteres"
                required
                type="password"
                value={newPassword}
              />
            </label>
            <label>
              Confirmar contraseña
              <input
                autoComplete="new-password"
                name="confirmPassword"
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Repite la contraseña"
                required
                type="password"
                value={confirmPassword}
              />
            </label>
          </div>

          <FieldError errors={errors} />
          <StatusMessage message={successMessage} tone="success" />

          <button className="primary-action" disabled={isSubmitting || !!successMessage} type="submit">
            <KeyRound aria-hidden="true" size={18} />
            {isSubmitting ? 'Guardando...' : 'Guardar contraseña'}
          </button>
        </form>

        <div className="auth-footer">
          <p><Link to={AUTH_ROUTES.login}>Volver al inicio de sesión</Link></p>
        </div>
      </div>
    </div>
  );
}
