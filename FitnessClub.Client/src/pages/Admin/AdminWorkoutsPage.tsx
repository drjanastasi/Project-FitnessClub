import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Workout,
  type WorkoutFormData,
} from '../../api/fitnessClubService'

const days = [
  'Неділя',
  'Понеділок',
  'Вівторок',
  'Середа',
  'Четвер',
  'П’ятниця',
  'Субота',
]

const levels = [
  { value: 0, label: 'Початковий' },
  { value: 1, label: 'Середній' },
  { value: 2, label: 'Високий' },
]

const emptyForm: WorkoutFormData = {
  name: '',
  description: '',
  trainerId: 1,
  clubId: 1,
  dayOfWeek: 1,
  startTime: '18:00',
  durationMinutes: 60,
  level: 0,
  maxParticipants: 20,
}

function AdminWorkoutsPage() {
  const [workouts, setWorkouts] = useState<Workout[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const [form, setForm] = useState<WorkoutFormData>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [formVisible, setFormVisible] = useState(false)
  const [saving, setSaving] = useState(false)

  async function loadWorkouts() {
    setLoading(true)
    setError('')

    try {
      const result = await fitnessClubService.getWorkouts()
      setWorkouts(result)
    } catch {
      setError('Не вдалося завантажити тренування.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadWorkouts()
  }, [])

  function handleChange<K extends keyof WorkoutFormData>(
    field: K,
    value: WorkoutFormData[K],
  ) {
    setForm((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  function openCreateForm() {
    setForm(emptyForm)
    setEditingId(null)
    setMessage('')
    setFormVisible(true)
  }

  function openEditForm(workout: Workout) {
    setForm({
      name: workout.name,
      description: workout.description ?? '',
      trainerId: workout.trainerId,
      clubId: workout.clubId,
      dayOfWeek:
        typeof workout.dayOfWeek === 'number'
            ? workout.dayOfWeek
            : [
                'Sunday',
                'Monday',
                'Tuesday',
                'Wednesday',
                'Thursday',
                'Friday',
                'Saturday',
            ].indexOf(String(workout.dayOfWeek)),
      startTime: workout.startTime.slice(0, 5),
      durationMinutes: workout.durationMinutes,
      level:
        typeof workout.level === 'number'
            ? workout.level
            : workout.level === 'Beginner'
            ? 0
            : workout.level === 'Intermediate'
                ? 1
                : workout.level === 'Advanced'
                ? 2
                : 0,
      maxParticipants: workout.maxParticipants,
    })

    setEditingId(workout.id)
    setMessage('')
    setFormVisible(true)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError('')
    setMessage('')

    try {
      if (editingId === null) {
        await fitnessClubService.createWorkout(form)
        setMessage('Тренування успішно створено.')
      } else {
        await fitnessClubService.updateWorkout(editingId, form)
        setMessage('Тренування успішно оновлено.')
      }

      setFormVisible(false)
      setEditingId(null)
      setForm(emptyForm)

      await loadWorkouts()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Не вдалося зберегти тренування.',
      )
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id: number) {
    const confirmed = window.confirm(
      'Ви дійсно хочете видалити це тренування?',
    )

    if (!confirmed) {
      return
    }

    setError('')
    setMessage('')

    try {
      await fitnessClubService.deleteWorkout(id)
      setMessage('Тренування успішно видалено.')
      await loadWorkouts()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Не вдалося видалити тренування.',
      )
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження тренувань...</p>
  }

  return (
    <div className="workouts-page">
      <div className="workouts-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування тренуваннями</h1>
        <p className="workouts-subtitle">
          Створення, редагування та видалення тренувань.
        </p>
      </div>

      <button className="card-button" onClick={openCreateForm}>
        + Додати тренування
      </button>

      {error && <p className="error-message">{error}</p>}
      {message && <p className="booking-message">{message}</p>}

      {formVisible && (
        <form className="training-card" onSubmit={handleSubmit}>
          <div className="training-content">
            <h2>
              {editingId === null
                ? 'Нове тренування'
                : 'Редагування тренування'}
            </h2>

            <label>Назва</label>
            <input
              value={form.name}
              onChange={(e) => handleChange('name', e.target.value)}
              required
              minLength={2}
              maxLength={100}
            />

            <label>Опис</label>
            <textarea
              value={form.description ?? ''}
              onChange={(e) =>
                handleChange('description', e.target.value)
              }
              maxLength={500}
            />

            <label>Trainer ID</label>
            <input
              type="number"
              min={1}
              value={form.trainerId}
              onChange={(e) =>
                handleChange('trainerId', Number(e.target.value))
              }
              required
            />

            <label>Club ID</label>
            <input
              type="number"
              min={1}
              value={form.clubId}
              onChange={(e) =>
                handleChange('clubId', Number(e.target.value))
              }
              required
            />

            <label>День тижня</label>
            <select
              value={form.dayOfWeek}
              onChange={(e) =>
                handleChange('dayOfWeek', Number(e.target.value))
              }
            >
              {days.map((day, index) => (
                <option value={index} key={index}>
                  {day}
                </option>
              ))}
            </select>

            <label>Час початку</label>
            <input
              type="time"
              value={form.startTime}
              onChange={(e) => handleChange('startTime', e.target.value)}
              required
            />

            <label>Тривалість (хвилини)</label>
            <input
              type="number"
              min={15}
              max={240}
              value={form.durationMinutes}
              onChange={(e) =>
                handleChange('durationMinutes', Number(e.target.value))
              }
              required
            />

            <label>Рівень складності</label>
            <select
              value={form.level}
              onChange={(e) =>
                handleChange('level', Number(e.target.value))
              }
            >
              {levels.map((level) => (
                <option value={level.value} key={level.value}>
                  {level.label}
                </option>
              ))}
            </select>

            <label>Максимальна кількість учасників</label>
            <input
              type="number"
              min={1}
              max={100}
              value={form.maxParticipants}
              onChange={(e) =>
                handleChange('maxParticipants', Number(e.target.value))
              }
              required
            />

            <div className="training-footer">
              <button
                type="submit"
                className="card-button"
                disabled={saving}
              >
                {saving ? 'Збереження...' : 'Зберегти'}
              </button>

              <button
                type="button"
                className="card-button"
                onClick={() => setFormVisible(false)}
              >
                Скасувати
              </button>
            </div>
          </div>
        </form>
      )}

      {workouts.length === 0 ? (
        <p className="page-message">Тренувань поки немає.</p>
      ) : (
        <div className="workouts-list">
          {workouts.map((workout) => (
            <article className="training-card" key={workout.id}>
              <div className="training-content">
                <h3>{workout.name}</h3>

                <p>
                  {workout.description || 'Опис відсутній.'}
                </p>

                <div className="workout-details">
                  <span>
                    📅 {days[Number(workout.dayOfWeek)]}
                  </span>
                  <span>🕒 {workout.startTime}</span>
                  <span>⏱ {workout.durationMinutes} хв</span>
                  <span>👥 {workout.maxParticipants} учасників</span>
                  <span>🏋️ Trainer №{workout.trainerId}</span>
                  <span>📍 Club №{workout.clubId}</span>
                </div>

                <div className="training-footer">
                  <button
                    className="card-button"
                    onClick={() => openEditForm(workout)}
                  >
                    Редагувати
                  </button>

                  <button
                    className="card-button"
                    onClick={() => handleDelete(workout.id)}
                  >
                    Видалити
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

export default AdminWorkoutsPage