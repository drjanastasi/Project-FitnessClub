const dayNamesUk = [
  'Неділя',
  'Понеділок',
  'Вівторок',
  'Середа',
  'Четвер',
  'П’ятниця',
  'Субота',
]

const dayNamesEn = [
  'Sunday',
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
]

/** Індекс дня тижня 0–6 (0 = неділя) з числа або англійської назви ("Monday"). */
export function getDayIndex(day: number | string): number {
  if (typeof day === 'number') return day

  const byName = dayNamesEn.indexOf(day)
  return byName !== -1 ? byName : Number(day)
}

/** Українська назва дня тижня. */
export function getDayName(day: number | string): string {
  return dayNamesUk[getDayIndex(day)] ?? String(day)
}

/** Date → "yyyy-MM-dd" за локальним часом. */
export function toDateString(date: Date): string {
  return [
    date.getFullYear(),
    String(date.getMonth() + 1).padStart(2, '0'),
    String(date.getDate()).padStart(2, '0'),
  ].join('-')
}

export function todayString(): string {
  return toDateString(new Date())
}

/** Найближча дата заданого дня тижня (сьогодні, якщо день збігається). */
export function getNextWorkoutDate(dayOfWeek: number | string): string {
  const today = new Date()
  const daysUntil = (getDayIndex(dayOfWeek) - today.getDay() + 7) % 7

  const next = new Date(today)
  next.setDate(today.getDate() + daysUntil)

  return toDateString(next)
}

/** "2026-11-30" → "30.11.2026" */
export function formatDate(value: string): string {
  const [year, month, day] = value.split('-')
  return `${day}.${month}.${year}`
}

/** "18:00:00" → "18:00" */
export function formatTime(value: string): string {
  return value.slice(0, 5)
}