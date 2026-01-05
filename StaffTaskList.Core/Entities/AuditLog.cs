using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffTaskList.Core.Entities
{
    public class AuditLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TableName { get; set; }   // Hangi tablo
        public string KeyValues { get; set; }   // Primary Key bilgisi
        public string? OldValues { get; set; }   // Değişiklikten önceki değerler (JSON)
        public string? NewValues { get; set; }   // Değişiklikten sonraki değerler (JSON)
        public string Action { get; set; }      // Insert, Update, Delete
        public string UserName { get; set; }    // Kim yaptı
        public DateTime ChangedAt { get; set; } // Ne zaman
    }
}
