
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

function Header() {
  const location = useLocation()
  const navigate = useNavigate()
  const { user, isAuthenticated, logout } = useAuth()

  const pageTitles: Record<string, string> = {
    '/': 'Головна',
    '/workouts': 'Тренування',
    '/clubs': 'Клуби',
    '/memberships': 'Абонементи',
    '/bookings': 'Мої записи',
    '/profile': 'Профіль',
    '/login': 'Вхід',
    '/register': 'Реєстрація',
    '/admin/workouts': 'Керування тренуваннями',
    '/admin/clubs': 'Керування клубами',
    '/admin/memberships': 'Керування абонементами',
    '/admin/members': 'Керування клієнтами',
    '/admin/bookings': 'Керування записами',
    '/admin/users': 'Керування користувачами',
  }

  const currentTitle = pageTitles[location.pathname] ?? 'FitnessClub'

  function handleAuthAction() {
    if (isAuthenticated) {
      logout()
    } else {
      navigate('/login')
    }
  }

  return (
    <header className="topbar">
      <div>
        <p className="breadcrumb">
          FitnessClub / {currentTitle}
        </p>
      </div>

      <div className="header-user">
        {isAuthenticated && user && (
          <button
            className="header-user-name"
            onClick={() => navigate('/profile')}
          >
            {user.firstName} {user.lastName}
          </button>
        )}

        <button
          className="login-button"
          onClick={handleAuthAction}
        >
          {isAuthenticated ? 'Вийти' : 'Увійти'}
        </button>
      </div>
    </header>
  )
}

export default Header
