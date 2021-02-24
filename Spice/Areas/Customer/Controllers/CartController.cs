using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class CartController : Controller
    {
        // GET: CartControler
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }
        public CartController(ApplicationDbContext db)
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

            if(HttpContext.Session.GetString(SD.ssCouponCode)!=null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }


            return View(detailCart);
        }
        
        public IActionResult AddCoupon()
        {
            if(detailCart.OrderHeader.CouponCode==null)
            {
                detailCart.OrderHeader.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, detailCart.OrderHeader.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if(cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();

                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count; //New Count
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();

            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count; //New Count
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Summary()
        {
            detailCart = new OrderDetailsCart() //Create detailCart
            {
                OrderHeader = new Models.OrderHeader() //We will populate this prop (list later)
            };

            detailCart.OrderHeader.OrderTotal = 0; // Order total = 0

            var claimsIdenity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdenity.FindFirst(ClaimTypes.NameIdentifier); //Get Identity of user
            ApplicationUser applicationUser = await _db.ApplicationUser.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();

            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value); //Find user Cart
            if (cart != null) //Przypisanie do listy w viewmodel naszej listy menuitmeów z osoby zidentifikowanej.
            {
                detailCart.listCart = cart.ToList();
            }
            foreach (var list in detailCart.listCart) //Liczenie ceny dla każdego z produktów
            {
                list.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == list.MenuItemId);
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
            }

            detailCart.OrderHeader.OrderTotalOriginal = detailCart.OrderHeader.OrderTotal; // BO nie ma kodu -15% (obie wartości te same)
            detailCart.OrderHeader.PickUpName = applicationUser.Name;
            detailCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detailCart.OrderHeader.PickUpTime = DateTime.Now;
            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }


            return View(detailCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost()
        {

            var claimsIdenity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdenity.FindFirst(ClaimTypes.NameIdentifier);
            detailCart.listCart = await _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToListAsync();

            detailCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending; // Order total = 0 
            detailCart.OrderHeader.OrderDate = DateTime.Now; // Order total = 0 
            detailCart.OrderHeader.UserId = claim.Value; // Order total = 0 
            detailCart.OrderHeader.Status = SD.PaymentStatusPending; // Order total = 0 
            detailCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailCart.OrderHeader.PickUpTime.ToShortTimeString()); // Order total = 0 

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailCart.OrderHeader.OrderTotalOriginal = 0;
            
            foreach (var item in detailCart.listCart) //Liczenie ceny dla każdego z produktów
            {
                item.MenuItem = await _db.MenuItem.FirstOrDefaultAsync(m => m.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemId = item.MenuItem.Id,
                    OrderId = detailCart.OrderHeader.Id,
                    Description = item.MenuItem.Description,
                    Name = item.MenuItem.Name,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                detailCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails); //Dodanie dopiero po SaveChanges!!!!
                

            }

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null) //If coupon has been used
            {
                detailCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupon.Where(c => c.Name.ToLower() == detailCart.OrderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detailCart.OrderHeader.OrderTotal = SD.DiscountedPrice(couponFromDb, detailCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotalOriginal; //No coupon used.
            }

            detailCart.OrderHeader.CouponCodeDiscount = detailCart.OrderHeader.OrderTotalOriginal - detailCart.OrderHeader.OrderTotal;
            //Remove session attributes
            //Remove items from shopping cart
            _db.ShoppingCart.RemoveRange(detailCart.listCart);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);

            await _db.SaveChangesAsync();

            //return RedirectToAction("Index", "Home");
            return RedirectToAction("Confirm", "Order", new { id = detailCart.OrderHeader.Id });
        }

    }
}
