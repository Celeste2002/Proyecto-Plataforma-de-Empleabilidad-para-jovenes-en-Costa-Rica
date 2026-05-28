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
      localErrors.push('La contrasena debe tener al menos 8 caracteres.');
    }
    if (newPassword !== confirmPassword) {
      localErrors.push('Las contrasenas no coinciden.');
    }
    if (localErrors.length > 0) {
      setErrors(localErrors);
      return;
    }

    setIsSubmitting(true);

    try {
      await resetPassword(token, newPassword);
      setSuccessMessage('Contrasena restablecida. Puedes iniciar sesion ahora.');
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
          <StatusMessage message="El enlace de recuperacion no es valido." tone="error" />
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
          <p className="eyebrow">Nueva contrasena</p>
          <h2>Restablecer contrasena</h2>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="auth-fields">
            <label>
              Nueva contrasena
              <input
                autoComplete="new-password"
                minLength={8}
                name="newPassword"
                onChange={(e) => setNewPassword(e.target.value)}
                placeholder="Minimo 8 caracteres"
                required
                type="password"
                value={newPassword}
              />
            </label>
            <label>
              Confirmar contrasena
              <input
                autoComplete="new-password"
                name="confirmPassword"
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="Repite la contrasena"
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
            {isSubmitting ? 'Guardando...' : 'Guardar contrasena'}
          </button>
        </form>

        <div className="auth-footer">
          <p><Link to="/login">Volver al inicio de sesion</Link></p>
        </div>
      </div>
    </div>
  );
}
