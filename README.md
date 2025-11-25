# NotificationGateway

## 🎯 Назначение

Единый REST API для отправки уведомлений через Telegram, Email, Webhook с идемпотентностью и retry-логикой.

## 🛠 Технологии
* .NET 9 + ASP.NET Core
* PostgreSQL + EF Core
* Hangfire - фоновая обработка
* Redis - кэш идемпотентности
    
## 🚀 Фичи
* Идемпотентность - защита от дублей
* Мониторинг через Hangfire Dashboard
* Авто-очистка старых уведомлений
* Валидация + обработка ошибок
* Retry with exponential backoff
