# YaEvents
**YaEvents** Это учебный проект разрабатываемый в рамках курса **Продвинутая разработка на C# и .NET** в **Практикум**

## Общая информация о проекте
**YaEvents** представляет сервис для управления мероприятиями на ASP.NET Core Web API/

## Запуск сервиса
**Запуск из командной строки Windows**
1. Из папки проекта содержащий файл **YaEvents.slnx** выполнить команду **dotnet run -lp https --project Presentation\**
2. Открыть браузер и перейти по адресу **https://localhost:7067/swagger/index.html**

## Авторизация в Swagger
1. При необходимости создать пользователя используя POST /auth/register
2. Авторизоваться используя POST /auth/login
3. После успешной авторизации скопировать из тела ответа токен.
4. Нажать Authorize. В появившемся окне вставить в поле Value токен и нажать Authorize. 

## Запуск тестов
**Запуск из командной строки Windows**
1. Из папки проекта содержащий файл **YaEvents.slnx** выполнить команду **dotnet test**

## Изменения в рамках ревью восьмого спринта
1. В BookingServiceTests добавлены тесты:
	CreateBookingAsync_BookingLimitOfSomeUsersDoesNotAffectOthers_ThrowLimitOfActiveBookingsExceededExceptionForOneAndCorectResultForOthers()
	CancelBooking_UserCancelHisBooking_ReturnsTrue()
	CancelBooking_UserCancelNotHisBooking_ThrowNoRightsToOperationException()
	CancelBooking_AdminCancelBooking_ReturnsTrue()
2. Согласованы ключи для claim при создании токенов и в эндпоинтах.
3. Внесены изменения в BookingsRepository.Get, SecurityServise, EventsController, Program.cs. Добавлено DTO UserAuthentication и исправлен POST /auth/login, 

## Изменения в рамках восьмого спринта
1. Добавлена сущность пользователя. Бронирование связано с пользователем и создана соответсвующая миграция.
2. Реализованы доменные правила: запрет бронирования прошедшего события, лимит активных броней, проверка прав при отмене.
3. Реализован эндпоинт логина, возвращающий JWT-токен. Секрет JWT вынесен в конфигурацию. 
4. JWT-аутентификация настроена; защищённые эндпоинты отклоняют запросы без токена (401).
Создание, редактирование и удаление событий доступны только администраторам (403 для других).
Отмена чужой брони без прав администратора возвращает 403.
При неверных учётных данных возвращается одно и то же сообщение об ошибке.
5. Пароли пользователей хранятся в виде хеша.
6. Обновлены старые тесты и добавлены новые unit и интеграционные тесты.
7. Сервис генерации токена вынесен в Infrastructure, его абстракция определена в Application.
8. Swagger настроен для работы с JWT (кнопка «Authorize»).
9. Лимит активных броней вынесен в конфигурацию. Сообщение об ошибке при превышении лимита содержит само значение лимита.





## Изменения в рамках седьмого спринта
1. Солюшен разделён на четыре отдельных проекта (сборки): Domain, Application, Infrastructure, Presentation.
2. Доменные сущности и доменные исключения находятся в слое Domain.
3. Бизнес-логика находится в слое Application.
4. Интерфейсы портов (репозитории/шлюзы) определены в Application.
5. Реализации портов (DbContext, репозитории) находятся в Infrastructure.
6. Интерфейсы репозиториев расположены в слое Application.
7. Регистрация зависимостей находится в Presentation.
8. В проекте создана начальная миграция для events и bookings.
Для создания и применения миграций из командной строки необходимо выполнять команды из директории YaEvents\Infrastructure
Для создания нового файла миграции на основе изменений в модели данных используется команда dotnet ef migrations add [NAME]
Для применения миграции к базе данных используется команда dotnet ef database update [MIGRATION]
9. Для регистрации зависимостей каждого слоя используются extension-методы
10. Unit тесты разделены и перенесены в отдельные сборки: ApplicationTests, PresentationTests.
11. Интеграционные тесты остались в сборке YaEvents.IntegrationTests. 

## Изменения в рамках шестого спринта 
1. Для управленией схемой базы данных используются миграции.
2. В проекте создана начальная миграция для events и bookings.
Для создания нового файла миграции на основе изменений в модели данных используется команда dotnet ef migrations add [NAME]
Для применения миграции к базе данных используется команда dotnet ef database update [MIGRATION]
3. Реализованы репозитории YaEvents.Infrastructure.Repositories.BookingsRepository и
YaEvents.Infrastructure.Repositories.EventsRepository.
4. Сервисы для получения объектов из БД используют репозитории полученные через DI.
5. Созданы интеграционные тесты с реальным PostgreSQL через Testcontainers. Тесты покрывают все метады обоих репозиториев.
Тесты для EventsRepository рассположены YaEvents.IntegrationTests.EventsRepositoryTests.
Тесты для BookingsRepository рассположены YaEvents.IntegrationTests.BookingsRepositoryTests.
6. В YaEvents.IntegrationTests.BookingsRepositoryTests созданы тесты для проверки ограничений AddBooking_InvalidEventId_ThrowsDbUpdateException() и связей GetByEventId_ReturnsOnlyReqieredBookings().
7. Все тесты используют один контейнер PostgreSQL, база приводится к чистому состоянию между тестами.
8. Для корректной работы тестов необходим запущеный Docker.

## Изменения в рамках пятого спринта
1. Реализовано хранение данных в базе данных PostgreSql.
1. Строка подключения расположена в appsettings.json
2. Схема базы данных создается автоматически в Program.cs при запуске метода EnsureCreated().
3. В тестах для имитации работы с базой данных реализовано использование InMemory-провайдера.

## Изменения в рамках четвертого спринта 
1. В сущность Event добавлены свойства TotalSeats и AvailableSeats. В сущность Event добавлены методы TryReserveSeats(int count = 1) и ReleaseSeats(int count = 1).
Обновлены DTO сущности CreateEvent и EventInfo. При создании нового события обязателен ввод TotalSeats, значение должно быть больше 0.
2. В сущность Event добавлено свойство(get) EventSemaphore.
3. В сущность Booking добавлено свойство(get;) BookingSemaphore.
4. В BookingService.CreateBookingAsync критическая секция защищена с помощью SemaphoreSlim (в задании lock, но lock не компилируется с await). SemaphoreSlim расположен в объекте Event.
Это сделано чтобы блокировать только создания бронирований для одного и того же события. 
5. При создании брони если свободных мест нет выбрасывается исключение NoAvailableSeatsException и возвращается ответ 409 Conflict.
6. Для обработки бронирований в фоновом сервисе BookingBackgroundService вызывается метод await bookingService.ProcessBookings(token); метод реализован в классе BookingService.
Синхронизация работы по обработке бронирований производится с помощью SemaphoreSlim. Используются объекты SemaphoreSlim из экземпляров Booking и Event.
7. Добавлены Unit-тесты:
Создание брони уменьшает AvailableSeats на 1.(CreateBookingAsync_CorrectParam_CorrectAvailableSeatsAfterBooking() класс BookingServiceTests);
Создание нескольких броней (до лимита) — все успешны, у каждой уникальный Id.(CreateBookingAsync_MethodRunSeveralTimesWithOneEventId_ReturnDifferentBookingInfo() класс BookingServiceTests);
После исчерпания мест следующая попытка выбрасывает NoAvailableSeatsException.(CreateBookingAsync_BookMoreThanAvailableSeats_ThrowNoAvailableSeatsException() класс BookingServiceTests);
Бронирование для несуществующего события → NotFoundException.(CreateBookingAsync_NotExistingEvent_ThrowNotFoundException() класс BookingServiceTests);
Бронирование при отсутствии мест → NoAvailableSeatsException.(CreateBookingAsync_BookMoreThanAvailableSeats_ThrowNoAvailableSeatsException() класс BookingServiceTests);
После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.(ProcessBookingAsync_CorrectParameters_BookingStatusEqualConfirmed() и ProcessBookingAsync_CorrectParameters_ProcessedAtNotNull() класс BookingServiceTests);
После вызова Reject() бронь возвращает статус Rejected и заполненный ProcessedAt.(RejectBookingAsync_CorrectParameters_BookingStatusEqualRejected() и RejectBookingAsync_CorrectParameters_ProcessedAtChanged() класс BookingServiceTests);
После Reject() ReleaseSeats() количество свободных мест восстанавливается.(RejectBookingAsync_CorrectParameters_CorrectAvailableSeats() класс BookingServiceTests);
После Reject() ReleaseSeats() можно успешно создать новую бронь на то же место.(CreateBookingAsync_BookSeatAfterReleaseSeats_CorrectAvailableSeatsCount() класс BookingServiceTests);
тесты на конкурентность.(CreateBookingAsync_FifteenConcurrentRequests_FiveSuccessAndTenException() и CreateBookingAsync_TenConcurrentRequests_TenUniqueId() класс BookingServiceTests);

## Изменения в рамках третьего спринта
1.Реализована модель **Booking** (namespace **YaEvents.Data.Models**), перечисления для статусов **BookingStatus** и **EventStatus** (namespace **YaEvents.Infrastructure.Enums**).
Добавлено хранилище для бронирований **BookingsRepository** (namespace **YaEvents.Infrastructure.Repositories.BookingsRepository**).
Объекты бронирования создаются со статусом **Pending** уникальным Id и текущей датой в **CreatedAt**.

2. Реализован сервис бронирования **BookingService** (namespace **YaEvents.Application.Services.BookingService**).

3. Реализованы эндпоинты **POST /events/{id}/book** и **GET /bookings/{id}** (см. **EventEndpoints** и **BookingEndpoints** namespace **YaEvents.Presentation.Endpoints**).
Эндпоинты зарегистрированы с помощью метода расширения **EndpointsExtension.AddEndpoints(this WebApplication app)** (namespace **YaEvents.Presentation**).

4. Реализован фоновый сервис **BookingsBackgroundService** (namespace **YaEvents.Application.BackgroundServices**).
Добавлено логирование. Логика обработки бронирований вынесена в **BookingService**. При обработке бронирований предусмотрена задержка 2 секунды.
Для созданных бронирований статус меняется на **Confirmed**, если событие к которому привязано бронирование было удалено статус бронирования меняется на **Rejected**.
При изменении статуса заполняется свойство **ProcessedAt**. Фоновый сервис зарегистрирован через **AddHostedService**

5. Добавлены юнит-тесты. **BookingServiceTests**, **BookingEndpointsTests** и **EventEndpointsTests**. Протестированы успешные и неуспешные сценарии.

## Изменения в рамках второго спринта
1. Реализован middleware **ExceptionHandlingMiddleware** для глобальной обработки исключений. В middleware реализовано логирование ошибок.
middleware расположен в namespace **YaEvents.Application.Middleware**. Формат ответа при ошибках Problem Details (RFC 7807).

2. Добавлена фильтрация событий по **title**, **from**, **to**. Валидация параметров реализована в контроллере **EventsController**.
Фильтрация реализована в сервисе **EventService**

3. Добавлена пагинация событий. Валидация параметров реализована в контроллере **EventsController**.
Пагинация реализована в сервисе **EventService**.

4. Добавлены юнит-тесты реализующие проверку успешных и неуспешных сценариев работы методов класса **EventService**.
Тесты реализованы в классе **EventServiceTests** расположенном в namespace **YaEvents.Tests.Application.Services**


### Пример запроса GET /events с query который не пройдет валидации
запрос: **https://localhost:7067/events?title=%D1%82&from=2020-01-01&to=2025-01-01&page=0&pageSize=10**
ответ: **{"title":"В запросе на получение событий переданы некорректные параметры.","status":400,"detail":"Детали ошибки: Номер страницы не может быть меньше 1. "}**


