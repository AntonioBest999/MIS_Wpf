using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class User : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        [Required, MaxLength(50)]
        public string Login { get; set; }
     
        public string? Phone { get; set; } = null;
        public string? Email { get; set; } = null;
       
        [Required]
        public string PasswordHash { get; set; }
        public virtual Employee? Employee { get; set; }


        public enum UserRole
        {
            ChiefDoctor = 0,
            DeputyChief = 1,
            HeadOfDepartment = 2,
            Doctor = 3,
            LabTechnician = 4,
            Nurse = 5,
            ReceptionDoctor = 6,
            ReceptionNurse = 7,
            Admin = 8            
        }

        [Required, MaxLength(50)]        
        public UserRole Role { get; set; }
    }
}
