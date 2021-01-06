using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models.ViewModels
{
    public class MenuItemViewModel
    {
        public MenuItem MenuItem{ get; set; }
        public IEnumerable<Subcategory> SubCategoryList { get; set; }

        public IEnumerable<Category> CategoryList { get; set; }

    }
}
