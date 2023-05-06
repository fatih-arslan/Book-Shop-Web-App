using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BookShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var userId = claim.Value;
                int itemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, itemCount);
            }            
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(products);
        }

		public IActionResult Details(int productId)
		{
            ShoppingCart cart = new ShoppingCart 
            { 
                Product = _unitOfWork.Product.Get(p => p.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };
			return View(cart);
		}

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;    
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDatabase = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if(cartFromDatabase != null) // if shopping cart already exists
            {
                cartFromDatabase.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDatabase);
                _unitOfWork.Save();
            }
            else // add cart record
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                int itemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, itemCount);   
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}