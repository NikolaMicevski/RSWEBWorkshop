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
    public class TeachersController : Controller
    {
        private readonly WorkshopV2Context _context;

        public TeachersController(WorkshopV2Context context)
        {
            _context = context;
        }

        // GET: Teachers
        public async Task<IActionResult> Index(string firstName, string lastName, string degree, string academicRank)
        {
            var teachers = _context.Teacher.AsQueryable();

            if (!string.IsNullOrEmpty(firstName))
            {
                teachers = teachers.Where(t => t.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                teachers = teachers.Where(t => t.LastName.Contains(lastName));
            }

            if (!string.IsNullOrEmpty(degree))
            {
                teachers = teachers.Where(t => t.Degree.Contains(degree));
            }

            if (!string.IsNullOrEmpty(academicRank))
            {
                teachers = teachers.Where(t => t.AcademicRank.Contains(academicRank));
            }

            ViewBag.FirstNames = await _context.Teacher
                .Where(t => t.FirstName != null)
                .Select(t => t.FirstName)
                .Distinct()
                .ToListAsync();

            ViewBag.LastNames = await _context.Teacher
                .Where(t => t.LastName != null)
                .Select(t => t.LastName)
                .Distinct()
                .ToListAsync();

            ViewBag.Degrees = await _context.Teacher
                .Where(t => t.Degree != null)
                .Select(t => t.Degree)
                .Distinct()
                .ToListAsync();

            ViewBag.AcademicRanks = await _context.Teacher
                .Where(t => t.AcademicRank != null)
                .Select(t => t.AcademicRank)
                .Distinct()
                .ToListAsync();

            return View(await teachers.ToListAsync());
        }


        // GET: Teachers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .Include(t => t.CoursesAsFirstTeacher)
                .Include(t => t.CoursesAsSecondTeacher)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }


        // GET: Teachers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(teacher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        // GET: Teachers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teachers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
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
            return View(teacher);
        }

        // GET: Teachers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teacher
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Teachers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher != null)
            {
                _context.Teacher.Remove(teacher);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
    }
}
