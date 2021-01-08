using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel() //Bez model bindingu
            {
                MenuItemList = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                CategoryList = await _db.Category.ToArrayAsync(),
                CouponList = await _db.Coupon.Where(c=>c.IsActive == true).ToListAsync() //Tylko aktywne

            };
            return View(IndexVM);
        }

        public  IActionResult AddDishPropsition()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComputeViewModel data) //Dane z widoku, mogły być inaczej zapisane, jednak dla przykładu zostały wprowadzone inną klasą.
        { 
            int value = Int32.Parse(data.Value);
            if (ModelState.IsValid)
            {
                var dishbuilder = new DishBuilder();
                var dishbuilderdirector = new DishBuilderDirector(dishbuilder);
                dishbuilderdirector.BuildDish(data.Name,data.Desc,value);
                Dish dish = dishbuilderdirector.GetDishFromBuilder();
                _db.Dish.Add(dish);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
