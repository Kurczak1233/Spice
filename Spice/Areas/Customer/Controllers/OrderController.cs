using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
   
    public class OrderController : Controller
    {
        private ApplicationDbContext _db;
        private int PageSize = 2;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            OrderDetailsViewmodel orderDetailsViewmodel = new OrderDetailsViewmodel()
            {
                OrderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()
            };
            return View(orderDetailsViewmodel);
        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            var claimsIdenity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdenity.FindFirst(ClaimTypes.NameIdentifier); //Sprawdzamy Id użytkownika.


            OrderListViewModel orderListViewModel = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewmodel>(),
            };


            List<OrderDetailsViewmodel> orderList = new List<OrderDetailsViewmodel>();

            List<OrderHeader> OrderHeaderlist = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in OrderHeaderlist)
            {
                OrderDetailsViewmodel individial = new OrderDetailsViewmodel()
                {
                    OrderHeader = item,
                    OrderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListViewModel.Orders.Add(individial);
            }

            var count = orderListViewModel.Orders.Count;
            orderListViewModel.Orders = orderListViewModel.Orders.OrderByDescending(p => p.OrderHeader.Id).Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

            orderListViewModel.PagingInfo = new PagingInfo()
            {
                CurrentPage = productPage,
                ItemsForPage = PageSize,
                TotalItem = count,
                urlParam = "/Customer/Order/OrderHistory?productPage=:"
            };

            return View(orderListViewModel);
        }

        public async Task<IActionResult> GetOrderDetails(int Id)
        {

            OrderDetailsViewmodel orderDetailsViewmodel = new OrderDetailsViewmodel()
            {
                OrderHeader = await _db.OrderHeader.FirstOrDefaultAsync(m => m.Id == Id),
                OrderDetails = await _db.OrderDetails.Where(m => m.OrderId == Id).ToListAsync()
            };
            orderDetailsViewmodel.OrderHeader.ApplicationUser = await _db.ApplicationUser.FirstOrDefaultAsync(m => m.Id == orderDetailsViewmodel.OrderHeader.UserId);
            return PartialView("_IndividualOrderDetails", orderDetailsViewmodel);
        }
    }
}
