import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type Member,
  type MemberFormData,
  type Membership,
} from '../../api/fitnessClubService'
import { formatDate, todayString } from '../../utils/dates'
import { errorMessage } from '../../utils/labels'
import './Admin.css'

interface MemberFormState {
  firstName: string
  lastName: string
  email: string
  phone: string
  birthDate: string
  membershipId: string
  isActive: boolean
}

interface Notice {
  text: string
  isError: boolean
}

type StatusFilter = 'all' | 'active' | 'inactive'

const emptyForm: MemberFormState = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  birthDate: '',
  membershipId: '',
  isActive: true,
}

function AdminMembersPage() {
  const [members, setMembers] = useState<Member[]>([])
  const [memberships, setMemberships] = useState<Membership[]>([])
  const [loading, setLoading] = useState(true)
  const [notice, setNotice] = useState<Notice | null>(null)

  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<StatusFilter>('all')
  const [reloadKey, setReloadKey] = useState(0)

  const [form, setForm] = useState<MemberFormState>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [formVisible, setFormVisible] = useState(false)
  const [saving, setSaving] = useState(false)

  // Абонементи потрібні для вибору в формі та для назв у таблиці.
  useEffect(() => {
    fitnessClubService
      .getMemberships()
      .then(setMemberships)
      .catch((err: unknown) => {
        setNotice({
          text: errorMessage(err, 'Не вдалося завантажити абонементи.'),
          isError: true,
        })
      })
  }, [])

  // Список клієнтів: оновлюється при зміні пошуку чи фільтра (пошук із невеликою затримкою).
  useEffect(() => {
    let cancelled = false

    const timer = setTimeout(
      async () => {
        try {
          const data = await fitnessClubService.getMembers({
            search: search.trim() || undefined,
            isActive: status === 'all' ? undefined : status === 'active',
          })

          if (!cancelled) setMembers(data)
        } catch (err) {
          if (!cancelled) {
            setNotice({
              text: errorMessage(err, 'Не вдалося завантажити клієнтів.'),
              isError: true,
            })
          }
        } finally {
          if (!cancelled) setLoading(false)
        }
      },
      search ? 300 : 0,
    )

    return () => {
      cancelled = true
      clearTimeout(timer)
    }
  }, [search, status, reloadKey])

  function reload() {
    setReloadKey((key) => key + 1)
  }

  function handleChange<K extends keyof MemberFormState>(
    field: K,
    value: MemberFormState[K],
  ) {
    setForm((prev) => ({ ...prev, [field]: value }))
  }

  function openCreateForm() {
    const firstActive = memberships.find((membership) => membership.isActive)

    setForm({
      ...emptyForm,
      membershipId: firstActive ? String(firstActive.id) : '',
    })
    setEditingId(null)
    setNotice(null)
    setFormVisible(true)
  }

  function openEditForm(member: Member) {
    setForm({
      firstName: member.firstName,
      lastName: member.lastName,
      email: member.email,
      phone: member.phone,
      birthDate: member.birthDate,
      membershipId: String(member.membershipId),
      isActive: member.isActive,
    })
    setEditingId(member.id)
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

    const data: MemberFormData = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim(),
      email: form.email.trim(),
      phone: form.phone.trim(),
      birthDate: form.birthDate,
      isActive: form.isActive,
      membershipId: Number(form.membershipId),
    }

    try {
      if (editingId === null) {
        await fitnessClubService.createMember(data)
        setNotice({ text: 'Клієнта успішно додано.', isError: false })
      } else {
        await fitnessClubService.updateMember(editingId, data)
        setNotice({ text: 'Дані клієнта оновлено.', isError: false })
      }

      closeForm()
      reload()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося зберегти клієнта.'),
        isError: true,
      })
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(member: Member) {
    if (
      !window.confirm(
        `Видалити клієнта ${member.firstName} ${member.lastName}?`,
      )
    ) {
      return
    }

    setNotice(null)

    try {
      await fitnessClubService.deleteMember(member.id)
      setNotice({ text: 'Клієнта видалено.', isError: false })

      if (editingId === member.id) closeForm()

      reload()
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося видалити клієнта.'),
        isError: true,
      })
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження клієнтів...</p>
  }

  const membershipNames = new Map(
    memberships.map((membership) => [membership.id, membership.name]),
  )

  return (
    <div className="admin-page">
      <div className="admin-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування клієнтами</h1>
        <p className="admin-subtitle">
          Клієнти клубу: додавання, редагування, деактивація й видалення.
          Акаунт користувача пов’язується з клієнтом за email.
        </p>
      </div>

      <div className="admin-toolbar">
        <label className="admin-field">
          Пошук
          <input
            type="search"
            placeholder="Ім’я, прізвище або email"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </label>

        <label className="admin-field">
          Статус
          <select
            value={status}
            onChange={(e) => setStatus(e.target.value as StatusFilter)}
          >
            <option value="all">Усі</option>
            <option value="active">Активні</option>
            <option value="inactive">Неактивні</option>
          </select>
        </label>

        <span className="admin-toolbar-spacer" />

        <button
          type="button"
          className="admin-btn admin-btn-primary"
          onClick={openCreateForm}
        >
          + Додати клієнта
        </button>
      </div>

      {notice && (
        <p className={`admin-notice ${notice.isError ? 'error' : 'success'}`}>
          {notice.text}
        </p>
      )}

      {formVisible && (
        <form className="admin-panel" onSubmit={handleSubmit}>
          <h2>{editingId === null ? 'Новий клієнт' : 'Редагування клієнта'}</h2>

          <div className="admin-form-grid">
            <label className="admin-field">
              Ім’я
              <input
                value={form.firstName}
                onChange={(e) => handleChange('firstName', e.target.value)}
                required
                minLength={2}
                maxLength={50}
              />
            </label>

            <label className="admin-field">
              Прізвище
              <input
                value={form.lastName}
                onChange={(e) => handleChange('lastName', e.target.value)}
                required
                minLength={2}
                maxLength={50}
              />
            </label>

            <label className="admin-field">
              Email
              <input
                type="email"
                value={form.email}
                onChange={(e) => handleChange('email', e.target.value)}
                required
              />
            </label>

            <label className="admin-field">
              Телефон
              <input
                type="tel"
                placeholder="+380501234567"
                pattern="^\+?[0-9\s\-\(\)]{7,20}$"
                title="Номер телефону, наприклад +380501234567"
                value={form.phone}
                onChange={(e) => handleChange('phone', e.target.value)}
                required
              />
            </label>

            <label className="admin-field">
              Дата народження
              <input
                type="date"
                max={todayString()}
                value={form.birthDate}
                onChange={(e) => handleChange('birthDate', e.target.value)}
                required
              />
            </label>

            <label className="admin-field">
              Абонемент
              <select
                value={form.membershipId}
                onChange={(e) => handleChange('membershipId', e.target.value)}
                required
              >
                <option value="" disabled>
                  Оберіть абонемент
                </option>
                {memberships.map((membership) => (
                  <option value={membership.id} key={membership.id}>
                    {membership.name}
                    {membership.isActive ? '' : ' (неактивний)'}
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field admin-field-check">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => handleChange('isActive', e.target.checked)}
              />
              Активний клієнт
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

      {members.length === 0 ? (
        <p className="admin-empty">
          {search || status !== 'all'
            ? 'За вказаними умовами клієнтів не знайдено.'
            : 'Клієнтів поки немає.'}
        </p>
      ) : (
        <>
          <p className="admin-count">Знайдено клієнтів: {members.length}</p>

          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Клієнт</th>
                  <th>Телефон</th>
                  <th>Дата народження</th>
                  <th>Абонемент</th>
                  <th>Статус</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {members.map((member) => (
                  <tr key={member.id}>
                    <td>
                      <span className="admin-table-title">
                        {member.firstName} {member.lastName}
                      </span>
                      <span className="admin-table-muted">{member.email}</span>
                    </td>
                    <td>{member.phone}</td>
                    <td>{formatDate(member.birthDate)}</td>
                    <td>
                      {membershipNames.get(member.membershipId) ??
                        `№${member.membershipId}`}
                    </td>
                    <td>
                      <span
                        className={`admin-badge ${member.isActive ? 'green' : 'gray'}`}
                      >
                        {member.isActive ? 'Активний' : 'Неактивний'}
                      </span>
                    </td>
                    <td>
                      <div className="admin-row-actions">
                        <button
                          type="button"
                          className="admin-btn admin-btn-edit admin-btn-small"
                          onClick={() => openEditForm(member)}
                        >
                          Редагувати
                        </button>
                        <button
                          type="button"
                          className="admin-btn admin-btn-danger admin-btn-small"
                          onClick={() => handleDelete(member)}
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

export default AdminMembersPage
