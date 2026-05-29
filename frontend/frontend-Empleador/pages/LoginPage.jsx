import { LogIn } from 'lucide-react';
import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { login } from '../../shared/api/authApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function LoginPage() {
  const { login: saveSession } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState(
    location.state?.denied ? 'Acceso denegado para este tipo de usuario' : '',
  );

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage('');
    setIsSubmitting(true);

    try {
      const response = await login(email, password);

      if (response.role !== 'EMPLOYER') {
        setErrorMessage('Acceso denegado para este tipo de usuario');
        return;
      }

      saveSession(response.token, {
        userId: response.userId,
        email: response.email,
        role: response.role,
      });
      navigate('/empleador', { replace: true });
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
          <p className="eyebrow">Bienvenido de vuelta</p>
          <h2>Iniciar sesion</h2>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="auth-fields">
            <label>
              Correo electronico
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

            <label>
              Contrasena
              <input
                autoComplete="current-password"
                name="password"
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Tu contrasena"
                required
                type="password"
                value={password}
              />
            </label>
          </div>

          <div className="auth-forgot">
            <Link to={AUTH_ROUTES.recoverPassword}>¿Olvidaste tu contrasena?</Link>
          </div>

          <StatusMessage message={errorMessage} tone="error" />

          <button className="primary-action" disabled={isSubmitting} type="submit">
            <LogIn aria-hidden="true" size={18} />
            {isSubmitting ? 'Ingresando...' : 'Ingresar'}
          </button>
        </form>

        <div className="auth-footer">
          <p className="auth-coming-soon">
            ¿Buscas empleo? Accede desde el portal de candidatos
          </p>
        </div>
      </div>
    </div>
  );
}
