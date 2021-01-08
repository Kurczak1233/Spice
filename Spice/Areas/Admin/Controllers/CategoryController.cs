using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        } /*DEPENDECY INJECTION (Bo zadeklarowaliśmy na począktu w starcie i teraz to wstrzykujemy*/
        //GET - INDEX
        public async Task<IActionResult> Index()
        {
            return View(await _db.Category.ToListAsync());
        }
        //GET - CREATE
        public IActionResult Create()
        {
            return View();
        }
        //POST - CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if(ModelState.IsValid)
            {
                //If valid
                _db.Category.Add(category);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        { 
            if(id==null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);  //Znajduje kategorię w bazie danych po id
            if(category == null)
            {
                return NotFound();
            }
            return View(category); //ZWRACAMY KATEGORIĘ !!! (@MODEL przekazany z Table BUTTON jako route! (ID))
        }
        //POST - EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category) //Niejawne przekazanie obiektu jak jest ID?
        {
            if(ModelState.IsValid)
            {
                _db.Update(category); //Dla jednego prop jest to w porządku ale dla więcej NIE!
                await _db.SaveChangesAsync(); 
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        //GET - DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);  //Znajduje kategorię w bazie danych po id
            if (category == null)
            {
                return NotFound();
            }
            return View(category); //ZWRACAMY kategorię! !!! (@MODEL TO ID!)
        }
        //POST - DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id) //Jawne przekazanie obiektu ID z asp-route-id przy delete
        {
            var category = await _db.Category.FindAsync(id);
            if(category == null)
            {
                return View();
            }
            _db.Category.Remove(category);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); 

        }
        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);  //Znajduje kategorię w bazie danych po id
            if (category == null)
            {
                return NotFound();
            }
            return View(category); //ZWRACAMY KAtegorię! !!! (@MODEL TO ID!)
        }
    }
}
