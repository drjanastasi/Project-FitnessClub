/** Українські підписи для значень, які API повертає англійською або числом. */

const levelLabels: Record<string, string> = {
  Beginner: 'Початковий',
  Intermediate: 'Середній',
  Advanced: 'Високий',
  '0': 'Початковий',
  '1': 'Середній',
  '2': 'Високий',
}

export const levelOptions = [
  { value: 0, label: 'Початковий' },
  { value: 1, label: 'Середній' },
  { value: 2, label: 'Високий' },
]

export function getLevelName(level: number | string): string {
  return levelLabels[String(level)] ?? String(level)
}

/** Рівень складності (число або "Beginner") → число 0–2 для форми. */
export function getLevelIndex(level: number | string): number {
  const label = levelLabels[String(level)]
  const index = levelOptions.findIndex((option) => option.label === label)
  return index === -1 ? 0 : index
}

export const bookingStatusLabels: Record<string, string> = {
  Confirmed: 'Підтверджено',
  Cancelled: 'Скасовано',
}

export function getBookingStatusName(status: string): string {
  return bookingStatusLabels[status] ?? status
}

export const roleLabels: Record<string, string> = {
  Administrator: 'Адміністратор',
  User: 'Учасник клубу',
}

/** Текст помилки для користувача: повідомлення від API або запасний варіант. */
export function errorMessage(err: unknown, fallback: string): string {
  return err instanceof Error && err.message ? err.message : fallback
}
