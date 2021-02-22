using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        [TempData]
        public string StatusMessage { get; set; } 
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET INDEX
        public async Task<IActionResult> Index()
        {
            var subcategory = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subcategory);
        }

        //GET CREATE
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Subcategory(),
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),

                //Order = sortujemy
                //Select = Wybieramy z obiektu tylko nazwę
                //Distinct = Wyrózniamy nazwy
            };

            return View(model);
        }

        //POST CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Czy nasza kategoria istnieje w subkategorii?
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if(doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub category exists under" + doesSubCategoryExists.First().Category.Name  + " category. Please use another name.";
                }
                else
                {
                    _db.SubCategory.Add(model.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            //Zwracamy właściwości modelu, ponieważ zostaną utracone jeśli tego nie zrobimy!
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            }; 
            return View(modelVM);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<Subcategory> subCategories = new List<Subcategory>();
            subCategories = await (from subCategory in _db.SubCategory
                             where subCategory.CategoryId == id
                             select subCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }


        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m=>m.Id == id);

            if(subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),

                //Order = sortujemy
                //Select = Wybieramy z obiektu tylko nazwę
                //Distinct = Wyrózniamy nazwy
            };

            return View(model);
        }

        //POST EDIT
        [HttpPost] //REST.API
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Czy nasza subkategoria istnieje w db?
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Count() > 0)
                {
                    //Error
                    StatusMessage = "Error : Sub category exists under" + doesSubCategoryExists.First().Category.Name + " category. Please use another name.";
                }
                else
                {

                    //AKTUALIZUJEMY BAZĘ DANYCH
                    var subCatFromDb = await _db.SubCategory.FindAsync(id);
                    subCatFromDb.Name = model.SubCategory.Name; //Zaktualizowanie TYLKO JEDNEJ WŁAŚCIWIOŚCI!
                    //Można też po prostu drugą metodą aktualizującą WSZYSTKIE właściwości:                      

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            //Zwracamy właściwości modelu, ponieważ zostaną utracone jeśli tego nie zrobimy!
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };
            modelVM.SubCategory.Id = id;
            return View(modelVM);
        }
        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),

                //Order = sortujemy
                //Select = Wybieramy z obiektu tylko nazwę
                //Distinct = Wyrózniamy nazwy
            };

            return View(model);

        }

        //GET DELETE
        public async Task<IActionResult> Delete(int? id)
        {
                if (id == null)
                {
                    return NotFound();
                }
                var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);

                if (subCategory == null)
                {
                    return NotFound();
                }
                SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
                {
                    CategoryList = await _db.Category.ToListAsync(),
                    SubCategory = subCategory,
                    SubCategoryList = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync(),

                    //Order = sortujemy
                    //Select = Wybieramy z obiektu tylko nazwę
                    //Distinct = Wyrózniamy nazwy
                };

                return View(model);

        }

        //POST DELETE
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteSubCategory(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var subcat = _db.SubCategory.FirstOrDefault(m => m.Id == id);
            if(subcat == null)
            {
                return NotFound();
            }
            _db.SubCategory.Remove(subcat);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}