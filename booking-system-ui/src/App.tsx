import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AuthProvider } from './context/AuthContext'
import { ProfileProvider } from './context/ProfileContext'
import { CatalogPage } from './pages/CatalogPage'
import { ClientBookingsPage } from './pages/ClientBookingsPage'
import { HomePage } from './pages/HomePage'
import { AdminDashboardPage } from './pages/AdminDashboardPage'
import { HostApartmentsPage } from './pages/HostApartmentsPage'
import { LoginPage } from './pages/LoginPage'
import { ProfilePage } from './pages/ProfilePage'
import { RegisterPage } from './pages/RegisterPage'

export default function App() {
  return (
    <AuthProvider>
      <ProfileProvider>
      <BrowserRouter>
        <Routes>
          <Route element={<Layout />}>
            <Route index element={<HomePage />} />
            <Route path="apartments" element={<CatalogPage />} />
            <Route path="login" element={<LoginPage />} />
            <Route path="register" element={<RegisterPage />} />
            <Route
              path="profile"
              element={
                <ProtectedRoute>
                  <ProfilePage />
                </ProtectedRoute>
              }
            />
            <Route
              path="bookings"
              element={
                <ProtectedRoute roles={['Client']}>
                  <ClientBookingsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="host"
              element={
                <ProtectedRoute roles={['Host']}>
                  <HostApartmentsPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="admin"
              element={
                <ProtectedRoute roles={['Admin']}>
                  <AdminDashboardPage />
                </ProtectedRoute>
              }
            />
            <Route path="dashboard" element={<Navigate to="/apartments" replace />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
      </ProfileProvider>
    </AuthProvider>
  )
}
