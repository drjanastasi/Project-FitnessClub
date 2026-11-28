import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Club,
  type Workout,
  type WorkoutFormData,
} from '../../api/fitnessClubService'
import { formatTime, getDayIndex, getDayName } from '../../utils/dates'
import {
  errorMessage,
  getLevelIndex,
  getLevelName,
  levelOptions,
} from '../../utils/labels'
import './Admin.css'

// Тиждень показуємо з понеділка; значення — індекс дня (0 = неділя), як очікує API.
const weekOrder = [1, 2, 3, 4, 5, 6, 0]

const emptyForm: WorkoutFormData = {
  name: '',
  description: '',
  trainerId: 1,
  clubId: 0,
  dayOfWeek: 1,
  startTime: '18:00',
  durationMinutes: 60,
  level: 0,
  maxParticipants: 20,
}

function AdminWorkoutsPage() {
  const [workouts, setWorkouts] = useState<Workout[]>([])
  const [clubs, setClubs] = useState<Club[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')

  const [form, setForm] = useState<WorkoutFormData>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [formVisible, setFormVisible] = useState(false)
  const [saving, setSaving] = useState(false)

  async function loadWorkouts() {
    try {
      setWorkouts(await fitnessClubService.getWorkouts())
    } catch (err) {
      setError(errorMessage(err, 'Не вдалося завантажити тренування.'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadWorkouts()

    // Клуби потрібні для вибору в формі та для назв у списку.
    fitnessClubService
      .getClubs()
      .then(setClubs)
      .catch((err: unknown) => {
        setError(errorMessage(err, 'Не вдалося завантажити клуби.'))
      })
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
    setForm({ ...emptyForm, clubId: clubs[0]?.id ?? 0 })
    setEditingId(null)
    setError('')
    setMessage('')
    setFormVisible(true)
  }

  function openEditForm(workout: Workout) {
    setForm({
      name: workout.name,
      description: workout.description ?? '',
      trainerId: workout.trainerId,
      clubId: workout.clubId,
      dayOfWeek: getDayIndex(workout.dayOfWeek),
      startTime: formatTime(workout.startTime),
      durationMinutes: workout.durationMinutes,
      level: getLevelIndex(workout.level),
      maxParticipants: workout.maxParticipants,
    })

    setEditingId(workout.id)
    setError('')
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
      setError(errorMessage(err, 'Не вдалося зберегти тренування.'))
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(workout: Workout) {
    const confirmed = window.confirm(
      `Ви дійсно хочете видалити тренування «${workout.name}»?`,
    )

    if (!confirmed) {
      return
    }

    setError('')
    setMessage('')

    try {
      await fitnessClubService.deleteWorkout(workout.id)
      setMessage('Тренування успішно видалено.')

      if (editingId === workout.id) {
        setFormVisible(false)
        setEditingId(null)
        setForm(emptyForm)
      }

      await loadWorkouts()
    } catch (err) {
      setError(errorMessage(err, 'Не вдалося видалити тренування.'))
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження тренувань...</p>
  }

  const clubNames = new Map(clubs.map((club) => [club.id, club.name]))

  return (
    <div className="workouts-page">
      <div className="workouts-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування тренуваннями</h1>
        <p className="workouts-subtitle">
          Створення, редагування та видалення тренувань.
        </p>
      </div>

      <button className="admin-btn admin-btn-primary" onClick={openCreateForm}>
        + Додати тренування
      </button>

      {error && <p className="admin-notice error">{error}</p>}
      {message && <p className="admin-notice success">{message}</p>}

      {formVisible && (
        <form className="training-card admin-workout-form" onSubmit={handleSubmit}>
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
              onChange={(e) => handleChange('description', e.target.value)}
              maxLength={500}
            />

            <label>ID тренера</label>
            <input
              type="number"
              min={1}
              value={form.trainerId}
              onChange={(e) =>
                handleChange('trainerId', Number(e.target.value))
              }
              required
            />

            <label>Клуб</label>
            <select
              value={form.clubId}
              onChange={(e) => handleChange('clubId', Number(e.target.value))}
              required
            >
              <option value={0} disabled>
                Оберіть клуб
              </option>
              {clubs.map((club) => (
                <option value={club.id} key={club.id}>
                  {club.name}
                </option>
              ))}
            </select>

            <label>День тижня</label>
            <select
              value={form.dayOfWeek}
              onChange={(e) =>
                handleChange('dayOfWeek', Number(e.target.value))
              }
            >
              {weekOrder.map((index) => (
                <option value={index} key={index}>
                  {getDayName(index)}
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
              onChange={(e) => handleChange('level', Number(e.target.value))}
            >
              {levelOptions.map((level) => (
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
                disabled={saving || form.clubId === 0}
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

                <p>{workout.description || 'Опис відсутній.'}</p>

                <div className="workout-details">
                  <span>📅 {getDayName(workout.dayOfWeek)}</span>
                  <span>🕒 {formatTime(workout.startTime)}</span>
                  <span>⏱ {workout.durationMinutes} хв</span>
                  <span>📊 {getLevelName(workout.level)}</span>
                  <span>👥 {workout.maxParticipants} учасників</span>
                  <span>🏋️ Тренер №{workout.trainerId}</span>
                  <span>
                    📍 {clubNames.get(workout.clubId) ?? `Клуб №${workout.clubId}`}
                  </span>
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
                    onClick={() => handleDelete(workout)}
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
