using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Surgery : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int EmcId { get; set; }  
        [ForeignKey(nameof(EmcId))]
        public virtual EMC Emc { get; set; }  
        public DateTime SurgeryDate { get; set; }
        public string? SurgeryName { get; set; }
        public string? Protocol { get; set; }
        public string? SurgeonId { get; set; }
        [ForeignKey(nameof(SurgeonId))]
        public virtual Employee? Surgeon { get; set; }
    }
}
