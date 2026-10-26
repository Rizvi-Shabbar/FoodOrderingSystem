using FoodOrderingSystem.Models;
using FoodOrderingSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext context;

        public AccountController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password, // In production, hash this!
                    FullName = model.FullName,
                    Address = model.Address,
                    Phone = model.Phone
                };

                context.Users.Add(user);
                context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            var user = context.Users.FirstOrDefault(u =>
                u.Username == model.Username && u.Password == model.Password);

            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid username or password");
            return View(model);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = context.Users.Find(userId);
            if (user == null) return NotFound();
            var model = new ProfileViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Address = user.Address,
                Phone = user.Phone
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Profile(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var user = context.Users.Find(userId);
                if (user == null) return NotFound();

                // Check if email is already taken by another user
                var emailExists = context.Users.Any(u => u.Email == model.Email && u.Id != userId);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email is already registered to another account.");
                    return View(model);
                }

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Phone = model.Phone;
                user.Address = model.Address;

                context.SaveChanges();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult DeleteAccount()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = context.Users.Find(userId);
            if (user == null) return NotFound();

            // Optional: Check if user has pending orders
            var pendingOrders = context.Orders.Any(o => o.UserId == userId &&
                (o.Status == "Pending" || o.Status == "Confirmed" || o.Status == "Preparing"));

            if (pendingOrders)
            {
                TempData["Error"] = "Cannot delete account while you have active orders!";
                return RedirectToAction("Profile");
            }

            context.Users.Remove(user);
            context.SaveChanges();

            HttpContext.Session.Clear();
            TempData["Success"] = "Your account has been deleted.";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                var user = context.Users.Find(userId);
                if (user.Password != model.CurrentPassword) // In production, verify hash!
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                user.Password = model.NewPassword; // In production, hash the password!
                context.SaveChanges();

                TempData["Success"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }
    }
}
