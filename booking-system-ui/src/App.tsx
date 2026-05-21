import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AuthProvider } from './context/AuthContext'
import { ProfileProvider } from './context/ProfileContext'
import { ApartmentDetailPage } from './pages/ApartmentDetailPage'
import { BookingConfirmationPage } from './pages/BookingConfirmationPage'
import { BookingPaymentCancelledPage } from './pages/BookingPaymentCancelledPage'
import { BookingPaymentPage } from './pages/BookingPaymentPage'
import { BookingPaymentSuccessPage } from './pages/BookingPaymentSuccessPage'
import { CatalogPage } from './pages/CatalogPage'
import { BookingDetailPage } from './pages/BookingDetailPage'
import { ClientBookingsPage } from './pages/ClientBookingsPage'
import { HomePage } from './pages/HomePage'
import { AdminLayout } from './components/AdminLayout'
import { AdminDashboardPage } from './pages/AdminDashboardPage'
import { AdminUserDetailPage } from './pages/AdminUserDetailPage'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { AdminBookingsPage } from './pages/AdminBookingsPage'
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
            <Route path="apartments/:id" element={<ApartmentDetailPage />} />
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
              path="bookings/pay"
              element={
                <ProtectedRoute roles={['Client']}>
                  <BookingPaymentPage />
                </ProtectedRoute>
              }
            />
            <Route path="bookings/payment/success" element={<BookingPaymentSuccessPage />} />
            <Route path="bookings/payment/cancelled" element={<BookingPaymentCancelledPage />} />
            <Route
              path="bookings/confirmation"
              element={
                <ProtectedRoute roles={['Client']}>
                  <BookingConfirmationPage />
                </ProtectedRoute>
              }
            />
            <Route
              path="bookings/:id"
              element={
                <ProtectedRoute roles={['Client']}>
                  <BookingDetailPage />
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
                  <AdminLayout />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="/admin/analytics" replace />} />
              <Route path="analytics" element={<AdminDashboardPage />} />
              <Route path="users" element={<AdminUsersPage />} />
              <Route path="users/:userId" element={<AdminUserDetailPage />} />
              <Route path="bookings" element={<AdminBookingsPage />} />
            </Route>
            <Route path="dashboard" element={<Navigate to="/apartments" replace />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
      </ProfileProvider>
    </AuthProvider>
  )
}
