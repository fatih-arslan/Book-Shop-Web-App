using BookShop.DataAccess.Repository.IRepository;
using BookShop.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookShop.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(StaticDetails.SessionCart) == null)
                {
                    var userId = claim.Value;
                    int itemCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
                    HttpContext.Session.SetInt32(StaticDetails.SessionCart, itemCount);
                }                
                return View(HttpContext.Session.GetInt32(StaticDetails.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
