using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using StaffTaskList.Core.Entities;
using StaffTaskList.Service.IRepository;
using StaffTaskList.UI.Models;
using System.Security.Claims;

namespace StaffTaskList.UI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _repoUser;
        private readonly IToastNotification _toastNotification;

        public AccountController(IRepository<User> repoUser, IToastNotification toastNotification)
        {
            _repoUser = repoUser;
            _toastNotification = toastNotification;
        }

        [Route("giris")]
        public IActionResult Login()
        {
            return View();
        }

        [Route("giris")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var account = await _repoUser.GetAsync(x => x.Username == model.Username & x.Password == model.Password);
                    if (account == null)
                    {

                        _toastNotification.AddErrorToastMessage("Kullanıcı adı veya şifre hatalı", new ToastrOptions { Title = "Giriş" });
                        return View(model);
                    }
                    else
                    {
                        var claims = new List<Claim>()
                        {
                            new(ClaimTypes.Name,account.NameSurname),
                            new("Username",account.Username.ToString()),
                            new("UserId",account.Id.ToString()),
                            new("UserGuid",account.UserGuid.ToString())
                        };

                        var userIdentity = new ClaimsIdentity(claims, "Login");
                        ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);
                        await HttpContext.SignInAsync(userPrincipal);
                        _toastNotification.AddSuccessToastMessage("Giriş işlemi başarılı", new ToastrOptions { Title = "Giriş" });
                        return Redirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/anasayfa" : model.ReturnUrl);
                    }
                }
                catch (Exception)
                {
                    // loglama
                    _toastNotification.AddErrorToastMessage("Lütfen kullanıcı bilgilerini kontrol ediniz", new ToastrOptions { Title = "Giriş" });
                    return View(model);
                }
            }
            return View(model);
        }

        [Route("cikis")]
        public async Task<IActionResult> LogOutAsync()
        {
            await HttpContext.SignOutAsync();
            _toastNotification.AddSuccessToastMessage("Çıkış işlemi başarılı", new ToastrOptions { Title = "Çıkış" });
            return Redirect("giris");
        }

        [Route("profilim")]
        [Authorize]
        public async Task<IActionResult> MyProfileAsync()
        {
            User user = await _repoUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value.ToString());
            if (user == null)
            {
                _toastNotification.AddErrorToastMessage("Kullanıcı bulunamadı!", new ToastrOptions { Title = "Profilim" });
                return NotFound();
            }
            var model = new MyProfileViewModel()
            {
                Id = user.Id,
                Username = user.Username,
                NameSurname = user.NameSurname,
            };
            return View(model);
        }

        [Route("profilim")]
        [HttpPost, Authorize]
        public async Task<IActionResult> MyProfileAsync(MyProfileViewModel model, int id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _repoUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
                    user.Username = model.Username;
                    user.NameSurname = model.NameSurname;
                    _repoUser.Update(user);
                    var sonuc = await _repoUser.SaveChangesAsync();
                    if (sonuc > 0)
                    {
                        _toastNotification.AddSuccessToastMessage("Bilgileriniz başarıyla güncellenmiştir", new ToastrOptions { Title = "Profilim" });
                        return View(model);
                    }
                }
                catch (Exception)
                {
                    _toastNotification.AddErrorToastMessage("Bilgileriniz güncellenirken hata oluştu!", new ToastrOptions { Title = "Profilim" });
                }
            }
            return View(model);
        }

        [Route("sifremiunuttum")]
        [HttpGet]
        public IActionResult ForgetPasswordAsync()
        {
            return View();
        }

        [Route("sifremiunuttum")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    User user = await _repoUser.GetAsync(x => x.Username == model.Username);
                    if (user is null)
                    {
                        _toastNotification.AddErrorToastMessage("Sistemde kayıtlı böyle bir kullanıcı adı yok!", new ToastrOptions { Title = "Hata" });
                        return View(model);
                    }

                    user.Password = model.Password;
                    _repoUser.Update(user);
                    var sonuc = await _repoUser.SaveChangesAsync();
                    if (sonuc > 0)
                    {
                        _toastNotification.AddSuccessToastMessage("Şifreniz başarıyla değişti.", new ToastrOptions { Title = "Şifremi Unuttum" });
                        return View(model);
                    }

                }
                catch (Exception)
                {
                    _toastNotification.AddErrorToastMessage("Şifreniz değiştirilirken hata oluştu!", new ToastrOptions { Title = "Şifremi Unuttum" });
                }
            }
            return View(model);
        }

        [Route("sifremidegistir")]
        [HttpGet, Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Route("sifremidegistir")]
        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    User user = await _repoUser.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
                    user.Password = model.Password;
                    _repoUser.Update(user);
                    await _repoUser.SaveChangesAsync();
                    _toastNotification.AddSuccessToastMessage("Şifre değiştirme işlemi başarılı", new ToastrOptions { Title = "Şifre Değiştirme" });
                    return View(model);
                }
                catch (Exception)
                {
                    _toastNotification.AddErrorToastMessage("Lütfen şifre bilgilerini kontrol ediniz", new ToastrOptions { Title = "Şifre Değiştirme" });
                    return View(model);
                }
            }
            return View(model);
        }
    }
}
