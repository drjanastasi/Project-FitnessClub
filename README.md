# Fitness Club — вебсайт фітнес-клубу

Командний наскрізний проєкт на **.NET 11 Web API**: бекенд для сайту фітнес-клубу, через який клієнти переглядають абонементи й розклад тренувань та записуються на заняття, а адміністратор керує цими даними.

> **Етап 1** — проєктування та базовий REST API. Дані зберігаються в пам'яті (після перезапуску скидаються до тестових). База даних, EF Core та автентифікація з'являться на наступних етапах.

---

## Предметна область

**Яку проблему вирішує.** Інформація про фітнес-клуб (ціни, розклад, тренери) часто розкидана між соцмережами, дзвінками на рецепцію та паперовими таблицями. Клієнту складно швидко дізнатися, яке тренування коли проходить і чи є місця, а адміністратору — вести облік абонементів і записів.

**Користувачі:**
- **Відвідувач / клієнт** — переглядає абонементи, розклад тренувань, тренерів, записується на тренування.
- **Адміністратор клубу** — керує абонементами, розкладом, тренерами, клієнтами та записами.

**Для чого використовують:** обрати абонемент, знайти тренування за днем і рівнем складності, записатися або скасувати запис, а адміністратору — підтримувати актуальні дані клубу.

## Учасники команди

| Учасник | Зона відповідальності |
|---|---|
| Ромащенко Анастасія | Абонементи (`Memberships`) + Тренування (`Workouts`) |
| Горбатенко Назар | Клієнти (`Members`) + Тренери (`Trainers`) |
| _Ім'я Прізвище_ | Клуб (`Club`) + Записи (`Bookings`) |

## Основні сценарії використання

1. **Відвідувач переглядає абонементи** — список активних абонементів з ціною та тривалістю, фільтр за ціною.
2. **Відвідувач переглядає розклад тренувань** — фільтр за днем тижня, рівнем складності, тренером.
3. **Клієнт записується на тренування** — створюється запис (`Booking`) зі статусом `Confirmed`; запис можна скасувати (`Cancelled`).
4. **Адміністратор керує абонементами та розкладом** — створює, змінює, видаляє абонементи й тренування, вмикає/вимикає продаж абонемента.
5. **Адміністратор керує клієнтами та тренерами** — додає нового клієнта, призначає йому абонемент, додає тренера.

## Ресурси API

| Ресурс | Поля | Статус |
|---|---|---|
| **Memberships** — абонементи | назва, опис, ціна, тривалість (дні), чи активний | ✅ реалізовано |
| **Workouts** — тренування (містять розклад) | назва, опис, тренер, день тижня, час початку, тривалість (хв), рівень складності, макс. кількість учасників | ✅ реалізовано |
| **Members** — клієнти | ім'я, прізвище, email, телефон, дата народження, активний, абонемент | ✅ реалізовано |
| **Trainers** — тренери | ім'я, прізвище, email, телефон, спеціалізація, досвід | ✅ реалізовано |
| **Club** — інформація про клуб | назва, опис, адреса, місто, телефон, email, години роботи | ⏳ в роботі |
| **Bookings** — записи на тренування | клієнт, тренування, дата запису, статус (`Confirmed`, `Cancelled`) | ⏳ в роботі |

## Endpoints

### Memberships — `/api/memberships`

| Метод | Маршрут | Опис | Коди відповідей |
|---|---|---|---|
| GET | `/api/memberships` | Список абонементів | 200, 400 |
| GET | `/api/memberships/{id}` | Один абонемент + тренування, що до нього входять | 200, 404 |
| POST | `/api/memberships` | Створити абонемент | 201, 400, 409 |
| PUT | `/api/memberships/{id}` | Повністю оновити | 200, 400, 404, 409 |
| PATCH | `/api/memberships/{id}/status` | Активувати / деактивувати | 200, 400, 404 |
| DELETE | `/api/memberships/{id}` | Видалити | 204, 404 |
| POST | `/api/memberships/{id}/workouts/{workoutId}` | Додати тренування в абонемент | 200, 404, 409 |
| DELETE | `/api/memberships/{id}/workouts/{workoutId}` | Прибрати тренування з абонемента | 204, 404 |

Query-параметри для `GET /api/memberships`: `isActive`, `minPrice`, `maxPrice`, `search`, `workoutId` (абонементи, що включають тренування), `sortBy` (`name` \| `price` \| `duration`), `desc`.
Приклад: `GET /api/memberships?isActive=true&maxPrice=1000&sortBy=price`

### Workouts — `/api/workouts`

| Метод | Маршрут | Опис | Коди відповідей |
|---|---|---|---|
| GET | `/api/workouts` | Розклад тренувань (пн → нд, за часом) | 200, 400 |
| GET | `/api/workouts/{id}` | Одне тренування + абонементи, до яких воно входить | 200, 404 |
| POST | `/api/workouts` | Створити тренування | 201, 400 |
| PUT | `/api/workouts/{id}` | Повністю оновити | 200, 400, 404 |
| DELETE | `/api/workouts/{id}` | Видалити | 204, 404 |

Query-параметри для `GET /api/workouts`: `day` (`Monday`…`Sunday`), `level` (`Beginner` \| `Intermediate` \| `Advanced`), `trainerId`, `membershipId` (тренування, що входять в абонемент), `search`, `sortBy` (`schedule` \| `name` \| `duration`), `desc`.
Приклад: `GET /api/workouts?day=Monday&level=Beginner`, `GET /api/workouts?membershipId=3`

Приклад тіла `POST /api/workouts`:

```json
{
  "name": "Йога",
  "description": "Хатха-йога для розтяжки",
  "trainerId": 1,
  "dayOfWeek": "Monday",
  "startTime": "18:00",
  "durationMinutes": 60,
  "level": "Beginner",
  "maxParticipants": 15
}
```

### Валідація та помилки

- Вхідні дані приходять у **DTO** з атрибутами валідації (`[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]`). Невалідний запит автоматично отримує **400** з переліком помилок (`ValidationProblemDetails`).
- Неіснуючий `id` → **404**, дубль назви абонемента → **409**, успішне створення → **201** з заголовком `Location`, видалення → **204**.
- Enum-и в JSON передаються рядками (`"Monday"`, `"Beginner"`).

> **Етап 2** — дані зберігаються в реляційній базі даних **SQLite** через **Entity Framework Core** (Code First, міграції). Автентифікація з'явиться на наступних етапах.


## База даних

**СУБД:** SQLite — уся база зберігається в одному файлі `FitnessClub.Api/fitnessclub.db`, окремий сервер встановлювати не потрібно.
**ORM:** Entity Framework Core 9 (підхід Code First: спочатку C#-класи, з них міграціями створюється база).

Рядок підключення — в `FitnessClub.Api/appsettings.json`:

```json
"ConnectionStrings": { "FitnessClub": "Data Source=fitnessclub.db" }
```

### Модель даних

```
Memberships 1 ──── * MembershipWorkouts * ──── 1 Workouts
                                                    │ *
                                                    │
                                                    1  Trainers (етап 2, інший учасник)
```

**Memberships** — абонементи

| Поле | Тип | Обмеження |
|---|---|---|
| Id | int | PK |
| Name | string | обов'язкове, ≤100, унікальне |
| Description | string? | ≤500 |
| Price | decimal (у SQLite — REAL) | обов'язкове |
| DurationDays | int | обов'язкове |
| IsActive | bool | обов'язкове |

**Workouts** — тренування (разом з розкладом)

| Поле | Тип | Обмеження |
|---|---|---|
| Id | int | PK |
| Name | string | обов'язкове, ≤100 |
| Description | string? | ≤500 |
| TrainerId | int | обов'язкове; стане FK на Trainers |
| DayOfWeek | DayOfWeek (int) | обов'язкове, індекс |
| StartTime | TimeOnly | обов'язкове |
| DurationMinutes | int | обов'язкове |
| Level | enum → string | обов'язкове, ≤20 |
| MaxParticipants | int | обов'язкове |

**MembershipWorkouts** — проміжна таблиця, які тренування входять в абонемент

| Поле | Тип | Обмеження |
|---|---|---|
| MembershipId | int | PK (складений), FK → Memberships, ON DELETE CASCADE |
| WorkoutId | int | PK (складений), FK → Workouts, ON DELETE CASCADE |

### Зв'язки

- **Membership ↔ Workout — багато-до-багатьох** через `MembershipWorkouts`: один абонемент включає кілька тренувань, одне тренування доступне в кількох абонементах. Видалення абонемента чи тренування каскадно видаляє лише рядки зв'язку.
- **Trainer → Workout — один-до-багатьох** (підготовлено в `WorkoutConfiguration`, вмикається разом із сутністю Trainer). `Restrict` — не можна видалити тренера, у якого є тренування.

Конфігурація таблиць — через **Fluent API** у `Data/Configurations/*Configuration.cs`, валідація вхідних даних — через **Data Annotations** у DTO. Початкові дані додаються через `HasData` і потрапляють у базу міграцією.

### Міграції

Один раз встановити інструмент:

```bash
dotnet tool install --global dotnet-ef --version 9.*
```

Команди виконуються з кореня репозиторію:

```bash
# створити нову міграцію після зміни моделей
dotnet ef migrations add <НазваМіграції> --project FitnessClub.Api

# застосувати міграції до бази (створює fitnessclub.db, якщо її немає)
dotnet ef database update --project FitnessClub.Api

# переглянути список міграцій
dotnet ef migrations list --project FitnessClub.Api
```

Крім того, під час запуску застосунку викликається `Database.Migrate()`, тому всі наявні міграції застосовуються автоматично.

> Файл бази `*.db` не зберігається в Git — у кожного учасника він створюється локально з міграцій.


## Структура проєкту

```
FitnessClub.sln
FitnessClub.Api/
├── Controllers/     — REST-контролери (routing, HTTP-методи, status codes)
├── Data/            — FitnessClubDbContext + Configurations/ (Fluent API)
├── Migrations/      — міграції EF Core
├── DTOs/            — вхідні/вихідні моделі з валідацією
├── Mapping/         — перетворення Model ↔ DTO
├── Models/          — доменні сутності
├── Repositories/    — доступ до даних через EF Core (LINQ, async)
├── Program.cs       — налаштування сервісів, Swagger, CORS
└── FitnessClub.Api.http — готові запити для перевірки
```

Контролери залежать лише від інтерфейсів репозиторіїв (`IMembershipRepository`, `IWorkoutRepository`). Репозиторії будують LINQ-запити (фільтрація, пошук, сортування, `Include` для пов'язаних даних), а EF Core перетворює їх на SQL. Усі операції з базою асинхронні: `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, `ExecuteDeleteAsync`. SQL-запити видно в консолі під час роботи.


## Запуск

Потрібен **.NET 11 SDK** — https://dotnet.microsoft.com/download

```bash
git clone <посилання-на-репозиторій>
cd FitnessClub
dotnet restore
dotnet run --project FitnessClub.Api --launch-profile http
```

Після запуску відкрийте **Swagger UI**: http://localhost:5191/swagger
(корінь http://localhost:5191 теж перенаправляє на Swagger).

Також можна надсилати запити з файлу `FitnessClub.Api/FitnessClub.Api.http` у Visual Studio / Rider / VS Code (розширення REST Client).



