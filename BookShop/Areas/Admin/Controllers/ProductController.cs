using BookShop.DataAccess.Repository;
using BookShop.DataAccess.Repository.IRepository;
using BookShop.Models;
using BookShop.Models.ViewModels;
using BookShop.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;

namespace BookShop.Areas.Admin.Controllers
{
	[Area("Admin")]
    //[Authorize(Roles = StaticDetails.Role_Admin)]
    public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}
		public IActionResult Index()
		{
			List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return View(products);
		}

		public IActionResult Upsert(int? id) // Combination of update and insert
		{
			IEnumerable<SelectListItem> categoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem 
			{
				Text = c.Name,
				Value = c.Id.ToString(),
			});

			ProductVM productVM = new ProductVM
			{
				Product = new Product(),
				CategoryList = categoryList
			};
			if(id == null || id == 0)
			{
				// Create
				return View(productVM);
			}
			else
			{
				// Update
				productVM.Product = _unitOfWork.Product.Get(p => p.Id == id);
				return View(productVM);
			}
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? uploadedFile) // file is for image
		{
			// note: The name of the parameter and the name attribute of the input that takes that parameter should be the same,
			// So the name attribute of the input that takes the image file in upsert.cshtml should also have the value "uploadedFile".
			if (ModelState.IsValid)
			{
				// Saving the uploaded image in images/product in the wwwroot folder
				string wwwRootPath = _webHostEnvironment.WebRootPath; // getting the path for wwwroot folder
				if(uploadedFile != null)
				{					
					if (!String.IsNullOrEmpty(productVM.Product.ImageUrl)) // The case where the image of an existing product is being updated
					{
						/* note: when updating the image of an exsting product, for the ImageUrl property not to be null there has to be a hidden input
						* that takes the ImageUrl of the product in the upsert.cshtml
						*/

						// delete the old image
						var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

						if(System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName); // generating a unique file name
					string productPath = Path.Combine(wwwRootPath, @"images\product");

					using(var filestream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						uploadedFile.CopyTo(filestream);
					}

					productVM.Product.ImageUrl = @"\images\product\" + fileName;
				}

				if(productVM.Product.Id == 0) // adding a new product
				{
					_unitOfWork.Product.Add(productVM.Product);
					TempData["success"] = "Product created succesfully";
				}
				else // updating a product
				{
					_unitOfWork.Product.Update(productVM.Product);
					TempData["success"] = "Product updated succesfully";
				}

				_unitOfWork.Save();
				return RedirectToAction("Index");
			}		
			productVM.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem
			{
				Text = c.Name,
				Value = c.Id.ToString(),
			});
			return View(productVM);
		}		

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = products });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			Product product = _unitOfWork.Product.Get(p => p.Id == id);
			if(product == null)
			{
				return Json(new {success = false, message = "Error while deleting"});
			}
			var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

			if (System.IO.File.Exists(oldImagePath))
			{
				System.IO.File.Delete(oldImagePath);
			}

			_unitOfWork.Product.Remove(product);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Delete Successful" });

		}
		#endregion
	}
}
