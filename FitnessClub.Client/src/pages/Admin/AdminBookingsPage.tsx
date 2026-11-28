import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import {
  fitnessClubService,
  type Booking,
  type BookingFilters,
  type Club,
  type Member,
  type Workout,
  type WorkoutAvailability,
} from '../../api/fitnessClubService'
import {
  formatDate,
  formatTime,
  getDayIndex,
  getDayName,
  getNextWorkoutDate,
  todayString,
} from '../../utils/dates'
import { errorMessage, getBookingStatusName } from '../../utils/labels'
import './Admin.css'

interface Notice {
  text: string
  isError: boolean
}

interface BookingFormState {
  memberId: string
  workoutId: string
  date: string
}

type SortOrder = 'newest' | 'oldest'

function weekdayOf(date: string): number {
  return new Date(`${date}T00:00:00`).getDay()
}

function AdminBookingsPage() {
  const [searchParams] = useSearchParams()

  const [bookings, setBookings] = useState<Booking[]>([])
  const [members, setMembers] = useState<Member[]>([])
  const [workouts, setWorkouts] = useState<Workout[]>([])
  const [clubs, setClubs] = useState<Club[]>([])
  const [loading, setLoading] = useState(true)
  const [notice, setNotice] = useState<Notice | null>(null)
  const [busyId, setBusyId] = useState<number | null>(null)

  // Фільтри списку записів.
  const [statusFilter, setStatusFilter] = useState('')
  const [memberFilter, setMemberFilter] = useState('')
  const [workoutFilter, setWorkoutFilter] = useState('')
  const [clubFilter, setClubFilter] = useState('')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [sortOrder, setSortOrder] = useState<SortOrder>('newest')
  const [reloadKey, setReloadKey] = useState(0)

  // Форма створення запису (може бути заповнена з посилання зі сторінки «Тренування»).
  const [form, setForm] = useState<BookingFormState>({
    memberId: '',
    workoutId: searchParams.get('workoutId') ?? '',
    date: searchParams.get('date') ?? '',
  })
  const [formVisible, setFormVisible] = useState(searchParams.has('workoutId'))
  const [saving, setSaving] = useState(false)
  const [availability, setAvailability] = useState<WorkoutAvailability | null>(
    null,
  )
  const [availabilityError, setAvailabilityError] = useState('')

  // Довідники для списків вибору.
  useEffect(() => {
    Promise.all([
      fitnessClubService.getMembers(),
      fitnessClubService.getWorkouts(),
      fitnessClubService.getClubs(),
    ])
      .then(([membersData, workoutsData, clubsData]) => {
        setMembers(membersData)
        setWorkouts(workoutsData)
        setClubs(clubsData)
      })
      .catch((err: unknown) => {
        setNotice({
          text: errorMessage(err, 'Не вдалося завантажити довідники.'),
          isError: true,
        })
      })
  }, [])

  // Список записів: оновлюється при зміні фільтрів.
  useEffect(() => {
    let cancelled = false

    const filters: BookingFilters = {
      status: statusFilter || undefined,
      memberId: memberFilter ? Number(memberFilter) : undefined,
      workoutId: workoutFilter ? Number(workoutFilter) : undefined,
      clubId: clubFilter ? Number(clubFilter) : undefined,
      dateFrom: dateFrom || undefined,
      dateTo: dateTo || undefined,
      sortBy: 'date',
      desc: sortOrder === 'newest',
    }

    fitnessClubService
      .getBookings(filters)
      .then((data) => {
        if (!cancelled) setBookings(data)
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setNotice({
            text: errorMessage(err, 'Не вдалося завантажити записи.'),
            isError: true,
          })
        }
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
    }
  }, [
    statusFilter,
    memberFilter,
    workoutFilter,
    clubFilter,
    dateFrom,
    dateTo,
    sortOrder,
    reloadKey,
  ])

  // Вільні місця для обраного тренування й дати.
  const selectedWorkout = workouts.find(
    (workout) => String(workout.id) === form.workoutId,
  )
  const dateMatchesWorkout =
    selectedWorkout !== undefined &&
    form.date !== '' &&
    weekdayOf(form.date) === getDayIndex(selectedWorkout.dayOfWeek)

  useEffect(() => {
    if (!selectedWorkout || !form.date || !dateMatchesWorkout) {
      return
    }

    let cancelled = false

    fitnessClubService
      .getWorkoutAvailability(selectedWorkout.id, form.date)
      .then((data) => {
        if (!cancelled) {
          setAvailability(data)
          setAvailabilityError('')
        }
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          setAvailability(null)
          setAvailabilityError(
            errorMessage(err, 'Не вдалося перевірити вільні місця.'),
          )
        }
      })

    return () => {
      cancelled = true
    }
  }, [selectedWorkout, form.date, dateMatchesWorkout, reloadKey])

  function reload() {
    setReloadKey((key) => key + 1)
  }

  function handleWorkoutChange(workoutId: string) {
    const workout = workouts.find((item) => String(item.id) === workoutId)

    setAvailability(null)
    setAvailabilityError('')

    setForm((prev) => ({
      ...prev,
      workoutId,
      // Підставляємо найближчу дату потрібного дня тижня.
      date: workout ? getNextWorkoutDate(workout.dayOfWeek) : prev.date,
    }))
  }

  function handleDateChange(date: string) {
    setAvailability(null)
    setAvailabilityError('')
    setForm((prev) => ({ ...prev, date }))
  }

  function openForm() {
    setNotice(null)
    setFormVisible(true)
  }

  function closeForm() {
    setFormVisible(false)
    setForm({ memberId: '', workoutId: '', date: '' })
    setAvailability(null)
    setAvailabilityError('')
  }

  async function handleCreate(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setNotice(null)

    try {
      await fitnessClubService.createBooking({
        memberId: Number(form.memberId),
        workoutId: Number(form.workoutId),
        date: form.date,
      })

      setNotice({ text: 'Клієнта записано на тренування.', isError: false })
      closeForm()
      reload()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося створити запис.'),
        isError: true,
      })
    } finally {
      setSaving(false)
    }
  }

  async function handleCancel(booking: Booking) {
    if (
      !window.confirm(
        `Скасувати запис на «${booking.workoutName}» ${formatDate(booking.date)}?`,
      )
    ) {
      return
    }

    setBusyId(booking.id)
    setNotice(null)

    try {
      const updated = await fitnessClubService.cancelBooking(booking.id)

      setBookings((prev) =>
        prev.map((item) => (item.id === updated.id ? updated : item)),
      )
      setNotice({ text: 'Запис скасовано.', isError: false })
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося скасувати запис.'),
        isError: true,
      })
    } finally {
      setBusyId(null)
    }
  }

  async function handleDelete(booking: Booking) {
    if (
      !window.confirm(
        `Остаточно видалити запис №${booking.id}? Цю дію не можна скасувати.`,
      )
    ) {
      return
    }

    setBusyId(booking.id)
    setNotice(null)

    try {
      await fitnessClubService.deleteBooking(booking.id)
      setBookings((prev) => prev.filter((item) => item.id !== booking.id))
      setNotice({ text: 'Запис видалено.', isError: false })
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося видалити запис.'),
        isError: true,
      })
    } finally {
      setBusyId(null)
    }
  }

  function resetFilters() {
    setStatusFilter('')
    setMemberFilter('')
    setWorkoutFilter('')
    setClubFilter('')
    setDateFrom('')
    setDateTo('')
    setSortOrder('newest')
  }

  if (loading) {
    return <p className="page-message">Завантаження записів...</p>
  }

  const today = todayString()
  const memberNames = new Map(
    members.map((member) => [
      member.id,
      `${member.firstName} ${member.lastName}`,
    ]),
  )
  const hasFilters = Boolean(
    statusFilter ||
      memberFilter ||
      workoutFilter ||
      clubFilter ||
      dateFrom ||
      dateTo,
  )

  const activeMembers = members.filter((member) => member.isActive)

  return (
    <div className="admin-page">
      <div className="admin-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування записами</h1>
        <p className="admin-subtitle">
          Усі записи клієнтів на тренування: фільтрація, створення, скасування
          й остаточне видалення.
        </p>
      </div>

      <div className="admin-toolbar">
        <span className="admin-toolbar-spacer" />
        <button
          type="button"
          className="admin-btn admin-btn-primary"
          onClick={openForm}
        >
          + Записати клієнта
        </button>
      </div>

      {notice && (
        <p className={`admin-notice ${notice.isError ? 'error' : 'success'}`}>
          {notice.text}
        </p>
      )}

      {formVisible && (
        <form className="admin-panel" onSubmit={handleCreate}>
          <h2>Новий запис</h2>

          <div className="admin-form-grid">
            <label className="admin-field">
              Клієнт
              <select
                value={form.memberId}
                onChange={(e) =>
                  setForm((prev) => ({ ...prev, memberId: e.target.value }))
                }
                required
              >
                <option value="" disabled>
                  Оберіть клієнта
                </option>
                {activeMembers.map((member) => (
                  <option value={member.id} key={member.id}>
                    {member.firstName} {member.lastName} ({member.email})
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field">
              Тренування
              <select
                value={form.workoutId}
                onChange={(e) => handleWorkoutChange(e.target.value)}
                required
              >
                <option value="" disabled>
                  Оберіть тренування
                </option>
                {workouts.map((workout) => (
                  <option value={workout.id} key={workout.id}>
                    {workout.name} — {getDayName(workout.dayOfWeek)},{' '}
                    {formatTime(workout.startTime)}
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field">
              Дата заняття
              <input
                type="date"
                min={today}
                value={form.date}
                onChange={(e) => handleDateChange(e.target.value)}
                required
              />
            </label>

            <div className="admin-field">
              Вільні місця
              <span className="admin-table-muted" style={{ marginTop: 10 }}>
                {!selectedWorkout || !form.date
                  ? 'Оберіть тренування й дату.'
                  : !dateMatchesWorkout
                    ? `Це тренування проходить у такий день: ${getDayName(selectedWorkout.dayOfWeek)}.`
                    : availabilityError
                      ? availabilityError
                      : availability &&
                          availability.workoutId === selectedWorkout.id &&
                          availability.date === form.date
                        ? `${availability.freePlaces} з ${availability.maxParticipants}`
                        : 'Перевірка...'}
              </span>
            </div>
          </div>

          <div className="admin-form-actions">
            <button
              type="submit"
              className="admin-btn admin-btn-primary"
              disabled={saving || (!!form.workoutId && !dateMatchesWorkout)}
            >
              {saving ? 'Збереження...' : 'Записати'}
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

      <div className="admin-toolbar">
        <label className="admin-field">
          Статус
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
          >
            <option value="">Усі</option>
            <option value="Confirmed">Підтверджені</option>
            <option value="Cancelled">Скасовані</option>
          </select>
        </label>

        <label className="admin-field">
          Клієнт
          <select
            value={memberFilter}
            onChange={(e) => setMemberFilter(e.target.value)}
          >
            <option value="">Усі клієнти</option>
            {members.map((member) => (
              <option value={member.id} key={member.id}>
                {member.firstName} {member.lastName}
              </option>
            ))}
          </select>
        </label>

        <label className="admin-field">
          Тренування
          <select
            value={workoutFilter}
            onChange={(e) => setWorkoutFilter(e.target.value)}
          >
            <option value="">Усі тренування</option>
            {workouts.map((workout) => (
              <option value={workout.id} key={workout.id}>
                {workout.name} — {getDayName(workout.dayOfWeek)}
              </option>
            ))}
          </select>
        </label>

        <label className="admin-field">
          Клуб
          <select
            value={clubFilter}
            onChange={(e) => setClubFilter(e.target.value)}
          >
            <option value="">Усі клуби</option>
            {clubs.map((club) => (
              <option value={club.id} key={club.id}>
                {club.name}
              </option>
            ))}
          </select>
        </label>

        <label className="admin-field">
          Дата від
          <input
            type="date"
            value={dateFrom}
            max={dateTo || undefined}
            onChange={(e) => setDateFrom(e.target.value)}
          />
        </label>

        <label className="admin-field">
          Дата до
          <input
            type="date"
            value={dateTo}
            min={dateFrom || undefined}
            onChange={(e) => setDateTo(e.target.value)}
          />
        </label>

        <label className="admin-field">
          Порядок
          <select
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value as SortOrder)}
          >
            <option value="newest">Спочатку нові</option>
            <option value="oldest">Спочатку старі</option>
          </select>
        </label>

        {hasFilters && (
          <button
            type="button"
            className="admin-btn admin-btn-secondary"
            onClick={resetFilters}
          >
            Скинути фільтри
          </button>
        )}
      </div>

      {bookings.length === 0 ? (
        <p className="admin-empty">
          {hasFilters
            ? 'За вказаними умовами записів не знайдено.'
            : 'Записів поки немає.'}
        </p>
      ) : (
        <>
          <p className="admin-count">Записів: {bookings.length}</p>

          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>№</th>
                  <th>Клієнт</th>
                  <th>Тренування</th>
                  <th>Дата заняття</th>
                  <th>Статус</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {bookings.map((booking) => {
                  const canCancel =
                    booking.status === 'Confirmed' && booking.date >= today

                  return (
                    <tr key={booking.id}>
                      <td>{booking.id}</td>
                      <td>
                        <span className="admin-table-title">
                          {memberNames.get(booking.memberId) ??
                            `Клієнт №${booking.memberId}`}
                        </span>
                        <span className="admin-table-muted">
                          Створено: {formatDate(booking.createdAt.slice(0, 10))}
                        </span>
                      </td>
                      <td>
                        <span className="admin-table-title">
                          {booking.workoutName}
                        </span>
                        <span className="admin-table-muted">
                          {getDayName(booking.dayOfWeek)},{' '}
                          {formatTime(booking.startTime)}
                        </span>
                      </td>
                      <td>{formatDate(booking.date)}</td>
                      <td>
                        <span
                          className={`admin-badge ${
                            booking.status === 'Confirmed' ? 'green' : 'red'
                          }`}
                        >
                          {getBookingStatusName(booking.status)}
                        </span>
                      </td>
                      <td>
                        <div className="admin-row-actions">
                          {canCancel && (
                            <button
                              type="button"
                              className="admin-btn admin-btn-secondary admin-btn-small"
                              disabled={busyId === booking.id}
                              onClick={() => handleCancel(booking)}
                            >
                              Скасувати
                            </button>
                          )}
                          <button
                            type="button"
                            className="admin-btn admin-btn-danger admin-btn-small"
                            disabled={busyId === booking.id}
                            onClick={() => handleDelete(booking)}
                          >
                            Видалити
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}

export default AdminBookingsPage
