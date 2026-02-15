using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Avtoshkola_DZI.Data
{
    public class ApplicationDbContext : IdentityDbContext<Client>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseInstance> CourseInstances { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<StudentCourseInstance> StudentCourseInstances { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            // Две връзки от StudentCourseInstance към Client (Student и Instructor)
            builder.Entity<StudentCourseInstance>()
                .HasOne(s => s.Student)
                .WithMany(c => c.StudentCourseInstances)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentCourseInstance>()
                .HasOne(s => s.Instructor)
                .WithMany(c => c.InstructorCourseInstances)
                .HasForeignKey(s => s.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Избягване на множество cascade пътища (SQL Server ограничение)
            builder.Entity<StudentCourseInstance>()
                .HasOne(s => s.Vehicles)
                .WithMany(v => v.StudentCourseInstances)
                .HasForeignKey(s => s.VehicleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentCourseInstance>()
                .HasOne(s => s.CourseInstances)
                .WithMany(c => c.StudentCourseInstances)
                .HasForeignKey(s => s.CourseInstanceId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
