
import { NavLink } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

function Sidebar() {
  const { user, isAuthenticated } = useAuth()

  const initials = user
    ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase()
    : 'Г'

  const role = user?.roles.map((item) => {
    if (item === 'Administrator') return 'Адміністратор'
    if (item === 'User') return 'Учасник клубу'
    return item
  }).join(', ')

  const isAdmin = user?.roles.includes('Administrator') ?? false

  return (
    <aside className="sidebar">
      <div className="logo">
        <span className="logo-icon">F</span>
        <span>FitnessClub</span>
      </div>

      <nav className="navigation">
        <NavLink to="/" end className={({ isActive }) =>
          `nav-item ${isActive ? 'active' : ''}`
        }>
          <span>⌂</span> Головна
        </NavLink>

        <NavLink to="/workouts" className={({ isActive }) =>
          `nav-item ${isActive ? 'active' : ''}`
        }>
          <span>◈</span> Тренування
        </NavLink>

        <NavLink to="/memberships" className={({ isActive }) =>
          `nav-item ${isActive ? 'active' : ''}`
        }>
          <span>▣</span> Абонементи
        </NavLink>

        <NavLink to="/clubs" className={({ isActive }) =>
          `nav-item ${isActive ? 'active' : ''}`
        }>
          <span>⌖</span> Клуби
        </NavLink>

        {isAuthenticated && (
          <>
            <NavLink to="/bookings" className={({ isActive }) =>
              `nav-item ${isActive ? 'active' : ''}`
            }>
              <span>◷</span> {isAdmin ? 'Усі записи' : 'Мої записи'}
            </NavLink>

            <NavLink to="/profile" className={({ isActive }) =>
              `nav-item ${isActive ? 'active' : ''}`
            }>
              <span>◉</span> Мій профіль
            </NavLink>
          </>
        )}
      </nav>

      <div className="sidebar-bottom">
        <div className="profile-avatar">{initials}</div>

        <div>
          <p className="profile-name">
            {user ? `${user.firstName} ${user.lastName}` : 'Гість'}
          </p>
          <p className="profile-role">
            {isAuthenticated ? (role || 'Учасник клубу') : 'Перегляд без входу'}
          </p>
        </div>
      </div>
    </aside>
  )
}

export default Sidebar
