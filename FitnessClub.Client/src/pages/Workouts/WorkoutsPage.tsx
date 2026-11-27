
import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  fitnessClubService,
  type Workout,
  type WorkoutAvailability,
} from '../../api/fitnessClubService'
import { useAuth } from '../../contexts/AuthContext'
import { getDayName, getNextWorkoutDate } from '../../utils/dates'

const levels: Record<string, string> = {
  Beginner: 'Початковий',
  Intermediate: 'Середній',
  Advanced: 'Високий',
  '0': 'Початковий',
  '1': 'Середній',
  '2': 'Високий',
}

function getLevelName(level: number | string) {
  return levels[String(level)] ?? String(level)
}

function WorkoutsPage() {
  const { isAuthenticated } = useAuth()
  const navigate = useNavigate()

  const [workouts, setWorkouts] = useState<Workout[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const [selectedDates, setSelectedDates] = useState<Record<number, string>>({})
  const [availability, setAvailability] = useState<
    Record<number, WorkoutAvailability>
  >({})
  const [availabilityLoading, setAvailabilityLoading] = useState<
    Record<number, boolean>
  >({})

  const [bookingLoading, setBookingLoading] = useState<Record<number, boolean>>({})
  const [bookingMessages, setBookingMessages] = useState<Record<number, string>>({})

  useEffect(() => {
    fitnessClubService
      .getWorkouts()
      .then(setWorkouts)
      .catch(() => {
        setError('Не вдалося завантажити тренування.')
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  async function loadAvailability(workoutId: number, date: string) {
    setAvailabilityLoading((prev) => ({
      ...prev,
      [workoutId]: true,
    }))

    try {
      const result = await fitnessClubService.getWorkoutAvailability(
        workoutId,
        date,
      )

      setAvailability((prev) => ({
        ...prev,
        [workoutId]: result,
      }))
    } catch {
      setAvailability((prev) => {
        const updated = { ...prev }
        delete updated[workoutId]
        return updated
      })
    } finally {
      setAvailabilityLoading((prev) => ({
        ...prev,
        [workoutId]: false,
      }))
    }
  }

  useEffect(() => {
    workouts.forEach((workout) => {
      const date =
        selectedDates[workout.id] ??
        getNextWorkoutDate(workout.dayOfWeek)

      if (
        !availabilityLoading[workout.id] &&
        availability[workout.id]?.date !== date
      ) {
        loadAvailability(workout.id, date)
      }
    })
  }, [workouts, selectedDates])

  async function handleBooking(workoutId: number) {
    if (!isAuthenticated) {
      navigate('/login')
      return
    }

    const workout = workouts.find((item) => item.id === workoutId)

    if (!workout) {
      return
    }

    const date =
      selectedDates[workoutId] ??
      getNextWorkoutDate(workout.dayOfWeek)

    setBookingLoading((prev) => ({
      ...prev,
      [workoutId]: true,
    }))

    setBookingMessages((prev) => ({
      ...prev,
      [workoutId]: '',
    }))

    try {
      await fitnessClubService.createBooking({
        workoutId,
        date,
      })

      setBookingMessages((prev) => ({
        ...prev,
        [workoutId]: 'Ви успішно записалися на тренування!',
      }))

      await loadAvailability(workoutId, date)
    } catch (error) {
      setBookingMessages((prev) => ({
        ...prev,
        [workoutId]: error instanceof Error
          ? error.message
          : 'Не вдалося записатися на тренування.',
      }))
    } finally {
      setBookingLoading((prev) => ({
        ...prev,
        [workoutId]: false,
      }))
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження тренувань...</p>
  }

  if (error) {
    return <p className="page-message error-message">{error}</p>
  }

  return (
    <div className="workouts-page">
      <div className="workouts-header">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>Тренування</h1>
        <p className="workouts-subtitle">
          Переглядай доступні тренування та обирай заняття.
        </p>
      </div>

      {workouts.length === 0 ? (
        <p className="page-message">Тренувань поки немає.</p>
      ) : (
        <div className="workouts-list">
          {workouts.map((workout, index) => (
            <article className="training-card" key={workout.id}>
              <div className={`training-banner banner-${(index % 6) + 1}`}>
                <span className="training-category">
                  {getLevelName(workout.level)}
                </span>

                <span className="training-number">
                  {String(index + 1).padStart(2, '0')}
                </span>
              </div>

              <div className="training-content">
                <h3>{workout.name}</h3>

                <p>
                  {workout.description || 'Опис тренування відсутній.'}
                </p>

                <div className="workout-details">
                  <span>📅 {getDayName(workout.dayOfWeek)}</span>
                  <span>🕒 {workout.startTime}</span>
                  <span>⏱ {workout.durationMinutes} хв</span>
                  <span>👥 До {workout.maxParticipants} учасників</span>
                </div>

                <div className="workout-booking">
                  <label htmlFor={`date-${workout.id}`}>
                    Дата заняття
                  </label>

                  <input
                    id={`date-${workout.id}`}
                    type="date"
                    min={getNextWorkoutDate(workout.dayOfWeek)}
                    value={
                      selectedDates[workout.id] ??
                      getNextWorkoutDate(workout.dayOfWeek)
                    }
                    onChange={(event) => {
                      const date = event.target.value

                      setSelectedDates((prev) => ({
                        ...prev,
                        [workout.id]: date,
                      }))
                    }}
                  />

                  <div className="availability-info">
                    {availabilityLoading[workout.id] ? (
                      <span>Перевірка вільних місць...</span>
                    ) : availability[workout.id]?.date === (
                      selectedDates[workout.id] ??
                      getNextWorkoutDate(workout.dayOfWeek)
                    ) ? (
                      <span>
                        Вільних місць: {availability[workout.id].freePlaces}
                        {' '}з {availability[workout.id].maxParticipants}
                      </span>
                    ) : (
                      <span>Перевірка доступності...</span>
                    )}
                  </div>

                  {bookingMessages[workout.id] && (
                    <p className="booking-message">
                      {bookingMessages[workout.id]}
                    </p>
                  )}
                </div>

                <div className="training-footer">
                  <span className="duration">
                    Клуб №{workout.clubId}
                  </span>

                  <button
                    className="card-button"
                    onClick={() => handleBooking(workout.id)}
                    disabled={bookingLoading[workout.id]}
                  >
                    {bookingLoading[workout.id]
                      ? 'Записуємо...'
                      : isAuthenticated
                        ? 'Записатися'
                        : 'Увійти для запису'}
                  </button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  )
}

export default WorkoutsPage
