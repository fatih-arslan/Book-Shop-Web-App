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
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			List<Company> companys = _unitOfWork.Company.GetAll().ToList();
			return View(companys);
		}

		public IActionResult Upsert(int? id) // Combination of update and insert
		{			
			if(id == null || id == 0)
			{
				// Create
				return View(new Company());
			}
			else
			{
				// Update
				Company company = _unitOfWork.Company.Get(p => p.Id == id);
				return View(company);
			}
		}

		[HttpPost]
		public IActionResult Upsert(Company company) // file is for image
		{
			// note: The name of the parameter and the name attribute of the input that takes that parameter should be the same,
			if (ModelState.IsValid)
			{
				if(company.Id == 0) // adding a new company
				{
					_unitOfWork.Company.Add(company);
					TempData["success"] = "Company created succesfully";
				}
				else // updating a company
				{
					_unitOfWork.Company.Update(company);
					TempData["success"] = "Company updated succesfully";
				}

				_unitOfWork.Save();
				return RedirectToAction("Index");
			}		
			return View(company);
		}		

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> companys = _unitOfWork.Company.GetAll().ToList();
			return Json(new { data = companys });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			Company company = _unitOfWork.Company.Get(p => p.Id == id);
			if(company == null)
			{
				return Json(new {success = false, message = "Error while deleting"});
			}			

			_unitOfWork.Company.Remove(company);
			_unitOfWork.Save();
			return Json(new { success = true, message = "Delete Successful" });

		}
		#endregion
	}
}
