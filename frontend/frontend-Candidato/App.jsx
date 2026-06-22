import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../shared/context/AuthContext.jsx';
import { AUTH_ROUTES } from '../shared/constants/authRoutes.js';
import { ForgotPasswordPage } from '../shared/pages/ForgotPasswordPage.jsx';
import { ResetPasswordPage } from '../shared/pages/ResetPasswordPage.jsx';
import { ProtectedRoute } from './components/ProtectedRoute.jsx';
import { LoginPage } from './pages/LoginPage.jsx';
import { CandidateDashboardPage } from './pages/CandidateDashboardPage.jsx';
import { CandidateProfileUpdatePage } from './pages/CandidateProfileUpdatePage.jsx';
import { CandidateRegistrationPage } from './pages/CandidateRegistrationPage.jsx';
import { VacantesPage } from './pages/VacantesPage.jsx';
import { MisMensajesPage } from './pages/MisMensajesPage.jsx';
import { PostulacionesPage } from './pages/PostulacionesPage.jsx';
import { MiPerfilPage } from './pages/MiPerfilPage.jsx';
import { MicroCursosPage } from './pages/MicroCursosPage.jsx';

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path={AUTH_ROUTES.login} element={<LoginPage />} />
          <Route path={AUTH_ROUTES.recoverPassword} element={<ForgotPasswordPage />} />
          <Route path={AUTH_ROUTES.resetPassword} element={<ResetPasswordPage />} />
          <Route path={AUTH_ROUTES.legacyResetPassword} element={<ResetPasswordPage />} />
          <Route path="/registro" element={<CandidateRegistrationPage />} />
          <Route
            path="/candidato"
            element={
              <ProtectedRoute>
                <CandidateDashboardPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/candidato/actualizar-registro"
            element={
              <ProtectedRoute>
                <CandidateProfileUpdatePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/candidato/vacantes"
            element={
              <ProtectedRoute>
                <VacantesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/candidato/postulaciones"
            element={
              <ProtectedRoute>
                <PostulacionesPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/candidato/mi-perfil"
            element={
              <ProtectedRoute>
                <MiPerfilPage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/candidato/microcursos"
            element={
              <ProtectedRoute>
                <MicroCursosPage />
              </ProtectedRoute>
            }
          />
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}
