using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffTaskList.Core.Entities
{
    public class TaskDeparture : CommonEntity
    {
        [DisplayName("Görev")]
        public Task? Task { get; set; }
        [DisplayName("Görev")]
        public int? TaskId { get; set; }

        [DisplayName("Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [DisplayName("Ayrılış Tarihi")]
        public DateTime DepartureDate { get; set; }
    }
}
