using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Avtoshkola_DZI.Models
{
    public static class DbSeed
    {
        private const string DefaultPassword = "Test123!";

        private const string AdminEmail = "admin@avtoshkola.bg";

        private const string SuperAdminUserName = "superadmin";
        private const string SuperAdminEmail = "superadmin@avtoshkola.bg";
        private const string SuperAdminPassword = "123!\"£QWe";

        private const string DemoInstructorEmail = "demo.instructor@avtoshkola.bg";
        private const string DemoStudentEmail = "demo.student@avtoshkola.bg";

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Client>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();
            await EnsurePhotoDataColumnAsync(context);

            await EnsureRolesAsync(roleManager, userManager);
            await EnsureAdminUserAsync(userManager);
            await EnsureSuperAdminUserAsync(userManager);
            await EnsureDemoUsersHaveRolesAsync(userManager);
            await EnsureDemoLoginUsersAsync(userManager);

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
                    CreatedAt = now,
                    LastSignedIn = now,
                    LicenseNumber = "LN-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                    QualificationDesc = "Инструктор по обучение за категория B и A"
                };
                var result = await userManager.CreateAsync(client, DefaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(client, RoleNames.Instructor);
                    instructorIds.Add(client.Id);
                }
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
                    CreatedAt = now,
                    LastSignedIn = now
                };
                var result = await userManager.CreateAsync(client, DefaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(client, RoleNames.Student);
                    studentIds.Add(client.Id);
                }
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

        private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<Client> userManager)
        {
            foreach (var roleName in new[] { RoleNames.Administrator, RoleNames.Instructor, RoleNames.Student })
            {
                if (await roleManager.RoleExistsAsync(roleName)) continue;
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
            await MigrateLegacyRoleToStudentAsync(userManager);
        }

        /// <summary>Миграция: потребители с остаряла роля в БД се прехвърлят в роля Student.</summary>
        private static async Task MigrateLegacyRoleToStudentAsync(UserManager<Client> userManager)
        {
            const string legacyRoleName = "Kursist"; // историческо име в БД
            try
            {
                var usersInOldRole = await userManager.GetUsersInRoleAsync(legacyRoleName);
                foreach (var user in usersInOldRole)
                {
                    await userManager.RemoveFromRoleAsync(user, legacyRoleName);
                    if (!await userManager.IsInRoleAsync(user, RoleNames.Student))
                        await userManager.AddToRoleAsync(user, RoleNames.Student);
                }
            }
            catch
            {
                // Ролята може да не съществува
            }
        }

        private static async Task EnsureDemoUsersHaveRolesAsync(UserManager<Client> userManager)
        {
            var instructorEmails = new[] { "instructor1@avtoshkola.bg", "instructor2@avtoshkola.bg" };
            foreach (var email in instructorEmails)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) continue;
                if (await userManager.IsInRoleAsync(user, RoleNames.Instructor)) continue;
                await userManager.AddToRoleAsync(user, RoleNames.Instructor);
            }
            var studentEmails = new[] { "student1@test.bg", "student2@test.bg", "student3@test.bg" };
            foreach (var email in studentEmails)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) continue;
                if (await userManager.IsInRoleAsync(user, RoleNames.Student)) continue;
                const string legacyRole = "Kursist";
                if (await userManager.IsInRoleAsync(user, legacyRole))
                    await userManager.RemoveFromRoleAsync(user, legacyRole);
                await userManager.AddToRoleAsync(user, RoleNames.Student);
            }
        }

        private static async Task EnsureAdminUserAsync(UserManager<Client> userManager)
        {
            if (await userManager.FindByEmailAsync(AdminEmail) != null) return;
            var now = DateTime.UtcNow;
            var admin = new Client
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FirstName = "Администратор",
                LastName = "Система",
                CreatedAt = now,
                LastSignedIn = now,
                Description = ""
            };
            var result = await userManager.CreateAsync(admin, DefaultPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, RoleNames.Administrator);
        }

        private static async Task EnsureSuperAdminUserAsync(UserManager<Client> userManager)
        {
            if (await userManager.FindByNameAsync(SuperAdminUserName) != null) return;
            var now = DateTime.UtcNow;
            var superAdmin = new Client
            {
                UserName = SuperAdminUserName,
                Email = SuperAdminEmail,
                EmailConfirmed = true,
                FirstName = "Tihomir",
                LastName = "Petkov",
                CreatedAt = now,
                LastSignedIn = now,
                Description = "Super administrator"
            };
            var result = await userManager.CreateAsync(superAdmin, SuperAdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, RoleNames.Administrator);
        }

        private static async Task EnsureDemoLoginUsersAsync(UserManager<Client> userManager)
        {
            var now = DateTime.UtcNow;

            // Demo instructor
            var demoInstructor = await userManager.FindByEmailAsync(DemoInstructorEmail);
            if (demoInstructor == null)
            {
                demoInstructor = new Client
                {
                    UserName = DemoInstructorEmail,
                    Email = DemoInstructorEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "Instructor",
                    Description = "Demo instructor account",
                    CreatedAt = now,
                    LastSignedIn = now,
                    LicenseNumber = "LN-DEMO-INST",
                    QualificationDesc = "Demo instructor for testing"
                };
                var result = await userManager.CreateAsync(demoInstructor, DefaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoInstructor, RoleNames.Instructor);
                }
            }

            // Demo student
            var demoStudent = await userManager.FindByEmailAsync(DemoStudentEmail);
            if (demoStudent == null)
            {
                demoStudent = new Client
                {
                    UserName = DemoStudentEmail,
                    Email = DemoStudentEmail,
                    EmailConfirmed = true,
                    FirstName = "Demo",
                    LastName = "Student",
                    Description = "Demo student account",
                    CreatedAt = now,
                    LastSignedIn = now
                };
                var result = await userManager.CreateAsync(demoStudent, DefaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoStudent, RoleNames.Student);
                }
            }
        }

        /// <summary>Добавя колона PhotoData в AspNetUsers ако липсва (резервна миграция при стартиране).</summary>
        private static async Task EnsurePhotoDataColumnAsync(ApplicationDbContext context)
        {
            try
            {
                const string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'AspNetUsers' AND COLUMN_NAME = N'PhotoData')
                        ALTER TABLE AspNetUsers ADD PhotoData varbinary(max) NULL;
                    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'AspNetUsers' AND COLUMN_NAME = N'PhotoURL')
                        ALTER TABLE AspNetUsers DROP COLUMN PhotoURL;";
                await context.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // Игнорираме при грешка (напр. вече приложена миграция)
            }
        }
    }
}
