
import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { fitnessClubService } from '../../api/fitnessClubService'
import { useAuth } from '../../contexts/AuthContext'

function RegisterPage() {
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const { setAuthenticatedUser } = useAuth()

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')

    if (password !== confirmPassword) {
      setError('Паролі не збігаються.')
      return
    }

    if (password.length < 6) {
      setError('Пароль повинен містити щонайменше 6 символів.')
      return
    }

    setLoading(true)

    try {
      const response = await fitnessClubService.register({
        firstName,
        lastName,
        email,
        password,
      })

      localStorage.setItem('token', response.token)
      localStorage.setItem('user', JSON.stringify(response.user))

      setAuthenticatedUser(response.user)

      window.location.href = '/'
    } catch {
      setError('Не вдалося зареєструватися. Перевір введені дані.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>Створення облікового запису</h1>
        <p className="auth-subtitle">
          Заповни дані, щоб приєднатися до FitnessClub.
        </p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="firstName">Ім'я</label>
          <input
            id="firstName"
            type="text"
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
            placeholder="Введи ім'я"
            minLength={2}
            maxLength={50}
            required
          />

          <label htmlFor="lastName">Прізвище</label>
          <input
            id="lastName"
            type="text"
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
            placeholder="Введи прізвище"
            minLength={2}
            maxLength={50}
            required
          />

          <label htmlFor="registerEmail">Email</label>
          <input
            id="registerEmail"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="example@email.com"
            maxLength={100}
            required
          />

          <label htmlFor="registerPassword">Пароль</label>
          <div className="password-field">
            <input
              id="registerPassword"
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Мінімум 6 символів"
              minLength={6}
              maxLength={100}
              required
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? 'Сховати' : 'Показати'}
            </button>
          </div>

          <label htmlFor="confirmPassword">Підтвердження пароля</label>
          <input
            id="confirmPassword"
            type={showPassword ? 'text' : 'password'}
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="Повтори пароль"
            required
          />

          {error && <p className="auth-error">{error}</p>}

          <button
            type="submit"
            className="auth-submit"
            disabled={loading}
          >
            {loading ? 'Реєстрація...' : 'Зареєструватися'}
          </button>
        </form>

        <p className="auth-switch">
          Уже маєш обліковий запис?{' '}
          <Link to="/login">Увійти</Link>
        </p>
      </div>
    </div>
  )
}

export default RegisterPage
