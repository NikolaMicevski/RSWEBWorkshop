using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkshopV2.Models;

namespace WorkshopV2.Data
{
    public class WorkshopV2Context : IdentityDbContext<WorkshopV2User>
    {
        public WorkshopV2Context (DbContextOptions<WorkshopV2Context> options)
            : base(options)
        {
        }

        public DbSet<WorkshopV2.Models.Course> Course { get; set; } = default!;
        public DbSet<WorkshopV2.Models.Student> Student { get; set; } = default!;
        public DbSet<WorkshopV2.Models.Teacher> Teacher { get; set; } = default!;
        public DbSet<WorkshopV2.Models.Enrollment> Enrollment { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.FirstTeacher)
                .WithMany(t => t.CoursesAsFirstTeacher)
                .HasForeignKey(c => c.FirstTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.SecondTeacher)
                .WithMany(t => t.CoursesAsSecondTeacher)
                .HasForeignKey(c => c.SecondTeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
