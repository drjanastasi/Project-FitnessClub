
import {
  BrowserRouter,
  Routes,
  Route,
  Outlet,
  Navigate,
} from 'react-router-dom'

import { useAuth } from './contexts/AuthContext'
import { AuthProvider } from './contexts/AuthContext'

import Sidebar from './components/Sidebar'
import Header from './components/Header'

import HomePage from './pages/Home/HomePage'
import WorkoutsPage from './pages/Workouts/WorkoutsPage'
import ClubsPage from './pages/Clubs/ClubsPage'
import MembershipsPage from './pages/Memberships/MembershipsPage'
import ProfilePage from './pages/Profile/ProfilePage'
import LoginPage from './pages/Auth/LoginPage'
import RegisterPage from './pages/Auth/RegisterPage'
import BookingsPage from './pages/Bookings/BookingsPage'

import './App.css'

function AppLayout() {
  return (
    <div className="app">
      <Sidebar />
      <main className="main-content">
        <Header />
        <Outlet />
      </main>
    </div>
  )
}

function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth()

  if (isLoading) {
    return <div className="loading">Завантаження...</div>
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  return <Outlet />
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {/* Сторінки авторизації */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Відкриті сторінки */}
          <Route path="/" element={<AppLayout />}>
            <Route index element={<HomePage />} />
            <Route path="workouts" element={<WorkoutsPage />} />
            <Route path="clubs" element={<ClubsPage />} />
            <Route path="memberships" element={<MembershipsPage />} />
          </Route>

          {/* Захищені сторінки */}
          <Route element={<ProtectedRoute />}>
            <Route path="/bookings" element={<AppLayout />}>
              <Route index element={<BookingsPage />} />
            </Route>
            <Route path="/profile" element={<AppLayout />}>
              <Route index element={<ProfilePage />} />
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
