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
| Бондаренко Вероніка | Клуб (`Club`) + Записи (`Bookings`) |

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
| **Club** — клуби (філії) | назва, опис, адреса, телефон, email, години роботи | ✅ реалізовано |
| **Bookings** — записи на тренування | клієнт, тренування, дата заняття, статус (`Confirmed`, `Cancelled`), час створення | ✅ реалізовано |

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

Query-параметри для `GET /api/workouts`: `day` (`Monday`…`Sunday`), `level` (`Beginner` \| `Intermediate` \| `Advanced`), `trainerId`, `membershipId` (тренування, що входять в абонемент), `clubId` (тренування конкретного клубу), `search`, `sortBy` (`schedule` \| `name` \| `duration`), `desc`.
Приклад: `GET /api/workouts?day=Monday&level=Beginner`, `GET /api/workouts?membershipId=3`

Приклад тіла `POST /api/workouts`:

```json
{
  "name": "Йога",
  "description": "Хатха-йога для розтяжки",
  "trainerId": 1,
  "clubId": 1,
  "dayOfWeek": "Monday",
  "startTime": "18:00",
  "durationMinutes": 60,
  "level": "Beginner",
  "maxParticipants": 15
}
```


Query-параметри для `GET /api/workouts/schedule`: `level`, `trainerId`, `membershipId`, `clubId`, `includeEmptyDays` (за замовчуванням `true` — усі 7 днів).
Приклад: `GET /api/workouts/schedule?level=Beginner&includeEmptyDays=false`

**Правила розкладу** (перевіряються під час `POST` і `PUT`):
- тренер з `trainerId` має існувати → інакше **400**;
- клуб з `clubId` має існувати → інакше **400**;
- тренування має закінчитися до 24:00 того ж дня → інакше **400**;
- у тренера не може бути двох тренувань, що перетинаються за часом у той самий день → інакше **409**
  (наприклад, Йога 18:00–19:00 і Стретчинг о 18:30 у того самого тренера).

### Clubs — `/api/clubs`

Клуб — це філія фітнес-клубу. У кожної філії власний розклад: тренування прив'язані до клубу через `clubId`.

| Метод | Маршрут | Опис | Коди відповідей |
|---|---|---|---|
| GET | `/api/clubs` | Список клубів | 200, 400 |
| GET | `/api/clubs/{id}` | Один клуб | 200, 404 |
| POST | `/api/clubs` | Створити клуб (Administrator) | 201, 400, 401, 403, 409 |
| PUT | `/api/clubs/{id}` | Повністю оновити (Administrator) | 200, 400, 401, 403, 404, 409 |
| DELETE | `/api/clubs/{id}` | Видалити (Administrator) | 204, 401, 403, 404, 409 |

Query-параметри для `GET /api/clubs`: `search` (за назвою, адресою або описом), `sortBy` (`name` \| `address`), `desc`.
Приклад: `GET /api/clubs?search=Центр&sortBy=name&desc=true`

Приклад тіла `POST /api/clubs`:

```json
{
  "name": "FitnessClub Південь",
  "description": "Нова філія",
  "address": "м. Кривий Ріг, вул. Південна, 5",
  "phone": "+380509999999",
  "email": "south@fitnessclub.ua",
  "workHours": "Пн–Нд 08:00–21:00"
}
```

**Правила:**
- телефон — у форматі `+380XXXXXXXXX` (13 символів), email — коректного формату → інакше **400**;
- назва клубу унікальна → дубль дає **409**;
- клуб, у якому є тренування, видалити не можна → **409**.

### Bookings — `/api/bookings`

Запис — це реєстрація клієнта на тренування в конкретну **дату**. Тренування в розкладі повторюється щотижня (день тижня + час), тому дата заняття зберігається в самому записі. Клуб запису визначається через тренування (`Workout.ClubId`).

| Метод | Маршрут | Опис | Коди відповідей |
|---|---|---|---|
| GET | `/api/bookings` | Список записів (User, Administrator) | 200, 400, 401 |
| GET | `/api/bookings/availability` | Вільні місця на тренування в дату (без входу) | 200, 400, 404 |
| GET | `/api/bookings/{id}` | Один запис (User, Administrator) | 200, 401, 404 |
| POST | `/api/bookings` | Записати клієнта на тренування (User, Administrator) | 201, 400, 401, 409 |
| POST | `/api/bookings/{id}/cancel` | Скасувати запис (User, Administrator) | 200, 400, 401, 404, 409 |
| DELETE | `/api/bookings/{id}` | Видалити запис остаточно (Administrator) | 204, 401, 403, 404 |

Query-параметри для `GET /api/bookings`: `memberId`, `workoutId`, `clubId` (записи на тренування клубу), `status` (`Confirmed` \| `Cancelled`), `dateFrom`, `dateTo`, `sortBy` (`date` \| `createdAt`), `desc`.
Приклад: `GET /api/bookings?memberId=2&status=Confirmed&sortBy=date`

Параметри для `GET /api/bookings/availability`: `workoutId` і `date` (обидва обов'язкові).
Приклад: `GET /api/bookings/availability?workoutId=1&date=2026-11-30` → `{ "workoutId": 1, "date": "2026-11-30", "maxParticipants": 15, "booked": 1, "freePlaces": 14 }`

Приклад тіла `POST /api/bookings`:

```json
{
  "memberId": 2,
  "workoutId": 1,
  "date": "2026-11-30"
}
```

**Правила запису** (перевіряються в `BookingService` під час `POST`):
- тренування з `workoutId` і клієнт з `memberId` мають існувати → інакше **400**;
- дата не може бути в минулому → **400**;
- день тижня дати має збігатися з днем проведення тренування (Йога по понеділках — запис на вівторок неможливий) → **400**;
- клієнт має бути активним → **400**;
- абонемент клієнта має бути активним і включати це тренування → **400**;
- в одного клієнта не може бути двох активних записів на те саме тренування в ту саму дату → **409**;
- кількість підтверджених записів на дату не може перевищувати `maxParticipants` → **409**.

Перевірки дубля й вільних місць та сам запис виконуються в одній транзакції (`IUnitOfWork.ExecuteInTransactionAsync`).

**Скасування:** `POST /api/bookings/{id}/cancel` змінює статус на `Cancelled` і звільняє місце. Повторне скасування → **409**; скасувати запис на заняття, що вже минуло, неможливо → **400**. Після скасування клієнт може записатися знову.

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
Memberships 1 ──── * MembershipWorkouts * ──── 1 Workouts * ──── 1 Clubs
                                                    │ 1
                                                    │
                                                    * Bookings (MemberId → Members, поки без FK)

Workouts * ──── 1 Trainers   (етап 2, інший учасник)
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
| ClubId | int | обов'язкове, FK → Clubs, ON DELETE RESTRICT |
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

**Clubs** — клуби (філії)

| Поле | Тип | Обмеження |
|---|---|---|
| Id | int | PK |
| Name | string | обов'язкове, ≤100, унікальне |
| Description | string? | ≤500 |
| Address | string | обов'язкове, ≤200 |
| Phone | string | обов'язкове, ≤13 (формат `+380XXXXXXXXX`) |
| Email | string | обов'язкове, ≤100 |
| WorkHours | string? | ≤100 (наприклад, `Пн–Нд 07:00–22:00`) |

**Bookings** — записи на тренування

| Поле | Тип | Обмеження |
|---|---|---|
| Id | int | PK |
| MemberId | int | обов'язкове; стане FK на Members, коли клієнти перейдуть на EF Core |
| WorkoutId | int | обов'язкове, FK → Workouts, ON DELETE RESTRICT |
| Date | DateOnly | обов'язкове, дата заняття |
| Status | enum → string | обов'язкове, ≤20 (`Confirmed` \| `Cancelled`) |
| CreatedAt | DateTime | обов'язкове, UTC |

Індекси Bookings: `(WorkoutId, Date)` — швидкий підрахунок зайнятих місць; **унікальний** `(MemberId, WorkoutId, Date)` лише для `Status = 'Confirmed'` — захист від дубля на рівні БД (скасований запис не заважає записатися знову).

### Зв'язки

- **Club → Workout — один-до-багатьох**: тренування проходить у конкретному клубі (філії). `Restrict` — не можна видалити клуб, поки в ньому є тренування.
- **Workout → Booking — один-до-багатьох**: на одне тренування (у різні дати) є багато записів. `Restrict` — не можна видалити тренування, на яке є записи, щоб не втратити історію.
- **Member → Booking — один-до-багатьох** (логічний зв'язок через `MemberId`; зовнішній ключ у БД з'явиться, коли `Members` перейдуть на EF Core). Існування й активність клієнта перевіряє `BookingService`.
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


> **Етап 3** — повноцінний REST API: єдиний формат помилок, власні middleware для централізованої обробки винятків і логування запитів, бізнес-правила розкладу, документація Swagger/OpenAPI.

## Обробка помилок

Усі помилки повертаються в єдиному форматі **ProblemDetails**:

```json
{
  "title": "Тренер уже веде тренування «Йога» у цей день 18:00–19:00. Час тренувань не може перетинатися.",
  "status": 409,
  "instance": "/api/workouts",
  "traceId": "0HNQ6HOPUQUTF:00000001"
}
```

| Ситуація | Код | Хто обробляє |
|---|---|---|
| Невалідні дані в DTO або query-параметрах | **400** | `[ApiController]` + `InvalidModelStateResponseFactory` (поле `errors` зі списком помилок) |
| Порушення бізнес-правил у даних (тренера не існує тощо) | **400** | `BadRequestException` → middleware |
| Ресурс не знайдено | **404** | `NotFoundException` → middleware |
| Конфлікт даних (дублікат назви, перетин розкладу, зв'язок уже існує, порушення обмежень БД) | **409** | `ConflictException` / `DbUpdateException` → middleware |
| Непередбачена помилка | **500** | middleware: клієнт бачить лише загальне повідомлення і `traceId`, деталі — тільки в лозі сервера |

Успішні відповіді: **200** — отримано/оновлено, **201** — створено (із заголовком `Location`), **204** — видалено.

## Middleware

Власні middleware в папці `Middleware/`, підключені в `Program.cs` на початку конвеєра:

1. **`RequestLoggingMiddleware`** — логує кожен запит: метод, шлях, параметри, status code, час обробки.
   Приклад: `HTTP GET /api/workouts?day=Monday → 200 за 6 мс`
2. **`ExceptionHandlingMiddleware`** — централізована обробка винятків. Перехоплює всі необроблені винятки з контролерів, репозиторіїв і EF Core, обирає status code за типом винятку, повертає `ProblemDetails` і не розкриває клієнту внутрішню інформацію (текст винятку, stack trace). Повні деталі записуються в лог разом з `traceId`.

Контролери абонементів, тренувань, клубів і записів не формують відповіді з помилками вручну: вони кидають `NotFoundException` / `ConflictException` / `BadRequestException` (папка `Exceptions/`), а відповідь формує middleware.

Для демонстрації помилки 500 у режимі Development є endpoint `GET /api/diagnostics/error`.

> **Етап 4** — архітектура з розділенням відповідальності (Controllers → Services → Repositories), Dependency Injection з різними життєвими циклами, ASP.NET Core Identity, JWT, ролі та захищені endpoints.

## Архітектура

```
HTTP-запит
   │
   ▼
Middleware (логування, обробка помилок, автентифікація, авторизація)
   │
   ▼
Controllers   — лише HTTP: маршрути, коди відповідей, [Authorize]
   │
   ▼
Services      — бізнес-логіка: правила, перевірки, перетворення в DTO
   │
   ▼
Repositories  — доступ до даних (LINQ → EF Core → SQLite)
   │
   ▼
Data          — FitnessClubDbContext, Unit of Work, міграції
```

Кожен шар знає лише про інтерфейси наступного шару (`IMembershipService`, `IMembershipRepository` тощо), а конкретні реалізації підставляє контейнер Dependency Injection.

| Шар | Компоненти |
|---|---|
| Controllers | `MembershipsController`, `WorkoutsController`, `ClubsController`, `BookingsController`, `AuthController`, `UsersController`, `DiagnosticsController` |
| Services | `MembershipService`, `WorkoutService`, `ClubService`, `BookingService`, `WorkoutScheduleValidator`, `AuthService`, `JwtTokenService`, `SystemDateTimeProvider` |
| Repositories | `MembershipRepository`, `WorkoutRepository`, `ClubRepository`, `BookingRepository` (EF Core); `InMemoryTrainerRepository`, `InMemoryMemberRepository` |
| Models / Entities | `Membership`, `Workout`, `MembershipWorkout`, `Club`, `Booking`, `Member`, `Trainer`, `AppUser` |
| DTO | `DTOs/Memberships`, `DTOs/Workouts`, `DTOs/Clubs`, `DTOs/Bookings`, `DTOs/Members`, `DTOs/Trainers`, `DTOs/Auth` |
| Data / Infrastructure | `FitnessClubDbContext`, `UnitOfWork`, `IdentitySeeder`, `Configuraitons/`, `Migrations/`, `Extensions/`, `Middleware/` |

## Dependency Injection

Усі залежності реєструються в `Extensions/ServiceCollectionExtensions.cs`, а `Program.cs` лише викликає `AddDatabase`, `AddApplicationServices`, `AddIdentityAndJwt`, `AddSwaggerWithJwt`. Класи отримують залежності через конструктор і не створюють їх самі через `new`.

| Компонент | Життєвий цикл | Чому |
|---|---|---|
| `FitnessClubDbContext` | **Scoped** | Один контекст на HTTP-запит: відстежує зміни лише цього запиту, не потокобезпечний |
| `IUnitOfWork`, репозиторії EF Core | **Scoped** | Працюють із DbContext, тому живуть стільки ж, скільки він |
| `IMembershipService`, `IWorkoutService`, `IClubService`, `IBookingService`, `IAuthService` | **Scoped** | Залежать від Scoped-репозиторіїв і `UserManager` |
| `IWorkoutScheduleValidator` | **Transient** | Легкий, без власного стану — новий екземпляр на кожне використання |
| `ITokenService` (`JwtTokenService`) | **Singleton** | Без стану між запитами, лише читає налаштування JWT; ключ підпису створюється один раз |
| `IDateTimeProvider` | **Singleton** | Без стану, один на весь застосунок |
| `ITrainerRepository`, `IMemberRepository` (у пам'яті) | **Singleton** | Дані у списку мають жити, поки працює застосунок |

Правило, якого дотримано: компонент не залежить від компонента з коротшим життєвим циклом (Singleton не отримує Scoped), інакше DbContext «застряг» би між запитами. У режимі Development ASP.NET Core сам перевіряє це під час запуску.

Демонстрація: `GET /api/diagnostics/lifetimes` двічі отримує з DI кожен тип і показує Id екземплярів — Transient щоразу різний, Scoped однаковий у межах запиту і новий у наступному, Singleton завжди той самий.

### SOLID, DRY, KISS

- **S** — один клас, одна відповідальність: контролер — HTTP, сервіс — бізнес-логіка, репозиторій — дані, `WorkoutScheduleValidator` — лише правила розкладу, `JwtTokenService` — лише створення токена.
- **O** — нові правила розкладу додаються у валідатор, контролери та репозиторії при цьому не змінюються.
- **L** — будь-яку реалізацію інтерфейсу (наприклад, `InMemoryTrainerRepository` → майбутня EF-версія) можна підставити без змін у сервісах.
- **I** — невеликі інтерфейси під конкретне завдання: `ITokenService`, `IDateTimeProvider`, `IWorkoutScheduleValidator`.
- **D** — сервіси залежать від абстракцій (`IMembershipRepository`, `ITokenService`), а не від конкретних класів; навіть поточний час — через `IDateTimeProvider`.
- **DRY** — назви ролей і політик — константи (`Roles`, `Policies`), спільні перевірки — в одному методі сервісу, формування помилок — в одному middleware.
- **KISS** — без зайвих шарів: Unit of Work використано лише там, де потрібна транзакція.

### Unit of Work

`IUnitOfWork.ExecuteInTransactionAsync` виконує кілька операцій як одну транзакцію. Використовується під час **запису на тренування** (перевірка дубля та вільних місць разом зі створенням запису) і під час **реєстрації** (створення облікового запису + призначення ролі User) і **зміни ролі**: якщо друга дія не вдасться, перша буде скасована, і в базі не залишиться користувача без ролі.

### HttpClientFactory

Зовнішній HTTP-сервіс у предметній області фітнес-клубу на цьому етапі не потрібен, тому `HttpClientFactory` не використовується (за методичкою — «якщо це відповідає предметній області»).

## Автентифікація та авторизація

**ASP.NET Core Identity** зберігає користувачів і ролі в тій самій базі SQLite (таблиці `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` …). Паролі зберігаються лише у вигляді хешу.

**JWT**: після реєстрації або входу сервер повертає підписаний токен (HMAC-SHA256). Клієнт передає його в кожному запиті до захищених endpoints у заголовку `Authorization: Bearer <токен>`. Сервер перевіряє підпис, видавця, аудиторію і термін дії.

Налаштування — розділ `Jwt` в `appsettings.json` (`Issuer`, `Audience`, `Key`, `ExpiresMinutes`).

### Ролі

| Роль | Хто | Що може |
|---|---|---|
| `User` | кожен зареєстрований користувач | переглядати дані, записуватися на тренування й скасовувати запис, свій профіль `GET /api/auth/me` |
| `Administrator` | адміністратор клубу | усе, що User, + створювати/змінювати/видаляти абонементи, тренування й клуби, видаляти записи, керувати користувачами |

Під час першого запуску автоматично створюються обидві ролі та адміністратор з розділу `AdminUser` в `appsettings.json`:

- email: `admin@fitnessclub.ua`
- пароль: `Admin123!`

Політики авторизації (`Auth/AuthConstants.cs`): `AdminOnly` — лише Administrator; `MemberOrAdmin` — User або Administrator.

### Як отримати JWT

1. Зареєструватися: `POST /api/auth/register` → у відповіді одразу є `token`.
   Або увійти: `POST /api/auth/login` з email і паролем.
2. Скопіювати значення `token` з відповіді.
3. У Swagger натиснути **Authorize** (замок угорі сторінки), вставити токен (без слова `Bearer`) → **Authorize**.
4. Тепер Swagger додає заголовок `Authorization` до всіх запитів. Токен дійсний 60 хвилин.

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2026-11-20T20:40:00Z",
  "user": { "id": "…", "email": "admin@fitnessclub.ua", "firstName": "Адміністратор", "lastName": "Клубу", "roles": ["Administrator", "User"] }
}
```

### Доступ до endpoints

| Endpoint | Без входу | User | Administrator |
|---|---|---|---|
| `POST /api/auth/register`, `POST /api/auth/login` | ✅ | ✅ | ✅ |
| `GET /api/auth/me` | 401 | ✅ | ✅ |
| `GET /api/memberships`, `GET /api/memberships/{id}` | ✅ | ✅ | ✅ |
| `GET /api/workouts`, `/schedule`, `/{id}` | ✅ | ✅ | ✅ |
| `POST/PUT/PATCH/DELETE /api/memberships…` | 401 | 403 | ✅ |
| `POST/PUT/DELETE /api/workouts…` | 401 | 403 | ✅ |
| `GET /api/clubs`, `GET /api/clubs/{id}` | ✅ | ✅ | ✅ |
| `POST/PUT/DELETE /api/clubs…` | 401 | 403 | ✅ |
| `GET /api/bookings/availability` | ✅ | ✅ | ✅ |
| `GET /api/bookings`, `GET /api/bookings/{id}`, `POST /api/bookings`, `POST /api/bookings/{id}/cancel` | 401 | ✅ | ✅ |
| `DELETE /api/bookings/{id}` | 401 | 403 | ✅ |
| `GET /api/users`, `PUT /api/users/{id}/role` | 401 | 403 | ✅ |

**401 Unauthorized** — токен не передано або він недійсний; **403 Forbidden** — токен дійсний, але роль не дозволяє операцію. Обидві відповіді — у форматі ProblemDetails.

## Структура проєкту

```
FitnessClub.sln
FitnessClub.Api/
├── Auth/            — ролі, політики, налаштування JWT, українські помилки Identity
├── Controllers/     — REST-контролери: лише HTTP, маршрути, [Authorize]
├── Data/            — FitnessClubDbContext (+ Identity), UnitOfWork, IdentitySeeder, Configuraitons/ (Fluent API)
├── Diagnostics/     — класи для демонстрації життєвих циклів DI
├── Migrations/      — міграції EF Core
├── DTOs/            — вхідні/вихідні моделі з валідацією
├── Exceptions/      — власні винятки (NotFound, Conflict, BadRequest, Unauthorized)
├── Extensions/      — реєстрація залежностей (DI), Identity, JWT, Swagger
├── Mapping/         — перетворення Model ↔ DTO
├── Middleware/      — логування запитів і централізована обробка помилок
├── Models/          — доменні сутності + AppUser (користувач Identity)
├── Repositories/    — доступ до даних (абонементи, тренування, клуби й записи — EF Core, клієнти й тренери — поки в пам'яті)
├── Services/        — бізнес-логіка (Service Layer) та їхні інтерфейси
├── Program.cs       — збирання застосунку й конвеєр middleware
└── FitnessClub.Api.http — готові запити для перевірки
```

Контролери залежать лише від інтерфейсів сервісів (`IMembershipService`, `IWorkoutService`, `IClubService`, `IBookingService`, `IAuthService`), сервіси — від інтерфейсів репозиторіїв. Репозиторії будують LINQ-запити (фільтрація, пошук, сортування, `Include` для пов'язаних даних), а EF Core перетворює їх на SQL. Усі операції з базою асинхронні: `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, `ExecuteDeleteAsync`. SQL-запити видно в консолі під час роботи.


## Запуск

Потрібен **.NET 11 SDK** — https://dotnet.microsoft.com/download

```bash
git clone <посилання-на-репозиторій>
cd FitnessClub
dotnet restore
dotnet tool install --global dotnet-ef --version 9.*
dotnet ef database update --project FitnessClub.Api
dotnet run --project FitnessClub.Api --launch-profile http
```

Під час першого запуску створюються ролі та адміністратор (`admin@fitnessclub.ua` / `Admin123!`).

Після запуску відкрийте **Swagger UI**: http://localhost:5191/swagger
(корінь http://localhost:5191 теж перенаправляє на Swagger).

Також можна надсилати запити з файлу `FitnessClub.Api/FitnessClub.Api.http` у Visual Studio / Rider / VS Code (розширення REST Client).
