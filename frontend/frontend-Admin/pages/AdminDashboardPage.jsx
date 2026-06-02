import { LogOut, Shield, Users } from 'lucide-react';
import { useEffect, useState } from 'react';
import { getUsers, updateUserRole } from '../api/adminApi.js';
import { StatusMessage } from '../../shared/components/StatusMessage.jsx';
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

export function AdminDashboardPage() {
  const { user, token, logout } = useAuth();

  const [users, setUsers] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState('');
  const [pendingRoles, setPendingRoles] = useState({});
  const [savingId, setSavingId] = useState(null);
  const [successId, setSuccessId] = useState(null);

  useEffect(() => {
    getUsers(token)
      .then((data) => setUsers(data))
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

  const counts = users.reduce(
    (acc, u) => {
      acc[u.role] = (acc[u.role] ?? 0) + 1;
      return acc;
    },
    {}
  );

  return (
    <div className="admin-shell">
      <header className="admin-header">
        <div className="admin-header__brand">
          <Shield size={22} />
          <span>Panel de Administracion</span>
        </div>
        <div className="admin-header__user">
          <span className="admin-header__email">{user?.email}</span>
          <button className="admin-logout" onClick={logout} type="button">
            <LogOut size={16} />
            Salir
          </button>
        </div>
      </header>

      <main className="admin-main">
        <div className="admin-stats">
          <div className="admin-stat">
            <Users size={20} />
            <div>
              <p className="admin-stat__value">{users.length}</p>
              <p className="admin-stat__label">Usuarios totales</p>
            </div>
          </div>
          <div className="admin-stat">
            <div className="admin-stat__dot admin-stat__dot--candidate" />
            <div>
              <p className="admin-stat__value">{counts.CANDIDATE ?? 0}</p>
              <p className="admin-stat__label">Candidatos</p>
            </div>
          </div>
          <div className="admin-stat">
            <div className="admin-stat__dot admin-stat__dot--employer" />
            <div>
              <p className="admin-stat__value">{counts.EMPLOYER ?? 0}</p>
              <p className="admin-stat__label">Empleadores</p>
            </div>
          </div>
          <div className="admin-stat">
            <div className="admin-stat__dot admin-stat__dot--administrator" />
            <div>
              <p className="admin-stat__value">{counts.ADMINISTRATOR ?? 0}</p>
              <p className="admin-stat__label">Administradores</p>
            </div>
          </div>
        </div>

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
