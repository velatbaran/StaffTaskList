using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace StaffTaskList.UI.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        [DisplayName("Görev")]
        public int? TaskId { get; set; }

        [DisplayName("Personel"), StringLength(50)]
        public string Employee { get; set; }

        [DisplayName("Gidilen Yer"), StringLength(50)]
        public string PlaceGone { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }

        [DisplayName("Varış Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime ArrivalDate { get; set; }

        [DisplayName("Aktif Ayrılış Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime ActiveDepartureDate { get; set; }

        [DisplayName("Yeni Ayrılış Tarihi")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = false)]
        public DateTime NewDepartureDate { get; set; }

        [DisplayName("Toplam Gün")]
        public int TotalDay { get; set; }

        [DisplayName("Kaydeden"), StringLength(50)]
        public string? Created { get; set; }

        [DisplayName("Aktif Mi")]
        public bool IsActive { get; set; }

        [DisplayName("Kayıt Tarihi")]
        public DateTime? CreatedDate { get; set; }
    }
}
