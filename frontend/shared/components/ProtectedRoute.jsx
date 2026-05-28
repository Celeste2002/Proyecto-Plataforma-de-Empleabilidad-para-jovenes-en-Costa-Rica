import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

function dashboardFor(role) {
  if (role === 'CANDIDATE') return '/candidato';
  if (role === 'EMPLOYER') return '/empleador';
  if (role === 'ADMINISTRATOR') return '/admin';
  return '/login';
}

export function ProtectedRoute({ children, allowedRole }) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRole && user?.role !== allowedRole) {
    return <Navigate to={dashboardFor(user?.role)} replace />;
  }

  return children;
}
