using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;
using ЦРБ_и_все_все_все.Классы.RegularInspectionData;

using static ЦРБ_и_все_все_все.Классы.Department;
using static ЦРБ_и_все_все_все.Классы.Employee;
using static ЦРБ_и_все_все_все.Классы.Inspection;

namespace ЦРБ_и_все_все_все.Классы
{
    public class MisDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Hospitalization> Hospitalizations { get; set; }
        public DbSet<PrimaryInspection> PrimaryInspections { get; set; }
        public DbSet<EMC> Emcs { get; set; }
        public DbSet<Inspection> Inspections { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Surgery> Surgeries { get; set; }
        public DbSet<LaboratoryResult> LaboratoryResults { get; set; }
        public DbSet<DischargeSummary> DischargeSummaries { get; set; }
        public DbSet<MedicalFacility> MedicalFacilities { get; set; }
        public DbSet<PatientVisit> PatientVisits { get; set; }
        public DbSet<DiagnosisHistory> DiagnosisHistories { get; set; }
        public DbSet<HospitalizationHistory> HospitalizationHistories { get; set; }       
        public DbSet<AbdomenData> AbdomenData { get; set; } = null!;
        public DbSet<AnamnesisVitaeData> AnamnesisVitaeData { get; set; } = null!;
        public DbSet<CardiologyData> CardiologyData { get; set; } = null!;
        public DbSet<SkinData> SkinData { get; set; } = null!;
        public DbSet<RespiratoryData> RespiratoryData { get; set; } = null!;
        public DbSet<NeuroData> NeuroData { get; set; } = null!;
        public DbSet<PrimaryInspectionDetailedData> PrimaryInspectionDetailedData { get; set; } = null!;

        public MisDbContext(DbContextOptions<MisDbContext> options) : base(options) { }
        public DbSet<LabParameter> LabParameters { get; set; }
        public DbSet<LabOrder> LabOrders { get; set; }
        public DbSet<LabOrderItem> LabOrderItems { get; set; }
        public DbSet<GeneralData> GeneralData { get; set; } = null!;
        public DbSet<Attachment> Attachments { get; set; } = null!;
       // public DbSet<Inspection> Inspections { get; set; }
        public DbSet<SkinDataRegular> SkinDataRegular { get; set; } = null!;
        public DbSet<GastrointestinalDataRegular> GastrointestinalDataRegular { get; set; } = null!;
        public DbSet<RespiratoryDataRegular> RespiratoryDataRegular { get; set; } = null!;
        public DbSet<VitalSignsDataRegular> VitalSignsRegular { get; set; }
        public DbSet<CardiologyDataRegular> CardiologyDataRegular { get; set; }
        public DbSet<NeurologicalDataRegular> NeuroDataRegular { get; set; }
        public DbSet<GeneralStatusDataRegular> GeneralStatusDataRegular { get; set; }
        public DbSet<MedicationAssignment> MedicationAssignments { get; set; } = null!;
        public DbSet<MedicationIntake> MedicationIntakes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Soft-delete фильтры (для всех AuditableEntity)
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            // modelBuilder.Entity<PatientContact>().HasQueryFilter(pc => !pc.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Department>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Hospitalization>().HasQueryFilter(h => !h.IsDeleted);
            modelBuilder.Entity<EMC>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Inspection>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Surgery>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<LaboratoryResult>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<DischargeSummary>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<MedicalFacility>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<PatientVisit>().HasQueryFilter(pv => !pv.IsDeleted);
            //  modelBuilder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<LabOrder>()
                .HasOne(l => l.PrimaryInspection)
                .WithMany() // если у PrimaryInspection нет коллекции LabOrders, оставьте WithMany()
                .HasForeignKey(l => l.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.SetNull);
            // ================= НАЗНАЧЕНИЯ(ИНСТРУМЕНТАЛЬНЫЕ ИССЛЕДОВАНИЯ) =================
            modelBuilder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Attachments)           // нужно будет добавить в Patient позже
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Admission)
                .WithMany()
                .HasForeignKey(a => a.AdmissionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull); 
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Hospitalization)
                .WithMany()
                .HasForeignKey(a => a.HospitalizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
            //================= РЕГУЛЯРНЫЕ ОСМОТРЫ =================
            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.VitalSigns)                     
                .WithOne(v => v.Inspection)                    
                .HasForeignKey<VitalSignsDataRegular>(v => v.InspectionId) 
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.GeneralStatus)
                .WithOne(g => g.Inspection)
                .HasForeignKey<GeneralStatusDataRegular>(g => g.InspectionId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Skin)
                .WithOne(s => s.Inspection)
                .HasForeignKey<SkinDataRegular>(s => s.InspectionId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Respiratory)
                .WithOne(r => r.Inspection)
                .HasForeignKey<RespiratoryDataRegular>(r => r.InspectionId)
                .OnDelete(DeleteBehavior.Cascade); 

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Cardiology)
                .WithOne(c => c.Inspection)
                .HasForeignKey<CardiologyDataRegular>(c => c.InspectionId);

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Gastrointestinal)
                .WithOne(g => g.Inspection)
                .HasForeignKey<GastrointestinalDataRegular>(g => g.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Neurological)
                .WithOne(n => n.Inspection)
                .HasForeignKey<NeurologicalDataRegular>(n => n.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
            // ====================== ИСТОРИЯ ДИАГНОЗОВ ======================
            modelBuilder.Entity<DiagnosisHistory>().HasQueryFilter(dh => !dh.IsDeleted);
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");
                entity.HasKey(p => p.PatientId);
                entity.Property(p => p.PatientId)
                      .HasColumnName("id");
            });
            modelBuilder.Entity<Patient>()
                .HasIndex(p => new { p.FullName, p.BirthDate, p.PhoneNumber }).HasName("IX_Patient_SearchFields");

            modelBuilder.Entity<PatientContact>()
                .HasOne(pc => pc.Patient)
                .WithMany(p => p.Contacts)
                .HasForeignKey(pc => pc.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            // ====================== СОТРУДНИКИ ======================
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
       
            modelBuilder.Entity<Employee>()
                .HasIndex(e => new { e.DepartmentId, e.Position });  // Для проверки уникальности зав. отделения
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<int>();

      //======================== ГОСПИТАЛИЗАЦИИ ======================
            modelBuilder.Entity<Hospitalization>()
                .HasIndex(h => h.MedicalRecordNumber)
                .IsUnique();

            modelBuilder.Entity<Hospitalization>()
                .HasIndex(h => h.MedicalRecordNumber)
                .IsUnique();

            modelBuilder.Entity<Hospitalization>()
                .HasIndex(h => h.AttendingDoctorId);  //быстрый поиск "мои пациенты"
            // ====================== ОТДЕЛЕНИЯ ======================
            modelBuilder.Entity<Department>().HasIndex(d => d.Type);  // Индекс для быстрого поиска по типу
            modelBuilder.Entity<Department>().HasIndex(d => d.Type);
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.DepartmentId);                   
                entity.Property(d => d.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                // Связь с руководителем отдела (опционально)
                entity.HasOne(d => d.DepAhead)
                      .WithMany()
                      .HasForeignKey(d => d.DepAheadId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Сотрудники отдела
                entity.HasMany(d => d.Employees)
                      .WithOne(e => e.Department)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
                // Госпитализации в отделении
                entity.HasMany(d => d.Hospitalizations)
                      .WithOne(h => h.Department)
                      .HasForeignKey(h => h.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Type = DepartmentType.Admissions, Name = "ПРИЁМНОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 2, Type = DepartmentType.Surgical, Name = "ХИРУРГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 3, Type = DepartmentType.PurulentSeptic, Name = "ГНОЙНО-СЕПТИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 4, Type = DepartmentType.HeadNeckReconstructiveSurgery, Name = "ОТДЕЛЕНИЕ ГОЛОВЫ, ШЕИ И ВОССТАНОВИТЕЛЬНОЙ ХИРУРГИИ" },
                new Department { DepartmentId = 5, Type = DepartmentType.Oncology, Name = "ОНКОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 6, Type = DepartmentType.Urology, Name = "УРОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 7, Type = DepartmentType.OtolaryngologyOphthalmology, Name = "ОТДЕЛЕНИЕ ОТОЛАРИНГОЛОГИИ И ОФТАЛЬМОЛОГИИ" },
                new Department { DepartmentId = 8, Type = DepartmentType.Pulmonology, Name = "ПУЛЬМОНОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 9, Type = DepartmentType.Cardiology, Name = "КАРДИОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 10, Type = DepartmentType.Infectious, Name = "ИНФЕКЦИОННОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 11, Type = DepartmentType.Psychiatric, Name = "ПСИХИАТРИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 12, Type = DepartmentType.Gastroenterology, Name = "ГАСТРОЭНТЕРОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 13, Type = DepartmentType.Chemotherapy, Name = "ХИМИОТЕРАПЕВТИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 14, Type = DepartmentType.Hematology, Name = "ОТДЕЛЕНИЕ ГЕМАТОЛОГИИ" },
                new Department { DepartmentId = 15, Type = DepartmentType.MixedTrauma, Name = "ОТДЕЛЕНИЕ СМЕШАННОЙ ТРАВМЫ" },
                new Department { DepartmentId = 16, Type = DepartmentType.TraumaPoint, Name = "ТРАВМАТОЛОГИЧЕСКИЙ ПУНКТ" },
                new Department { DepartmentId = 17, Type = DepartmentType.OperatingBlock, Name = "ОПЕРАЦИОННЫЙ БЛОК" },
                new Department { DepartmentId = 18, Type = DepartmentType.PathologicalAnatomical, Name = "ПАТОЛОГОАНАТОМИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 19, Type = DepartmentType.DiagnosticUnits, Name = "ДИАГНОСТИЧЕСКИЕ ПОДРАЗДЕЛЕНИЯ" },
                new Department { DepartmentId = 20, Type = DepartmentType.ClinicalDiagnosticLaboratory, Name = "ЛАБОРАТОРИЯ КЛИНИКО-ДИАГНОСТИЧЕСКАЯ" },
                new Department { DepartmentId = 21, Type = DepartmentType.CytologicalLaboratory, Name = "ЛАБОРАТОРИЯ ЦИТОЛОГИЧЕСКАЯ" },
                new Department { DepartmentId = 22, Type = DepartmentType.HistologicalLaboratory, Name = "ЛАБОРАТОРИЯ ГИСТОЛОГИЧЕСКАЯ" },
                new Department { DepartmentId = 23, Type = DepartmentType.XRay, Name = "РЕНТГЕН-ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 24, Type = DepartmentType.Endoscopic, Name = "ЭНДОСКОПИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 25, Type = DepartmentType.Endocrinology, Name = "ЭНДОКРИНОЛОГИЧЕСКОЕ ОТДЕЛЕНИЕ" },
                new Department { DepartmentId = 26, Type = DepartmentType.ResuscitationAnesthesiology, Name = "ОТДЕЛЕНИЕ РЕАНИМАЦИИ И АНЕСТЕЗИОЛОГИИ" },
                new Department { DepartmentId = 27, Type = DepartmentType.BloodTransfusion, Name = "ОТДЕЛЕНИЕ ПЕРЕЛИВАНИЯ КРОВИ" },
                new Department { DepartmentId = 28, Type = DepartmentType.Administration, Name = "АДМИНИСТРАТИВНОЕ ОТДЕЛЕНИЕ" }
            );
            });
            // ====================== ИСХОДНЫЕ ЛАБОРАТОРНЫЕ ПОКАЗАТЕЛИ ======================

            modelBuilder.Entity<LabParameter>().HasData(
                // ==================== 1. ОАК ====================
                new LabParameter { LabParameterId = 1, Code = "HGB", Name = "Гемоглобин", Units = "г/л", Category = "ОАК", RefLow = 130, RefHigh = 160, RefLowFemale = 120, RefHighFemale = 140 },
                new LabParameter { LabParameterId = 2, Code = "RBC", Name = "Эритроциты", Units = "×10^12/л", Category = "ОАК", RefLow = 4.0m, RefHigh = 5.0m, RefLowFemale = 3.9m, RefHighFemale = 4.8m },
                new LabParameter { LabParameterId = 3, Code = "CP", Name = "Цветовой показатель", Units = "", Category = "ОАК", RefLow = 0.85m, RefHigh = 1.05m },
                new LabParameter { LabParameterId = 4, Code = "HCT", Name = "Гематокрит", Units = "%", Category = "ОАК", RefLow = 40, RefHigh = 48, RefLowFemale = 36, RefHighFemale = 46 },
                new LabParameter { LabParameterId = 5, Code = "RET", Name = "Ретикулоциты", Units = "‰", Category = "ОАК", RefLow = 2, RefHigh = 10 },
                new LabParameter { LabParameterId = 6, Code = "PLT", Name = "Тромбоциты", Units = "×10^9/л", Category = "ОАК", RefLow = 180, RefHigh = 320 },
                new LabParameter { LabParameterId = 7, Code = "WBC", Name = "Лейкоциты", Units = "×10^9/л", Category = "ОАК", RefLow = 4.0m, RefHigh = 9.0m },
                new LabParameter { LabParameterId = 8, Code = "ESR", Name = "СОЭ", Units = "мм/час", Category = "ОАК", RefLow = 2, RefHigh = 10, RefLowFemale = 2, RefHighFemale = 15 },

                // Лейкоцитарная формула
                new LabParameter { LabParameterId = 9, Code = "BLAST", Name = "Бластные клетки", Units = "%", Category = "ОАК", RefLow = 0, RefHigh = 0 },
                new LabParameter { LabParameterId = 10, Code = "NEUT_P", Name = "Палочкоядерные нейтрофилы", Units = "%", Category = "ОАК", RefLow = 1, RefHigh = 6 },
                new LabParameter { LabParameterId = 11, Code = "NEUT_S", Name = "Сегментоядерные нейтрофилы", Units = "%", Category = "ОАК", RefLow = 47, RefHigh = 72 },
                new LabParameter { LabParameterId = 12, Code = "EOS", Name = "Эозинофилы", Units = "%", Category = "ОАК", RefLow = 0.5m, RefHigh = 5 },
                new LabParameter { LabParameterId = 13, Code = "LYM", Name = "Лимфоциты", Units = "%", Category = "ОАК", RefLow = 19, RefHigh = 37 },
                new LabParameter { LabParameterId = 14, Code = "MON", Name = "Моноциты", Units = "%", Category = "ОАК", RefLow = 3, RefHigh = 11 },
                new LabParameter { LabParameterId = 15, Code = "BASO", Name = "Базофилы", Units = "%", Category = "ОАК", RefLow = 0, RefHigh = 1 },
                  // ==================== МОРФОЛОГИЯ ЛЕЙКОЦИТОВ ====================
                new LabParameter { LabParameterId = 16, Code = "TOX_GRAN", Name = "Токсогенная зернистость", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },
                new LabParameter { LabParameterId = 17, Code = "HYPERSEG", Name = "Гиперсегментация ядер", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },

                  // ==================== МОРФОЛОГИЯ ЭРИТРОЦИТОВ ====================
                new LabParameter { LabParameterId = 18, Code = "ANISO", Name = "Анизоцитоз", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },
                new LabParameter { LabParameterId = 19, Code = "POIKILO", Name = "Пойкилоцитоз", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },
                new LabParameter { LabParameterId = 20, Code = "POLYCHROM", Name = "Полихроматофилия", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },                new LabParameter { LabParameterId = 21, Code = "NORMOBL", Name = "Нормобласты", Units = "", Category = "ОАК", RefLow = 0, RefHigh = 0 },

                // ==================== 2. ОАМ ====================
                new LabParameter { LabParameterId = 50, Code = "VOL", Name = "Количество мочи", Units = "мл", Category = "ОАМ" },
                new LabParameter { LabParameterId = 51, Code = "COLOR", Name = "Цвет", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 52, Code = "TRANS", Name = "Прозрачность", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 53, Code = "SG", Name = "Удельный вес", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 54, Code = "PH", Name = "Реакция", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 55, Code = "PROT", Name = "Белок", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 56, Code = "GLU", Name = "Сахар", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 57, Code = "ACET", Name = "Ацетон", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 58, Code = "BIL", Name = "Желчные пигменты", Units = "", Category = "ОАМ" },

                new LabParameter { LabParameterId = 59, Code = "EPITH_F", Name = "Эпителий плоский", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 60, Code = "EPITH_T", Name = "Эпителий переходный", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 61, Code = "EPITH_U", Name = "Эпителий уретральный", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 62, Code = "EPITH_R", Name = "Эпителий почечный", Units = "", Category = "ОАМ" },

                new LabParameter { LabParameterId = 63, Code = "WBC_U", Name = "Лейкоциты", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 64, Code = "RBC_U", Name = "Эритроциты", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 65, Code = "RBC_MOD", Name = "Эритроциты изменённые", Units = "", Category = "ОАМ" },

                new LabParameter { LabParameterId = 66, Code = "CYL_H", Name = "Цилиндры гиалиновые", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 67, Code = "CYL_G", Name = "Цилиндры зернистые", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 68, Code = "CYL_W", Name = "Цилиндры восковидные", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 69, Code = "CYL_E", Name = "Цилиндры эпителиальные", Units = "", Category = "ОАМ" },

                new LabParameter { LabParameterId = 70, Code = "SLIME", Name = "Слизь", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 71, Code = "BACT", Name = "Бактерии", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 72, Code = "SALT", Name = "Соли", Units = "", Category = "ОАМ" },
                new LabParameter { LabParameterId = 73, Code = "FUNG", Name = "Грибки", Units = "", Category = "ОАМ" },

                // ==================== 3. БАК ====================
                new LabParameter { LabParameterId = 100, Code = "GLI", Name = "Гикемия", Units = "ммоль/л", Category = "БАК", RefLow = 3.9m, RefHigh = 6.1m },
                new LabParameter { LabParameterId = 101, Code = "UREA", Name = "Мочевина", Units = "ммоль/л", Category = "БАК", RefLow = 2.5m, RefHigh = 8.3m },
                new LabParameter { LabParameterId = 102, Code = "CREA", Name = "Креатинин", Units = "мкмоль/л", Category = "БАК", RefLow = 62, RefHigh = 115, RefLowFemale = 44, RefHighFemale = 97 },
                new LabParameter { LabParameterId = 103, Code = "TROP", Name = "Тропонин I", Units = "нг/мл", Category = "БАК", RefLow = 0, RefHigh = 0.04m },

                new LabParameter { LabParameterId = 104, Code = "BIL_T", Name = "Билирубин общий", Units = "мкмоль/л", Category = "БАК", RefLow = 8.5m, RefHigh = 20.5m },
                new LabParameter { LabParameterId = 105, Code = "BIL_D", Name = "Билирубин связанный", Units = "мкмоль/л", Category = "БАК", RefLow = 0, RefHigh = 4.0m },
                new LabParameter { LabParameterId = 106, Code = "BIL_F", Name = "Билирубин свободный", Units = "мкмоль/л", Category = "БАК", RefLow = 4.0m, RefHigh = 16.5m },

                new LabParameter { LabParameterId = 107, Code = "ALT", Name = "АлАТ", Units = "Ед/л", Category = "БАК", RefLow = 9, RefHigh = 50, RefLowFemale = 7, RefHighFemale = 40 },
                new LabParameter { LabParameterId = 108, Code = "AST", Name = "АсАТ", Units = "Ед/л", Category = "БАК", RefLow = 15, RefHigh = 40, RefLowFemale = 13, RefHighFemale = 35 },
                new LabParameter { LabParameterId = 109, Code = "ALP", Name = "Щелочная фосфатаза (ЩФ)", Units = "Ед/л", Category = "БАК", RefLow = 900, RefHigh = 2290, RefLowFemale = 740, RefHighFemale = 2100 },

                new LabParameter { LabParameterId = 110, Code = "TP", Name = "Общий белок", Units = "г/л", Category = "БАК", RefLow = 66, RefHigh = 83 },
                new LabParameter { LabParameterId = 111, Code = "ALB", Name = "Альбумины", Units = "г/л", Category = "БАК", RefLow = 35, RefHigh = 52 },

                // ==================== 4. Липидограмма ====================
                new LabParameter { LabParameterId = 160, Code = "CHOL", Name = "Общий холестерин", Units = "ммоль/л", Category = "Липидограмма", RefLow = 3.0m, RefHigh = 5.2m },
                new LabParameter { LabParameterId = 161, Code = "TRIG", Name = "Триглицериды", Units = "ммоль/л", Category = "Липидограмма", RefLow = 0.55m, RefHigh = 1.85m },
                new LabParameter { LabParameterId = 162, Code = "HDL", Name = "ЛПВП", Units = "ммоль/л", Category = "Липидограмма", RefLow = 1.04m, RefHigh = 1.55m },
                new LabParameter { LabParameterId = 163, Code = "LDL", Name = "ЛПНП", Units = "ммоль/л", Category = "Липидограмма", RefLow = 1.5m, RefHigh = 4.5m },
                new LabParameter { LabParameterId = 164, Code = "VLDL", Name = "ЛПОНП", Units = "ммоль/л", Category = "Липидограмма", RefLow = 0.25m, RefHigh = 0.84m },
                new LabParameter { LabParameterId = 165, Code = "KA", Name = "Коэффициент атерогенности", Units = "", Category = "Липидограмма", RefLow = 2.0m, RefHigh = 3.5m },

                // ==================== 5. Ионограмма ====================
                new LabParameter { LabParameterId = 200, Code = "CA", Name = "Кальций", Units = "ммоль/л", Category = "Ионограмма", RefLow = 2.2m, RefHigh = 2.6m },
                new LabParameter { LabParameterId = 201, Code = "P", Name = "Фосфор", Units = "ммоль/л", Category = "Ионограмма", RefLow = 0.81m, RefHigh = 1.45m },
                new LabParameter { LabParameterId = 202, Code = "MG", Name = "Магний", Units = "ммоль/л", Category = "Ионограмма", RefLow = 0.73m, RefHigh = 1.06m },

                // ==================== 6. Коагулограмма ====================
                new LabParameter { LabParameterId = 240, Code = "PT", Name = "Протромбиновое время", Units = "сек", Category = "Коагулограмма", RefLow = 11, RefHigh = 16 },
                new LabParameter { LabParameterId = 241, Code = "INR", Name = "МНО", Units = "", Category = "Коагулограмма", RefLow = 0.8m, RefHigh = 1.2m },
                new LabParameter { LabParameterId = 242, Code = "FIB", Name = "Фибриноген", Units = "г/л", Category = "Коагулограмма", RefLow = 2.0m, RefHigh = 4.0m },

                // ==================== 7. Гормоны ЩЖ ====================
                new LabParameter { LabParameterId = 300, Code = "T3", Name = "Т3 общий", Units = "нг/мл", Category = "Гормональные", RefLow = 0.8m, RefHigh = 2.1m },
                new LabParameter { LabParameterId = 301, Code = "T4", Name = "Т4 общий", Units = "мкг/дл", Category = "Гормональные", RefLow = 4.7m, RefHigh = 12.4m },
                new LabParameter { LabParameterId = 302, Code = "TSH", Name = "ТТГ", Units = "мМЕ/л", Category = "Гормональные", RefLow = 0.3m, RefHigh = 4.0m },
                new LabParameter { LabParameterId = 303, Code = "AT_TG", Name = "АТ-ТГ", Units = "МЕ/мл", Category = "Гормональные", RefLow = 0, RefHigh = 100 },
                new LabParameter { LabParameterId = 304, Code = "AT_TPO", Name = "АТ-ТПО", Units = "МЕ/мл", Category = "Гормональные", RefLow = 0, RefHigh = 30 },

                // ==================== ВИРУСОЛОГИЧЕСКИЕ ИССЛЕДОВАНИЯ ====================
                new LabParameter { LabParameterId = 400, Code = "HIV", Name = "ВИЧ (anti-HIV)", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 401, Code = "RW", Name = "МРС (RW)", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 402, Code = "HBsAg", Name = "anti-HBsAg", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 403, Code = "HBcAg", Name = "anti-HBcV", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 404, Code = "HCV", Name = "anti-HCV", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 405, Code = "HDV", Name = "HDV", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 406, Code = "HEV", Name = "HEV", Units = "", Category = "Вирусологические" },
                new LabParameter { LabParameterId = 407, Code = "COVID19", Name = "SARS-CoV-2 (COVID-19)", Units = "", Category = "Вирусологические" }
            );
     // =============== КОНФИГУРАЦИЯ ДЕТАЛЬНЫХ ТАБЛИЦ  ПЕРВИЧНОГО ОСМОТРА ===============
            modelBuilder.Entity<GeneralData>()
                .HasOne(g => g.PrimaryInspection)
                .WithMany()
                .HasForeignKey(g => g.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SkinData>()
                .HasOne(s => s.PrimaryInspection)
                .WithMany()
                .HasForeignKey(s => s.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RespiratoryData>()
                .HasOne(r => r.PrimaryInspection)
                .WithMany()
                .HasForeignKey(r => r.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CardiologyData>()
                .HasOne(c => c.PrimaryInspection)
                .WithMany()
                .HasForeignKey(c => c.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AbdomenData>()
                .HasOne(a => a.PrimaryInspection)
                .WithMany()
                .HasForeignKey(a => a.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NeuroData>()
                .HasOne(n => n.PrimaryInspection)
                .WithMany()
                .HasForeignKey(n => n.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AnamnesisVitaeData>()
                .HasOne(a => a.PrimaryInspection)
                .WithMany()
                .HasForeignKey(a => a.PrimaryInspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Проверяем уникальность сотрудников (до аудита)
            var modifiedEmployees = ChangeTracker.Entries<Employee>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var emp in modifiedEmployees)
            {
              //  var emp = entry.Entity;                
                if (emp.Position == PositionType.HeadOfDepartment)
                {
                    var existing = await Employees
                        .Where(e => e.DepartmentId == emp.DepartmentId
                                 && e.Position == PositionType.HeadOfDepartment
                                 && e.EmployeeId != emp.EmployeeId   
                                 && !e.IsDeleted)
                       // .FirstOrDefault();
                       .FirstOrDefaultAsync(cancellationToken);

                    if (existing != null)
                        throw new InvalidOperationException("Заведующий отделением уже назначен в этом отделении.");
                }               
                if (emp.Position == PositionType.ChiefDoctor)
                {
                    var existing = Employees
                        .Where(e => e.Position == PositionType.ChiefDoctor
                                 && e.EmployeeId != emp.EmployeeId   
                                 && !e.IsDeleted)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existing != null)
                        throw new InvalidOperationException("Главный врач уже назначен.");

                    if (emp.DepartmentId != null) 
                        throw new InvalidOperationException("Главный врач — глобальная роль, без привязки к отделению.");
                }

                // Зам. главврача — только один глобально
                if (emp.Position == PositionType.DeputyChief)
                {
                    var existing = Employees
                        .Where(e => e.Position == PositionType.DeputyChief
                                 && e.EmployeeId != emp.EmployeeId   // ← изменили
                                 && !e.IsDeleted)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (existing != null)
                        throw new InvalidOperationException("Заместитель главного врача уже назначен.");
                }
            }

            // 2. Аудит для AuditableEntity (без изменений, если CreatedById / UpdatedById — int)
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedById = CurrentUserId(); 
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedById = CurrentUserId();
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        

        private int? CurrentUserId()
        {
            // Логика: возьми из HttpContext.User.Claims (в API контроллере)
            return 1;  // ← Замени на реальную (например, int.Parse(HttpContext.User.FindFirst("Id").Value))
        }
    }
}
