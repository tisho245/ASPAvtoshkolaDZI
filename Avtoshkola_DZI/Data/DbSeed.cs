using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avtoshkola_DZI.Data
{
    public static class DbSeed
    {
        private const string DefaultPassword = "Test123!";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Client>>();

            await context.Database.MigrateAsync();

            if (await context.Categories.AnyAsync())
                return;

            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // Categories
            var catB = new Category { Name = "Категория B", Description = "Леки автомобили до 3.5т" };
            var catA = new Category { Name = "Категория A", Description = "Мотоциклети" };
            context.Categories.AddRange(catB, catA);
            await context.SaveChangesAsync();

            // Courses
            var courseB = new Course
            {
                Name = "Курс Категория B - София",
                Description = "Пълен курс за категория B с теория и практика.",
                Location = "София, бул. Цар Борис III 88",
                TotalTheoryHours = 40,
                TotalPracticeHours = 32,
                Price = 850.00m,
                CategoryId = catB.Id,
                CreatedAt = now
            };
            var courseB2 = new Course
            {
                Name = "Курс Категория B - Пловдив",
                Description = "Подготвителен курс за шофьорска книжка категория B.",
                Location = "Пловдив, ул. Княз Александър I 15",
                TotalTheoryHours = 40,
                TotalPracticeHours = 32,
                Price = 790.00m,
                CategoryId = catB.Id,
                CreatedAt = now
            };
            var courseA = new Course
            {
                Name = "Курс Категория A",
                Description = "Обучение за мотоциклети и скутери над 50см³.",
                Location = "София, кв. Лозенец",
                TotalTheoryHours = 32,
                TotalPracticeHours = 20,
                Price = 650.00m,
                CategoryId = catA.Id,
                CreatedAt = now
            };
            context.Courses.AddRange(courseB, courseB2, courseA);
            await context.SaveChangesAsync();

            // Vehicles
            var vehicles = new List<Vehicle>
            {
                new() { CategoryId = catB.Id, Brand = "Volkswagen", Model = "Golf VII", IsAutomatic = false, Year = 2019, CreatedAt = now },
                new() { CategoryId = catB.Id, Brand = "Škoda", Model = "Octavia", IsAutomatic = false, Year = 2020, CreatedAt = now },
                new() { CategoryId = catB.Id, Brand = "Toyota", Model = "Yaris", IsAutomatic = true, Year = 2021, CreatedAt = now },
                new() { CategoryId = catA.Id, Brand = "Honda", Model = "CB650R", IsAutomatic = false, Year = 2022, CreatedAt = now },
                new() { CategoryId = catA.Id, Brand = "Yamaha", Model = "MT-07", IsAutomatic = false, Year = 2020, CreatedAt = now }
            };
            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();

            // Course instances
            var instance1 = new CourseInstance
            {
                CourseId = courseB.Id,
                StartDate = today.AddMonths(1),
                EndDate = today.AddMonths(2),
                Description = "Януарска група 2025"
            };
            var instance2 = new CourseInstance
            {
                CourseId = courseB.Id,
                StartDate = today.AddMonths(2),
                EndDate = today.AddMonths(3),
                Description = "Февруарска група 2025"
            };
            var instance3 = new CourseInstance
            {
                CourseId = courseA.Id,
                StartDate = today.AddMonths(1),
                EndDate = today.AddMonths(2),
                Description = "Мото курс пролет 2025"
            };
            context.CourseInstances.AddRange(instance1, instance2, instance3);
            await context.SaveChangesAsync();

            // Clients (instructors and students) – via UserManager
            var instructors = new[]
            {
                new { FirstName = "Георги", LastName = "Петров", Email = "instructor1@avtoshkola.bg", Desc = "Инструктор с 15 години опит." },
                new { FirstName = "Мария", LastName = "Иванова", Email = "instructor2@avtoshkola.bg", Desc = "Специализирана в категория A." }
            };

            var students = new[]
            {
                new { FirstName = "Иван", LastName = "Димитров", Email = "student1@test.bg" },
                new { FirstName = "Елена", LastName = "Стоянова", Email = "student2@test.bg" },
                new { FirstName = "Петър", LastName = "Николов", Email = "student3@test.bg" }
            };

            var instructorIds = new List<string>();
            foreach (var i in instructors)
            {
                var client = new Client
                {
                    UserName = i.Email,
                    Email = i.Email,
                    EmailConfirmed = true,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    Description = i.Desc,
                    PhotoURL = "/images/avatar.png",
                    CreatedAt = now,
                    LastSignedIn = now,
                    LicenseNumber = "LN-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    QualificationDesc = "Инструктор по обучение за категория B и A"
                };
                var result = await userManager.CreateAsync(client, DefaultPassword);
                if (result.Succeeded)
                    instructorIds.Add(client.Id);
            }

            var studentIds = new List<string>();
            foreach (var s in students)
            {
                var client = new Client
                {
                    UserName = s.Email,
                    Email = s.Email,
                    EmailConfirmed = true,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Description = "Курсист",
                    PhotoURL = "/images/avatar.png",
                    CreatedAt = now,
                    LastSignedIn = now
                };
                var result = await userManager.CreateAsync(client, DefaultPassword);
                if (result.Succeeded)
                    studentIds.Add(client.Id);
            }

            await context.SaveChangesAsync();

            // StudentCourseInstances (only if we have both instructors and students)
            if (instructorIds.Count > 0 && studentIds.Count > 0)
            {
                var enrollments = new List<StudentCourseInstance>
                {
                    new()
                    {
                        StudentId = studentIds[0],
                        InstructorId = instructorIds[0],
                        VehicleId = vehicles[0].Id,
                        CourseInstanceId = instance1.Id,
                        CreateAt = now,
                        CurrentTheoryHours = 10,
                        CurrentPracticeHours = 4
                    },
                    new()
                    {
                        StudentId = studentIds[1],
                        InstructorId = instructorIds[0],
                        VehicleId = vehicles[1].Id,
                        CourseInstanceId = instance1.Id,
                        CreateAt = now,
                        CurrentTheoryHours = 20,
                        CurrentPracticeHours = 12
                    },
                    new()
                    {
                        StudentId = studentIds[2],
                        InstructorId = instructorIds[1],
                        VehicleId = vehicles[3].Id,
                        CourseInstanceId = instance3.Id,
                        CreateAt = now,
                        CurrentTheoryHours = 0,
                        CurrentPracticeHours = 0
                    }
                };
                context.StudentCourseInstances.AddRange(enrollments);
                await context.SaveChangesAsync();
            }
        }
    }
}
