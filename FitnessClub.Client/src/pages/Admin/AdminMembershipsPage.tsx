import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Membership,
  type MembershipDetails,
  type MembershipFormData,
  type Workout,
} from '../../api/fitnessClubService'
import { formatTime, getDayName } from '../../utils/dates'
import { errorMessage, getLevelName } from '../../utils/labels'
import './Admin.css'

interface MembershipFormState {
  name: string
  description: string
  price: string
  durationDays: string
  isActive: boolean
}

interface Notice {
  text: string
  isError: boolean
}

const emptyForm: MembershipFormState = {
  name: '',
  description: '',
  price: '',
  durationDays: '30',
  isActive: true,
}

function workoutLabel(workout: {
  name: string
  dayOfWeek: number | string
  startTime: string
}) {
  return `${workout.name} — ${getDayName(workout.dayOfWeek)}, ${formatTime(workout.startTime)}`
}

function AdminMembershipsPage() {
  const [memberships, setMemberships] = useState<Membership[]>([])
  const [loading, setLoading] = useState(true)
  const [notice, setNotice] = useState<Notice | null>(null)

  const [form, setForm] = useState<MembershipFormState>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [formVisible, setFormVisible] = useState(false)
  const [saving, setSaving] = useState(false)

  const [details, setDetails] = useState<MembershipDetails | null>(null)
  const [allWorkouts, setAllWorkouts] = useState<Workout[]>([])
  const [workoutToAdd, setWorkoutToAdd] = useState('')
  const [workoutBusy, setWorkoutBusy] = useState(false)

  async function loadMemberships() {
    try {
      setMemberships(await fitnessClubService.getMemberships())
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося завантажити абонементи.'),
        isError: true,
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadMemberships()
  }, [])

  function handleChange<K extends keyof MembershipFormState>(
    field: K,
    value: MembershipFormState[K],
  ) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  function openCreateForm() {
    setForm(emptyForm)
    setEditingId(null)
    setNotice(null)
    setFormVisible(true)
  }

  function openEditForm(membership: Membership) {
    setForm({
      name: membership.name,
      description: membership.description ?? '',
      price: String(membership.price),
      durationDays: String(membership.durationDays),
      isActive: membership.isActive,
    })
    setEditingId(membership.id)
    setNotice(null)
    setFormVisible(true)
  }

  function closeForm() {
    setFormVisible(false)
    setEditingId(null)
    setForm(emptyForm)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setNotice(null)

    const data: MembershipFormData = {
      name: form.name.trim(),
      description: form.description.trim() || null,
      price: Number(form.price),
      durationDays: Number(form.durationDays),
      isActive: form.isActive,
    }

    try {
      if (editingId === null) {
        await fitnessClubService.createMembership(data)
        setNotice({ text: 'Абонемент успішно створено.', isError: false })
      } else {
        await fitnessClubService.updateMembership(editingId, data)
        setNotice({ text: 'Абонемент успішно оновлено.', isError: false })
      }

      closeForm()
      await loadMemberships()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося зберегти абонемент.'),
        isError: true,
      })
    } finally {
      setSaving(false)
    }
  }

  async function handleToggleStatus(membership: Membership) {
    setNotice(null)

    try {
      await fitnessClubService.updateMembershipStatus(
        membership.id,
        !membership.isActive,
      )
      setNotice({
        text: membership.isActive
          ? 'Абонемент деактивовано.'
          : 'Абонемент активовано.',
        isError: false,
      })
      await loadMemberships()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося змінити статус абонемента.'),
        isError: true,
      })
    }
  }

  async function handleDelete(membership: Membership) {
    if (
      !window.confirm(`Ви дійсно хочете видалити абонемент «${membership.name}»?`)
    ) {
      return
    }

    setNotice(null)

    try {
      await fitnessClubService.deleteMembership(membership.id)
      setNotice({ text: 'Абонемент успішно видалено.', isError: false })

      if (editingId === membership.id) closeForm()
      if (details?.id === membership.id) setDetails(null)

      await loadMemberships()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося видалити абонемент.'),
        isError: true,
      })
    }
  }

  async function openWorkouts(membership: Membership) {
    setNotice(null)
    setWorkoutToAdd('')

    try {
      const [membershipDetails, workouts] = await Promise.all([
        fitnessClubService.getMembership(membership.id),
        allWorkouts.length > 0
          ? Promise.resolve(allWorkouts)
          : fitnessClubService.getWorkouts(),
      ])

      setAllWorkouts(workouts)
      setDetails(membershipDetails)
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося завантажити склад абонемента.'),
        isError: true,
      })
    }
  }

  async function handleAddWorkout() {
    if (!details || !workoutToAdd) return

    setWorkoutBusy(true)
    setNotice(null)

    try {
      const updated = await fitnessClubService.addWorkoutToMembership(
        details.id,
        Number(workoutToAdd),
      )
      setDetails(updated)
      setWorkoutToAdd('')
      await loadMemberships()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося додати тренування в абонемент.'),
        isError: true,
      })
    } finally {
      setWorkoutBusy(false)
    }
  }

  async function handleRemoveWorkout(workoutId: number) {
    if (!details) return

    setWorkoutBusy(true)
    setNotice(null)

    try {
      await fitnessClubService.removeWorkoutFromMembership(details.id, workoutId)
      setDetails(await fitnessClubService.getMembership(details.id))
      await loadMemberships()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося прибрати тренування з абонемента.'),
        isError: true,
      })
    } finally {
      setWorkoutBusy(false)
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження абонементів...</p>
  }

  const includedIds = new Set(details?.workouts.map((workout) => workout.id))
  const availableWorkouts = allWorkouts.filter(
    (workout) => !includedIds.has(workout.id),
  )

  return (
    <div className="admin-page">
      <div className="admin-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування абонементами</h1>
        <p className="admin-subtitle">
          Створення, зміна, активація й видалення абонементів, а також склад
          тренувань, що входять до кожного з них.
        </p>
      </div>

      <div className="admin-toolbar">
        <button
          type="button"
          className="admin-btn admin-btn-primary"
          onClick={openCreateForm}
        >
          + Додати абонемент
        </button>
      </div>

      {notice && (
        <p className={`admin-notice ${notice.isError ? 'error' : 'success'}`}>
          {notice.text}
        </p>
      )}

      {formVisible && (
        <form className="admin-panel" onSubmit={handleSubmit}>
          <h2>
            {editingId === null ? 'Новий абонемент' : 'Редагування абонемента'}
          </h2>

          <div className="admin-form-grid">
            <label className="admin-field admin-field-wide">
              Назва
              <input
                value={form.name}
                onChange={(e) => handleChange('name', e.target.value)}
                required
                minLength={2}
                maxLength={100}
              />
            </label>

            <label className="admin-field admin-field-wide">
              Опис
              <textarea
                value={form.description}
                onChange={(e) => handleChange('description', e.target.value)}
                maxLength={500}
              />
            </label>

            <label className="admin-field">
              Ціна, грн
              <input
                type="number"
                min={1}
                max={100000}
                step="0.01"
                value={form.price}
                onChange={(e) => handleChange('price', e.target.value)}
                required
              />
            </label>

            <label className="admin-field">
              Тривалість, днів
              <input
                type="number"
                min={1}
                max={365}
                value={form.durationDays}
                onChange={(e) => handleChange('durationDays', e.target.value)}
                required
              />
            </label>

            <label className="admin-field admin-field-check">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => handleChange('isActive', e.target.checked)}
              />
              Активний (доступний клієнтам)
            </label>
          </div>

          <div className="admin-form-actions">
            <button
              type="submit"
              className="admin-btn admin-btn-primary"
              disabled={saving}
            >
              {saving ? 'Збереження...' : 'Зберегти'}
            </button>
            <button
              type="button"
              className="admin-btn admin-btn-secondary"
              onClick={closeForm}
            >
              Скасувати
            </button>
          </div>
        </form>
      )}

      {details && (
        <section className="membership-workouts">
          <div className="membership-workouts-head">
            <h2>Склад абонемента «{details.name}»</h2>
            <button
              type="button"
              className="admin-btn admin-btn-secondary admin-btn-small"
              onClick={() => setDetails(null)}
            >
              Закрити
            </button>
          </div>

          {details.workouts.length === 0 ? (
            <p className="admin-count">
              До цього абонемента ще не додано жодного тренування.
            </p>
          ) : (
            <ul className="membership-workout-list">
              {details.workouts.map((workout) => (
                <li className="membership-workout-item" key={workout.id}>
                  <span>
                    {workoutLabel(workout)} · {getLevelName(workout.level)}
                  </span>
                  <button
                    type="button"
                    className="admin-btn admin-btn-danger admin-btn-small"
                    disabled={workoutBusy}
                    onClick={() => handleRemoveWorkout(workout.id)}
                  >
                    Прибрати
                  </button>
                </li>
              ))}
            </ul>
          )}

          <div className="membership-workout-add">
            <label className="admin-field">
              Додати тренування
              <select
                value={workoutToAdd}
                onChange={(e) => setWorkoutToAdd(e.target.value)}
              >
                <option value="">
                  {availableWorkouts.length === 0
                    ? 'Усі тренування вже додано'
                    : 'Оберіть тренування'}
                </option>
                {availableWorkouts.map((workout) => (
                  <option value={workout.id} key={workout.id}>
                    {workoutLabel(workout)}
                  </option>
                ))}
              </select>
            </label>

            <button
              type="button"
              className="admin-btn admin-btn-primary"
              disabled={workoutBusy || !workoutToAdd}
              onClick={handleAddWorkout}
            >
              Додати
            </button>
          </div>
        </section>
      )}

      {memberships.length === 0 ? (
        <p className="admin-empty">Абонементів поки немає.</p>
      ) : (
        <>
          <p className="admin-count">Усього абонементів: {memberships.length}</p>

          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Абонемент</th>
                  <th>Ціна</th>
                  <th>Тривалість</th>
                  <th>Тренувань</th>
                  <th>Статус</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {memberships.map((membership) => (
                  <tr key={membership.id}>
                    <td>
                      <span className="admin-table-title">{membership.name}</span>
                      <span className="admin-table-muted">
                        {membership.description || 'Без опису'}
                      </span>
                    </td>
                    <td>{membership.price.toLocaleString('uk-UA')} грн</td>
                    <td>{membership.durationDays} дн.</td>
                    <td>{membership.workoutsCount}</td>
                    <td>
                      <span
                        className={`admin-badge ${membership.isActive ? 'green' : 'gray'}`}
                      >
                        {membership.isActive ? 'Активний' : 'Неактивний'}
                      </span>
                    </td>
                    <td>
                      <div className="admin-row-actions">
                        <button
                          type="button"
                          className="admin-btn admin-btn-secondary admin-btn-small"
                          onClick={() => openWorkouts(membership)}
                        >
                          Склад
                        </button>
                        <button
                          type="button"
                          className="admin-btn admin-btn-secondary admin-btn-small"
                          onClick={() => handleToggleStatus(membership)}
                        >
                          {membership.isActive ? 'Деактивувати' : 'Активувати'}
                        </button>
                        <button
                          type="button"
                          className="admin-btn admin-btn-edit admin-btn-small"
                          onClick={() => openEditForm(membership)}
                        >
                          Редагувати
                        </button>
                        <button
                          type="button"
                          className="admin-btn admin-btn-danger admin-btn-small"
                          onClick={() => handleDelete(membership)}
                        >
                          Видалити
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}

export default AdminMembershipsPage
