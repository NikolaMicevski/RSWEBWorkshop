using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopV2.Data;
using WorkshopV2.Models;

namespace WorkshopV2.Controllers
{
    public class StudentsController : Controller
    {
        private readonly WorkshopV2Context _context;
        private readonly UserManager<WorkshopV2User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentsController(
            WorkshopV2Context context,
            UserManager<WorkshopV2User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Students
        public async Task<IActionResult> Index(string studentId, string firstName, string lastName)
        {
            var students = _context.Student.AsQueryable();

            if (!string.IsNullOrEmpty(studentId))
            {
                students = students.Where(s => s.StudentId.Contains(studentId));
            }

            if (!string.IsNullOrEmpty(firstName))
            {
                students = students.Where(s => s.FirstName.Contains(firstName)); 
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                students = students.Where(s => s.LastName.Contains(lastName)); 
            }

            ViewBag.StudentIds = await _context.Student
                .Where(s => s.StudentId != null)
                .Select(s => s.StudentId)
                .Distinct()
                .ToListAsync();

            ViewBag.FirstNames = await _context.Student
                .Where(s => s.FirstName != null)
                .Select(s => s.FirstName)
                .Distinct()
                .ToListAsync();

            ViewBag.LastNames = await _context.Student
                .Where(s => s.LastName != null)
                .Select(s => s.LastName)
                .Distinct()
                .ToListAsync();

            return View(await students.ToListAsync());
        }


        // GET: Students/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student,string email,IFormFile? image)  // <-- image parameter
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "students");

                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);

                    var filePath = Path.Combine(imagePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    student.ImageUrl = "/images/students/" + fileName;
                }

                _context.Add(student);
                await _context.SaveChangesAsync();

                var user = new WorkshopV2User
                {
                    UserName = email,
                    Email = email,
                    StudentId = student.Id
                };

                string defaultPassword = "Student123!";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(student);
        }

        // GET: Students/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,StudentId,FirstName,LastName,EnrollmentDate,AcquiredCredits,CurrentSemestar,EducationLevel")] Student student, IFormFile? image)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingStudent = await _context.Student.FindAsync(id);
                if (existingStudent == null)
                {
                    return NotFound();
                }

                existingStudent.StudentId = student.StudentId;
                existingStudent.FirstName = student.FirstName;
                existingStudent.LastName = student.LastName;
                existingStudent.EnrollmentDate = student.EnrollmentDate;
                existingStudent.AcquiredCredits = student.AcquiredCredits;
                existingStudent.CurrentSemestar = student.CurrentSemestar;
                existingStudent.EducationLevel = student.EducationLevel;

                if (image != null && image.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existingStudent.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingStudent.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(imagePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    existingStudent.ImageUrl = "/images/" + fileName;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            return View(student);
        }

        // GET: Students/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var student = await _context.Student.FindAsync(id);
            if (student != null)
            {
                if (!string.IsNullOrEmpty(student.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.StudentId == student.Id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Unable to delete associated user account.");
                        return View(student);
                    }
                }

                _context.Student.Remove(student);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(long id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
    }
}
