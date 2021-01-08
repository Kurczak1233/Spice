using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class Dish
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Dish name")]
        public string Name { get; set; }
        [Display(Name = "Dish description")]
        public string Description { get; set; }
        [Display(Name = "Dish price")]
        public int Price { get; set; }
    }

    public interface IDishBuilder
    {
        void BuildName(string name);
        void BuildDescription(string description);
        void BuildPrice(int price);
        Dish GetDish();
    }

    public class DishBuilder : IDishBuilder
    {
        Dish _dish = new Dish();
        public void BuildName(string name)
        {
            _dish.Name = name;
        }

        public void BuildDescription(string description)
        {
            _dish.Description = description;
        }

        public void BuildPrice(int price)
        {
            _dish.Price = price;
        }
        public Dish GetDish()
        {
            Dish danie = _dish;
            Clear();
            return danie;
        }
        private void Clear() => _dish = new Dish();
    }
    public class DishBuilderDirector
    {
        private readonly IDishBuilder _dishBuilder;
        public DishBuilderDirector(IDishBuilder dishbuilder)
        {
            _dishBuilder = dishbuilder;
        }
        public void BuildDish(string name, string description, int price)
        {
            _dishBuilder.BuildName(name);
            _dishBuilder.BuildDescription(description);
            _dishBuilder.BuildPrice(price);
        }
        public Dish GetDishFromBuilder()
        {
            return _dishBuilder.GetDish();
        }
    }
}
