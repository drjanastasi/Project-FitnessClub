const API_BASE_URL: string =
  import.meta.env.VITE_API_URL ?? 'https://localhost:7028/api'

/** Помилка API: код статусу та зрозумілий текст (від сервера, якщо він його надіслав). */
export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

const defaultMessages: Record<number, string> = {
  400: 'Некоректний запит.',
  401: 'Потрібно увійти в акаунт.',
  403: 'Недостатньо прав для цієї дії.',
  404: 'Нічого не знайдено.',
  409: 'Дія конфліктує з наявними даними.',
}

async function readErrorMessage(response: Response): Promise<string> {
  const fallback =
    defaultMessages[response.status] ?? `Помилка сервера (${response.status}).`

  try {
    const data = await response.json()

    // Помилки валідації: беремо перше повідомлення з errors.
    if (data?.errors && typeof data.errors === 'object') {
      const first = Object.values(data.errors as Record<string, string[]>)[0]?.[0]
      if (first) return first
    }

    if (typeof data?.title === 'string' && data.title) {
      return data.title
    }
  } catch {
    // Тіло відповіді порожнє або не є JSON: використовуємо стандартний текст.
  }

  return fallback
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const token = localStorage.getItem('token')

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${endpoint}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...options.headers,
      },
    })
  } catch {
    throw new ApiError(0, 'Не вдалося зʼєднатися із сервером. Перевірте, чи запущений API.')
  }

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}