# YaEvents
**YaEvents** Это учебный проект разрабатываемый в рамках курса **Продвинутая разработка на C# и .NET** в **Практикум**

## Общая информация о проекте
**YaEvents** представляет сервис для управления мероприятиями на ASP.NET Core Web API/

## Запуск сервиса
**Запуск из командной строки Windows**
1. Из папки проекта содержащий файл **YaEvents.slnx** выполнить команду **dotnet run -lp https**
2. Открыть браузер и перейти по адресу **https://localhost:7067/swagger/index.html**

## Запуск тестов
**Запуск из командной строки Windows**
1. Из папки проекта содержащий файл **YaEvents.slnx** выполнить команду **dotnet test**

## Изменения в рамках четвертого спринта
1. В сущность Event добавлены свойства TotalSeats и AvailableSeats. В сущность Event добавлены методы TryReserveSeats(int count = 1) и ReleaseSeats(int count = 1).
Обновлены DTO сущности CreateEvent и EventInfo. При создании нового события обязателен ввод TotalSeats, значение должно быть больше 0.
2. В BookingService.CreateBookingAsync критическая секция защищена с помощью SemaphoreSlim (в задании lock, но lock не компилируется с await).
3. При создании брони если свободных мест нет выбрасывается исключение NoAvailableSeatsException и возвращается ответ 409 Conflict.
4. Для обработки бронирований в фоновом сервисе BookingBackgroundService вызывается метод await bookingService.ProcessBookings(token); метод реализован в классе BookingService.
Синхронизация работы по обработке бронирований производится с помощью SemaphoreSlim.
5. Добавлены Unit-тесты:
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


