using System.ComponentModel.DataAnnotations;
using FoodDeliveryApp.Constants;
using FoodDeliveryApp.Data;
using FoodDeliveryApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDeliveryApp.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminDriversController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminDriversController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var drivers = await _userManager.GetUsersInRoleAsync(AppRoles.Driver);

            var driverIds = drivers.Select(d => d.Id).ToList();

            var assignedOrderCounts = await _context.Orders
                .Where(o => o.DriverId != null && driverIds.Contains(o.DriverId))
                .GroupBy(o => o.DriverId!)
                .Select(g => new
                {
                    DriverId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.DriverId, x => x.Count);

            ViewBag.AssignedOrderCounts = assignedOrderCounts;

            return View(drivers.OrderBy(d => d.FullName).ThenBy(d => d.Email).ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateDriverInputModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDriverInputModel input)
        {
            if (!ModelState.IsValid)
            {
                return View(input);
            }

            var email = input.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(input.Email), "A user with this email already exists.");
                return View(input);
            }

            var driver = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = input.FullName.Trim(),
                Address = input.Address?.Trim()
            };

            var createResult = await _userManager.CreateAsync(driver, input.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(input);
            }

            var roleResult = await _userManager.AddToRoleAsync(driver, AppRoles.Driver);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(driver);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(input);
            }

            TempData["AdminDriverSuccess"] = $"Driver '{driver.FullName}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public class CreateDriverInputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [Display(Name = "Address")]
            public string? Address { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}