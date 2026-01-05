using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffTaskList.Core.Entities
{
    public class Employee : CommonEntity
    {
        [DisplayName("Ad Soyad"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string NameSurname { get; set; }

        [DisplayName("Pozisyon"), StringLength(50), Required(ErrorMessage = "{0} alanı boş geçilemez")]
        public string Position { get; set; }

        public ICollection<Task> Tasks { get; set; }

        public Employee()
        {
            Tasks = new List<Task>();
        }
    }
}
