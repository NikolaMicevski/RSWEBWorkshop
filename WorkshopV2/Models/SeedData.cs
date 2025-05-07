using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WorkshopV2.Models;

namespace WorkshopV2.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new WorkshopV2Context(
                serviceProvider.GetRequiredService<DbContextOptions<WorkshopV2Context>>());

            // Check if data already exists
            if (context.Student.Any() || context.Teacher.Any() || context.Course.Any() || context.Enrollment.Any())
            {
                return; // DB has been seeded
            }

            // Seed Teachers
            var teacher1 = new Teacher
            {
                FirstName = "Elena",
                LastName = "Petrova",
                Degree = "PhD",
                AcademicRank = "Professor",
                OfficeNumber = "B-101",
                HireDate = new DateTime(2010, 5, 15)
            };

            var teacher2 = new Teacher
            {
                FirstName = "Marko",
                LastName = "Jovanov",
                Degree = "MSc",
                AcademicRank = "Assistant",
                OfficeNumber = "B-102",
                HireDate = new DateTime(2018, 9, 1)
            };

            context.Teacher.AddRange(teacher1, teacher2);
            context.SaveChanges();

            // Seed Students
            var student1 = new Student
            {
                StudentId = "20200001",
                FirstName = "Ana",
                LastName = "Stojanovska",
                EnrollmentDate = new DateTime(2020, 10, 1),
                AcquiredCredits = 120,
                CurrentSemestar = 6,
                EducationLevel = "Undergraduate"
            };

            var student2 = new Student
            {
                StudentId = "20200002",
                FirstName = "Ivan",
                LastName = "Trajanov",
                EnrollmentDate = new DateTime(2021, 10, 1),
                AcquiredCredits = 60,
                CurrentSemestar = 4,
                EducationLevel = "Undergraduate"
            };

            context.Student.AddRange(student1, student2);
            context.SaveChanges();

            // Seed Courses
            var course1 = new Course
            {
                Title = "Algorithms",
                Credits = 6,
                Semester = 5,
                Programme = "Computer Science",
                EducationLevel = "Undergraduate",
                FirstTeacherId = teacher1.Id,
                SecondTeacherId = teacher2.Id
            };

            var course2 = new Course
            {
                Title = "Databases",
                Credits = 6,
                Semester = 4,
                Programme = "Computer Science",
                EducationLevel = "Undergraduate",
                FirstTeacherId = teacher2.Id,
                SecondTeacherId = teacher1.Id
            };

            context.Course.AddRange(course1, course2);
            context.SaveChanges();

            // Seed Enrollments
            var enrollment1 = new Enrollment
            {
                CourseId = course1.Id,
                StudentId = student1.Id,
                Semester = "Summer",
                Year = 2023,
                Grade = 10,
                ExamPoints = 85,
                SeminalPoints = 10,
                ProjectPoints = 5,
                FinishDate = new DateTime(2023, 6, 20)
            };

            var enrollment2 = new Enrollment
            {
                CourseId = course2.Id,
                StudentId = student2.Id,
                Semester = "Winter",
                Year = 2023,
                Grade = 9,
                ExamPoints = 80,
                SeminalPoints = 10,
                ProjectPoints = 5,
                FinishDate = new DateTime(2023, 1, 15)
            };

            context.Enrollment.AddRange(enrollment1, enrollment2);
            context.SaveChanges();
        }
    }
}
