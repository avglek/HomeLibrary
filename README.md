# 📚 Домашняя библиотека

Веб-приложение для учёта домашней библиотеки: карточки книг, редактирование
оглавления в WYSIWYG-редакторе, поиск по названию, автору и содержимому оглавления.

**Стек:**

- **ASP.NET Core 10 Minimal API** — бэкенд
- **Dapper** — микро-ORM для доступа к БД
- **PostgreSQL 18** — хранилище, включая тип `xml` для оглавления
- **Trumbowyg** — HTML-редактор оглавления на фронте (локально, без CDN)
- **jQuery** — для работы редактора и упрощения работы с DOM
- **xUnit + Moq + FluentAssertions** — unit- и интеграционные тесты
- **Docker / docker-compose** — упаковка и запуск
- **Хранимые процедуры PostgreSQL** — вся работа с БД идёт через них

---

## 📋 Возможности

- ✅ CRUD по книгам (создание, чтение, обновление, удаление)
- ✅ Оглавление хранится как XML-документ в поле `toc_content`
- ✅ Редактирование оглавления в HTML-редакторе Trumbowyg
- ✅ Поиск по названию, автору и **содержимому** оглавления (через `xpath`)
- ✅ Все операции с БД — через **хранимые процедуры** PostgreSQL
- ✅ Автоматическая инициализация БД через `db/init.sql`
- ✅ Swagger UI для тестирования API
- ✅ Покрытие бизнес-логики unit-тестами

---

## 🏗️ Структура проекта

```
HomeLibrary/
│
├── HomeLibrary.sln                     # Файл решения
│
├── HomeLibrary/                        # Основной проект (Minimal API)
│   │
│   ├── HomeLibrary.csproj              # Файл проекта (.NET 8)
│   ├── Program.cs                      # Точка входа: DI, middleware, запуск
│   ├── appsettings.json                # Конфигурация (строка подключения)
│   ├── appsettings.Development.json    # Настройки для dev-окружения
│   │
│   ├── Configuration/                  # Настройки и DI-расширения
│   │   ├── DatabaseOptions.cs          # Strongly-typed options для БД
│   │   ├── ApplicationServiceExtensions.cs   # DI для бизнес-логики
│   │   ├── InfrastructureServiceExtensions.cs # DI для инфраструктуры
│   │   └── ApiServiceExtensions.cs     # DI для Swagger, CORS, JSON
│   │
│   ├── Data/                           # Работа с БД
│   │   ├── IDbConnectionFactory.cs     # Абстракция над подключением
│   │   └── NpgsqlConnectionFactory.cs  # Реализация для PostgreSQL
│   │
│   ├── Models/                         # Модели и DTO
│   │   ├── Book.cs                     # Доменная модель книги
│   │   └── BookCreateDto.cs            # DTO для создания/обновления
│   │
│   ├── Repositories/                   # Репозитории
│   │   ├── IBookRepository.cs          # Интерфейс репозитория
│   │   └── BookRepository.cs           # Реализация через Dapper
│   │
│   ├── Services/                       # Бизнес-логика
│   │   ├── IBookService.cs             # Интерфейс сервиса
│   │   ├── BookService.cs              # Реализация сервиса
│   │   ├── IBookValidator.cs           # Интерфейс валидатора
│   │   └── BookValidator.cs            # Валидация DTO
│   │
│   ├── Endpoints/                      # API-эндпоинты
│   │   └── BookEndpoints.cs            # CRUD + поиск для /api/books
│   │
│   └── wwwroot/                        # Статические файлы
│       ├── index.html                  # Список книг + поиск
│       ├── book.html                   # Карточка книги + Trumbowyg
│       ├── css/
│       │   └── site.css                # Стили
│       ├── lib/                        # Локальные библиотеки
│       │   ├── jquery/
│       │   │   └── jquery-3.7.1.min.js
│       │   └── trumbowyg/
│       │       ├── trumbowyg.min.css
│       │       ├── trumbowyg.min.js
│       │       ├── langs/
│       │       │   └── ru.min.js       # Русская локализация
│       │       └── ui/
│       │           └── icons.svg       # Иконки редактора
│       └── js/
│           ├── api.js                  # Обёртки над $.ajax
│           ├── list.js                 # Логика страницы списка
│           └── card.js                 # Логика карточки + Trumbowyg
│
├── HomeLibrary.Tests/                  # Тестовый проект
│   ├── HomeLibrary.Tests.csproj        # xUnit + Moq + FluentAssertions
│   ├── Fixtures/
│   │   ├── TestData.cs                 # Фабрика тестовых данных
│   │   └── CustomWebApplicationFactory.cs # WebApplicationFactory с мок-репозиторием
│   ├── Services/
│   │   └── BookServiceTests.cs         # Unit-тесты бизнес-логики
│   └── Api/
│       └── BooksApiTests.cs            # Интеграционные тесты HTTP-слоя
│
├── db/
│   └── init.sql                        # DDL: таблица + 6 хранимых процедур
│
├── Dockerfile                          # Сборка образа API (multi-stage)
├── docker-compose.yml                  # API + PostgreSQL
├── .dockerignore                       # Исключения для сборки образа
├── .gitignore                          # Исключения для Git
└── README.md                           # Этот файл
```

---

## 🚀 Быстрый старт

### Вариант 1: Docker (рекомендуется)

**Требования:** Docker Desktop 4.x+ (или Docker + docker-compose).

```bash
git clone <url-репозитория> HomeLibrary

cd HomeLibrary
docker-compose up --build
```

После старта:

| Сервис        | URL                                                                      |
| ------------- | ------------------------------------------------------------------------ |
| Веб-интерфейс | http://localhost:8080/                                                   |
| Swagger UI    | http://localhost:8080/swagger                                            |
| PostgreSQL    | `localhost:5432` (user: `postgres`, pass: `postgres`, db: `homelibrary`) |

**Остановить:**

```bash
docker-compose down
```

**Остановить и удалить данные БД** (чтобы `init.sql` выполнился заново при следующем старте):

```bash
docker-compose down -v
docker-compose up --build
```

### Вариант 2: Локально без Docker

**Требования:**

- .NET 10 SDK
- PostgreSQL 18 (установленный локально или в контейнере)

**Шаги:**

1. Установите PostgreSQL и создайте БД:

   ```bash
   psql -U postgres -c "CREATE DATABASE homelibrary;"
   ```

2. Примените `db/init.sql` — создаст таблицу, индексы, триггер и 6 хранимых функций:

   ```bash
   psql -U postgres -d homelibrary -f db/init.sql
   ```

3. Проверьте `HomeLibrary/appsettings.json` — строка подключения должна указывать на ваш PostgreSQL:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=homelibrary;Username=postgres;Password=postgres"
     }
   }
   ```

4. Запустите приложение:

   ```bash
   cd HomeLibrary
   dotnet run
   ```

5. Откройте браузер: **http://localhost:5000** (или порт из консоли).

---

## 🧪 Запуск тестов

**Требования:** .NET 10 SDK.

```bash
# Из корневой папки решения
dotnet test
```

Тесты используют **мок `IBookRepository`** через Moq и не требуют работающей БД.

---

## 🔌 API endpoints

| Метод    | Маршрут               | Описание                            |
| -------- | --------------------- | ----------------------------------- |
| `GET`    | `/api/books`          | Список книг (без оглавления)        |
| `GET`    | `/api/books?search=…` | Поиск по названию/автору/оглавлению |
| `GET`    | `/api/books/{id}`     | Книга по ID, включая `tocContent`   |
| `POST`   | `/api/books`          | Создать книгу                       |
| `PUT`    | `/api/books/{id}`     | Обновить книгу                      |
| `DELETE` | `/api/books/{id}`     | Удалить книгу                       |

### Пример создания книги

```bash
curl -X POST http://localhost:8080/api/books \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Война и мир",
    "author": "Лев Толстой",
    "publishYear": 1869,
    "tocContent": "<div><![CDATA[<h1>Том 1</h1><h2>Глава 1</h2>]]></div>"
  }'
```

### Пример поиска

```bash
curl "http://localhost:8080/api/books?search=Толстой"
curl "http://localhost:8080/api/books?search=Глава"
```

---

## 🗄️ Хранимые процедуры

Все операции с БД выполняются через функции PostgreSQL (см. `db/init.sql`):

| Функция                                     | Назначение                                   |
| ------------------------------------------- | -------------------------------------------- |
| `insert_book(title, author, year, toc)`     | Создать книгу, вернуть ID                    |
| `update_book(id, title, author, year, toc)` | Обновить книгу                               |
| `delete_book(id)`                           | Удалить книгу                                |
| `get_all_books()`                           | Список всех книг (без оглавления)            |
| `get_book_by_id(id)`                        | Книга по ID + `toc_content::text`            |
| `search_books(term)`                        | Поиск по названию/автору/`xpath('//text()')` |

### Поиск по XML

В `search_books` используется встроенная функция PostgreSQL `xpath`:

```sql
SELECT 1 FROM unnest(xpath('//text()', b.toc_content)) AS x
WHERE x::text ILIKE '%' || term || '%'
```

## Это позволяет искать по тексту внутри XML-оглавления без полнотекстового индекса.

## 🧩 Формат хранения оглавления

Оглавление хранится в поле `toc_content` типа `XML`. Клиент (Trumbowyg)
генерирует HTML, который оборачивается в CDATA, чтобы `xmlparse` не падал
на невалидных для XML конструкциях:

```html
<div>
  <![CDATA[ <h1>Том первый</h1> <h2>Глава I. Салон Анны Павловны</h2> <p>…</p>
  ]]>
</div>
```

---

## 🏛️ Архитектура и DI

Проект построен по принципам чистой архитектуры с использованием DI:

```
HTTP-запрос
    │
    ▼
BookEndpoints (Minimal API)      ← маршрутизация, HTTP-коды
    │
    ▼
IBookService / BookService        ← бизнес-логика, валидация, нормализация
    │
    ▼
IBookValidator / BookValidator    ← правила валидации
    │
    ▼
IBookRepository / BookRepository  ← Dapper, вызовы хранимок
    │
    ▼
IDbConnectionFactory              ← создание NpgsqlConnection
    │
    ▼
PostgreSQL                        ← хранимые процедуры
```

**Жизненные циклы сервисов:**

| Сервис                 | Lifetime  | Почему                                      |
| ---------------------- | --------- | ------------------------------------------- |
| `IDbConnectionFactory` | Singleton | Stateless, хранит только строку подключения |
| `IBookRepository`      | Scoped    | Открывает подключение на каждый запрос      |
| `IBookService`         | Scoped    | Зависит от scoped-репозитория               |
| `IBookValidator`       | Scoped    | Stateless, но для единообразия              |

**Регистрация в `Program.cs`:**

```csharp
builder.Services
    .AddInfrastructure(builder.Configuration)  // БД, репозитории
    .AddApplication()                          // сервисы, валидатор
    .AddApiServices(builder.Configuration);    // Swagger, CORS, JSON
```

---

## 📄 Лицензия

MIT — используйте свободно.
