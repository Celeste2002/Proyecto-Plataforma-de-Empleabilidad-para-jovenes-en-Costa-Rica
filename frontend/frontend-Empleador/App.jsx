import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../shared/context/AuthContext.jsx';
import { AUTH_ROUTES } from '../shared/constants/authRoutes.js';
import { ForgotPasswordPage } from '../shared/pages/ForgotPasswordPage.jsx';
import { ResetPasswordPage } from '../shared/pages/ResetPasswordPage.jsx';
import { ProtectedRoute } from './components/ProtectedRoute.jsx';
import { LoginPage } from './pages/LoginPage.jsx';
import { EmployerDashboardPage } from './pages/EmployerDashboardPage.jsx';
import { MisVacantesPage } from './pages/MisVacantesPage.jsx';
import { PanelCandidatosPage } from './pages/PanelCandidatosPage.jsx';
import { PublicarVacantePage } from './pages/PublicarVacantePage.jsx';
import { RegisterPage } from './pages/RegisterPage.jsx';
import { PostulacionesVacanteEmpleadorPage } from './pages/PostulacionesVacanteEmpleadorPage.jsx';
import { PostulacionDetailPage } from './pages/PostulacionDetailPage.jsx';

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path={AUTH_ROUTES.login} element={<LoginPage />} />
          <Route path="/registro" element={<RegisterPage />} />
          <Route path={AUTH_ROUTES.recoverPassword} element={<ForgotPasswordPage />} />
          <Route path={AUTH_ROUTES.resetPassword} element={<ResetPasswordPage />} />
          <Route path={AUTH_ROUTES.legacyResetPassword} element={<ResetPasswordPage />} />
          <Route
            path="/empleador"
            element={
              <ProtectedRoute>
                <EmployerDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/empleador/vacantes"
            element={
              <ProtectedRoute>
                <MisVacantesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/empleador/vacantes/nueva"
            element={
              <ProtectedRoute>
                <PublicarVacantePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/empleador/vacantes/:vacanteId/postulaciones"
            element={
              <ProtectedRoute>
                <PostulacionesVacanteEmpleadorPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/empleador/postulaciones/:postulacionId"
            element={
              <ProtectedRoute>
                <PostulacionDetailPage />
              </ProtectedRoute>
            }
          />
          <Route path="/" element={<Navigate to={AUTH_ROUTES.login} replace />} />
          <Route path="*" element={<Navigate to={AUTH_ROUTES.login} replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
