using FoodOrderingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext context;

        public MenuController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int ? categoryId)
        {
            var categories = context.Categories.ToList();
            ViewBag.Categories = categories;

            // Fetch items with optional category filtering
            var foodItems = context.FoodItems
                .Include(f => f.Category)
                .Where(f => f.IsAvailable && (categoryId == null || f.CategoryId == categoryId))
                .ToList();

            return View(foodItems);
        }
    }
}
