# Hotels Management System API - Development Guide

ეს დოკუმენტი არის პრაქტიკული გეგმა `hms.Api` პროექტის სწორად აწყობისთვის Clean Architecture სტილში.

dotnet ef migrations add InitialCreate --project ../hms.Infrastructure --startup-project .
dotnet ef database update --project ../hms.Infrastructure --startup-project .

## 1. არქიტექტურა და Solution სტრუქტურა

პროექტები:

- `hms.Domain`
- `hms.Application`
- `hms.Infrastructure`
- `hms.Api`

დამოკიდებულებები (მიმართულება):

- `hms.Api -> hms.Application`
- `hms.Api -> hms.Infrastructure`
- `hms.Infrastructure -> hms.Application`
- `hms.Application -> hms.Domain`

წესი:

- `Domain` არ უნდა იცნობდეს `EF Core`-ს, `HTTP`-ს ან `JWT`-ს.

---

## 2. Layer-ების პასუხისმგებლობა

### `hms.Domain`

- Entity-ები: `Hotel`, `Room`, `Manager`, `Guest`, `Reservation`, `ReservationRoom`
- Enum-ები: `Roles`
- Domain exceptions (სურვილისამებრ)

### `hms.Application`

- DTO/Contracts
- Interfaces: Repositories, `IUnitOfWork`, Auth services, DateTime provider
- Use Cases (Services ან CQRS Handlers)
- Validation (`FluentValidation` რეკომენდებულია)
- Mapping (`AutoMapper` ან `Mapster`)

### `hms.Infrastructure`

- `DbContext` და EF Configurations
- Repository იმპლემენტაციები
- Auth (`JWT`), password hashing
- Migrations / Seed

### `hms.Api`

- Controllers / Endpoints
- Global Exception Middleware
- DI კონფიგურაცია
- Authentication/Authorization კონფიგურაცია
- Swagger

---

## 3. პირველი ნაბიჯები (Solution Setup)

1. შექმენი solution და 4 პროექტი ზემოთ მითითებული სახელებით.
2. დააკონფიგურირე project references არქიტექტურის წესის მიხედვით.
3. დაამატე NuGet პაკეტები (მინიმუმ):

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`
- `FluentValidation.AspNetCore` (რეკომენდებული)
- `AutoMapper.Extensions.Microsoft.DependencyInjection` ან `Mapster`
- `MediatR` (optional, თუ CQRS გინდა)

---

## 4. Domain მოდელი (Entities)

ურთიერთობები:

- `Hotel` -> ბევრი `Manager`
- `Hotel` -> ბევრი `Room`
- `Guest` -> ბევრი `Reservation`
- `Reservation` <-> ბევრი `Room` (`ReservationRoom` join table-ით)

სწორი პრაქტიკა:

- `Reservation`-ში `CheckInDate` და `CheckOutDate`: `DateOnly` (ან `DateTime` UTC)
- `Room.Price`: `decimal(18,2)`
- უნიკალურობები Domain-ში არ ჩაშენდეს EF-სთან მიბმულად; ინდექსები განისაზღვროს Infrastructure configuration-ში.

---

## 5. Infrastructure (EF Core Code First)

### 5.1 `DbContext`

`HmsDbContext : DbContext` შემდეგი `DbSet`-ებით:

- `Hotels`
- `Rooms`
- `Managers`
- `Guests`
- `Reservations`
- `ReservationRooms`

### 5.2 Fluent Configurations

საქაღალდე: `Infrastructure/Persistence/Configurations`

კონფიგურაციები:

- `HotelConfiguration`
- `RoomConfiguration`
- `ManagerConfiguration`
- `GuestConfiguration`
- `ReservationConfiguration`
- `ReservationRoomConfiguration`

აუცილებელი წესები:

- Unique Indexes:
- `Manager.Email`
- `Manager.PersonalNumber`
- `Guest.PersonalNumber`
- `Guest.PhoneNumber`
- კავშირები:
- `Hotel (1-M) Managers`
- `Hotel (1-M) Rooms`
- `Guest (1-M) Reservations`
- `Reservation (M-M) Rooms` `ReservationRoom`-ით
- `ReservationRoom` composite key: `ReservationId + RoomId`
- Delete behavior-ში სასურველია `Restrict`, რომ შემთხვევითი cascade delete არ მოხდეს.

### 5.3 Migrations

- `ConnectionStrings` განთავსდეს `hms.Api/appsettings.json`-ში
- შექმენი საწყისი migration
- გაუშვი database update

### 5.4 Seed

მინიმალური seed:

- Admin user
- Roles / claims
- Demo Hotels/Rooms (optional)

---

## 6. Authentication / Authorization სტრატეგია

ვარიანტი A (რეკომენდებული): `ASP.NET Core Identity`

- Built-in user/roles/hash
- ცალკე profile table-ები `Manager`/`Guest`-ისთვის
- JWT issuance მარტივია

ვარიანტი B: Custom Auth

- `Manager`/`Guest`-ში: `PasswordHash`, `PasswordSalt` (optional), `Role`
- Login (`email/phone + password`) -> JWT

JWT claims (მინიმუმ):

- `sub` / `userId`
- `role` (`Admin`, `Manager`, `Guest`)
- `hotelId` (Manager-ის authorization-ისთვის)

Policy-ები:

- `AdminOnly`
- `ManagerOnly`
- `GuestOnly`
- `ManagerOfHotel` (route `hotelId` == claim `hotelId`)

---

## 7. Application Layer (Use Cases)

ორგანიზების ორი სტილი:

### Service-based

- `Hotels`
- `Rooms`
- `Managers`
- `Guests`
- `Reservations`
- `Auth`

### CQRS-based

მაგალითად:

- `Hotels/Commands/CreateHotel`
- `Hotels/Queries/GetHotels`

ნებისმიერ შემთხვევაში საჭიროა:

- DTOs (Request/Response)
- Validators
- Mapping profiles

---

## 8. Repository Pattern + Unit of Work

`Infrastructure`-ში:

- `IGenericRepository<T>`
- `IUnitOfWork`

`IGenericRepository<T>` ძირითადი მეთოდები:

- `GetByIdAsync`
- `AddAsync`
- `Update`
- `Delete`
- `Query` (`IQueryable`)

შენიშვნა:

- Reservation/Availability query-ებში გამოიყენე `Include` + projection (`DTO`), რომ აირიდო `N+1` პრობლემა.

---

## 9. API Layer (Controllers და Endpoints)

Controllers:

- `HotelsController`
- `RoomsController` (`/api/hotels/{hotelId}/rooms`)
- `ManagersController` (`/api/hotels/{hotelId}/managers`)
- `ReservationsController` (`/api/hotels/{hotelId}/reservations`)
- `AuthController` (`/api/auth/login`, `/api/auth/register`)

Uniform Response Wrapper:

```csharp
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Result { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

HTTP სტატუს კოდები:

- `200 OK` (GET/UPDATE)
- `201 Created` (Create)
- `204 NoContent` (Delete)
- `400 BadRequest` (Validation)
- `401 Unauthorized`
- `403 Forbidden`
- `404 NotFound`
- `409 Conflict` (მაგ: room already booked)

---

## 10. Global Exception Middleware

`hms.Api`-ში middleware-ს უნდა შეეძლოს:

- სპეციფიკური exception-ების mapping შესაბამის HTTP status-ზე:
- `NotFoundException`
- `ValidationException`
- `ConflictException`
- `ForbiddenException`
- გაუთვალისწინებელი შეცდომა -> `500` + ზოგადი მესიჯი
- დეტალური ინფორმაცია ჩაიწეროს log-ში

---

## 11. კრიტიკული ბიზნეს წესები

### Hotel Delete

დაშვებულია მხოლოდ თუ:

- სასტუმროს არ აქვს ოთახები
- არ აქვს აქტიური/მომავალი დაჯავშნები

რადგან `Reservation` პირდაპირ `Hotel`-ს არ უკავშირდება, შემოწმება ხდება ჯაჭვით:

- `ReservationRooms -> Rooms -> HotelId`

### Room Availability

არ გამოიყენო `IsAvailable` ველი როგორც სიმართლე.
Availability განისაზღვროს თარიღის დიაპაზონით.

Overlap rule:

```text
existing.CheckIn < newCheckOut AND newCheckIn < existing.CheckOut
```

### Room Delete

დაშვებულია მხოლოდ თუ აქტიური/მომავალი reservation არ არსებობს.

### Reservation Create

აუცილებელი ვალიდაციები:

- მხოლოდ `Guest` role
- `CheckIn >= Today`
- `CheckOut > CheckIn`
- ყველა room ეკუთვნის route-ის `hotelId`-ს
- ყველა room თავისუფალია (overlap check)

### Reservation Update

- შეიცვალოს მხოლოდ თარიღები
- room-ების შეცვლა დაიბლოკოს
- overlap შემოწმება გაკეთდეს სხვა reservation-ებთან (თავისი თავის გამორიცხვით)

### Reservation Search

ფილტრები:

- `hotelId`, `guestId`, `roomId`, date range
- `active` და `completed`

მაგალითი:

- active: `today` შედის `[CheckIn, CheckOut)` შუალედში
- completed: `CheckOut < today`

---

## 12. Authorization წესები

### Admin

- სრული წვდომა

### Manager

- მხოლოდ თავისი `HotelId`-ის ფარგლებში:
- rooms create/update/delete
- managers create/update/delete (შეზღუდვებით)
- reservations view/search

### Guest

- მხოლოდ საკუთარი პროფილი
- საკუთარი reservation create/update/cancel

აუცილებელი შემოწმებები:

- route `hotelId` + manager claim `hotelId`
- reservation ownership: `GuestId == token userId`

---

## 13. Swagger და Versioning

- JWT Bearer security definition
- საჭიროების შემთხვევაში request examples
- სურვილისამებრ API versioning (`v1` route pattern)

---

## 14. ტესტირება (მინიმუმი)

სასურველია:

- Unit tests (`Application`-ზე: validation + business rules)
- Integration tests (`API`/`Infrastructure`)

თუ დრო შეზღუდულია, მინიმუმ დაფარე:

- overlap logic
- hotel delete restrictions
- reservation create validations

---

## 15. რეკომენდებული შესრულების რიგი

1. Solution skeleton + DI
2. Domain entities
3. EF Core DbContext + Configurations + Migrations
4. Generic Repository + UnitOfWork
5. Global exception middleware + `ApiResponse<T>`
6. Auth (register/login) + JWT + Roles
7. Hotels CRUD + filter
8. Rooms CRUD + availability search
9. Guests CRUD
10. Managers CRUD
11. Reservations create/update/cancel + search
12. Swagger polish + seed + tests
