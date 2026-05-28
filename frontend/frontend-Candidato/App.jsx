import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../shared/context/AuthContext.jsx';
import { ForgotPasswordPage } from '../shared/pages/ForgotPasswordPage.jsx';
import { ResetPasswordPage } from '../shared/pages/ResetPasswordPage.jsx';
import { ProtectedRoute } from './components/ProtectedRoute.jsx';
import { LoginPage } from './pages/LoginPage.jsx';
import { CandidateDashboardPage } from './pages/CandidateDashboardPage.jsx';
import { CandidateRegistrationPage } from './pages/CandidateRegistrationPage.jsx';

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/recuperar-contrasena" element={<ForgotPasswordPage />} />
          <Route path="/restablecer-contrasena" element={<ResetPasswordPage />} />
          <Route path="/registro" element={<CandidateRegistrationPage />} />
          <Route
            path="/candidato"
            element={
              <ProtectedRoute>
                <CandidateDashboardPage />
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
