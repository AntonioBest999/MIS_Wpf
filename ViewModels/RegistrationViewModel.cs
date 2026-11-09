using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using MisWpf.Infrastructure;
using MisWpf.Services;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.ViewModels
{
    public class PatientRowVm
    {
        public Patient Patient { get; init; } = null!;
        public string Fio => Patient.FullName;
        public string Info => $"{(Patient.BirthDate == DateOnly.MinValue ? "дата рождения не указана" : Patient.BirthDate.ToString("dd.MM.yyyy"))} · {Format.Age(Patient)} · {Format.Sex(Patient.Gender)}"
                              + (string.IsNullOrWhiteSpace(Patient.PhoneNumber) ? "" : " · " + Patient.PhoneNumber);
    }

    public class ShiftAdmissionVm
    {
        public string Time { get; init; } = "";
        public string Fio { get; init; } = "";
        public string Number { get; init; } = "";
        public string Status { get; init; } = "";
    }

    /// <summary>Оформление поступления: поиск или регистрация пациента, данные поступления, первичные назначения.</summary>
    public class RegistrationViewModel : ObservableObject
    {
        private readonly DispatcherTimer _searchDelay = new() { Interval = TimeSpan.FromMilliseconds(400) };

        private string _searchText = "";
        private PatientRowVm? _selectedPatient;
        private bool _isNewPatient;
        private bool _isUnknown;
        private string _fullName = "";
        private DateTime? _birthDate;
        private string _gender = "";
        private string _phone = "";
        private string _address = "";
        private string _passport = "";
        private string _patientStatus = "Работник";
        private string _workPlace = "";
        private string _approxAge = "";
        private string _contactName = "";
        private string _contactPhone = "";
        private string _deliveredBy = "";
        private string _sourceDetails = "";
        private string _sheetNumber = "";
        private string _complaint = "";
        private string _smpDiagnosis = "";
        private string _providedCare = "";
        private string _searchInfo = "Введите минимум 2 символа: ФИО, телефон, паспорт или дату рождения (дд.мм.гггг).";

        public event Action<int>? Registered;

        public ObservableCollection<PatientRowVm> SearchResults { get; } = new();
        public ObservableCollection<ChoiceVm> Sources { get; } = new();
        public ObservableCollection<ChoiceVm> LabProfiles { get; } = new();
        public ObservableCollection<ChoiceVm> InstrumentalChoices { get; } = new();
        public ObservableCollection<ShiftAdmissionVm> ShiftAdmissions { get; } = new();

        public List<string> Genders { get; } = new() { "Мужской", "Женский" };
        public List<string> PatientStatuses { get; } = new() { "Работник", "Безработный", "Пенсионер", "Студент", "Учащийся", "Инвалид", "Ребёнок" };

        public ICommand NewPatientCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand SearchCommand { get; }

        public RegistrationViewModel()
        {
            _searchDelay.Tick += async (_, _) => { _searchDelay.Stop(); await SearchAsync(); };

            Sources.Add(new ChoiceVm("Доставлен СМП", AdmissionSourceType.SMP, PickSource) { IsSelected = true });
            Sources.Add(new ChoiceVm("Самообращение", AdmissionSourceType.SelfAppeal, PickSource));
            Sources.Add(new ChoiceVm("Переведён из ЛПУ", AdmissionSourceType.Transfer, PickSource));
            Sources.Add(new ChoiceVm("Другое", AdmissionSourceType.Other, PickSource));

            foreach (var p in OrderService.Profiles)
                LabProfiles.Add(new ChoiceVm(p.Name, p, c => c.IsSelected = !c.IsSelected));
            foreach (var i in OrderService.Instrumental.Take(2)) // на этапе поступления — ЭКГ и рентген
                InstrumentalChoices.Add(new ChoiceVm(i.Name, i.Type, c => c.IsSelected = !c.IsSelected));

            NewPatientCommand = new RelayCommand(StartNewPatient);
            SelectPatientCommand = new RelayCommand(p => { if (p is PatientRowVm row) SelectedPatient = row; });
            SaveCommand = new AsyncRelayCommand(SaveAsync);
            ClearCommand = new RelayCommand(Reset);
            SearchCommand = new AsyncRelayCommand(SearchAsync);
        }

        public async Task InitAsync() => await LoadShiftAsync();

        // ---------- Поиск ----------
        public string SearchText
        {
            get => _searchText;
            set { if (Set(ref _searchText, value)) { _searchDelay.Stop(); _searchDelay.Start(); } }
        }

        public string SearchInfo { get => _searchInfo; set => Set(ref _searchInfo, value); }

        private async Task SearchAsync()
        {
            var text = SearchText.Trim();
            SearchResults.Clear();
            if (text.Length < 2) { SearchInfo = "Введите минимум 2 символа."; return; }
            var found = await RegistrationService.SearchAsync(text);
            foreach (var p in found) SearchResults.Add(new PatientRowVm { Patient = p });
            SearchInfo = found.Count == 0 ? "Не найдено. Зарегистрируйте нового пациента." : $"Найдено: {found.Count}";
        }

        public PatientRowVm? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (!Set(ref _selectedPatient, value)) return;
                if (value != null) IsNewPatient = false;
                RaiseMany(nameof(HasPatient), nameof(PatientCaption));
            }
        }

        private void StartNewPatient()
        {
            SelectedPatient = null;
            IsNewPatient = true;
            if (!string.IsNullOrWhiteSpace(SearchText) && !char.IsDigit(SearchText.Trim()[0]) && string.IsNullOrWhiteSpace(FullName))
                FullName = SearchText.Trim();
        }

        public bool IsNewPatient
        {
            get => _isNewPatient;
            set { if (Set(ref _isNewPatient, value)) RaiseMany(nameof(HasPatient), nameof(PatientCaption)); }
        }

        public bool HasPatient => SelectedPatient != null || IsNewPatient;

        public string PatientCaption => SelectedPatient != null
            ? $"Пациент: {SelectedPatient.Fio}"
            : IsNewPatient ? "Новый пациент" : "Пациент не выбран";

        // ---------- Новый пациент ----------
        public bool IsUnknown { get => _isUnknown; set => Set(ref _isUnknown, value); }
        public string FullName { get => _fullName; set => Set(ref _fullName, value); }
        public DateTime? BirthDate { get => _birthDate; set => Set(ref _birthDate, value); }
        public string Gender { get => _gender; set => Set(ref _gender, value); }
        public string Phone { get => _phone; set => Set(ref _phone, value); }
        public string Address { get => _address; set => Set(ref _address, value); }
        public string Passport { get => _passport; set => Set(ref _passport, value); }
        public string PatientStatus { get => _patientStatus; set => Set(ref _patientStatus, value); }
        public string WorkPlace { get => _workPlace; set => Set(ref _workPlace, value); }
        public string ApproxAge { get => _approxAge; set => Set(ref _approxAge, value); }
        public string ContactName { get => _contactName; set => Set(ref _contactName, value); }
        public string ContactPhone { get => _contactPhone; set => Set(ref _contactPhone, value); }

        // ---------- Поступление ----------
        private void PickSource(ChoiceVm c) { ChoiceVm.SelectSingle(Sources, c); Raise(nameof(IsSmp)); }
        public bool IsSmp => Sources.FirstOrDefault(s => s.IsSelected)?.Value is AdmissionSourceType t && t == AdmissionSourceType.SMP;

        public string DeliveredBy { get => _deliveredBy; set => Set(ref _deliveredBy, value); }
        public string SourceDetails { get => _sourceDetails; set => Set(ref _sourceDetails, value); }
        public string SheetNumber { get => _sheetNumber; set => Set(ref _sheetNumber, value); }
        public string Complaint { get => _complaint; set => Set(ref _complaint, value); }
        public string SmpDiagnosis { get => _smpDiagnosis; set => Set(ref _smpDiagnosis, value); }
        public string ProvidedCare { get => _providedCare; set => Set(ref _providedCare, value); }

        // ---------- Сохранение ----------
        private async Task SaveAsync()
        {
            if (!HasPatient) { Ui.Info("Найдите пациента в базе или нажмите «Новый пациент»."); return; }

            Patient? newPatient = null;
            if (IsNewPatient)
            {
                if (IsUnknown)
                {
                    newPatient = new Patient
                    {
                        IsUnknown = true,
                        FullName = "Неизвестный пациент " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                        ApproximateAge = string.IsNullOrWhiteSpace(ApproxAge) ? null : ApproxAge.Trim(),
                        Gender = Gender ?? "",
                        BirthDate = DateOnly.MinValue
                    };
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(FullName)) { Ui.Info("Укажите ФИО пациента."); return; }
                    if (BirthDate == null) { Ui.Info("Укажите дату рождения."); return; }
                    if (string.IsNullOrWhiteSpace(Gender)) { Ui.Info("Укажите пол."); return; }
                    newPatient = new Patient
                    {
                        FullName = FullName.Trim(),
                        BirthDate = DateOnly.FromDateTime(BirthDate.Value),
                        Gender = Gender,
                        PhoneNumber = Empty(Phone),
                        Address = Empty(Address),
                        PassportNumber = Empty(Passport),
                        PatientStatus = string.IsNullOrWhiteSpace(PatientStatus) ? "Работник" : PatientStatus,
                        WorkPlace = Empty(WorkPlace)
                    };
                }
            }

            var source = Sources.First(s => s.IsSelected).Value is AdmissionSourceType st ? st : AdmissionSourceType.Other;
            var labCodes = LabProfiles.Where(p => p.IsSelected).SelectMany(p => ((LabProfile)p.Value!).Codes).Distinct().ToList();
            var inst = InstrumentalChoices.Where(i => i.IsSelected).Select(i => (AttachmentType)i.Value!).ToList();

            var request = new RegistrationRequest
            {
                ExistingPatient = SelectedPatient?.Patient,
                NewPatient = newPatient,
                Contact = string.IsNullOrWhiteSpace(ContactName) ? null
                    : new PatientContact { FullName = ContactName.Trim(), PhoneNumber = ContactPhone?.Trim() ?? "", Relationship = null },
                Source = source,
                DeliveredBy = DeliveredBy,
                SourceDetails = SourceDetails,
                SheetNumber = SheetNumber,
                Complaint = Complaint,
                SmpDiagnosis = SmpDiagnosis,
                ProvidedCare = ProvidedCare,
                LabCodes = labCodes,
                Instrumental = inst
            };

            int id = await RegistrationService.RegisterAsync(request);
            string fio = SelectedPatient?.Fio ?? newPatient?.FullName ?? "";
            string orders = (labCodes.Count > 0 ? " Анализы отправлены в лабораторию." : "")
                          + (inst.Count > 0 ? " Направления: " + string.Join(", ", inst.Select(OrderService.InstrumentalName)) + "." : "");
            Ui.Toast($"Поступление {Format.AdmissionNumber(id)} оформлено: {fio}.{orders}");
            Registered?.Invoke(id);
            Reset();
            await LoadShiftAsync();
        }

        private async Task LoadShiftAsync()
        {
            var list = await RegistrationService.LoadShiftAdmissionsAsync();
            ShiftAdmissions.Clear();
            foreach (var a in list)
                ShiftAdmissions.Add(new ShiftAdmissionVm { Time = Format.Local(a.Time), Fio = a.Fio, Number = Format.AdmissionNumber(a.Id), Status = a.Status });
        }

        private void Reset()
        {
            SearchText = "";
            SearchResults.Clear();
            SelectedPatient = null;
            IsNewPatient = false;
            IsUnknown = false;
            FullName = ""; BirthDate = null; Gender = ""; Phone = ""; Address = ""; Passport = "";
            PatientStatus = "Работник"; WorkPlace = ""; ApproxAge = ""; ContactName = ""; ContactPhone = "";
            DeliveredBy = ""; SourceDetails = ""; SheetNumber = ""; Complaint = ""; SmpDiagnosis = ""; ProvidedCare = "";
            PickSource(Sources[0]);
            foreach (var p in LabProfiles) p.IsSelected = false;
            foreach (var i in InstrumentalChoices) i.IsSelected = false;
            _searchDelay.Stop();
        }

        private static string? Empty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
