using Microsoft.AspNetCore.Authorization;
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
using System.Security.Claims;
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

        public IActionResult AddDishPropsition()
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

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            //Bazując na id musimy otrzymać menuItem z naszej bazy danych
            //Załaczamy do niej także categorię i podkategorię
            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();
            ShoppingCart cartObj = new ShoppingCart() //Tworzymy obiekt Cart
            {
                MenuItem = menuItemFromDb, //Przypisujemy mu z bazy danych dane naszego MenuItem
                MenuItemId = menuItemFromDb.Id //Przypisujemy cartowi ID z naszego obiektu menuItem
            };
            return View(cartObj);

        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId && c.MenuItemId == CartObject.MenuItemId).FirstOrDefaultAsync();
                if (cartFromDb == null)
                {
                    await _db.ShoppingCart.AddAsync(CartObject);
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + CartObject.Count; //Adding count if obj exists
                }
                await _db.SaveChangesAsync();

                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("ssCartCount", count); //Session!

                return RedirectToAction("Index");
            }
            else
            {
                var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == CartObject.MenuItemId).FirstOrDefaultAsync();
                ShoppingCart cartObj = new ShoppingCart() //Tworzymy obiekt Cart
                {
                    MenuItem = menuItemFromDb, //Przypisujemy mu z bazy danych dane naszego MenuItem
                    MenuItemId = menuItemFromDb.Id //Przypisujemy cartowi ID z naszego obiektu menuItem
                };
                return View(cartObj);
            }
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
