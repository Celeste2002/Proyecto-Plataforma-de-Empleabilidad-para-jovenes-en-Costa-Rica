import {
  ArrowLeft,
  Award,
  BookOpen,
  Briefcase,
  CheckCircle,
  Plus,
  Trash2,
  User,
  XCircle,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  addCurso,
  addExperiencia,
  addHabilidad,
  deleteCurso,
  deleteExperiencia,
  deleteHabilidad,
  getHabilidadesBlandasSugeridas,
  getMyFullProfile,
  updateMyAvailability,
} from '../api/candidatesApi.js';
import { BrandHomeLink } from '../../shared/components/BrandHomeLink.jsx';
import { FieldError } from '../../shared/components/FieldError.jsx';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const emptyExperiencia = {
  empresa: '',
  cargo: '',
  fechaInicio: '',
  fechaFin: '',
  esTrabajoActual: false,
  descripcion: '',
};

const emptyCurso = {
  nombreCurso: '',
  institucion: '',
  fechaCompletado: '',
  esDePlataforma: false,
};

export function MiPerfilPage() {
  const { token } = useAuth();
  const [perfil, setPerfil] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [globalError, setGlobalError] = useState('');
  const [softSkillSuggestions, setSoftSkillSuggestions] = useState([]);
  const [suggestionsError, setSuggestionsError] = useState('');

  useEffect(() => {
    loadPerfil();
    loadSoftSkillSuggestions();
  }, [token]);

  async function loadPerfil() {
    setIsLoading(true);
    setGlobalError('');
    try {
      const data = await getMyFullProfile(token);
      setPerfil(data);
    } catch (error) {
      setGlobalError(error.message);
    } finally {
      setIsLoading(false);
    }
  }

  async function loadSoftSkillSuggestions() {
    setSuggestionsError('');
    try {
      const suggestions = await getHabilidadesBlandasSugeridas(token);
      setSoftSkillSuggestions(suggestions);
    } catch (error) {
      setSuggestionsError(error.message);
      setSoftSkillSuggestions([]);
    }
  }

  async function handleToggleAvailability() {
    try {
      await updateMyAvailability(token, !perfil.isAvailableForContact);
      setPerfil((prev) => ({ ...prev, isAvailableForContact: !prev.isAvailableForContact }));
    } catch (error) {
      setGlobalError(error.message);
    }
  }

  if (isLoading) {
    return (
      <main className="application-shell">
        <ProfileHeader />
        <section className="employer-view">
          <p className="empty-state">Cargando perfil...</p>
        </section>
      </main>
    );
  }

  if (!perfil) {
    return (
      <main className="application-shell">
        <ProfileHeader />
        <section className="employer-view">
          <FieldError errors={[globalError || 'No se pudo cargar el perfil.']} />
        </section>
      </main>
    );
  }

  return (
    <main className="application-shell">
      <ProfileHeader />

      <section className="employer-view">
        <div className="section-heading">
          <p className="eyebrow">Mi cuenta</p>
          <h2>Mi perfil público</h2>
          <p className="section-description">
            Esta información es visible para los empleadores al revisar tu postulación.
          </p>
        </div>

        {globalError && <FieldError errors={[globalError]} />}

        <div className="perfil-layout">
          {/* Disponibilidad */}
          <AvailabilityToggle
            isAvailable={perfil.isAvailableForContact}
            onToggle={handleToggleAvailability}
          />

          {/* Datos básicos */}
          <BasicInfoCard perfil={perfil} />

          {/* Experiencia laboral */}
          <ExperienciasSection
            experiencias={perfil.experiencias}
            onAdd={async (data) => {
              const nueva = await addExperiencia(token, data);
              setPerfil((prev) => ({
                ...prev,
                experiencias: [nueva, ...prev.experiencias],
              }));
            }}
            onDelete={async (id) => {
              await deleteExperiencia(token, id);
              setPerfil((prev) => ({
                ...prev,
                experiencias: prev.experiencias.filter((e) => e.id !== id),
              }));
            }}
          />

          {/* Habilidades */}
          <HabilidadesSection
            habilidades={perfil.habilidades}
            softSkillSuggestions={softSkillSuggestions}
            suggestionsError={suggestionsError}
            onAdd={async (nombre) => {
              const nueva = await addHabilidad(token, nombre);
              setPerfil((prev) => ({
                ...prev,
                habilidades: [...prev.habilidades, nueva],
              }));
            }}
            onDelete={async (id) => {
              await deleteHabilidad(token, id);
              setPerfil((prev) => ({
                ...prev,
                habilidades: prev.habilidades.filter((h) => h.id !== id),
              }));
            }}
          />

          {/* Cursos completados */}
          <CursosSection
            cursos={perfil.cursos}
            onAdd={async (data) => {
              const nuevo = await addCurso(token, data);
              setPerfil((prev) => ({
                ...prev,
                cursos: [nuevo, ...prev.cursos],
              }));
            }}
            onDelete={async (id) => {
              await deleteCurso(token, id);
              setPerfil((prev) => ({
                ...prev,
                cursos: prev.cursos.filter((c) => c.id !== id),
              }));
            }}
          />
        </div>
      </section>
    </main>
  );
}

function ProfileHeader() {
  return (
    <header className="top-bar">
      <BrandHomeLink to="/candidato" />
      <nav className="dashboard-nav" aria-label="Navegación del perfil">
        <Link className="secondary-action" to="/candidato">
          <ArrowLeft aria-hidden="true" size={16} />
          Volver
        </Link>
      </nav>
    </header>
  );
}

function AvailabilityToggle({ isAvailable, onToggle }) {
  return (
    <div className="availability-banner" data-available={isAvailable}>
      <div className="availability-info">
        {isAvailable
          ? <CheckCircle aria-hidden="true" size={20} />
          : <XCircle aria-hidden="true" size={20} />}
        <span>
          {isAvailable
            ? 'Disponible para ser contactado por empleadores'
            : 'No disponible para ser contactado'}
        </span>
      </div>
      <button className="secondary-action" onClick={onToggle} type="button">
        {isAvailable ? 'Marcar como no disponible' : 'Marcar como disponible'}
      </button>
    </div>
  );
}

function BasicInfoCard({ perfil }) {
  return (
    <div className="profile-basic-card">
      <div className="profile-avatar-large">
        {perfil.photoUrl
          ? (
            <img
              alt={perfil.fullName}
              className="profile-photo"
              onError={(e) => { e.currentTarget.style.display = 'none'; }}
              src={perfil.photoUrl}
            />
          )
          : <User aria-hidden="true" size={48} />}
      </div>
      <div className="profile-basic-info">
        <h3>{perfil.fullName}</h3>
        <p>{perfil.email}</p>
        <div className="profile-tags">
          <span className="profile-tag">{perfil.province}</span>
          <span className="profile-tag">{perfil.educationLevel}</span>
          <span className="profile-tag">{perfil.age} años</span>
        </div>
      </div>
      <div className="profile-basic-actions">
        <Link className="secondary-action" to="/candidato/actualizar-registro">
          Editar datos básicos
        </Link>
      </div>
    </div>
  );
}

function ExperienciasSection({ experiencias, onAdd, onDelete }) {
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyExperiencia);
  const [errors, setErrors] = useState([]);
  const [isSaving, setIsSaving] = useState(false);

  function updateField(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleAdd(e) {
    e.preventDefault();
    const validationErrors = [];
    if (!form.empresa.trim()) validationErrors.push('La empresa es obligatoria.');
    if (!form.cargo.trim()) validationErrors.push('El cargo es obligatorio.');
    if (!form.fechaInicio) validationErrors.push('La fecha de inicio es obligatoria.');
    if (validationErrors.length > 0) { setErrors(validationErrors); return; }

    setIsSaving(true);
    setErrors([]);
    try {
      await onAdd({
        empresa: form.empresa,
        cargo: form.cargo,
        fechaInicio: form.fechaInicio,
        fechaFin: form.esTrabajoActual ? null : (form.fechaFin || null),
        esTrabajoActual: form.esTrabajoActual,
        descripcion: form.descripcion || null,
      });
      setForm(emptyExperiencia);
      setShowForm(false);
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="perfil-section">
      <div className="perfil-section-header">
        <div className="perfil-section-title">
          <Briefcase aria-hidden="true" size={20} />
          <h3>Experiencia laboral</h3>
        </div>
        <button
          className="secondary-action"
          onClick={() => setShowForm((v) => !v)}
          type="button"
        >
          <Plus aria-hidden="true" size={16} />
          Agregar
        </button>
      </div>

      {showForm && (
        <form className="perfil-add-form" noValidate onSubmit={handleAdd}>
          <div className="form-grid">
            <label>
              Empresa
              <input
                onChange={(e) => updateField('empresa', e.target.value)}
                placeholder="Ej. Empresa S.A."
                type="text"
                value={form.empresa}
              />
            </label>
            <label>
              Cargo
              <input
                onChange={(e) => updateField('cargo', e.target.value)}
                placeholder="Ej. Asistente administrativo"
                type="text"
                value={form.cargo}
              />
            </label>
            <label>
              Fecha de inicio
              <input
                onChange={(e) => updateField('fechaInicio', e.target.value)}
                type="date"
                value={form.fechaInicio}
              />
            </label>
            {!form.esTrabajoActual && (
              <label>
                Fecha de fin
                <input
                  onChange={(e) => updateField('fechaFin', e.target.value)}
                  type="date"
                  value={form.fechaFin}
                />
              </label>
            )}
            <label className="wide-field checkbox-label">
              <input
                checked={form.esTrabajoActual}
                onChange={(e) => updateField('esTrabajoActual', e.target.checked)}
                type="checkbox"
              />
              Trabajo actual
            </label>
            <label className="wide-field">
              Descripción (opcional)
              <textarea
                onChange={(e) => updateField('descripcion', e.target.value)}
                placeholder="Describe tus responsabilidades principales..."
                rows={3}
                value={form.descripcion}
              />
            </label>
          </div>
          <FieldError errors={errors} />
          <div className="form-actions">
            <button className="primary-action" disabled={isSaving} type="submit">
              {isSaving ? 'Guardando...' : 'Guardar experiencia'}
            </button>
            <button
              className="secondary-action"
              onClick={() => { setShowForm(false); setErrors([]); }}
              type="button"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {experiencias.length === 0 && !showForm && (
        <p className="empty-state-small">No has agregado experiencias laborales aún.</p>
      )}

      <ul className="perfil-item-list">
        {experiencias.map((exp) => (
          <li className="perfil-item" key={exp.id}>
            <div className="perfil-item-content">
              <strong>{exp.cargo}</strong>
              <span>{exp.empresa}</span>
              <span className="perfil-item-dates">
                {formatDate(exp.fechaInicio)} —{' '}
                {exp.esTrabajoActual ? 'Presente' : formatDate(exp.fechaFin)}
              </span>
              {exp.descripcion && <p className="perfil-item-desc">{exp.descripcion}</p>}
            </div>
            <button
              aria-label="Eliminar experiencia"
              className="icon-action"
              onClick={() => onDelete(exp.id)}
              type="button"
            >
              <Trash2 aria-hidden="true" size={16} />
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}

function HabilidadesSection({ habilidades, softSkillSuggestions, suggestionsError, onAdd, onDelete }) {
  const [newHabilidad, setNewHabilidad] = useState('');
  const [errors, setErrors] = useState([]);
  const [isSaving, setIsSaving] = useState(false);
  const habilidadesRegistradas = new Set(
    habilidades.map((hab) => hab.nombre.toLocaleLowerCase('es-CR')),
  );
  const availableSuggestions = softSkillSuggestions.filter(
    (suggestion) => !habilidadesRegistradas.has(suggestion.toLocaleLowerCase('es-CR')),
  );

  function handleSuggestionChange(event) {
    setNewHabilidad(event.target.value);
    setErrors([]);
  }

  async function handleAdd(e) {
    e.preventDefault();
    if (!newHabilidad.trim()) { setErrors(['El nombre de la habilidad es obligatorio.']); return; }

    setIsSaving(true);
    setErrors([]);
    try {
      await onAdd(newHabilidad.trim());
      setNewHabilidad('');
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="perfil-section">
      <div className="perfil-section-header">
        <div className="perfil-section-title">
          <BookOpen aria-hidden="true" size={20} />
          <h3>Habilidades</h3>
        </div>
      </div>

      <form className="habilidad-add-form" noValidate onSubmit={handleAdd}>
        <div className="habilidad-suggestion-row">
          <label>
            Habilidad blanda sugerida
            <select
              onChange={handleSuggestionChange}
              value=""
            >
              <option value="">Seleccionar sugerencia</option>
              {availableSuggestions.length === 0 && (
                <option disabled value="">No hay sugerencias disponibles</option>
              )}
              {availableSuggestions.map((suggestion) => (
                <option key={suggestion} value={suggestion}>
                  {suggestion}
                </option>
              ))}
            </select>
          </label>
          <label>
            Habilidad
            <input
              onChange={(e) => setNewHabilidad(e.target.value)}
              placeholder="Ej. Microsoft Excel, Trabajo en equipo..."
              type="text"
              value={newHabilidad}
            />
          </label>
        </div>
        <button className="primary-action habilidad-add-button" disabled={isSaving} type="submit">
          <Plus aria-hidden="true" size={16} />
          {isSaving ? 'Agregando...' : 'Agregar'}
        </button>
        {suggestionsError && (
          <p className="empty-state-small">
            No se pudieron cargar sugerencias. Puedes escribir tu habilidad manualmente.
          </p>
        )}
        <FieldError errors={errors} />
      </form>

      {habilidades.length === 0 && (
        <p className="empty-state-small">No has agregado habilidades aún.</p>
      )}

      <div className="habilidades-tags">
        {habilidades.map((hab) => (
          <span className="habilidad-tag" key={hab.id}>
            {hab.nombre}
            <button
              aria-label={`Eliminar habilidad ${hab.nombre}`}
              className="habilidad-tag-remove"
              onClick={() => onDelete(hab.id)}
              type="button"
            >
              ×
            </button>
          </span>
        ))}
      </div>
    </div>
  );
}

function CursosSection({ cursos, onAdd, onDelete }) {
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyCurso);
  const [errors, setErrors] = useState([]);
  const [isSaving, setIsSaving] = useState(false);

  function updateField(field, value) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleAdd(e) {
    e.preventDefault();
    const validationErrors = [];
    if (!form.nombreCurso.trim()) validationErrors.push('El nombre del curso es obligatorio.');
    if (!form.institucion.trim()) validationErrors.push('La institución es obligatoria.');
    if (!form.fechaCompletado) validationErrors.push('La fecha de completado es obligatoria.');
    if (validationErrors.length > 0) { setErrors(validationErrors); return; }

    setIsSaving(true);
    setErrors([]);
    try {
      await onAdd({
        nombreCurso: form.nombreCurso,
        institucion: form.institucion,
        fechaCompletado: form.fechaCompletado,
        esDePlataforma: form.esDePlataforma,
      });
      setForm(emptyCurso);
      setShowForm(false);
    } catch (error) {
      setErrors(error.validationErrors?.length ? error.validationErrors : [error.message]);
    } finally {
      setIsSaving(false);
    }
  }

  const cursosDePlataforma = cursos.filter((c) => c.esDePlataforma);
  const cursosExternos = cursos.filter((c) => !c.esDePlataforma);

  return (
    <div className="perfil-section">
      <div className="perfil-section-header">
        <div className="perfil-section-title">
          <Award aria-hidden="true" size={20} />
          <h3>Cursos y certificados</h3>
        </div>
        <button
          className="secondary-action"
          onClick={() => setShowForm((v) => !v)}
          type="button"
        >
          <Plus aria-hidden="true" size={16} />
          Agregar
        </button>
      </div>

      {cursosDePlataforma.length > 0 && (
        <div className="cursos-plataforma">
          <p className="cursos-plataforma-label">Insignias de la plataforma Sinergia</p>
          <div className="insignias-grid">
            {cursosDePlataforma.map((c) => (
              <div className="insignia-card" key={c.id}>
                <Award aria-hidden="true" size={32} />
                <span className="insignia-nombre">{c.nombreCurso}</span>
                <span className="insignia-fecha">{formatDate(c.fechaCompletado)}</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {showForm && (
        <form className="perfil-add-form" noValidate onSubmit={handleAdd}>
          <div className="form-grid">
            <label>
              Nombre del curso
              <input
                onChange={(e) => updateField('nombreCurso', e.target.value)}
                placeholder="Ej. Excel avanzado"
                type="text"
                value={form.nombreCurso}
              />
            </label>
            <label>
              Institución
              <input
                onChange={(e) => updateField('institucion', e.target.value)}
                placeholder="Ej. INA, Coursera, UCR..."
                type="text"
                value={form.institucion}
              />
            </label>
            <label>
              Fecha de completado
              <input
                onChange={(e) => updateField('fechaCompletado', e.target.value)}
                type="date"
                value={form.fechaCompletado}
              />
            </label>
          </div>
          <FieldError errors={errors} />
          <div className="form-actions">
            <button className="primary-action" disabled={isSaving} type="submit">
              {isSaving ? 'Guardando...' : 'Guardar curso'}
            </button>
            <button
              className="secondary-action"
              onClick={() => { setShowForm(false); setErrors([]); }}
              type="button"
            >
              Cancelar
            </button>
          </div>
        </form>
      )}

      {cursosExternos.length === 0 && !showForm && cursosDePlataforma.length === 0 && (
        <p className="empty-state-small">No has agregado cursos aún.</p>
      )}

      <ul className="perfil-item-list">
        {cursosExternos.map((curso) => (
          <li className="perfil-item" key={curso.id}>
            <div className="perfil-item-content">
              <strong>{curso.nombreCurso}</strong>
              <span>{curso.institucion}</span>
              <span className="perfil-item-dates">{formatDate(curso.fechaCompletado)}</span>
            </div>
            <button
              aria-label="Eliminar curso"
              className="icon-action"
              onClick={() => onDelete(curso.id)}
              type="button"
            >
              <Trash2 aria-hidden="true" size={16} />
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}

function formatDate(dateStr) {
  if (!dateStr) return '';
  const date = new Date(`${dateStr}T00:00:00`);
  return date.toLocaleDateString('es-CR', { year: 'numeric', month: 'short' });
}
