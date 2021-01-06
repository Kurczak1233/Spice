using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Coupon Coupons{ get; set; }
        public CouponController(ApplicationDbContext db)
        {
            _db = db;
            Coupons = new Coupon();

        }
        //GET INDEX
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }

        //GET CREATE
        public IActionResult Create()//Nic nie musimy posyłać do create!
        {
            return View(Coupons);
        }
        //POST CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            if(ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files; //Przechwytujemy zdjęcie
                if (files.Count() > 0)
                { //METODA ZAPISYWANIA ZDJĘCIA NA BAZIE DANYCH!
                    //File was uploaded so:
                    byte[] p1 = null;
                    using(var filestream1 = files[0].OpenReadStream())
                    {
                        using(var ms1 = new MemoryStream())
                        {
                            filestream1.CopyTo(ms1);
                            p1 = ms1.ToArray();

                        }
                    }
                    Coupons.Picture = p1;
                }
                _db.Coupon.Add(Coupons);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(Coupons);
        }
        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            //Wczutujemy odpowiedni obiekt
            Coupons = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if(Coupons == null)
            {
                return NotFound();
            }
            return View(Coupons);
        }
        //POST EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                return View(Coupons);
            }
            var couponinDb = await _db.Coupon.FindAsync(Coupons.Id); //FindAsync pobiera wpis po podanym ID
            var files = HttpContext.Request.Form.Files; //Pobieram zdjęcie
            if (files.Count() > 0)
            {
                //METODA ZAPISYWANIA ZDJĘCIA NA BAZIE DANYCH!
                //File was uploaded so:
                Coupons.Picture = null; //Usuwam zdjęcie

                byte[] p1 = null;
                using (var filestream1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        filestream1.CopyTo(ms1);
                        p1 = ms1.ToArray();

                    }
                }
                Coupons.Picture = p1;
                couponinDb.Picture = Coupons.Picture;
            }
                couponinDb.Name = Coupons.Name;  //Nadpisywanie pozostałych ustawień.
                couponinDb.IsActive = Coupons.IsActive;
                couponinDb.MinimumAmount = Coupons.MinimumAmount;
                couponinDb.CouponType = Coupons.CouponType;
                couponinDb.Discount = Coupons.Discount;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
        }
        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            Coupons = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            return View(Coupons);
        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? id) //Pamiętaj że pracujemy cały czas na Coupon (DataBinding)
        {
            if (id == null)
            {
                return NotFound();
            }
            Coupons = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            return View(Coupons);
        }
        //POST DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            Coupons = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if(Coupons == null)
            {
                return NotFound();
            }
            _db.Coupon.Remove(Coupons);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
