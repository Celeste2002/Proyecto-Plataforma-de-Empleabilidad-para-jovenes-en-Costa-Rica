import {
  BookOpen,
  BriefcaseBusiness,
  ClipboardList,
  FileDown,
  KeyRound,
  LogOut,
  Shield,
  Users,
} from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getReportData, getUsers, updateUserRole } from '../api/adminApi.js';
import { generateReportPdf } from '../utils/generatePdfReport.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
import { AUTH_ROUTES } from '../../shared/constants/authRoutes.js';
import { useAuth } from '../../shared/context/AuthContext.jsx';

const ROLES = ['CANDIDATE', 'EMPLOYER', 'ADMINISTRATOR'];

const ROLE_LABELS = {
  CANDIDATE: 'Candidato',
  EMPLOYER: 'Empleador',
  ADMINISTRATOR: 'Administrador',
};

function RoleBadge({ role }) {
  return <span className={`role-badge role-badge--${role.toLowerCase()}`}>{ROLE_LABELS[role] ?? role}</span>;
}

function StatCard({ icon, value, label, accent }) {
  return (
    <div className="admin-stat" style={accent ? { borderLeft: `3px solid ${accent}` } : {}}>
      {icon}
      <div>
        <p className="admin-stat__value">{value ?? '—'}</p>
        <p className="admin-stat__label">{label}</p>
      </div>
    </div>
  );
}

export function AdminDashboardPage() {
  const { user, token, logout } = useAuth();

  const [users, setUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [pendingRoles, setPendingRoles] = useState({});
  const [savingId, setSavingId] = useState(null);
  const [successId, setSuccessId] = useState(null);

  const [reportData, setReportData] = useState(null);
  const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);
  const [pdfError, setPdfError] = useState('');

  useEffect(() => {
    Promise.all([getUsers(token), getReportData(token)])
      .then(([usersData, report]) => {
        setUsers(usersData);
        setReportData(report);
      })
      .catch((err) => setErrorMessage(err.message))
      .finally(() => setIsLoading(false));
  }, [token]);

  function handleRoleChange(userId, newRole) {
    setPendingRoles((prev) => ({ ...prev, [userId]: newRole }));
  }

  async function handleSaveRole(userId) {
    const newRole = pendingRoles[userId];
    if (!newRole) return;

    setSavingId(userId);
    setErrorMessage('');

    try {
      await updateUserRole(userId, newRole, token);
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, role: newRole } : u))
      );
      setPendingRoles((prev) => {
        const next = { ...prev };
        delete next[userId];
        return next;
      });
      setSuccessId(userId);
      setTimeout(() => setSuccessId(null), 2000);
    } catch (err) {
      setErrorMessage(err.message);
    } finally {
      setSavingId(null);
    }
  }

  async function handleGeneratePdf() {
    setIsGeneratingPdf(true);
    setPdfError('');
    try {
      let data = reportData;
      if (!data) {
        data = await getReportData(token);
        setReportData(data);
      }
      generateReportPdf(data, user?.email);
    } catch (err) {
      setPdfError(err.message ?? 'Error al generar el reporte PDF.');
    } finally {
      setIsGeneratingPdf(false);
    }
  }

  const counts = users.reduce((acc, u) => {
    acc[u.role] = (acc[u.role] ?? 0) + 1;
    return acc;
  }, {});

  return (
    <div className="admin-shell">
      <header className="admin-header">
        <div className="admin-header__brand">
          <Shield size={22} />
          <span>Panel de Administracion</span>
        </div>
        <div className="admin-header__user">
          <span className="admin-header__email">{user?.email}</span>
          <Link className="admin-header__link" to={AUTH_ROUTES.recoverPassword}>
            <KeyRound size={16} />
            Restablecer contraseña
          </Link>
          <button className="admin-logout" onClick={logout} type="button">
            <LogOut size={16} />
            Salir
          </button>
        </div>
      </header>

      <main className="admin-main">

        {/* ── Botón Reporte PDF ── */}
        <div className="admin-report-bar">
          <div>
            <p className="admin-section__title" style={{ margin: 0 }}>Panel de control</p>
            <p style={{ fontSize: '0.82rem', color: '#6b7280', marginTop: 2 }}>
              Datos en tiempo real desde la base de datos
            </p>
          </div>
          <button
            className="admin-pdf-btn"
            disabled={isGeneratingPdf}
            onClick={handleGeneratePdf}
            type="button"
          >
            <FileDown size={16} />
            {isGeneratingPdf ? 'Generando PDF...' : 'Generar Reporte PDF'}
          </button>
        </div>

        {pdfError && (
          <div style={{ margin: '0 0 12px', padding: '8px 12px', background: '#fef2f2', borderRadius: 6, color: '#b91c1c', fontSize: '0.85rem' }}>
            {pdfError}
          </div>
        )}

        {/* ── Sección: Usuarios ── */}
        <p className="admin-stats-section-label">Usuarios</p>
        <div className="admin-stats">
          <StatCard icon={<Users size={20} />} value={users.length} label="Usuarios totales" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--candidate" />} value={counts.CANDIDATE ?? 0} label="Candidatos" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--employer" />} value={counts.EMPLOYER ?? 0} label="Empleadores" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--administrator" />} value={counts.ADMINISTRATOR ?? 0} label="Administradores" />
        </div>

        {/* ── Sección: Vacantes ── */}
        <p className="admin-stats-section-label">Vacantes</p>
        <div className="admin-stats">
          <StatCard icon={<BriefcaseBusiness size={20} />} value={reportData?.totalVacantes} label="Vacantes totales" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#16a34a' }} />} value={reportData?.activeVacantes} label="Activas" accent="#16a34a" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#9ca3af' }} />} value={reportData?.closedVacantes} label="Cerradas" accent="#9ca3af" />
        </div>

        {/* ── Sección: Postulaciones ── */}
        <p className="admin-stats-section-label">Postulaciones</p>
        <div className="admin-stats">
          <StatCard icon={<ClipboardList size={20} />} value={reportData?.totalPostulaciones} label="Postulaciones totales" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#7c3aed' }} />} value={reportData?.candidatesWithPostulaciones} label="Candidatos postulados" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#0ea5e9' }} />} value={reportData?.vacantesWithPostulaciones} label="Vacantes con postulaciones" />
        </div>

        {/* ── Estado de Postulaciones ── */}
        {reportData?.postulacionesByStatus?.length > 0 && (
          <div className="admin-section" style={{ marginTop: 8 }}>
            <h3 className="admin-section__title" style={{ fontSize: '0.9rem' }}>Estado de postulaciones</h3>
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Estado</th>
                    <th>Cantidad</th>
                  </tr>
                </thead>
                <tbody>
                  {reportData.postulacionesByStatus.map((s) => (
                    <tr key={s.status}>
                      <td>{s.status}</td>
                      <td style={{ textAlign: 'center' }}>{s.count}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* ── Formación / Microcursos ── */}
        <p className="admin-stats-section-label">Formación</p>
        <div className="admin-stats">
          <StatCard icon={<BookOpen size={20} />} value={reportData?.totalMicrocursos ?? 0} label="Microcursos disponibles" />
        </div>

        {/* ── Gestión de Usuarios ── */}
        <section className="admin-section">
          <h2 className="admin-section__title">Gestion de usuarios</h2>

          <StatusMessage message={errorMessage} tone="error" />

          {isLoading ? (
            <p className="admin-loading">Cargando usuarios...</p>
          ) : users.length === 0 ? (
            <p className="empty-state">No hay usuarios registrados.</p>
          ) : (
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Correo</th>
                    <th>Rol actual</th>
                    <th>Cambiar rol</th>
                    <th>Registrado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((u) => {
                    const selectedRole = pendingRoles[u.id] ?? u.role;
                    const isDirty = pendingRoles[u.id] && pendingRoles[u.id] !== u.role;
                    const isSaving = savingId === u.id;
                    const isSuccess = successId === u.id;

                    return (
                      <tr key={u.id} className={isSuccess ? 'admin-table__row--saved' : ''}>
                        <td className="admin-table__email">{u.email}</td>
                        <td><RoleBadge role={u.role} /></td>
                        <td>
                          <select
                            className="admin-role-select"
                            onChange={(e) => handleRoleChange(u.id, e.target.value)}
                            value={selectedRole}
                          >
                            {ROLES.map((r) => (
                              <option key={r} value={r}>{ROLE_LABELS[r]}</option>
                            ))}
                          </select>
                        </td>
                        <td className="admin-table__date">
                          {new Date(u.createdAtUtc).toLocaleDateString('es-CR')}
                        </td>
                        <td>
                          {isDirty && (
                            <button
                              className="admin-save-btn"
                              disabled={isSaving}
                              onClick={() => handleSaveRole(u.id)}
                              type="button"
                            >
                              {isSaving ? 'Guardando...' : 'Guardar'}
                            </button>
                          )}
                          {isSuccess && <span className="admin-saved-msg">Guardado</span>}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </main>
    </div>
  );
}
