using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace StaffTaskList.Core.Entities
{
    public class Task : CommonEntity
    {
        [DisplayName("Personel")]
        public Employee? Employee { get; set; }

        [DisplayName("Personel")]
        public int? EmployeeId { get; set; }

        [DisplayName("Gidilen Yer"), StringLength(50)]
        public string PlaceGone { get; set; }

        [DisplayName("Toplam Gün")]
        public int TotalDay { get; set; }

        [DisplayName("Varış Tarihi")]
        public DateTime ArrivalDate { get; set; }

        [DisplayName("Açıklama")]
        public string? Description { get; set; }

        public ICollection<TaskDeparture>? TaskDepartures { get; set; }
        public Task()
        {
            TaskDepartures = new List<TaskDeparture>();
        }

    }
}
