using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StaffTaskList.Core.ICurrentUser;

namespace StaffTaskList.Core.Entities
{
    public class CommonEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DisplayName("Kaydeden"), StringLength(50)]
        public string? Created { get; set; } 

        [DisplayName("Kayıt Tarihi")]
        public DateTime? CreatedDate { get; set; }

    }
}
