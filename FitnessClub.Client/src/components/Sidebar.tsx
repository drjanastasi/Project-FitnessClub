
import { NavLink } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

const adminLinks = [
  { to: '/admin/workouts', label: 'Тренування' },
  { to: '/admin/clubs', label: 'Клуби' },
  { to: '/admin/memberships', label: 'Абонементи' },
  { to: '/admin/members', label: 'Клієнти' },
  { to: '/admin/bookings', label: 'Записи' },
  { to: '/admin/users', label: 'Користувачі' },
]

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

        {isAuthenticated && !isAdmin && (
          <NavLink to="/bookings" className={({ isActive }) =>
            `nav-item ${isActive ? 'active' : ''}`
          }>
            <span>◷</span> Мої записи
          </NavLink>
        )}

        {isAuthenticated && (
          <NavLink to="/profile" className={({ isActive }) =>
            `nav-item ${isActive ? 'active' : ''}`
          }>
            <span>◉</span> Мій профіль
          </NavLink>
        )}

        {isAdmin && (
          <>
            <p className="nav-section-title">Адміністрування</p>

            {adminLinks.map((link) => (
              <NavLink
                key={link.to}
                to={link.to}
                className={({ isActive }) =>
                  `nav-item ${isActive ? 'active' : ''}`
                }
              >
                <span>⚙</span> {link.label}
              </NavLink>
            ))}
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
