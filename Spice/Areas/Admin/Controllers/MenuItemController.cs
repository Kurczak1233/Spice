using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment; //Dla Zdjęcia
        
        [BindProperty]  //Dzięki Bind Property, możemy łatwiej mieć dostęp do modelu. 
        //Inicjujemy go w konstruktorze, dodajemy prop (poniżej) i modyfikujemy GET CREATE
        public MenuItemViewModel MenuItemVM { get; set; }
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostEnvironment;
            MenuItemVM = new MenuItemViewModel
            {
                CategoryList = _db.Category,
                //SubCategoryList = _db.SubCategory,
                MenuItem = new Models.MenuItem() //A subcategory?
            }; 
        }

        //GET INDEX
        public async Task<IActionResult> Index()
        {
            var MenuItems = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync(); //Pobieranie wszystkich MenuItems z db i przesyłanie do widoku!
            //Dodatkowo, musiby pobrać ForeignKey'e z Category i SubCategory
            return View(MenuItems);
        }
        //GET CREATE
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }
        //POST CREATE
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST(/*MenuItemViewModel MenuItemVM*/)  //MenuItemVM jest załadowany przez BIND PROPERTY
        {
            //Mamy wszystko prócz listy podkategorii więc pobieramy je następującą komendą:
            //SubCategoryID to foreing key odnoszący się do obiektu innej klasy.
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString()); //Sub... - name z widoku
            if(!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem); //Dodajemy tylko obiekty klasy MENUITEM nie cały VM bo tam są listy też.
            await _db.SaveChangesAsync();

            //Work on the img saving system section (in DB)
            string webRootPath = _hostingEnvironment.WebRootPath;
            //Wyciągamy wszystkie uploadowane pliki
            var files = HttpContext.Request.Form.Files; //Tutaj znajduje się nasz wrzucony plik ze strony (lub pusto)
            //Wyciągamy nasz MenuItem z DB po id
            //FindAsync wyszukuje wpis po określonych charakterystykach (KeyWalues to może być ID)
            //Zwraca MenuItem z db a pracujemy na modelu dlatego używamy MenuItemVM.MenuItem.Id
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //var Files has been found
                var uploads = Path.Combine(webRootPath, "images"); //Tworzymy ścieżkę do naszego folderu zdjęcia!
                var extention = Path.GetExtension(files[0].FileName); //Tylko jeden obrazek więc [0]
                //Tworzymy ścieżkę: Wykorzystujemy utworzoną już do images, nazwę obrazu (jako ID) i rozszerzenie.
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extention), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                //Aktualizujemy bazę danych
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extention;
            }
            else
            {
                //No file was found, so use default 
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                //Kopiujemy zdjęcie z naszego folderu do tej ścieżki:
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                //Aktualizujemy bazę danych
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //GET EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            //Wczutujemy odpowiedni obiekt
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m=>m.Id == id);
            MenuItemVM.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if(MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }
        //POST EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id /*MenuItemViewModel MenuItemVM*/)  //MenuItemVM jest załadowany przez BIND PROPERTY
        {
            //Mamy wszystko prócz listy podkategorii więc pobieramy je następującą komendą:
            //SubCategoryID to foreing key odnoszący się do obiektu innej klasy.
            if(id==null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString()); //Sub... - name z widoku
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            //Work on the img saving system section (in DB)
            string webRootPath = _hostingEnvironment.WebRootPath;
            //Wyciągamy wszystkie uploadowane pliki
            var files = HttpContext.Request.Form.Files; //Tutaj znajduje się nasz wrzucony plik ze strony (lub pusto)
            //Wyciągamy nasz MenuItem z DB po id
            //FindAsync wyszukuje wpis po określonych charakterystykach (KeyWalues to może być ID)
            //Zwraca MenuItem z db a pracujemy na modelu dlatego używamy MenuItemVM.MenuItem.Id
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //NEW IMAGE HAS BEEN UPLOADED!
                var uploads = Path.Combine(webRootPath, "images"); //Tworzymy ścieżkę do naszego folderu zdjęcia!
                var extention_new = Path.GetExtension(files[0].FileName); //Tylko jeden obrazek więc [0]
                //Tworzymy ścieżkę: Wykorzystujemy utworzoną już do images, nazwę obrazu (jako ID) i rozszerzenie.

                //TWORZENIE ŚCIEŻKI DO ORGINALENGO PLIKU ORGINALNEGO PLIKU!
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                //POTWIERDZANIE USUNIĘCIA
                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                //
                //Upload nowego zdjęcia
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extention_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                //Aktualizujemy bazę danych
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extention_new;
            }
            //IF IMG NOT CHANGED WE DO NOT HAVE TO DO ANYTHING
            //
            //UPDATING ALL PROPS IN DB
            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //GET DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            //Wczutujemy odpowiedni obiekt
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }
        //GET DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            //Wczutujemy odpowiedni obiekt
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            //Wczutujemy odpowiedni obiekt
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            //MenuItemVM.SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            _db.MenuItem.Remove(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
