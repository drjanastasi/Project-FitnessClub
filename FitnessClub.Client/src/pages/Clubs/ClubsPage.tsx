
import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Club,
} from '../../api/fitnessClubService'

function ClubsPage() {
  const [clubs, setClubs] = useState<Club[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    fitnessClubService
      .getClubs()
      .then(setClubs)
      .catch(() => {
        setError('Не вдалося завантажити список клубів.')
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  if (loading) {
    return <p className="page-message">Завантаження клубів...</p>
  }

  if (error) {
    return <p className="page-message error-message">{error}</p>
  }

  return (
    <div className="clubs-page">
      <div className="clubs-header">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>Наші клуби</h1>
        <p className="clubs-subtitle">
          Переглядай клуби та обирай місце для занять.
        </p>
      </div>

      {clubs.length === 0 ? (
        <p className="page-message">Клубів поки немає.</p>
      ) : (
        <div className="clubs-grid">
          {clubs.map((club, index) => (
            <article className="club-card" key={club.id}>
              <div className="club-banner">
                <span className="club-number">
                  {String(index + 1).padStart(2, '0')}
                </span>
                <span className="club-symbol">FC</span>
              </div>

              <div className="club-content">
                <h3>{club.name}</h3>

                <p className="club-address">
                  <span>⌖</span> {club.address}
                </p>

                <p className="club-description">
                  {club.description || 'Опис клубу відсутній.'}
                </p>

                <div className="club-contact">
                  <p>Телефон: {club.phone}</p>
                  <p>Email: {club.email}</p>
                  <p>Години роботи: {club.workHours || 'Не вказано'}</p>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}

export default ClubsPage
