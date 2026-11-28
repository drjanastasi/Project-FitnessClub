import { useEffect, useState } from 'react'
import { fitnessClubService } from '../../api/fitnessClubService'
import type { Club, ClubFormData } from '../../api/fitnessClubService'

const emptyForm: ClubFormData = {
  name: '',
  description: '',
  address: '',
  phone: '',
  email: '',
  workHours: '',
}

export default function AdminClubsPage() {
  const [clubs, setClubs] = useState<Club[]>([])
  const [form, setForm] = useState<ClubFormData>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const loadClubs = async () => {
    try {
      const data = await fitnessClubService.getClubs()
      setClubs(data)
    } catch {
      setError('Не вдалося завантажити клуби.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadClubs()
  }, [])

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
  ) => {
    const { name, value } = e.target
    setForm(prev => ({ ...prev, [name]: value }))
  }

  const resetForm = () => {
    setForm(emptyForm)
    setEditingId(null)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setMessage('')

    try {
      if (editingId !== null) {
        await fitnessClubService.updateClub(editingId, form)
        setMessage('Клуб успішно оновлено.')
      } else {
        await fitnessClubService.createClub(form)
        setMessage('Клуб успішно додано.')
      }

      resetForm()
      await loadClubs()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Не вдалося зберегти клуб.',
      )
    }
  }

  const handleEdit = (club: Club) => {
    setEditingId(club.id)
    setForm({
      name: club.name,
      description: club.description ?? '',
      address: club.address,
      phone: club.phone,
      email: club.email,
      workHours: club.workHours ?? '',
    })
    setMessage('')
    setError('')
  }

  const handleDelete = async (id: number) => {
    if (!window.confirm('Ви дійсно хочете видалити цей клуб?')) {
      return
    }

    setError('')
    setMessage('')

    try {
      await fitnessClubService.deleteClub(id)
      setMessage('Клуб успішно видалено.')

      if (editingId === id) {
        resetForm()
      }

      await loadClubs()
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Не вдалося видалити клуб.',
      )
    }
  }

  if (loading) {
    return <div className="admin-clubs-page">Завантаження клубів...</div>
  }

  return (
    <div className="admin-clubs-page">
      <h1>Управління клубами</h1>

      <form onSubmit={handleSubmit} className="admin-club-form">
        <h2>
          {editingId !== null ? 'Редагування клубу' : 'Додавання клубу'}
        </h2>

        <label>Назва клубу</label>
        <input
          name="name"
          value={form.name}
          onChange={handleChange}
          required
          minLength={2}
          maxLength={100}
        />

        <label>Опис</label>
        <textarea
          name="description"
          value={form.description ?? ''}
          onChange={handleChange}
          maxLength={500}
        />

        <label>Адреса</label>
        <input
          name="address"
          value={form.address}
          onChange={handleChange}
          required
          minLength={5}
          maxLength={200}
        />

        <label>Телефон (+380XXXXXXXXX)</label>
        <input
          name="phone"
          value={form.phone}
          onChange={handleChange}
          required
          pattern="^\+380\d{9}$"
          placeholder="+380XXXXXXXXX"
        />

        <label>Email</label>
        <input
          name="email"
          type="email"
          value={form.email}
          onChange={handleChange}
          required
          maxLength={100}
        />

        <label>Години роботи</label>
        <input
          name="workHours"
          value={form.workHours ?? ''}
          onChange={handleChange}
          maxLength={100}
          placeholder="Наприклад: Пн-Пт 08:00-22:00"
        />

        <button type="submit">
          {editingId !== null ? 'Зберегти зміни' : 'Додати клуб'}
        </button>

        {editingId !== null && (
          <button type="button" onClick={resetForm}>
            Скасувати редагування
          </button>
        )}
      </form>

      {message && <p>{message}</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}

      <h2>Список клубів</h2>

      {clubs.length === 0 ? (
        <p>Клубів поки немає.</p>
      ) : (
        clubs.map(club => (
          <div key={club.id} className="admin-club-card">
            <h3>{club.name}</h3>

            <p>
              <strong>Опис:</strong> {club.description || 'Не вказано'}
            </p>

            <p>
              <strong>Адреса:</strong> {club.address}
            </p>

            <p>
              <strong>Телефон:</strong> {club.phone}
            </p>

            <p>
              <strong>Email:</strong> {club.email}
            </p>

            <p>
              <strong>Години роботи:</strong>{' '}
              {club.workHours || 'Не вказано'}
            </p>

            <button onClick={() => handleEdit(club)}>
              Редагувати
            </button>

            <button onClick={() => handleDelete(club.id)}>
              Видалити
            </button>
          </div>
        ))
      )}
    </div>
  )
}