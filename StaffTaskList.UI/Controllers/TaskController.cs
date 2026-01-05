using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StaffTaskList.Core.Entities;
using StaffTaskList.Service.IRepository;
using StaffTaskList.UI.Models;
using System.Threading.Tasks;
using Task = StaffTaskList.Core.Entities.Task;

namespace StaffTaskList.UI.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly IRepository<Task> _repoTask;
        private readonly IRepository<TaskDeparture> _repoTaskDeparture;
        private readonly IRepository<Employee> _repoEmployee;
        private readonly IToastNotification _toastNotification;

        public TaskController(IRepository<Task> repoTask, IRepository<TaskDeparture> repoTaskDeparture, IToastNotification toastNotification, IRepository<Employee> repoEmployee)
        {
            _repoTask = repoTask;
            _repoTaskDeparture = repoTaskDeparture;
            _toastNotification = toastNotification;
            _repoEmployee = repoEmployee;
        }

        [Route("gorevler")]
        public async Task<IActionResult> IndexAsync()
        {
            // 1️⃣ Uzatma yapılmış TaskId'leri bul
            ViewBag.ExtendedTaskIds = _repoTaskDeparture.GetQueryable()
                .GroupBy(x => x.TaskId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key).ToList();


            var tasks = await _repoTaskDeparture.GetQueryable()
                                            .Include(x => x.Task)
                                            .ThenInclude(t => t.Employee)
                                            .Where(x => x.IsActive)
                                            .Select(v => new TaskViewModel
                                            {
                                                Id = v.Task.Id,
                                                Employee = v.Task.Employee.NameSurname,
                                                PlaceGone = v.Task.PlaceGone,
                                                ArrivalDate = v.Task.ArrivalDate,
                                                ActiveDepartureDate = v.DepartureDate,
                                                Description = v.Task.Description,
                                                TotalDay = v.Task.TotalDay,
                                                Created = v.Created,
                                                CreatedDate = v.CreatedDate,
                                            })
            .OrderByDescending(x=>x.CreatedDate).ToListAsync(); 

            //var tasks = await _repoTask.GetQueryable().Include("Employee").Include("TaskDepartures").Where(x=>x.TaskDeparture)
            //    .Select(v => new TaskViewModel
            //    {
            //        Id = v.Id,
            //        Employee = v.Employee.NameSurname,
            //        PlaceGone = v.PlaceGone,
            //        ArrivalDate = v.ArrivalDate,
            //        ActiveDepartureDate = v.TaskDepartures
            //        .Max(d => (DateTime?)d.DepartureDate),
            //        TotalDay = v.TotalDay,
            //        Created = v.Created,
            //        CreatedDate = v.CreatedDate,
            //    })
            //.ToListAsync();
            return View(tasks);
        }

        private static int CalculateTotalDay(DateTime arrivalDate, DateTime departureDate)
        {
            return Math.Abs((departureDate.Date - arrivalDate.Date).Days + 1);
        }

        [Route("gorevekle")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname");
            return View();
        }

        [Route("gorevekle")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int totalDay = CalculateTotalDay(model.ArrivalDate, model.NewDepartureDate);

                    if (model.NewDepartureDate < model.ArrivalDate)
                    {
                        _toastNotification.AddErrorToastMessage(
                            "Ayrılış tarihi, varış tarihinden önce olamaz!",
                            new ToastrOptions { Title = "Görev Güncelleme" }
                        );
                        return View(model);
                    }

                    if (totalDay > 90)
                    {
                        _toastNotification.AddErrorToastMessage("Görev süresi 90 günü geçemez!", new ToastrOptions { Title = "Görev Kayıt" });
                        ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", model.EmployeeId);
                        return View(model);
                    }

                    var task = new Task
                    {
                        EmployeeId = model.EmployeeId,
                        PlaceGone = model.PlaceGone,
                        ArrivalDate = model.ArrivalDate,
                        TotalDay = totalDay,
                        Description = model.Description,
                        //Created = model.Created,
                        //CreatedDate = DateTime.Now,
                    };

                    _repoTask.Add(task);
                    await _repoTask.SaveChangesAsync();

                    var taskDepartures = new TaskDeparture
                    {
                        TaskId = task.Id,
                        DepartureDate = model.NewDepartureDate,
                        IsActive = true,
                    };

                    _repoTaskDeparture.Add(taskDepartures);
                    await _repoTaskDeparture.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Görev kayıt işlemi başarılı", new ToastrOptions { Title = "Görev Kayıt" });
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message, new ToastrOptions { Title = "Görev Kayıt" });
                }
            }
            ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", model.EmployeeId);
            return View(model);
        }

        [Route("gorevguncelle/{id?}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskdeparture = await _repoTaskDeparture.GetQueryable()
                                                        .Include(x => x.Task)
                                                        .ThenInclude(t => t.Employee)
                                                        .Where(x => x.TaskId == id && x.IsActive).FirstOrDefaultAsync();

            if (await _repoTaskDeparture.AnyAsync(x => x.TaskId == taskdeparture.TaskId && !x.IsActive))
            {
                _toastNotification.AddErrorToastMessage("Süre uzatımı yapılan bir görev olduğu içi güncelleme yapılamaz!", new ToastrOptions { Title = "Görev Güncelleme" });
                return RedirectToAction(nameof(Index));
            }

            if (taskdeparture == null)
            {
                return NotFound();
            }

            var _task = new AddTaskViewModel()
            {
                Id = taskdeparture.Task.Id,
                EmployeeId = taskdeparture.Task.EmployeeId,
                PlaceGone = taskdeparture.Task.PlaceGone,
                ArrivalDate = taskdeparture.Task.ArrivalDate,
                Description = taskdeparture.Task.Description,
                NewDepartureDate = taskdeparture.DepartureDate,
            };
            ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", taskdeparture.Task.EmployeeId);
            return View(_task);
        }

        [Route("gorevguncelle/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddTaskViewModel model, int? id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int totalDay = CalculateTotalDay(model.ArrivalDate, model.NewDepartureDate);

                    if (model.NewDepartureDate < model.ArrivalDate)
                    {
                        _toastNotification.AddErrorToastMessage(
                            "Ayrılış tarihi, varış tarihinden önce olamaz!",
                            new ToastrOptions { Title = "Görev Güncelleme" }
                        );
                        return View(model);
                    }

                    if (totalDay > 90)
                    {
                        _toastNotification.AddErrorToastMessage("Görev süresi 90 günü geçemez!", new ToastrOptions { Title = "Görev Güncelleme" });
                        ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", model.EmployeeId);
                        return View(model);
                    }

                    var task = await _repoTask.GetByIdAsync(id.Value);
                    var taskdeparture = await _repoTaskDeparture.GetQueryable().Include("Task").Where(x => x.TaskId == id.Value && x.IsActive == true).FirstOrDefaultAsync();

                    task.EmployeeId = model.EmployeeId;
                    task.PlaceGone = model.PlaceGone;
                    task.ArrivalDate = model.ArrivalDate;
                    task.TotalDay = totalDay;
                    task.Description = model.Description;
                    _repoTask.Update(task);
                   await _repoTask.SaveChangesAsync();

                    //     taskdeparture.TaskId = task.Id;
                    taskdeparture.DepartureDate = model.NewDepartureDate;
                    //     taskdeparture.IsActive = true;
                    _repoTaskDeparture.Update(taskdeparture);
                    await _repoTaskDeparture.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Görev güncellme işlemi başarılı", new ToastrOptions { Title = "Görev Güncelleme" });
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message, new ToastrOptions { Title = "Görev Güncelleme" });
                }
            }

            ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", model.EmployeeId);
            return View(model);
        }

        [Route("gorevuzat/{id?}")]
        [HttpGet]
        public async Task<IActionResult> Extend(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskdeparture = await _repoTaskDeparture.GetQueryable()
                                                        .Include(x => x.Task)
                                                        .ThenInclude(t => t.Employee)
                                                        .Where(x => x.TaskId == id && x.IsActive).FirstOrDefaultAsync();

            if (taskdeparture == null)
            {
                return NotFound();
            }

            var _task = new AddTaskViewModel()
            {
                Id = taskdeparture.Task.Id,
                EmployeeId = taskdeparture.Task.EmployeeId,
                PlaceGone = taskdeparture.Task.PlaceGone,
                ArrivalDate = taskdeparture.Task.ArrivalDate,
                NewDepartureDate = taskdeparture.DepartureDate,
                Description = taskdeparture.Task.Description,
            };
            ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", taskdeparture.Task.EmployeeId);
            return View(_task);
        }

        [Route("gorevuzat/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(AddTaskViewModel model, int? id)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    int totalDay = CalculateTotalDay(model.ArrivalDate, model.NewDepartureDate);

                    if (model.NewDepartureDate < model.ArrivalDate)
                    {
                        _toastNotification.AddErrorToastMessage(
                            "Ayrılış tarihi, varış tarihinden önce olamaz!",
                            new ToastrOptions { Title = "Görev Uzatma" }
                        );
                        return View(model);
                    }

                    if (totalDay > 90)
                    {
                        _toastNotification.AddErrorToastMessage("Görev süresi 90 günü geçemez!", new ToastrOptions { Title = "Görev Uzatma" });
                        ViewBag.Employee = new SelectList(_repoEmployee.GetAll(), "Id", "NameSurname", model.EmployeeId);
                        return View(model);
                    }

                    var task = await _repoTask.GetByIdAsync(id.Value);
                    var taskdeparture = await _repoTaskDeparture.GetQueryable().Include("Task").Where(x => x.TaskId == id.Value && x.IsActive == true).FirstOrDefaultAsync();

                    task.TotalDay = totalDay;
                    task.Description = model.Description;
                    _repoTask.Update(task);
                    await _repoTask.SaveChangesAsync();

                    //     taskdeparture.TaskId = task.Id;
                    taskdeparture.IsActive = false;
                    //     taskdeparture.IsActive = true;
                    _repoTaskDeparture.Update(taskdeparture);
                    await _repoTaskDeparture.SaveChangesAsync();

                    var extendTaskDeoarture = new TaskDeparture()
                    {
                        TaskId = task.Id,
                        DepartureDate = model.NewDepartureDate,
                        IsActive = true,
                    };

                    _repoTaskDeparture.Add(extendTaskDeoarture);
                    await _repoTaskDeparture.SaveChangesAsync();

                    _toastNotification.AddSuccessToastMessage("Görev uzatma işlemi başarılı", new ToastrOptions { Title = "Görev Uzatma" });
                    return RedirectToAction(nameof(Index));

                }
                catch (Exception ex)
                {
                    _toastNotification.AddErrorToastMessage(ex.Message, new ToastrOptions { Title = "Görev Güncelleme" });
                }
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _repoTask.GetByIdAsync(id);
            List<TaskDeparture> taskdepartures = await _repoTaskDeparture.GetQueryable().Include("Task").Where(x => x.TaskId == id).ToListAsync();
            if (task != null)
            {
                _repoTask.Delete(task);
            }
            if (taskdepartures != null)
            {
                _repoTaskDeparture.DeleteRange(taskdepartures);
            }

            await _repoTask.SaveChangesAsync();
            await _repoTaskDeparture.SaveChangesAsync();
            _toastNotification.AddSuccessToastMessage("Silme işlemi başarılı", new ToastrOptions { Title = "Görev Silme" });
            return RedirectToAction(nameof(Index));
        }

        [Route("uzatilangorevler")]
        public async Task<IActionResult> ExtendedTasks()
        {
            // 1️⃣ Uzatma yapılmış TaskId'leri bul
            var extendedTaskIds = _repoTaskDeparture.GetQueryable()
                .GroupBy(x => x.TaskId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            // 2️⃣ Bu TaskId'lere ait TÜM TaskDeparture kayıtlarını getir
            var tasks = await _repoTaskDeparture.GetQueryable()
                .Include(x => x.Task)
                    .ThenInclude(t => t.Employee)
                .Where(x => extendedTaskIds.Contains(x.TaskId))
                .Select(v => new TaskViewModel
                {
                    Id = v.Task.Id,
                    Employee = v.Task.Employee.NameSurname,
                    PlaceGone = v.Task.PlaceGone,
                    ArrivalDate = v.Task.ArrivalDate,
                    ActiveDepartureDate = v.DepartureDate,
                    Description = v.Task.Description,
                    TotalDay = CalculateTotalDay(v.Task.ArrivalDate, v.DepartureDate),
                    Created = v.Created,
                    CreatedDate = v.CreatedDate,
                })
            .OrderByDescending(x => x.CreatedDate).ToListAsync();

            return View(tasks);

        }

        [Route("uzatilangorev/{id?}")]
        public async Task<IActionResult> ExtendedTasksOne(int id)
        {
            // 1️⃣ Uzatma yapılmış TaskId'leri bul
            var extendedTaskIds = _repoTaskDeparture.GetQueryable()
                .GroupBy(x => x.TaskId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            // 2️⃣ Bu TaskId'lere ait TÜM TaskDeparture kayıtlarını getir
            var tasks = await _repoTaskDeparture.GetQueryable()
                 .Where(x => x.TaskId == id) 
                .Include(x => x.Task)
                    .ThenInclude(t => t.Employee)
                .Select(v => new TaskViewModel
                {
                    Id = v.Task.Id,
                    Employee = v.Task.Employee.NameSurname,
                    PlaceGone = v.Task.PlaceGone,
                    ArrivalDate = v.Task.ArrivalDate,
                    ActiveDepartureDate = v.DepartureDate,
                    Description = v.Task.Description,
                    TotalDay = CalculateTotalDay(v.Task.ArrivalDate,v.DepartureDate),
                    Created = v.Created,
                    CreatedDate = v.CreatedDate,
                })
            .OrderBy(x => x.CreatedDate).ToListAsync();

            return View(tasks);

        }

        [Route("uzatilmayangorevler")]
        public async Task<IActionResult> NotExtendedTasks()
        {
            // 1️⃣ Uzatma yapılmış TaskId'leri bul
            var extendedTaskIds = _repoTaskDeparture.GetQueryable()
                .GroupBy(x => x.TaskId)
                .Where(g => g.Count() == 1)
                .Select(g => g.Key);

            // 2️⃣ Bu TaskId'lere ait TÜM TaskDeparture kayıtlarını getir
            var tasks = await _repoTaskDeparture.GetQueryable()
                .Include(x => x.Task)
                    .ThenInclude(t => t.Employee)
                .Where(x => extendedTaskIds.Contains(x.TaskId))
                .Select(v => new TaskViewModel
                {
                    Id = v.Task.Id,
                    Employee = v.Task.Employee.NameSurname,
                    PlaceGone = v.Task.PlaceGone,
                    ArrivalDate = v.Task.ArrivalDate,
                    ActiveDepartureDate = v.DepartureDate,
                    Description = v.Task.Description,
                    TotalDay = v.Task.TotalDay,
                    Created = v.Created,
                    CreatedDate = v.CreatedDate,
                })
            .OrderByDescending(x => x.CreatedDate).ToListAsync();

            return View(tasks);

        }
    }
}
