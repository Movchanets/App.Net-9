# App.Net-9 — Інструкція для запуску

Короткий посібник для запуску проекту локально, створення власних `.env` файлів та налаштування `appsettings`.

## Вимоги

- Встановлений .NET SDK (перевірити: `dotnet --version`).
- (Опціонально) Для роботи з БД: SQL Server / PostgreSQL / інша СУБД, яку ви використовуєте та налаштована в `ConnectionStrings`.
- Bash shell (на Windows: Git Bash або WSL) — приклади команд в цьому README призначені для bash.

## Загальний підхід

Проект складається з трьох шарів: `API`, `Application`, `Infrastructure`.
Конфігурація читається з `appsettings.json`, `appsettings.{Environment}.json` та змінних оточення. Для секретів/локальних налаштувань рекомендується використовувати `.env` або `dotnet user-secrets`.

## 1) Налаштування `appsettings` (локально)

У цьому репозиторії є локальні файли конфігурації в `API/` — зазвичай це `API/appsettings.json` та `API/appsettings.Development.json`.
Зверніть увагу: у файлі `.gitignore` вже є записи, які ігнорують ці файли, наприклад:

- `API/appsettings.json`
- `API/appsettings.*.json`

Це означає, що реальні файли з секретами не повинні потрапляти в репозиторій. Для зручності і безпеки нижче я додаю санітизовані версії обох файлів — їх можна використовувати як шаблон (замість секретів підставляйте змінні з `.env` або `user-secrets`).

API/appsettings.json (SANITIZED - секрети видалені/замінені):

{
"Serilog": {
"Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
"MinimumLevel": {
"Default": "Information",
"Override": {
"Microsoft": "Warning",
"Microsoft.AspNetCore": "Warning",
"Microsoft.EntityFrameworkCore": "Warning",
"System": "Warning"
}
},
"WriteTo": [
{ "Name": "Console", "Args": { "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console", "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}" } },
{ "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day", "retainedFileCountLimit": 7, "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}" } }
],
"Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ]
},
"Logging": {
"LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
},
"JwtSettings": {
"Issuer": "MyAPPServer",
"Audience": "MyAPPClient",
"AccessTokenSecret": "<REDACTED:ACCESS_TOKEN_SECRET>",
"RefreshTokenSecret": "<REDACTED:REFRESH_TOKEN_SECRET>",
"AccessTokenExpirationMinutes": 15,
"RefreshTokenExpirationDays": 7
},
"SmtpSettings": {
"Host": "smtp.gmail.com",
"Port": 587,
"Username": "<REDACTED:SMTP_USERNAME>",
"Password": "<REDACTED:SMTP_PASSWORD>",
"From": "noreply@example.com",
"EnableSsl": true,
"FromName": "My App <noreply@example.com>",
"TemplatePath": "API/EmailTemplates/reset-password.html"
},
"ConnectionStrings": {
"DefaultConnection": "Server=localhost;Port=5432;User Id=postgres;Password=<REDACTED_DB_PASSWORD>;Database=application;"
},
"Turnstile": {
"Secret": "<REDACTED_TURNSTILE_SECRET>"
},
"AllowedHosts": "\*"
}

API/appsettings.Development.json (SANITIZED - секрети видалені/замінені):

{
"Logging": {
"LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" }
},
"ConnectionStrings": {
"DefaultConnection": "Server=localhost;Port=5432;User Id=postgres;Password=<REDACTED_DB_PASSWORD>;Database=application;"
},
"Turnstile": {
"Secret": "<REDACTED_TURNSTILE_SECRET>"
}
}

Порада: краще тримати короткі, немасивні налаштування у `appsettings.Development.json` і переносити реальні секрети (паролі, ключі, SMTP-паролі) у `.env` або `dotnet user-secrets`.

## 2) Команди для розробки

Запуск API локально (в середовищі Development):

dotnet run --project API

Запуск тестів:

dotnet test

EF Core — додати міграцію (як у репозиторії):

dotnet ef migrations add Name --project Infrastructure --startup-project API

Застосувати міграції до БД:

dotnet ef database update --project Infrastructure --startup-project API

(Зауваження: ці команди потрібно запускати у середовищі, де доступна БД згідно з `ConnectionStrings`.)

## 3) Конфігурація JWT (важливо)

Ключі, які проект очікує (знайдено в `Application/Services/TokenService.cs`):

- `JwtSettings:AccessTokenSecret` — секрет для підпису JWT (має бути довгим/сильним).
- `JwtSettings:Issuer` — issuer токена.
- `JwtSettings:Audience` — audience.
- `JwtSettings:AccessTokenExpirationMinutes` — тривалість життя access token.

Refresh-токени зберігаються в сутності користувача (див. `UsersController`), тому переконайтесь, що час життя та логіка оновлення налаштовані правильно.

## 4) Логи

Логи записуються у `API/logs/` (файли `log-*.txt`). Переглядайте їх при налагодженні.

## 5) Типові проблеми та виправлення

- "Cannot connect to database": перевірте `ConnectionStrings` та доступність сервера БД, застосуйте міграції.
- "Invalid JWT signature": переконайтесь, що `JwtSettings:AccessTokenSecret` однаковий у середовищі, яке підписує, і середовищі, яке перевіряє.
- Порт зайнятий: змініть порт або завершіть процес, що прослуховує.

## 6) Коротке резюме

- Запустіть API: `dotnet run --project API`.
- Запустіть тести: `dotnet test`.
- Керуйте міграціями через `dotnet ef` з параметром `--project Infrastructure --startup-project API`.

## Front-end (Front)

Короткий гайд для фронтенду (Vite + React / TypeScript) який знаходиться в `Front/`.

Що потрібно

- Node.js (рекомендовано LTS, наприклад 18+). Перевірте: `node --version`.
- npm або yarn (приклади нижче використовують `npm`).

Швидкий старт

1. Перейдіть до каталогу фронтенду:

```bash
cd Front
```

2. Встановіть залежності:

```bash
npm install
```

3. Створіть локальний `.env` на основі прикладу (файл `.env` ігнорується в Git):

```bash
cp .env.example .env
# Відредагуйте .env і підставте свої значення
```

Пояснення: Vite робить доступними лише змінні з префіксом `VITE_` в браузерному коді (наприклад `import.meta.env.VITE_TURNSTILE_SITEKEY`).

Доступні змінні (в `.env.example`):

- `VITE_API_URL` — базова URL API (напр., `http://localhost:5000`).
- `VITE_TURNSTILE_SITEKEY` — site key для Turnstile (використовується в компоненті `TurnstileWidget`).

Запуск у режимі розробки

```bash
npm run dev
```

Це запустить Vite dev server (горячий перезавантажувач). В адресі консолі Vite буде вказано локальний порт, зазвичай `http://localhost:5173`.

Збірка для продакшна

```bash
npm run build
```

Перегляд зібраного сайту локально (preview):

```bash
npm run preview
```

Примітки та поради

- `.env` у `Front` вже додано до `.gitignore`, і я прибрав його з індексу Git — так, щоб локальні ключі не потрапляли у репозиторій.
- Якщо вам потрібна змінна для API, використовуйте `VITE_API_URL` у коді, наприклад `axios.create({ baseURL: import.meta.env.VITE_API_URL })`.
- Для CI/CD замініть значення `VITE_*` у середовищі збірки (GitHub Actions, GitLab CI тощо) — Vite підхоплює їх під час збірки.

Файл прикладу змінних створено: `Front/.env.example` (без секретів). Копіюйте його в `.env` і заповнюйте свої значення.
