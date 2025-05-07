using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopV2.Data;
using WorkshopV2.Models;

namespace WorkshopV2.Controllers
{
    public class CoursesController : Controller
    {
        private readonly WorkshopV2Context _context;

        public CoursesController(WorkshopV2Context context)
        {
            _context = context;
        }

        // GET: Courses
        public async Task<IActionResult> Index(string Title, int? Semester, string Programme)
        {
            var courses = _context.Course.AsQueryable();

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
                courses = courses.Where(c => c.Programme.Contains(Programme));
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


        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
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

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(course);
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Course
                .Include(c => c.Enrollment)
                .ThenInclude(e => e.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            var allStudents = await _context.Student.ToListAsync();
            var enrolledStudentIds = course.Enrollment?.Select(e => e.StudentId).ToList() ?? new List<long>();

            var availableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();
            var enrolledStudents = allStudents.Where(s => enrolledStudentIds.Contains(s.Id)).ToList();

            ViewData["AllStudents"] = new MultiSelectList(availableStudents, "Id", "FirstName");
            ViewData["EnrolledStudents"] = new MultiSelectList(enrolledStudents, "Id", "FirstName");

            return View(course);
        }


        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Title,Credits,Semester,Programme,EducationLevel,FirstTeacherId,SecondTeacherId")] Course course,
            long[] SelectedStudentIds,
            long[] RemoveStudentIds)
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

                    foreach (var studentId in SelectedStudentIds)
                    {
                        bool alreadyEnrolled = _context.Enrollment.Any(e => e.CourseId == id && e.StudentId == studentId);
                        if (!alreadyEnrolled)
                        {
                            var enrollment = new Enrollment
                            {
                                CourseId = id,
                                StudentId = studentId
                            };
                            _context.Enrollment.Add(enrollment);
                        }
                    }

                    foreach (var studentId in RemoveStudentIds)
                    {
                        var enrollment = await _context.Enrollment
                            .FirstOrDefaultAsync(e => e.CourseId == id && e.StudentId == studentId);
                        if (enrollment != null)
                        {
                            _context.Enrollment.Remove(enrollment);
                        }
                    }

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

            var allStudents = await _context.Student.ToListAsync();
            var enrolledStudentIds = _context.Enrollment.Where(e => e.CourseId == id).Select(e => e.StudentId).ToList();

            var availableStudents = allStudents.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();
            var enrolledStudents = allStudents.Where(s => enrolledStudentIds.Contains(s.Id)).ToList();

            ViewData["AllStudents"] = new MultiSelectList(availableStudents, "Id", "FirstName", SelectedStudentIds);
            ViewData["EnrolledStudents"] = new MultiSelectList(enrolledStudents, "Id", "FirstName", RemoveStudentIds);

            return View(course);
        }



        // GET: Courses/Delete/5
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
    }
}
