import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../shared/context/AuthContext.jsx';
import { AUTH_ROUTES } from '../shared/constants/authRoutes.js';
import { ForgotPasswordPage } from '../shared/pages/ForgotPasswordPage.jsx';
import { ResetPasswordPage } from '../shared/pages/ResetPasswordPage.jsx';
import { ProtectedRoute } from './components/ProtectedRoute.jsx';
import { LoginPage } from './pages/LoginPage.jsx';
import { AdminDashboardPage } from './pages/AdminDashboardPage.jsx';

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path={AUTH_ROUTES.login} element={<LoginPage />} />
          <Route path={AUTH_ROUTES.recoverPassword} element={<ForgotPasswordPage />} />
          <Route path={AUTH_ROUTES.resetPassword} element={<ResetPasswordPage />} />
          <Route path={AUTH_ROUTES.legacyResetPassword} element={<ResetPasswordPage />} />
          <Route
            path="/admin"
            element={
              <ProtectedRoute>
                <AdminDashboardPage />
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
