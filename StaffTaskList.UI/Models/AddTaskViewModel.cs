using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace StaffTaskList.UI.Models
{
    public class AddTaskViewModel
    {
        public int Id { get; set; }

        [DisplayName("Personel")]
        public int? EmployeeId { get; set; }

        [DisplayName("Gidilen Yer"), StringLength(50,ErrorMessage =" Max. {0} karakter olmalıdır."),Required(ErrorMessage ="{0} alanı boşgeçilemez!")]
        public string PlaceGone { get; set; }

        [DisplayName("Varış Tarihi"), Required(ErrorMessage = "{0} alanı boşgeçilemez!")]
        public DateTime ArrivalDate { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }

        //[DisplayName("Aktif Ayrılış Tarihi")]
        //public DateTime? ActiveDepartureDate { get; set; }

        [DisplayName("Ayrılış Tarihi"), Required(ErrorMessage = "{0} alanı boşgeçilemez!")]
        public DateTime NewDepartureDate { get; set; }


    }
}
