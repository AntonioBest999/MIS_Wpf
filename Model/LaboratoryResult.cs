using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class LaboratoryResult : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int EmcId { get; set; }  
        [ForeignKey(nameof(EmcId))]
        public virtual EMC Emc { get; set; }  
        public string TestType { get; set; }
        public string Result { get; set; }
        public DateTime SampleDate { get; set; }
        public DateTime ReceivedDate { get; set; }
    }
}
