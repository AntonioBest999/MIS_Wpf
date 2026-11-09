using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Employee : AuditableEntity
    {
        [Key]
        public string EmployeeId { get; set; } = GenerateEmployeeId();

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string? Passport { get; set; } = null;
        public DateOnly BirthDate { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public PositionType Position { get; set; }  
        public int DepartmentId { get; set; }
        public string? PhotoPath { get; set; }

        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<Appointment> AppointmentsExecuted { get; set; } = new List<Appointment>();
        public virtual ICollection<PrimaryInspection> PrimaryInspections { get; set; } = new List<PrimaryInspection>();
        public virtual ICollection<Surgery> SurgeriesPerformed { get; set; } = new List<Surgery>();
        public virtual ICollection<Hospitalization> HospitalizationsAttended { get; set; } = new List<Hospitalization>();
        public enum PositionType
        {
            ChiefDoctor,
            DeputyChief,
            HeadOfDepartment,
            Doctor,
            LabTechnician,
            Nurse,
            ReceptionNurse,
            ReceptionDoctor,
            Admin              

        }
                
        public bool IsActing { get; set; } = false;
        public static string GenerateEmployeeId()
        {
            const string digitsStartEnd = "23456789"; 
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            char first = digitsStartEnd[random.Next(digitsStartEnd.Length)];            
            char last = digitsStartEnd[random.Next(digitsStartEnd.Length)];               

            var middle = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray()); 

            return $"{first}{middle}{last}";
        }
    }
}
