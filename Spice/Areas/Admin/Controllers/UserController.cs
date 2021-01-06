using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier); // Sprawdzanie zalogowanego usera
            //Id usera zalogowango będzie w środku claima jako wartość
            //Chcemy więc pokazać listę wszystkich prócz zalogowanego co przekażemy warunkiem Where.
            return View(await _db.ApplicationUser.Where(m=>m.Id != claim.Value).ToListAsync());
        }
        public async Task<IActionResult> Lock(string id) //Bo @Item.Id to GUID (string) a nie int! <Z bazy danych>
        {
            if(id == null)
            {
                return NotFound();
            }
            var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);
            //Pobieramy usera z db który jest zgodny z otrzymanym ID

            if(applicationUser == null)
            {
                return NotFound();
            }
           
            applicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Unlock(string id) //Bo @Item.Id to GUID (string) a nie int! <Z bazy danych>
        {
            if (id == null)
            {
                return NotFound();
            }
            var applicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == id);
            //Pobieramy usera z db który jest zgodny z otrzymanym ID

            if (applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.LockoutEnd = DateTime.Now;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
