import { Navigate } from 'react-router-dom';
import { useAuth } from '../../shared/context/AuthContext.jsx';

export function ProtectedRoute({ children }) {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (user?.role !== 'ADMINISTRATOR') return <Navigate to="/login" replace state={{ denied: true }} />;

  return children;
}
