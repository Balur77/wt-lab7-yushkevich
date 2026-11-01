# Результаты локального тестирования WT-7

Проверка выполнена после запуска PostgreSQL 16 в Docker и приложения
`dotnet run` на `http://localhost:10000`.

```text
PostgreSQL: localhost:5432, база wt_lab7
Приложение: http://localhost:10000
```

## Применённые миграции

```text
$ dotnet ef migrations list
20261101210710_InitialCreate
20261101210747_AddCreatedAt
20261101210825_SeedCargoes
20261101210925_AddSupplier
```

## Структура PostgreSQL

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;
```

```text
      table_name
-----------------------
 Cargoes
 __EFMigrationsHistory
```

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'Cargoes'
ORDER BY ordinal_position;
```

```text
 column_name |        data_type         | is_nullable
-------------+--------------------------+-------------
 Id          | integer                  | NO
 Name        | character varying        | NO
 Category    | character varying        | NO
 Quantity    | integer                  | NO
 Location    | character varying        | NO
 Weight      | numeric                  | NO
 Description | character varying        | NO
 IsAvailable | boolean                  | NO
 CreatedAt   | timestamp with time zone | NO
 Supplier    | character varying        | NO
```

```sql
SELECT * FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

```text
         MigrationId          | ProductVersion
------------------------------+----------------
 20261101210710_InitialCreate | 9.0.11
 20261101210747_AddCreatedAt  | 9.0.11
 20261101210825_SeedCargoes   | 9.0.11
 20261101210925_AddSupplier   | 9.0.11
```

## Начальные данные

```sql
SELECT "Id", "Name", "Category", "Weight", "Supplier"
FROM "Cargoes"
ORDER BY "Id";
```

```text
 Id |               Name                |    Category     | Weight |     Supplier
----+-----------------------------------+-----------------+--------+-------------------
  1 | Ноутбуки Dell Latitude 5440       | Электроника     |  2.400 | Dell Technologies
  2 | Офисные кресла ErgoPro            | Мебель          | 12.500 | ErgoPro
  3 | Кофемашины Jura E8                | Бытовая техника |  9.800 | Jura
  4 | Наборы строительного инструмента  | Инструменты     | 25.000 | ИнструментТорг
  5 | Медицинские маски                 | Медицина        |  0.005 | МедСнаб
  6 | Серверное оборудование Supermicro | Электроника     | 18.600 | Supermicro
```

## HTTP-запросы

### GET `/`

```text
Minimal API работает!
HTTP 200
```

### GET `/api/info`

```json
{
  "application": "Web Technologies",
  "technology": "ASP.NET Core Minimal API + Entity Framework Core",
  "version": "2.0",
  "domain": "Склад и логистика",
  "entity": "Cargo",
  "storage": "PostgreSQL (EF Core, Code First)",
  "database": "wt_lab7",
  "documentation": "/scalar/v1",
  "swagger": "/swagger"
}
```

`HTTP 200`

### GET `/api/cargoes`

`HTTP 200`, 6 объектов из PostgreSQL. Первый объект:

```json
{
  "id": 1,
  "name": "Ноутбуки Dell Latitude 5440",
  "category": "Электроника",
  "quantity": 24,
  "location": "Сектор A-1, стеллаж 3",
  "weight": 2.400,
  "description": "Партия офисных ноутбуков для оснащения новых рабочих мест. Гарантия производителя — 24 месяца.",
  "isAvailable": true,
  "createdAt": "2026-01-10T09:00:00Z",
  "supplier": "Dell Technologies",
  "totalWeight": 57.60,
  "status": "На складе"
}
```

### GET `/api/cargoes/1`

`HTTP 200`, объект с `id = 1` (см. выше).

### GET `/api/cargoes/999`

`HTTP 404 Not Found`, тело пустое.

### GET `/api/cargoes/search?search=ноут`

`HTTP 200`, найдена одна запись — «Ноутбуки Dell Latitude 5440».

### GET `/api/cargoes/available`

`HTTP 200`, 5 доступных грузов, отсортированных по названию. LINQ-запрос:

```csharp
db.Cargoes
    .Where(cargo => cargo.IsAvailable)
    .OrderBy(cargo => cargo.Name)
```

### GET `/api/cargoes/heavy?minWeight=10`

`HTTP 200`, 3 груза с весом не меньше 10 кг. LINQ-запрос:

```csharp
db.Cargoes
    .Where(cargo => cargo.Weight >= threshold)
    .OrderByDescending(cargo => cargo.Weight)
```

### GET `/api/cargoes/stats`

`HTTP 200`, агрегаты по категориям:

```json
[
  {
    "category": "Инструменты",
    "count": 1,
    "totalQuantity": 45,
    "totalWeight": 1125.000000,
    "averageWeight": 25.0000000000000000,
    "availableCount": 1
  },
  {
    "category": "Электроника",
    "count": 2,
    "totalQuantity": 28,
    "totalWeight": 132.000000,
    "averageWeight": 10.5000000000000000,
    "availableCount": 1
  }
]
```

### GET `/api/cargoes/recent?days=3650`

`HTTP 200`, 6 записей, отсортированных по `CreatedAt` по убыванию.

### POST `/api/cargoes`

Тело:

```json
{
  "name": "Тестовый груз",
  "category": "Проверка",
  "quantity": 10,
  "location": "Сектор T-1",
  "weight": 1.5,
  "description": "Объект для проверки полного цикла CRUD.",
  "isAvailable": true,
  "supplier": "ТестПоставщик"
}
```

Ответ `HTTP 201 Created`:

```json
{
  "id": 7,
  "name": "Тестовый груз",
  "category": "Проверка",
  "quantity": 10,
  "location": "Сектор T-1",
  "weight": 1.5,
  "description": "Объект для проверки полного цикла CRUD.",
  "isAvailable": true,
  "createdAt": "2026-11-01T21:15:36.4182998Z",
  "supplier": "ТестПоставщик",
  "totalWeight": 15.0,
  "status": "На складе"
}
```

Заголовок `Location`: `/api/cargoes/7`.

### POST `/api/cargoes` с некорректными данными

`HTTP 400 Bad Request`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Введите название груза"],
    "Category": ["Укажите категорию"],
    "Quantity": ["Количество должно быть от 1 до 1 000 000"],
    "Location": ["Укажите место хранения"],
    "Weight": ["Вес должен быть положительным числом"],
    "Description": ["Укажите описание груза"],
    "Supplier": ["Укажите поставщика"]
  }
}
```

### PUT `/api/cargoes/7`

Тело:

```json
{
  "name": "Тестовый груз (изменён)",
  "category": "Проверка",
  "quantity": 12,
  "location": "Сектор T-2",
  "weight": 2,
  "description": "Груз обновлён через PUT.",
  "isAvailable": false,
  "supplier": "ТестПоставщик"
}
```

Ответ `HTTP 200 OK`, `totalWeight` пересчитан (`24`), `status` —
`"Нет в наличии"`, `createdAt` не изменился.

### DELETE `/api/cargoes/7`

`HTTP 204 No Content`.

### GET `/api/cargoes/7` после удаления

`HTTP 404 Not Found`.

## Документация

| Endpoint | Результат |
| --- | --- |
| `GET /openapi/v1.json` | `200 OK`, OpenAPI 3.0.4 |
| `GET /scalar/v1` | `200 OK`, HTML Scalar UI |
| `GET /swagger` | `301` → `/swagger/index.html` |

## Проверка автоматического применения миграций

При повторном запуске приложения в логе:

```text
info: Microsoft.EntityFrameworkCore.Migrations[20405]
      No migrations were applied. The database is already up to date.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:10000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

После изменения модели и создания миграции `AddSupplier` при следующем запуске
EF Core применил её автоматически, без ручного SQL.

## Проверка Docker-образа

Образ собран командой:

```bash
docker build -t wt-lab7-7postgres-efcore .
```

Контейнер запущен с отдельной чистой базой `wt_lab7_docker`:

```bash
docker run -d --name wt-lab7-api -p 10001:10000 \
  -e DATABASE_URL="postgres://postgres:postgres@host.docker.internal:5432/wt_lab7_docker" \
  wt-lab7-7postgres-efcore
```

В логах видно автоматическое применение всех четырёх миграций к чистой базе:

```text
Applying migration '20261101210710_InitialCreate'.
Applying migration '20261101210747_AddCreatedAt'.
Applying migration '20261101210825_SeedCargoes'.
Applying migration '20261101210925_AddSupplier'.
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://0.0.0.0:10000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Проверка контейнера:

```text
GET http://localhost:10001/api/info   -> 200 OK
GET http://localhost:10001/api/cargoes/1 -> 200 OK
```

Поле `database` в ответе `/api/info` содержит `wt_lab7_docker`, что подтверждает
подключение к базе через переменную `DATABASE_URL`.
