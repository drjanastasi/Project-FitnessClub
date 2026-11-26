
import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Membership,
} from '../../api/fitnessClubService'

function MembershipsPage() {
  const [memberships, setMemberships] = useState<Membership[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    fitnessClubService
      .getMemberships()
      .then(setMemberships)
      .catch(() => {
        setError('Не вдалося завантажити абонементи.')
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  if (loading) {
    return <p className="page-message">Завантаження абонементів...</p>
  }

  if (error) {
    return <p className="page-message error-message">{error}</p>
  }

  const activeMemberships = memberships.filter(
    (membership) => membership.isActive,
  )

  return (
    <div className="memberships-page">
      <div className="memberships-header">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>Абонементи</h1>
        <p className="memberships-subtitle">
          Переглядай доступні варіанти абонементів.
        </p>
      </div>

      {activeMemberships.length === 0 ? (
        <p className="page-message">Активних абонементів поки немає.</p>
      ) : (
        <div className="memberships-grid">
          {activeMemberships.map((membership, index) => (
            <article className="membership-card" key={membership.id}>
              <div
                className={`membership-banner membership-banner-${String(
                  (index % 3) + 1,
                ).padStart(2, '0')}`}
              >
                <span className="membership-number">
                  {String(index + 1).padStart(2, '0')}
                </span>
                <span className="membership-symbol">FC</span>
              </div>

              <div className="membership-content">
                <h3>{membership.name}</h3>

                <div className="membership-price">
                  <span className="price-value">
                    {membership.price.toLocaleString('uk-UA')} грн
                  </span>
                  <span className="price-period">
                    на {membership.durationDays} дн.
                  </span>
                </div>

                <p className="membership-description">
                  {membership.description || 'Опис абонемента відсутній.'}
                </p>

                <ul className="membership-features">
                  <li>
                    <span className="feature-check">✓</span>
                    Тривалість: {membership.durationDays} днів
                  </li>
                  <li>
                    <span className="feature-check">✓</span>
                    Доступно тренувань: {membership.workoutsCount}
                  </li>
                  <li>
                    <span className="feature-check">✓</span>
                    Активний абонемент
                  </li>
                </ul>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}

export default MembershipsPage
