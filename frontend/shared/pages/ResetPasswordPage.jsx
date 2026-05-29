import { KeyRound } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { resetPassword } from '../api/authApi.js';
import { FieldError } from '../components/FieldError.jsx';
import { StatusMessage } from '../components/StatusMessage.jsx';

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
      setTimeout(() => navigate('/login'), 2500);
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSubmitting(false);
    }
  }

  if (!token) {
    return (
      <div className="auth-page">
        <div className="auth-card">
          <StatusMessage message="El enlace de recuperación no es válido." tone="error" />
          <div className="auth-footer">
            <p><Link to="/recuperar-contrasena">Solicitar un nuevo enlace</Link></p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <div className="auth-brand">
          <img
            alt="Sinergia"
            className="brand-logo"
            onError={(e) => { e.currentTarget.style.display = 'none'; }}
            src="/Logo_Sinergia.png"
          />
          <h1>Sinergia</h1>
        </div>

        <div className="section-heading">
          <p className="eyebrow">Nueva contraseña</p>
          <h2>Restablecer contraseña</h2>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="auth-fields">
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
          <p><Link to="/login">Volver al inicio de sesión</Link></p>
        </div>
      </div>
    </div>
  );
}
