
import { useAuth } from '../../contexts/AuthContext'

function ProfilePage() {
  const { user } = useAuth()

  if (!user) {
    return <p>Не вдалося завантажити профіль.</p>
  }

  return (
    <section className="page-container">
      <div className="page-heading">
        <div>
          <h1>Мій профіль</h1>
          <p>Інформація про твій обліковий запис</p>
        </div>
      </div>

      <div className="profile-card">
        <div className="profile-avatar">
          {user.firstName.charAt(0).toUpperCase()}
        </div>

        <div className="profile-info">
          <h2>
            {user.firstName} {user.lastName}
          </h2>
          <p className="profile-email">{user.email}</p>
        </div>

        <div className="profile-details">
          <div className="profile-detail">
            <span>Ім'я</span>
            <strong>{user.firstName}</strong>
          </div>

          <div className="profile-detail">
            <span>Прізвище</span>
            <strong>{user.lastName}</strong>
          </div>

          <div className="profile-detail">
            <span>Електронна пошта</span>
            <strong>{user.email}</strong>
          </div>

          <div className="profile-detail">
            <span>Роль</span>
            <strong>
              {user.roles.join(', ') || 'Не визначена'}
            </strong>
          </div>
        </div>
      </div>
    </section>
  )
}

export default ProfilePage
