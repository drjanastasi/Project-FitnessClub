import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Booking,
} from '../../api/fitnessClubService'
import { useAuth } from '../../contexts/AuthContext'
import {
  formatDate,
  formatTime,
  getDayName,
  todayString,
} from '../../utils/dates'

const statusLabels: Record<string, string> = {
  Confirmed: 'Підтверджено',
  Cancelled: 'Скасовано',
}

interface Notice {
  text: string
  isError: boolean
}

function BookingsPage() {
  const { user } = useAuth()
  const isAdmin = user?.roles.includes('Administrator') ?? false

  const [bookings, setBookings] = useState<Booking[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState<Notice | null>(null)
  const [cancellingId, setCancellingId] = useState<number | null>(null)

  useEffect(() => {
    fitnessClubService
      .getBookings()
      .then(setBookings)
      .catch((err: unknown) => {
        setError(
          err instanceof Error ? err.message : 'Не вдалося завантажити записи.',
        )
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  async function handleCancel(booking: Booking) {
    const confirmed = window.confirm(
      `Скасувати запис на «${booking.workoutName}» ${formatDate(booking.date)}?`,
    )

    if (!confirmed) {
      return
    }

    setCancellingId(booking.id)
    setNotice(null)

    try {
      const updated = await fitnessClubService.cancelBooking(booking.id)

      setBookings((prev) =>
        prev.map((item) => (item.id === updated.id ? updated : item)),
      )
      setNotice({ text: 'Запис скасовано.', isError: false })
    } catch (err) {
      setNotice({
        text:
          err instanceof Error ? err.message : 'Не вдалося скасувати запис.',
        isError: true,
      })
    } finally {
      setCancellingId(null)
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження записів...</p>
  }

  if (error) {
    return <p className="page-message error-message">{error}</p>
  }

  const today = todayString()

  const isUpcoming = (booking: Booking) =>
    booking.status === 'Confirmed' && booking.date >= today

  const upcoming = bookings
    .filter(isUpcoming)
    .sort(
      (a, b) =>
        a.date.localeCompare(b.date) || a.startTime.localeCompare(b.startTime),
    )

  const history = bookings
    .filter((booking) => !isUpcoming(booking))
    .sort((a, b) => b.date.localeCompare(a.date))

  function renderCard(booking: Booking, canCancel: boolean) {
    return (
      <article className="booking-card" key={booking.id}>
        <div className="booking-info">
          <h3>{booking.workoutName}</h3>
          <div className="booking-meta">
            <span>
              📅 {formatDate(booking.date)} ({getDayName(booking.dayOfWeek)})
            </span>
            <span>🕒 {formatTime(booking.startTime)}</span>
            {isAdmin && <span>Клієнт №{booking.memberId}</span>}
          </div>
        </div>

        <div className="booking-actions">
          <span
            className={`booking-status ${booking.status.toLowerCase()}`}
          >
            {statusLabels[booking.status] ?? booking.status}
          </span>

          {canCancel && (
            <button
              type="button"
              className="booking-cancel-button"
              disabled={cancellingId === booking.id}
              onClick={() => handleCancel(booking)}
            >
              {cancellingId === booking.id ? 'Скасування...' : 'Скасувати'}
            </button>
          )}
        </div>
      </article>
    )
  }

  return (
    <div className="bookings-page">
      <div className="bookings-header">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>{isAdmin ? 'Усі записи' : 'Мої записи'}</h1>
        <p className="bookings-subtitle">
          {isAdmin
            ? 'Записи всіх клієнтів на тренування.'
            : 'Твої записи на тренування: майбутні та історія.'}
        </p>
      </div>

      {notice && (
        <p className={`booking-notice ${notice.isError ? 'error' : 'success'}`}>
          {notice.text}
        </p>
      )}

      {bookings.length === 0 ? (
        <p className="page-message">
          Записів поки немає. Обери тренування на сторінці «Тренування».
        </p>
      ) : (
        <>
          <section className="bookings-section">
            <h2>Майбутні</h2>
            {upcoming.length === 0 ? (
              <p className="page-message">Майбутніх записів немає.</p>
            ) : (
              <div className="bookings-list">
                {upcoming.map((booking) => renderCard(booking, true))}
              </div>
            )}
          </section>

          {history.length > 0 && (
            <section className="bookings-section">
              <h2>Історія</h2>
              <div className="bookings-list">
                {history.map((booking) => renderCard(booking, false))}
              </div>
            </section>
          )}
        </>
      )}
    </div>
  )
}

export default BookingsPage