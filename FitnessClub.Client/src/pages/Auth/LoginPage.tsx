
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { fitnessClubService } from '../../api/fitnessClubService'
import { useAuth } from '../../contexts/AuthContext'

function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const { setAuthenticatedUser } = useAuth()

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setLoading(true)
    setError('')

    try {
      const response = await fitnessClubService.login({
        email,
        password,
      })

      localStorage.setItem('token', response.token)
      localStorage.setItem('user', JSON.stringify(response.user))

      setAuthenticatedUser(response.user)

      window.location.href = '/'
    } catch {
      setError('Не вдалося увійти. Перевір email і пароль.')
    } finally {
      setLoading(false)
    }

  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <p className="eyebrow">FITNESSCLUB</p>
        <h1>Вхід до системи</h1>
        <p className="auth-subtitle">
          Увійди до свого облікового запису.
        </p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="email">Email</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="example@email.com"
            required
          />

          <label htmlFor="password">Пароль</label>
          <div className="password-field">
            <input
              id="password"
              type={showPassword ? 'text' : 'password'}
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Введи пароль"
              required
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
            >
              {showPassword ? 'Сховати' : 'Показати'}
            </button>
          </div>

          {error && <p className="auth-error">{error}</p>}

          <button
            type="submit"
            className="auth-submit"
            disabled={loading}
          >
            {loading ? 'Вхід...' : 'Увійти'}
          </button>
        </form>

        <p className="auth-switch">
          Ще немає облікового запису?{' '}
          <Link to="/register">Зареєструватися</Link>
        </p>
      </div>
    </div>
  )
}

export default LoginPage
