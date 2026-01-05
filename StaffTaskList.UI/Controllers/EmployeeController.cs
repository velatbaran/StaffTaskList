using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using StaffTaskList.Core.Entities;
using StaffTaskList.Service.IRepository;

namespace StaffTaskList.UI.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IRepository<Employee> _repoEmployee;
        private readonly IToastNotification _toastNotification;

        public EmployeeController(IRepository<Employee> repoEmployee, IToastNotification toastNotification)
        {
            _repoEmployee = repoEmployee;
            _toastNotification = toastNotification;
        }

        [Route("personeller")]
        public IActionResult Index()
        {
            return View(_repoEmployee.GetQueryable().OrderByDescending(c=>c.CreatedDate).ToList());
        }

        [Route("personelekle")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("personelekle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(Employee employee)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _repoEmployee.Add(employee);
                    await _repoEmployee.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Personel kayıt işlemi başarılı", new ToastrOptions { Title = "Kayıt" });
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message, new ToastrOptions { Title = "Kayıt" });
                }
            }
            _toastNotification.AddWarningToastMessage("Lütfen gerekli alanları doldurun", new ToastrOptions { Title = "Kayıt" });
            return View(employee);
        }

        [Route("personelguncelle/{id?}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _repoEmployee.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [Route("personelguncelle/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Employee employee, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var _employee = await _repoEmployee.GetByIdAsync(id);

                    _employee.NameSurname = employee.NameSurname;
                    _employee.Position = employee.Position;

                    _repoEmployee.Update(_employee);
                    await _repoEmployee.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Personel güncelleme işlemi başarılı", new ToastrOptions { Title = "Güncelleme" });
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message, new ToastrOptions { Title = "Hata" });
                }
            }
            _toastNotification.AddWarningToastMessage("Lütfen gerekli alanları doldurun", new ToastrOptions { Title = "Uyarı" });
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _repoEmployee.GetByIdAsync(id);
            if (user != null)
            {
                _repoEmployee.Delete(user);
            }

            await _repoEmployee.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Personel silme işlemi başarılı", new ToastrOptions { Title = "Silme" });
            return RedirectToAction(nameof(Index));
        }
    }
}
