using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace StaffTaskList.UI.Models
{
    public class LoginViewModel
    {
        [DisplayName("Kullanıcı Adı"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string Username { get; set; }

        [DisplayName("Şifre"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
