import {
  BookOpen,
  BriefcaseBusiness,
  CircleCheck,
  CircleOff,
  ClipboardList,
  FileDown,
  KeyRound,
  LogOut,
  Shield,
  Users,
} from 'lucide-react';
import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getReportData, getUsers, getVacantes, updateUserRole, updateVacanteStatus } from '../api/adminApi.js';
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
        <p className="admin-stat__value">{value ?? '-'}</p>
        <p className="admin-stat__label">{label}</p>
      </div>
    </div>
  );
}

export function AdminDashboardPage() {
  const { user, token, logout } = useAuth();

  const [users, setUsers] = useState([]);
  const [vacantes, setVacantes] = useState([]);
  const [reportData, setReportData] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [pendingRoles, setPendingRoles] = useState({});
  const [savingId, setSavingId] = useState(null);
  const [successId, setSuccessId] = useState(null);
  const [savingVacanteId, setSavingVacanteId] = useState(null);
  const [successVacanteId, setSuccessVacanteId] = useState(null);
  const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);
  const [pdfError, setPdfError] = useState('');

  const loadAdminData = useCallback(async (showLoading = false) => {
    if (showLoading) {
      setIsLoading(true);
    }

    setErrorMessage('');

    try {
      const [usersData, vacantesData, report] = await Promise.all([
        getUsers(token),
        getVacantes(token),
        getReportData(token),
      ]);
      setUsers(usersData);
      setVacantes(vacantesData);
      setReportData(report);
    } catch (err) {
      setErrorMessage(err.message);
    } finally {
      if (showLoading) {
        setIsLoading(false);
      }
    }
  }, [token]);

  useEffect(() => {
    loadAdminData(true);
    const intervalId = setInterval(() => {
      loadAdminData(false);
    }, 15000);

    return () => clearInterval(intervalId);
  }, [loadAdminData]);

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
        prev.map((u) => (u.id === userId ? { ...u, role: newRole } : u)),
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

  async function handleVacanteStatusChange(vacanteId, isActive) {
    setSavingVacanteId(vacanteId);
    setErrorMessage('');

    try {
      const updatedVacante = await updateVacanteStatus(vacanteId, isActive, token);
      setVacantes((prev) => prev.map((vacante) => (
        vacante.id === vacanteId ? updatedVacante : vacante
      )));
      setReportData((prev) => {
        if (!prev) return prev;
        return {
          ...prev,
          activeVacantes: prev.activeVacantes + (isActive ? 1 : -1),
          closedVacantes: prev.closedVacantes + (isActive ? -1 : 1),
        };
      });
      setSuccessVacanteId(vacanteId);
      setTimeout(() => setSuccessVacanteId(null), 2000);
    } catch (err) {
      setErrorMessage(err.message);
    } finally {
      setSavingVacanteId(null);
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

  const vacancyCounts = vacantes.reduce(
    (acc, vacante) => {
      acc.total += 1;
      if (vacante.isActive) {
        acc.active += 1;
      } else {
        acc.inactive += 1;
      }
      return acc;
    },
    { total: 0, active: 0, inactive: 0 },
  );

  const totalVacantes = reportData?.totalVacantes ?? vacancyCounts.total;
  const activeVacantes = reportData?.activeVacantes ?? vacancyCounts.active;
  const closedVacantes = reportData?.closedVacantes ?? vacancyCounts.inactive;

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

        <StatusMessage message={errorMessage} tone="error" />

        {pdfError && (
          <div style={{ margin: '0 0 12px', padding: '8px 12px', background: '#fef2f2', borderRadius: 6, color: '#b91c1c', fontSize: '0.85rem' }}>
            {pdfError}
          </div>
        )}

        <p className="admin-stats-section-label">Usuarios</p>
        <div className="admin-stats">
          <StatCard icon={<Users size={20} />} value={users.length} label="Usuarios totales" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--candidate" />} value={counts.CANDIDATE ?? 0} label="Candidatos" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--employer" />} value={counts.EMPLOYER ?? 0} label="Empleadores" />
          <StatCard icon={<div className="admin-stat__dot admin-stat__dot--administrator" />} value={counts.ADMINISTRATOR ?? 0} label="Administradores" />
        </div>

        <p className="admin-stats-section-label">Vacantes</p>
        <div className="admin-stats">
          <StatCard icon={<BriefcaseBusiness size={20} />} value={totalVacantes} label="Vacantes totales" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#16a34a' }} />} value={activeVacantes} label="Activas" accent="#16a34a" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#9ca3af' }} />} value={closedVacantes} label="Cerradas" accent="#9ca3af" />
        </div>

        <p className="admin-stats-section-label">Postulaciones</p>
        <div className="admin-stats">
          <StatCard icon={<ClipboardList size={20} />} value={reportData?.totalPostulaciones} label="Postulaciones totales" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#7c3aed' }} />} value={reportData?.candidatesWithPostulaciones} label="Candidatos postulados" />
          <StatCard icon={<div className="admin-stat__dot" style={{ background: '#0ea5e9' }} />} value={reportData?.vacantesWithPostulaciones} label="Vacantes con postulaciones" />
        </div>

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

        <p className="admin-stats-section-label">Formacion</p>
        <div className="admin-stats">
          <StatCard icon={<BookOpen size={20} />} value={reportData?.totalMicrocursos ?? 0} label="Microcursos disponibles" />
        </div>

        <section className="admin-section">
          <h2 className="admin-section__title">Gestion de usuarios</h2>

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

        <section className="admin-section">
          <h2 className="admin-section__title">Gestion de vacantes</h2>

          {isLoading ? (
            <p className="admin-loading">Cargando vacantes...</p>
          ) : vacantes.length === 0 ? (
            <p className="empty-state">No hay vacantes registradas.</p>
          ) : (
            <div className="admin-table-wrapper">
              <table className="admin-table">
                <thead>
                  <tr>
                    <th>Empresa</th>
                    <th>Vacante</th>
                    <th>Estado</th>
                    <th>Publicado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {vacantes.map((vacante) => {
                    const isSaving = savingVacanteId === vacante.id;
                    const isSuccess = successVacanteId === vacante.id;

                    return (
                      <tr key={vacante.id} className={isSuccess ? 'admin-table__row--saved' : ''}>
                        <td>{vacante.companyName}</td>
                        <td>
                          <div className="vacante-table__title">
                            <BriefcaseBusiness size={16} />
                            <div>
                              <strong>{vacante.jobTitle}</strong>
                              <p>{vacante.sector} · {vacante.modality}</p>
                            </div>
                          </div>
                        </td>
                        <td>
                          <span className={`vacante-badge ${vacante.isActive ? 'vacante-badge--active' : 'vacante-badge--inactive'}`}>
                            {vacante.isActive ? 'Activa' : 'Desactivada'}
                          </span>
                        </td>
                        <td className="admin-table__date">
                          {new Date(vacante.publishedAt).toLocaleDateString('es-CR')}
                        </td>
                        <td>
                          <button
                            className={`admin-save-btn ${vacante.isActive ? 'admin-save-btn--danger' : ''}`}
                            disabled={isSaving}
                            onClick={() => handleVacanteStatusChange(vacante.id, !vacante.isActive)}
                            type="button"
                          >
                            {isSaving ? 'Actualizando...' : vacante.isActive ? (
                              <>
                                <CircleOff size={16} />
                                Desactivar
                              </>
                            ) : (
                              <>
                                <CircleCheck size={16} />
                                Activar
                              </>
                            )}
                          </button>
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
