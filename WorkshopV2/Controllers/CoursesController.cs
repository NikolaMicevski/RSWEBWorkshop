using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopV2.Data;
using WorkshopV2.Models;
using WorkshopV2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace WorkshopV2.Controllers
{
    public class CoursesController : Controller
    {
        private readonly WorkshopV2Context _context;
        private readonly UserManager<WorkshopV2User> _userManager;

        public CoursesController(UserManager<WorkshopV2User> userManager, WorkshopV2Context context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string Title, int? Semester, string Programme)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUser = await _userManager.FindByIdAsync(userId);

            IQueryable<Course> courses = _context.Course.AsQueryable();

            if (User.IsInRole("Teacher"))
            {
                if (currentUser != null && currentUser.TeacherId.HasValue)
                {
                    int teacherId = currentUser.TeacherId.Value;
                    courses = courses.Where(c => c.FirstTeacherId == teacherId || c.SecondTeacherId == teacherId);
                }
                else
                {
                    courses = courses.Where(c => false);
                }
            }
            else if (User.IsInRole("Student"))
            {
                if (currentUser != null && currentUser.StudentId.HasValue)
                {
                    long studentId = currentUser.StudentId.Value;

                    courses = _context.Enrollment
                                .Where(e => e.StudentId == studentId)
                                .Select(e => e.Course)
                                .Where(c => c != null)
                                .AsQueryable();
                }
                else
                {
                    courses = courses.Where(c => false);
                }
            }
            if (!string.IsNullOrEmpty(Title))
            {
                courses = courses.Where(c => c.Title == Title);
            }

            if (Semester.HasValue)
            {
                courses = courses.Where(c => c.Semester == Semester);
            }

            if (!string.IsNullOrEmpty(Programme))
            {
                courses = courses.Where(c => c.Programme != null && c.Programme.Contains(Programme));
            }

            ViewBag.Titles = await _context.Course
                .Where(c => c.Title != null)
                .Select(c => c.Title)
                .Distinct()
                .ToListAsync();

            ViewBag.Semesters = await _context.Course
                .Select(c => c.Semester)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.Programmes = await _context.Course
                .Where(c => c.Programme != null)
                .Select(c => c.Programme)
                .Distinct()
                .ToListAsync();

            return View(await courses.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id, int? year)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            long? currentStudentId = currentUser?.StudentId;

            var availableYears = course.Enrollment
                .Where(e => e.Year.HasValue)
                .Select(e => e.Year.Value)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            if (year.HasValue)
            {
                course.Enrollment = course.Enrollment.Where(e => e.Year == year.Value).ToList();
            }
            else
            {
                var currentYear = DateTime.Now.Year;
                course.Enrollment = course.Enrollment.Where(e => e.Year == currentYear).ToList();
                year = currentYear;
            }

            if (User.IsInRole("Student") && currentStudentId.HasValue)
            {
                course.Enrollment = course.Enrollment
                    .Where(e => e.Student != null && e.Student.Id == currentStudentId.Value)
                    .ToList();
            }
            else if (User.IsInRole("Teacher") || User.IsInRole("Admin"))
            {
            }
            else
            {
                course.Enrollment = new List<Enrollment>();
            }

            ViewBag.AvailableYears = availableYears;
            ViewBag.Year = year;
            ViewBag.CurrentStudentId = currentStudentId;

            return View(course);
        }

        // GET: Courses/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var teachers = await _context.Teacher
                .Select(t => new { t.Id, FullName = t.FirstName + " " + t.LastName })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");

            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var teachers = await _context.Teacher
                .Select(t => new { t.Id, FullName = t.FirstName + " " + t.LastName })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", course.FirstTeacherId);

            return View(course);
        }

        // GET: Courses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(course);
        }

        // GET: Courses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .FirstOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Course.FindAsync(id);
            if (course != null)
            {
                _context.Course.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Course.Any(e => e.Id == id);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageStudents(int id)
        {
            var course = await _context.Course
                .Include(c => c.Enrollment)
                    .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return NotFound();

            var students = await _context.Student.ToListAsync();

            var model = students.Select(s =>
            {
                var enrollment = course.Enrollment.FirstOrDefault(e => e.StudentId == s.Id);
                return new StudentEnrollmentViewModel
                {
                    StudentId = s.Id,
                    FullName = $"{s.FirstName} {s.LastName}",
                    IsEnrolled = enrollment != null,
                    IsFinished = enrollment?.FinishDate != null
                };
            }).ToList();

            ViewBag.CourseId = id;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEnrollment(long studentId, int courseId, string action)
        {
            var enrollment = await _context.Enrollment
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            switch (action.ToLower())
            {
                case "enroll":
                    if (enrollment == null)
                    {
                        _context.Enrollment.Add(new Enrollment
                        {
                            StudentId = studentId,
                            CourseId = courseId,
                            Semester = DateTime.Now.Month <= 6 ? "Summer" : "Winter",
                            Year = DateTime.Now.Year,
                            FinishDate = null
                        });
                    }
                    else if (enrollment.FinishDate != null)
                    {
                        enrollment.FinishDate = null;
                    }
                    break;

                case "finish":
                    if (enrollment != null && enrollment.FinishDate == null)
                    {
                        enrollment.FinishDate = DateTime.Now;
                    }
                    break;

                case "disenroll":
                    if (enrollment != null)
                    {
                        _context.Enrollment.Remove(enrollment);
                    }
                    break;

                case "remove-finished":
                    if (enrollment != null && enrollment.FinishDate != null)
                    {
                        _context.Enrollment.Remove(enrollment);
                    }
                    break;

                default:
                    return BadRequest();
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateGrades(int CourseId, int Year, List<Enrollment> Enrollments)
        {
            foreach (var updated in Enrollments)
            {
                var enrollment = await _context.Enrollment.FindAsync(updated.Id);

                if (enrollment == null)
                    continue;

                if (!enrollment.FinishDate.HasValue || enrollment.FinishDate > DateTime.Now)
                {
                    enrollment.ExamPoints = updated.ExamPoints;
                    enrollment.SeminalPoints = updated.SeminalPoints;
                    enrollment.ProjectPoints = updated.ProjectPoints;
                    enrollment.AdditionalPoints = updated.AdditionalPoints;
                    enrollment.Grade = updated.Grade;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = CourseId, year = Year });
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadStudentFile(int courseId)
        {
            var studentId = User.Identity.Name;

            var enrollment = await _context.Enrollment
                .Include(e => e.Course)
                .Include(e => e.Student)
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.Student.Id.ToString() == studentId);

            if (enrollment == null) return NotFound();

            return View(enrollment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> UploadStudentFile(int enrollmentId, IFormFile file, string projectUrl)
        {
            var enrollment = await _context.Enrollment.FindAsync((long)enrollmentId);

            if (enrollment == null) return BadRequest();

            if (file != null && file.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                var fileName = Path.GetFileName(file.FileName);
                var fullPath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                enrollment.SeminalUrl = "/uploads/" + fileName;
            }

            if (!string.IsNullOrWhiteSpace(projectUrl))
            {
                enrollment.ProjectUrl = projectUrl;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = enrollment.CourseId });
        }
    }
}
