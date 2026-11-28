import { useEffect, useState } from 'react'
import {
  fitnessClubService,
  type UserProfile,
  type UserRole,
} from '../../api/fitnessClubService'
import { useAuth } from '../../contexts/AuthContext'
import { errorMessage } from '../../utils/labels'
import './Admin.css'

interface Notice {
  text: string
  isError: boolean
}

function isAdministrator(profile: UserProfile) {
  return profile.roles.includes('Administrator')
}

function AdminUsersPage() {
  const { user: currentUser } = useAuth()

  const [users, setUsers] = useState<UserProfile[]>([])
  const [loading, setLoading] = useState(true)
  const [notice, setNotice] = useState<Notice | null>(null)
  const [search, setSearch] = useState('')
  const [busyId, setBusyId] = useState<string | null>(null)

  useEffect(() => {
    fitnessClubService
      .getUsers()
      .then(setUsers)
      .catch((err: unknown) => {
        setNotice({
          text: errorMessage(err, 'Не вдалося завантажити користувачів.'),
          isError: true,
        })
      })
      .finally(() => {
        setLoading(false)
      })
  }, [])

  async function handleChangeRole(target: UserProfile) {
    const newRole: UserRole = isAdministrator(target) ? 'User' : 'Administrator'
    const fullName = `${target.firstName} ${target.lastName}`

    const question =
      newRole === 'Administrator'
        ? `Надати користувачу ${fullName} права адміністратора?`
        : `Забрати в користувача ${fullName} права адміністратора?`

    if (!window.confirm(question)) {
      return
    }

    setBusyId(target.id)
    setNotice(null)

    try {
      const updated = await fitnessClubService.changeUserRole(target.id, newRole)

      setUsers((prev) =>
        prev.map((item) => (item.id === updated.id ? updated : item)),
      )
      setNotice({
        text:
          newRole === 'Administrator'
            ? `${fullName} тепер адміністратор.`
            : `${fullName} тепер звичайний учасник клубу.`,
        isError: false,
      })
    } catch (err) {
      setNotice({
        text: errorMessage(err, 'Не вдалося змінити роль.'),
        isError: true,
      })
    } finally {
      setBusyId(null)
    }
  }

  if (loading) {
    return <p className="page-message">Завантаження користувачів...</p>
  }

  const query = search.trim().toLowerCase()
  const visibleUsers = query
    ? users.filter((item) =>
        `${item.firstName} ${item.lastName} ${item.email}`
          .toLowerCase()
          .includes(query),
      )
    : users

  return (
    <div className="admin-page">
      <div className="admin-header">
        <p className="eyebrow">ADMINISTRATION</p>
        <h1>Керування користувачами</h1>
        <p className="admin-subtitle">
          Облікові записи сайту та їхні ролі. Власну роль змінити не можна,
          щоб випадково не втратити доступ до адміністрування.
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
      </div>

      {notice && (
        <p className={`admin-notice ${notice.isError ? 'error' : 'success'}`}>
          {notice.text}
        </p>
      )}

      {visibleUsers.length === 0 ? (
        <p className="admin-empty">
          {query
            ? 'За вашим запитом користувачів не знайдено.'
            : 'Користувачів поки немає.'}
        </p>
      ) : (
        <>
          <p className="admin-count">Користувачів: {visibleUsers.length}</p>

          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead>
                <tr>
                  <th>Користувач</th>
                  <th>Роль</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {visibleUsers.map((item) => {
                  const admin = isAdministrator(item)
                  const isSelf = item.id === currentUser?.id

                  return (
                    <tr key={item.id}>
                      <td>
                        <span className="admin-table-title">
                          {item.firstName} {item.lastName}
                          {isSelf && ' (ви)'}
                        </span>
                        <span className="admin-table-muted">{item.email}</span>
                      </td>
                      <td>
                        <span
                          className={`admin-badge ${admin ? 'blue' : 'gray'}`}
                        >
                          {admin ? 'Адміністратор' : 'Учасник клубу'}
                        </span>
                      </td>
                      <td>
                        <div className="admin-row-actions">
                          <button
                            type="button"
                            className={`admin-btn admin-btn-small ${
                              admin ? 'admin-btn-danger' : 'admin-btn-edit'
                            }`}
                            disabled={isSelf || busyId === item.id}
                            title={
                              isSelf
                                ? 'Власну роль змінити не можна'
                                : undefined
                            }
                            onClick={() => handleChangeRole(item)}
                          >
                            {admin
                              ? 'Забрати права адміністратора'
                              : 'Призначити адміністратором'}
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

export default AdminUsersPage
