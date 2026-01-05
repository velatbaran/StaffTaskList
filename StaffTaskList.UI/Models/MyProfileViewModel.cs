using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace StaffTaskList.UI.Models
{
    public class MyProfileViewModel
    {
        public int Id { get; set; }
        [DisplayName("Ad Soyad"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string NameSurname { get; set; }

        [DisplayName("Kullanıcı Adı"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string Username { get; set; }
    }
}
