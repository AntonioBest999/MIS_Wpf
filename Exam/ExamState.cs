using MisWpf.Infrastructure;

namespace MisWpf.Exam
{
    public enum DecisionKind { None, Hospitalize, NotNeeded, TransferOut, Observe }

    /// <summary>
    /// Всё, что врач заполнил по одному поступлению. Живёт в памяти, пока не принято решение
    /// (тогда сохраняется в БД через InspectionService).
    /// </summary>
    public class ExamState : ObservableObject
    {
        private string _complaint = "";
        private string _anamnesisMorbi = "";
        private string _careInReception = "";
        private DecisionKind _decision = DecisionKind.None;
        private int? _departmentId;
        private string _diagnosis = "";
        private string _refusalReason = "";
        private string _recommendations = "";
        private string _transferTo = "";

        public int AdmissionId { get; }
        public int Step { get; set; } = 1;

        public SectionVm Anamnesis { get; } = new(ExamCatalog.Anamnesis, collapsible: false);
        public List<SectionVm> Objective { get; } = new();

        public ExamState(int admissionId, string complaintFromAdmission)
        {
            AdmissionId = admissionId;
            _complaint = complaintFromAdmission ?? "";
            foreach (var def in ExamCatalog.Objective)
                Objective.Add(new SectionVm(def, collapsible: true, open: def.Id == "general"));
        }

        public string Complaint { get => _complaint; set => Set(ref _complaint, value); }
        public string AnamnesisMorbi { get => _anamnesisMorbi; set => Set(ref _anamnesisMorbi, value); }
        public string CareInReception { get => _careInReception; set => Set(ref _careInReception, value); }

        public DecisionKind Decision { get => _decision; set => Set(ref _decision, value); }
        public int? DepartmentId { get => _departmentId; set => Set(ref _departmentId, value); }
        public string Diagnosis { get => _diagnosis; set => Set(ref _diagnosis, value); }
        public string RefusalReason { get => _refusalReason; set => Set(ref _refusalReason, value); }
        public string Recommendations { get => _recommendations; set => Set(ref _recommendations, value); }
        public string TransferTo { get => _transferTo; set => Set(ref _transferTo, value); }

        public SectionVm Section(string id) => Objective.First(s => s.Id == id);

        public bool Step1Done => !string.IsNullOrWhiteSpace(Complaint) && Anamnesis.Filled > 0;
        public bool Step2Done => Objective.All(s => s.Filled > 0);

        /// <summary>Аллергия, отмеченная в анамнезе жизни.</summary>
        public string AllergyNote
        {
            get
            {
                var a = Anamnesis.Get("all");
                return a.SelectedIndex == 1 ? (string.IsNullOrWhiteSpace(a.DetailText) ? "отягощён" : a.DetailText.Trim()) : "";
            }
        }

        public bool HasFever
        {
            get
            {
                var t = Section("general").Get("temp").NumericValue;
                return t.HasValue && t.Value > 37.0;
            }
        }

        public List<(string Title, string Text)> BuildProtocol(string smpText)
        {
            var list = new List<(string, string)>();
            if (!string.IsNullOrWhiteSpace(smpText)) list.Add(("Со слов бригады СМП", smpText));
            list.Add(("Жалобы", Empty(Complaint)));
            list.Add(("Анамнез заболевания", Empty(AnamnesisMorbi)));
            list.Add(("Анамнез жизни", Anamnesis.Filled > 0 ? Anamnesis.Summary : "не собран"));
            foreach (var s in Objective) list.Add((s.Title, s.Filled > 0 ? s.Summary : "не заполнено"));
            if (!string.IsNullOrWhiteSpace(CareInReception)) list.Add(("Помощь в приёмном отделении", CareInReception.Trim()));
            if (!string.IsNullOrWhiteSpace(Diagnosis)) list.Add(("Диагноз", Diagnosis.Trim()));
            return list;
        }

        private static string Empty(string s) => string.IsNullOrWhiteSpace(s) ? "—" : s.Trim();
    }
}
