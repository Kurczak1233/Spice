using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Areas.Customer.Controllers
{
    public class CartControler : Controller
    {
        // GET: CartControler
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }
        public CartControler(ApplicationDbContext db)
        {
            _db = db; //Dependecy injection
        }
        public async Task<IActionResult> Index()
        {
            detailCart = new OrderDetailsCart() //Create detailCart
            {
                OrderHeader = new Models.OrderHeader() //We will populate this prop (list later)
            };

             detailCart.OrderHeader.OrderTotal = 0; // Order total = 0
             
            var claimsIdenity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdenity.FindFirst(ClaimTypes.NameIdentifier); //Get Identity of user


            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value); //Find user Cart
            if(cart != null) //Przypisanie do listy w viewmodel naszej listy menuitmeów z osoby zidentifikowanej.
            {
                detailCart.listCart = cart.ToList();
            }
            foreach(var list in detailCart.listCart) //Liczenie ceny dla każdego z produktów
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);
                if(list.MenuItem.Description.Length>100)
                {
                    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                }
            }

            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal; // BO nie ma kodu -15% (obie wartości te same)

            return View(detailCart);
        }      
    }
}
