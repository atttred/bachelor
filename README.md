# Vy.Crm

*[English](README.en.md)*

Рішення Dynamics 365 / Dataverse для CRM відділу продажів. Містить серверні
плагіни, користувацькі API та клієнтські вебресурси, а також експортоване
кероване рішення. Інтерфейсні рядки локалізовано українською.

## Структура

```text
Vy.Crm.sln                  Рішення Visual Studio
Vy.Crm.Plugins/             Збірка плагінів і користувацьких API (.NET Framework 4.7.1)
Vy.Crm.Shared/              Спільний проєкт: ранньозв'язані сутності, помічники, сервіси
Vy.Crm.WebResources/        Вебресурси JavaScript, HTML та CSS
solution_managed/           Експортоване кероване рішення (налаштування, процеси)
packages/                   Залежності NuGet
```

## Плагіни

| Плагін | Сутність | Етап | Призначення |
| --- | --- | --- | --- |
| `AccountContactCascade` | Компанія | Post-operation | Копіює електронну пошту компанії до дочірніх контактів, у яких її немає |
| `ActivityAutoNumber` | Дія | Pre-operation | Присвоює послідовну тему (`ACT-NNNNN`), якщо її не вказано |
| `EmailDuplicateGuard` | Контакт | Pre-validation | Блокує контакти з дубльованою адресою електронної пошти |
| `CheckOpportunityUniqueness` | Угода | Pre-validation | Запобігає дублюванню активних угод для того самого клієнта й значення |
| `OpportunityStageGuard` | Угода | Pre-operation | Не дає етапу продажу повертатися назад |
| `OpportunityWinProbability` | Угода | Post-operation | Встановлює ймовірність виграшу залежно від етапу продажу |
| `AuditTrailPlugin` | Угода | Post-operation | Записує зміни атрибутів у журнал трасування плагіна |

## Користувацькі API

| API | Обробник | Призначення |
| --- | --- | --- |
| `vy_QualifyLeadAdvanced` | `QualifyLeadAdvancedHandler` | Кваліфікує потенційного клієнта у записи компанії, контакту та угоди |
| `vy_RecalculateForecast` | `RecalculateForecastHandler` | Підсумовує зважену очікувану вартість за відкритими угодами |
| `vy_SendQuoteEmail` | `SendQuoteEmailHandler` | Створює лист із комерційною пропозицією для угоди |

## Вебресурси

Скрипти форм розміщено в `Vy.Crm.WebResources/vy_/JS/` (по одному на сутність,
а також спільні `constants.js`, `webapi.js` та `xrm-utility.js`). Папка `HTML/`
містить вбудовані віджети (картки KPI, воронка продажів, прогрес етапу
продажу), а `CSS/` містить спільні стилі форм.

## Збірка

Збірка плагінів націлена на .NET Framework 4.7.1 і підписана ключем
`Vy.Crm.Plugins/key.snk`.

```sh
dotnet build Vy.Crm.Plugins/Vy.Crm.Plugins.csproj
```

Збирайте у Visual Studio або через MSBuild для всього рішення. Вебресурси є
звичайними файлами, які не потребують збірки.

## Розгортання

Зареєструйте кроки плагінів та імпортуйте кероване рішення з
`solution_managed/` у цільове середовище Dataverse. Перевірка мінімальної
вартості угоди зчитує змінну середовища `vy_plugins_min_opportunity_value`.
