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
    public class TeachersController : Controller
    {
        private readonly WorkshopV2Context _context;
        private readonly UserManager<WorkshopV2User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeachersController(
            WorkshopV2Context context,
            UserManager<WorkshopV2User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Teachers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher,string email,IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "teachers");
                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(imagePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    teacher.ImageUrl = "/images/teachers/" + fileName;
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();

                var user = new WorkshopV2User
                {
                    UserName = email,
                    Email = email,
                    TeacherId = teacher.Id
                };

                string defaultPassword = "Teacher123!";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(teacher);
        }

        // GET: Teachers/Edit/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,[Bind("Id,FirstName,LastName,Degree,AcademicRank,OfficeNumber,HireDate")] Teacher teacher,IFormFile? image)  
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingTeacher = await _context.Teacher.FindAsync(id);
                if (existingTeacher == null)
                    return NotFound();

                existingTeacher.FirstName = teacher.FirstName;
                existingTeacher.LastName = teacher.LastName;
                existingTeacher.Degree = teacher.Degree;
                existingTeacher.AcademicRank = teacher.AcademicRank;
                existingTeacher.OfficeNumber = teacher.OfficeNumber;
                existingTeacher.HireDate = teacher.HireDate;

                if (image != null && image.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existingTeacher.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingTeacher.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                            System.IO.File.Delete(oldImagePath);
                    }

                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "teachers");
                    if (!Directory.Exists(imagePath))
                        Directory.CreateDirectory(imagePath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(imagePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    existingTeacher.ImageUrl = "/images/teachers/" + fileName;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(teacher);
        }

        // GET: Teachers/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teacher.FindAsync(id);
            if (teacher != null)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TeacherId == teacher.Id);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Unable to delete associated user account.");
                        return View(teacher);
                    }
                }

                _context.Teacher.Remove(teacher);

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id)
        {
            return _context.Teacher.Any(e => e.Id == id);
        }
    }
}
