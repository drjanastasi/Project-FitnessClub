import { apiRequest } from './api'

export interface Workout {
  id: number
  name: string
  description: string | null
  trainerId: number
  clubId: number
  dayOfWeek: number | string
  startTime: string
  durationMinutes: number
  level: number | string
  maxParticipants: number
}

export interface WorkoutFormData {
  name: string
  description: string | null
  trainerId: number
  clubId: number
  dayOfWeek: number
  startTime: string
  durationMinutes: number
  level: number
  maxParticipants: number
}

export interface CreateWorkoutData {
  name: string
  description: string | null
  trainerId: number
  clubId: number
  dayOfWeek: number
  startTime: string
  durationMinutes: number
  level: number
  maxParticipants: number
}

export interface Club {
  id: number
  name: string
  description: string | null
  address: string
  phone: string
  email: string
  workHours: string | null
}

export interface ClubFormData {
  name: string
  description: string | null
  address: string
  phone: string
  email: string
  workHours: string | null
}

export interface Membership {
  id: number
  name: string
  description: string | null
  price: number
  durationDays: number
  isActive: boolean
  workoutsCount: number
}


export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}

export interface AuthResponse {
  token: string
  tokenType: string
  expiresAt: string
  user: UserProfile
}

export interface RegisterData {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface LoginData {
  email: string
  password: string
}

export interface Booking {
  id: number
  memberId: number
  workoutId: number
  workoutName: string
  dayOfWeek: number | string
  startTime: string
  date: string
  status: string
  createdAt: string
}

export interface WorkoutAvailability {
  workoutId: number
  date: string
  maxParticipants: number
  booked: number
  freePlaces: number
}

export interface CreateBookingData {
  workoutId: number
  date: string
  memberId?: number
}

export const fitnessClubService = {
  getWorkouts: () => apiRequest<Workout[]>('/workouts'),

  createWorkout: (data: CreateWorkoutData) =>
    apiRequest<Workout>('/workouts', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  updateWorkout: (id: number, data: CreateWorkoutData) =>
    apiRequest<Workout>(`/workouts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  deleteWorkout: (id: number) =>
    apiRequest<void>(`/workouts/${id}`, {
      method: 'DELETE',
    }),

  getClubs: () => apiRequest<Club[]>('/clubs'),

  createClub: (data: ClubFormData) =>
    apiRequest<Club>('/clubs', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  updateClub: (id: number, data: ClubFormData) =>
    apiRequest<Club>(`/clubs/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  deleteClub: (id: number) =>
    apiRequest<void>(`/clubs/${id}`, {
      method: 'DELETE',
    }),

  getMemberships: () => apiRequest<Membership[]>('/memberships'),

  getBookings: () =>
  apiRequest<Booking[]>('/bookings'),

  getWorkoutAvailability: (workoutId: number, date: string) =>
    apiRequest<WorkoutAvailability>(
      `/bookings/availability?workoutId=${workoutId}&date=${date}`
    ),

  createBooking: (data: CreateBookingData) =>
    apiRequest<Booking>('/bookings', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  cancelBooking: (id: number) =>
    apiRequest<Booking>(`/bookings/${id}/cancel`, {
      method: 'POST',
    }),

  register: (data: RegisterData) =>
  apiRequest<AuthResponse>('/auth/register', {
    method: 'POST',
    body: JSON.stringify(data),
  }),

  login: (data: LoginData) =>
    apiRequest<AuthResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
    
  getCurrentUser: () => apiRequest<UserProfile>('/auth/me'),
}