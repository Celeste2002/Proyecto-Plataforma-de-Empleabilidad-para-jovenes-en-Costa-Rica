import { Building2 } from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { registerEmployer } from '../api/employerApi.js';

const SECTORS = [
  'Tecnología',
  'Manufactura',
  'Comercio',
  'Servicios',
  'Educación',
  'Salud',
  'Construcción',
  'Turismo',
  'Agroindustria',
  'Transporte y logística',
  'Finanzas y banca',
  'Medios y comunicación',
  'Otro',
];

export function RegisterPage() {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    companyName: '',
    legalId: '',
    sector: '',
    contactName: '',
    contactPhone: '',
    location: '',
    email: '',
    password: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [validationErrors, setValidationErrors] = useState([]);
  const [successMessage, setSuccessMessage] = useState('');

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setErrorMessage('');
    setValidationErrors([]);
    setSuccessMessage('');
    setIsSubmitting(true);

    try {
      const response = await registerEmployer(form);
      setSuccessMessage(response.message);
      setTimeout(() => navigate('/login'), 4000);
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

  if (successMessage) {
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
          <StatusMessage message={successMessage} tone="success" />
          <p style={{ textAlign: 'center', marginTop: '1rem', fontSize: '0.9rem', color: '#666' }}>
            Serás redirigido al inicio de sesión en unos segundos…
          </p>
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
          <p className="eyebrow">Registro de empresa</p>
          <h2>Crear cuenta de empleador</h2>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="auth-fields">
            <label>
              Nombre de la empresa
              <input
                name="companyName"
                onChange={handleChange}
                placeholder="Nombre legal de la empresa"
                required
                type="text"
                value={form.companyName}
              />
            </label>

            <label>
              Cédula jurídica
              <input
                name="legalId"
                onChange={handleChange}
                placeholder="Ej: 3-101-123456"
                required
                type="text"
                value={form.legalId}
              />
            </label>

            <label>
              Sector
              <select
                name="sector"
                onChange={handleChange}
                required
                value={form.sector}
              >
                <option value="">Seleccionar sector...</option>
                {SECTORS.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </label>

            <label>
              Nombre del contacto
              <input
                name="contactName"
                onChange={handleChange}
                placeholder="Persona de contacto en la empresa"
                required
                type="text"
                value={form.contactName}
              />
            </label>

            <label>
              Teléfono de contacto
              <input
                name="contactPhone"
                onChange={handleChange}
                placeholder="Ej: 8888-8888"
                required
                type="tel"
                value={form.contactPhone}
              />
            </label>

            <label>
              Ubicación
              <input
                name="location"
                onChange={handleChange}
                placeholder="Provincia o cantón donde opera"
                required
                type="text"
                value={form.location}
              />
            </label>

            <label>
              Correo electrónico
              <input
                autoComplete="email"
                name="email"
                onChange={handleChange}
                placeholder="correo@empresa.com"
                required
                type="email"
                value={form.email}
              />
            </label>

            <label>
              Contraseña
              <input
                autoComplete="new-password"
                minLength={8}
                name="password"
                onChange={handleChange}
                placeholder="Mínimo 8 caracteres"
                required
                type="password"
                value={form.password}
              />
            </label>
          </div>

          {validationErrors.length > 0 && (
            <ul className="validation-error-list" role="alert">
              {validationErrors.map((err) => (
                <li key={err}>{err}</li>
              ))}
            </ul>
          )}

          <StatusMessage message={errorMessage} tone="error" />

          <button className="primary-action" disabled={isSubmitting} type="submit">
            <Building2 aria-hidden="true" size={18} />
            {isSubmitting ? 'Registrando...' : 'Registrar empresa'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            ¿Ya tienes cuenta?{' '}
            <Link to="/login">Iniciar sesión</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
