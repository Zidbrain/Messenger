# Messenger
Simple messenger app backend

## Найстройка и запуск
1. Скачать и установить [MySQL](https://dev.mysql.com/downloads/windows/installer/8.0.html). (Установить Server и Workbench)
2. Добавить схему [messenger](https://github.com/Zidbrain/Messenger/blob/42da86f7f85dbc27d0964a216d538e1307919342/Messenger/messenger.sql).
3. Настроить подключение в appsettings.json
    - В поле "MessengerContext" добавить имя пользователя и пароль для подключения к базе данных в виде uid={имя пользователя};pwd={пароль}.
    - При необходимости изменить название базы данных и порт. 
    - Поле должно выглядеть примерно так: `"MessengerContext": "server=localhost;port=3306;database=Messenger;uid=root;pwd=123"`.
4. Скачать и установить [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
5. Для запуска сервера из деректории Messenger (там где лежит файл .csproj) выполнить команду `dotnet run`.

По умолчанию сервер запускается на порте 7230. Используется протокол HTTPS.
