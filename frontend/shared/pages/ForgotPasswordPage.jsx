import { MailCheck } from 'lucide-react';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { forgotPassword } from '../api/authApi.js';
import { StatusMessage } from '../components/StatusMessage.jsx';

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  async function handleSubmit(event) {
    event.preventDefault();
    setSuccessMessage('');
    setErrorMessage('');
    setIsSubmitting(true);

    try {
      const response = await forgotPassword(email);
      setSuccessMessage(response.message);
      setEmail('');
    } catch (error) {
      setErrorMessage(error.message);
    } finally {
      setIsSubmitting(false);
    }
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
          <p className="eyebrow">Recuperar acceso</p>
          <h2>Olvidé mi contraseña</h2>
          <p className="section-description">
            Ingresa tu correo registrado y te enviaremos un enlace para restablecer tu contraseña.
          </p>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="auth-fields">
            <label>
              Correo electrónico
              <input
                autoComplete="email"
                name="email"
                onChange={(e) => setEmail(e.target.value)}
                placeholder="tu.correo@ejemplo.com"
                required
                type="email"
                value={email}
              />
            </label>
          </div>

          <StatusMessage message={successMessage} tone="success" />
          <StatusMessage message={errorMessage} tone="error" />

          <button className="primary-action" disabled={isSubmitting || !!successMessage} type="submit">
            <MailCheck aria-hidden="true" size={18} />
            {isSubmitting ? 'Enviando...' : 'Enviar enlace'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            <Link to="/login">Volver al inicio de sesión</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
