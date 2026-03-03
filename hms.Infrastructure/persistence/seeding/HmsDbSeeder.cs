using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using hms.Domain.Entities;
using hms.Infrastructure.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace hms.Infrastructure.Persistence.Seeding
{
    public static class HmsDbSeeder
    {
        private static readonly Guid HotelId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid RoomId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid ManagerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        private static readonly Guid GuestId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private static readonly Guid ReservationId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        private static readonly Guid AdminUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<HmsDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedDomainDataAsync(dbContext, cancellationToken);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
        {
            foreach (var role in Enum.GetValues<AppRole>())
            {
                var roleName = role.ToRoleName();

                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                });
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminUserName = "admin";
            const string adminEmail = "admin@hms.local";
            const string adminPassword = "Admin123!";

            var user = await userManager.FindByNameAsync(adminUserName)
                ?? await userManager.FindByEmailAsync(adminEmail);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Id = AdminUserId,
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Admin",
                    PersonalNumber = "00000000001"
                };

                var createResult = await userManager.CreateAsync(user, adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to seed admin user: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, AppRole.Admin.ToRoleName()))
            {
                var addToRoleResult = await userManager.AddToRoleAsync(user, AppRole.Admin.ToRoleName());

                if (!addToRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addToRoleResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to assign admin role: {errors}");
                }
            }
        }

        private static async Task SeedDomainDataAsync(HmsDbContext dbContext, CancellationToken cancellationToken)
        {
            if (!await dbContext.Hotels.AnyAsync(x => x.Id == HotelId, cancellationToken))
            {
                dbContext.Hotels.Add(new Hotel
                {
                    Id = HotelId,
                    Name = "Sunrise Hotel",
                    Rating = 4,
                    Address = "12 Rustaveli Ave",
                    City = "Tbilisi"
                });
            }

            if (!await dbContext.Rooms.AnyAsync(x => x.Id == RoomId, cancellationToken))
            {
                dbContext.Rooms.Add(new Room
                {
                    Id = RoomId,
                    Name = "101",
                    Price = 180.00m,
                    HotelId = HotelId
                });
            }

            if (!await dbContext.Managers.AnyAsync(x => x.Id == ManagerId, cancellationToken))
            {
                dbContext.Managers.Add(new Manager
                {
                    Id = ManagerId,
                    FirstName = "Nino",
                    LastName = "Manager",
                    PersonalNumber = "00000000002",
                    Email = "manager@hms.local",
                    PhoneNumber = "+995555000001",
                    HotelId = HotelId
                });
            }

            if (!await dbContext.Guests.AnyAsync(x => x.Id == GuestId, cancellationToken))
            {
                dbContext.Guests.Add(new Guest
                {
                    Id = GuestId,
                    FirstName = "Giorgi",
                    LastName = "Guest",
                    PersonalNumber = "00000000003",
                    PhoneNumber = "+995555000002"
                });
            }

            if (!await dbContext.Reservations.AnyAsync(x => x.Id == ReservationId, cancellationToken))
            {
                dbContext.Reservations.Add(new Reservation
                {
                    Id = ReservationId,
                    CheckInDate = new DateTime(2026, 3, 10),
                    CheckOutDate = new DateTime(2026, 3, 12),
                    GuestId = GuestId
                });
            }

            if (!await dbContext.ReservationRooms.AnyAsync(
                x => x.ReservationId == ReservationId && x.RoomId == RoomId,
                cancellationToken))
            {
                dbContext.ReservationRooms.Add(new ReservationRoom
                {
                    ReservationId = ReservationId,
                    RoomId = RoomId
                });
            }

            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
