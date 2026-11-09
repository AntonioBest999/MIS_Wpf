using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using MisWpf.Exam;
using MisWpf.Infrastructure;
using MisWpf.Services;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.ViewModels
{
    /// <summary>Рабочий стол врача приёмного отделения.</summary>
    public class ReceptionDoctorViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private readonly Dictionary<int, ExamState> _exams = new();
        private readonly HashSet<int> _opened = new();
        private readonly Dictionary<int, QueueStage> _lastStage = new();
        private List<QueueCardVm> _cards = new();
        private List<(Department Dept, int Patients)> _departments = new();
        private int _secondsToRefresh;
        private bool _reloading;

        private QueueCardVm? _selected;
        private ExamState? _exam;
        private string _clock = "";
        private int _shiftTotal, _shiftHosp, _shiftRefused;

        private string _resultTitle = "";
        private string _resultText = "";
        private bool _showResult;
        private bool _resultWaiting;
        private bool _resultNeedsConclusion;
        private string _instrumentalConclusion = "";
        private Attachment? _resultAttachment;

        public event Action? NewAdmissionRequested;
        public event Action<string, List<(string Title, string Text)>>? PreviewRequested;

        public ObservableCollection<QueueGroupVm> Groups { get; } = new()
        {
            new QueueGroupVm("ОЖИДАЮТ ОСМОТРА", "neutral", QueueStage.Waiting),
            new QueueGroupVm("НА ОСМОТРЕ", "accent", QueueStage.Examining),
            new QueueGroupVm("ЖДУТ РЕЗУЛЬТАТОВ", "warn", QueueStage.WaitingResults),
            new QueueGroupVm("ГОТОВЫ К РЕШЕНИЮ", "ok", QueueStage.ReadyForDecision),
            new QueueGroupVm("ВОЗВРАЩЕНЫ ЗАВЕДУЮЩИМИ", "danger", QueueStage.Returned),
        };

        public ObservableCollection<StepVm> Steps { get; } = new();
        public ObservableCollection<OrderRowVm> Orders { get; } = new();
        public ObservableCollection<ResultRowVm> ResultRows { get; } = new();
        public ObservableCollection<ChoiceVm> LabProfiles { get; } = new();
        public ObservableCollection<ChoiceVm> InstrumentalButtons { get; } = new();
        public ObservableCollection<ChoiceVm> DecisionOptions { get; } = new();
        public ObservableCollection<DeptVm> Departments { get; } = new();
        public ObservableCollection<ChoiceVm> Reasons { get; } = new();

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand AllNormCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand NewAdmissionCommand { get; }
        public ICommand SendLabCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand CloseResultCommand { get; }
        public ICommand MarkInstrumentalDoneCommand { get; }
        public ICommand GoToOrdersCommand { get; }
        public ICommand GoToDecisionCommand { get; }

        public ReceptionDoctorViewModel()
        {
            Steps.Add(new StepVm(1, "Поступление", GoStep));
            Steps.Add(new StepVm(2, "Объективно", GoStep));
            Steps.Add(new StepVm(3, "Исследования", GoStep));
            Steps.Add(new StepVm(4, "Решение", GoStep));

            foreach (var p in OrderService.Profiles)
                LabProfiles.Add(new ChoiceVm(p.Name, p, c => c.IsSelected = !c.IsSelected));
            foreach (var i in OrderService.Instrumental)
                InstrumentalButtons.Add(new ChoiceVm(i.Name, i.Type, c => _ = SendInstrumentalAsync((AttachmentType)c.Value!)));

            DecisionOptions.Add(new ChoiceVm("Госпитализировать", DecisionKind.Hospitalize, PickDecision) { Description = "Направить заведующему отделения" });
            DecisionOptions.Add(new ChoiceVm("Не нуждается", DecisionKind.NotNeeded, PickDecision) { Description = "Отказ, рекомендации пациенту" });
            DecisionOptions.Add(new ChoiceVm("Перевод в другое ЛПУ", DecisionKind.TransferOut, PickDecision) { Description = "Указать учреждение" });
            DecisionOptions.Add(new ChoiceVm("Наблюдение", DecisionKind.Observe, PickDecision) { Description = "Повторный осмотр в приёмном" });

            foreach (var r in new[] { "нет показаний", "самоотказ пациента", "лечение амбулаторно", "нет профильного отделения" })
                Reasons.Add(new ChoiceVm(r, r, PickReason));

            NextCommand = new RelayCommand(() => GoStep(CurrentStep + 1), () => Exam != null && CurrentStep < 4);
            BackCommand = new RelayCommand(() => GoStep(CurrentStep - 1), () => Exam != null && CurrentStep > 1);
            PreviewCommand = new RelayCommand(OpenPreview, () => Exam != null);
            AllNormCommand = new RelayCommand(() => { if (Exam != null) foreach (var s in Exam.Objective) s.SetNormal(); });
            RefreshCommand = new AsyncRelayCommand(ReloadAsync);
            NewAdmissionCommand = new RelayCommand(() => NewAdmissionRequested?.Invoke());
            SendLabCommand = new AsyncRelayCommand(SendLabAsync, () => Selected != null && LabProfiles.Any(p => p.IsSelected));
            SubmitCommand = new AsyncRelayCommand(SubmitAsync, () => Exam != null && Exam.Decision != DecisionKind.None);
            CloseResultCommand = new RelayCommand(() => ShowResult = false);
            MarkInstrumentalDoneCommand = new AsyncRelayCommand(MarkInstrumentalDoneAsync, () => _resultAttachment != null && !string.IsNullOrWhiteSpace(InstrumentalConclusion));
            GoToOrdersCommand = new RelayCommand(() => GoStep(3), () => Exam != null);
            GoToDecisionCommand = new RelayCommand(() => GoStep(4), () => Exam != null);

            _timer.Tick += async (_, _) => await OnTickAsync();
        }

        public async Task InitAsync()
        {
            _departments = await QueueService.LoadDepartmentsAsync();
            await ReloadAsync();
            _timer.Start();
        }

        public void Stop() => _timer.Stop();

        // ================= Верхняя панель =================
        public string Clock { get => _clock; set => Set(ref _clock, value); }
        public int ShiftTotal { get => _shiftTotal; set => Set(ref _shiftTotal, value); }
        public int ShiftHosp { get => _shiftHosp; set => Set(ref _shiftHosp, value); }
        public int ShiftRefused { get => _shiftRefused; set => Set(ref _shiftRefused, value); }
        public int ActiveCount => _cards.Count;
        public string ThresholdsText => $"жёлтый от {AppConfig.WarnMinutes} мин · красный от {AppConfig.CriticalMinutes} мин";

        // ================= Очередь =================
        private async Task OnTickAsync()
        {
            var now = DateTime.UtcNow;
            Clock = DateTime.Now.ToString("HH:mm");
            foreach (var c in _cards) c.Update(now);
            RaiseMany(nameof(HeaderTimer), nameof(HeaderTimerKind));

            if (++_secondsToRefresh >= AppConfig.RefreshSeconds)
            {
                _secondsToRefresh = 0;
                try { await ReloadAsync(); } catch { /* сеть/БД недоступна — попробуем в следующий раз */ }
            }
        }

        public async Task ReloadAsync()
        {
            if (_reloading) return;
            _reloading = true;
            try
            {
                var items = await QueueService.LoadAsync();
                var stats = await QueueService.LoadShiftStatsAsync();
                ShiftTotal = stats.Total; ShiftHosp = stats.Hospitalized; ShiftRefused = stats.Refused;

                int? selectedId = Selected?.AdmissionId;
                var cards = new List<QueueCardVm>();
                foreach (var it in items)
                {
                    var stage = StageOf(it);
                    if (_lastStage.TryGetValue(it.Admission.Id, out var prev) && prev == QueueStage.WaitingResults && stage == QueueStage.ReadyForDecision)
                        Ui.Toast($"{it.Patient.FullName}: все результаты исследований готовы.");
                    _lastStage[it.Admission.Id] = stage;
                    cards.Add(new QueueCardVm(it, stage, c => SelectCard(c)));
                }
                _cards = cards;

                foreach (var g in Groups)
                    g.Replace(cards.Where(c => c.Stage == g.Stage).OrderByDescending(c => c.Minutes(DateTime.UtcNow)));
                Raise(nameof(ActiveCount));

                var again = selectedId == null ? null : cards.FirstOrDefault(c => c.AdmissionId == selectedId);
                if (again != null) SelectCard(again, keepResult: true);
                else if (selectedId != null) ClearSelection();
            }
            finally
            {
                _reloading = false;
            }
        }

        private QueueStage StageOf(QueueItem it)
        {
            if (it.IsReturned) return QueueStage.Returned;
            if (it.HasOrders) return it.AllOrdersDone ? QueueStage.ReadyForDecision : QueueStage.WaitingResults;
            return _opened.Contains(it.Admission.Id) ? QueueStage.Examining : QueueStage.Waiting;
        }

        private void SelectCard(QueueCardVm card, bool keepResult = false)
        {
            bool firstOpen = _opened.Add(card.AdmissionId);
            if (firstOpen && card.Stage == QueueStage.Waiting)
            {
                // Перекладываем из «Ожидают осмотра» в «На осмотре»
                var moved = new QueueCardVm(card.Item, QueueStage.Examining, c => SelectCard(c));
                _cards[_cards.IndexOf(card)] = moved;
                _lastStage[card.AdmissionId] = QueueStage.Examining;
                foreach (var g in Groups)
                    g.Replace(_cards.Where(c => c.Stage == g.Stage).OrderByDescending(c => c.Minutes(DateTime.UtcNow)));
                card = moved;
            }

            foreach (var c in _cards) c.IsSelected = ReferenceEquals(c, card);
            Selected = card;

            if (!_exams.TryGetValue(card.AdmissionId, out var exam))
            {
                exam = new ExamState(card.AdmissionId, card.Item.Admission.Complaint ?? "");
                if (card.IsReturned) exam.Step = 4;
                exam.Anamnesis.Changed += UpdateSteps;
                foreach (var s in exam.Objective) s.Changed += UpdateSteps;
                exam.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ExamState.Complaint)) UpdateSteps();
                    if (e.PropertyName == nameof(ExamState.Decision)) RaiseDecisionFlags();
                };
                _exams[card.AdmissionId] = exam;
            }
            Exam = exam;

            BuildOrders(card.Item);
            BuildDepartments();
            SyncDecisionChoices();
            if (!keepResult) ShowResult = false;
            GoStep(exam.Step);
            RaiseHeader();
        }

        private void ClearSelection()
        {
            Selected = null;
            Exam = null;
            Orders.Clear();
            ShowResult = false;
            RaiseHeader();
        }

        // ================= Выбранный пациент =================
        public QueueCardVm? Selected
        {
            get => _selected;
            private set { if (Set(ref _selected, value)) RaiseMany(nameof(HasSelected), nameof(NoSelection)); }
        }

        public bool HasSelected => Selected != null;
        public bool NoSelection => Selected == null;

        public ExamState? Exam
        {
            get => _exam;
            private set { if (Set(ref _exam, value)) RaiseMany(nameof(ObjectiveSections), nameof(AnamnesisSection)); }
        }

        public List<SectionVm>? ObjectiveSections => Exam?.Objective;
        public SectionVm? AnamnesisSection => Exam?.Anamnesis;

        private Admission? Adm => Selected?.Item.Admission;

        public string PatientFio => Selected?.Fio ?? "";
        public string PatientMeta => Selected == null ? ""
            : $"{Format.Age(Selected.Item.Patient)} · {Format.Sex(Selected.Item.Patient.Gender)} · № {Format.AdmissionNumber(Selected.AdmissionId)} · {Format.SourceLong(Selected.Item.Admission.SourceType).ToLowerInvariant()}";
        public string HeaderTimer => Selected?.Timer ?? "";
        public string HeaderTimerKind => Selected?.TimerKind ?? "normal";
        public string Allergy => Exam?.AllergyNote ?? "";
        public bool NoAllergyInfo => Exam != null && !Exam.Anamnesis.Get("all").IsFilled;
        public string PriorStay => Selected?.Item.PriorStay ?? "";
        public bool IsReturned => Selected?.IsReturned == true;
        public string ReturnTitle => Selected?.Item.RejectedHospitalization == null ? ""
            : "Отказ в приёме: " + (Selected.Item.RejectedHospitalization.Department?.Name ?? "отделение");
        public string ReturnHint => Selected?.Item.RejectedHospitalization == null ? ""
            : $"История болезни № {Selected.Item.RejectedHospitalization.MedicalRecordNumber}. Направьте в другое отделение или оформите отказ — номер истории сохранится.";

        public bool HasArrivalInfo => Adm != null && (!string.IsNullOrWhiteSpace(Adm.DeliveredBy) || !string.IsNullOrWhiteSpace(Adm.DiagnosisOnAdmission)
                                                     || !string.IsNullOrWhiteSpace(Adm.ProvidedCare) || !string.IsNullOrWhiteSpace(Adm.SourceDetails));
        public string ArrivalDeliveredBy => Join(Adm?.DeliveredBy, string.IsNullOrWhiteSpace(Adm?.AccompanyingSheetNumber) ? null : "сопр. лист № " + Adm!.AccompanyingSheetNumber);
        public string ArrivalDetails => Adm?.SourceDetails ?? "";
        public string ArrivalDiagnosis => Adm?.DiagnosisOnAdmission ?? "";
        public string ArrivalCare => Adm?.ProvidedCare ?? "";
        public string ArrivalTime => Adm == null ? "" : "Поступил в " + Format.Local(Adm.AdmissionDateTime);
        public string ArrivalSource => Adm == null ? "" : Format.SourceLong(Adm.SourceType);
        public string ArrivalNumber => Adm == null ? "" : "№ " + Format.AdmissionNumber(Adm.Id);

        private static string Join(string? a, string? b) => string.Join(", ", new[] { a, b }.Where(s => !string.IsNullOrWhiteSpace(s)));

        private void RaiseHeader()
        {
            RaiseMany(nameof(PatientFio), nameof(PatientMeta), nameof(HeaderTimer), nameof(HeaderTimerKind), nameof(Allergy),
                nameof(NoAllergyInfo), nameof(PriorStay), nameof(IsReturned), nameof(ReturnTitle), nameof(ReturnHint),
                nameof(HasArrivalInfo), nameof(ArrivalDeliveredBy), nameof(ArrivalDetails), nameof(ArrivalDiagnosis),
                nameof(ArrivalCare), nameof(ArrivalTime), nameof(ArrivalSource), nameof(ArrivalNumber));
        }

        // ================= Шаги =================
        public int CurrentStep => Exam?.Step ?? 1;
        public bool IsStep1 => Exam != null && CurrentStep == 1;
        public bool IsStep2 => Exam != null && CurrentStep == 2;
        public bool IsStep3 => Exam != null && CurrentStep == 3;
        public bool IsStep4 => Exam != null && CurrentStep == 4;
        public string NextText => CurrentStep == 3 ? "К решению" : "Далее";

        private void GoStep(int step)
        {
            if (Exam == null) return;
            Exam.Step = Math.Clamp(step, 1, 4);
            foreach (var s in Steps) s.IsCurrent = s.Number == Exam.Step;
            UpdateSteps();
            RaiseMany(nameof(CurrentStep), nameof(IsStep1), nameof(IsStep2), nameof(IsStep3), nameof(IsStep4), nameof(NextText));
        }

        private void UpdateSteps()
        {
            if (Exam == null) return;
            Steps[0].IsDone = Exam.Step1Done;
            Steps[1].IsDone = Exam.Step2Done;
            Steps[2].IsDone = Selected?.Item.AllOrdersDone == true;
            Steps[3].IsDone = false;
            RaiseMany(nameof(Allergy), nameof(NoAllergyInfo));
        }

        // ================= Исследования =================
        public bool HasOrders => Orders.Count > 0;
        public bool NoOrders => Orders.Count == 0;

        private void BuildOrders(QueueItem it)
        {
            Orders.Clear();
            bool female = Format.IsFemale(it.Patient.Gender);
            foreach (var o in it.LabOrders)
            {
                bool done = o.Status == LabOrderStatus.Completed;
                bool flag = done && o.Items.Any(i => i.Parameter != null && LabWorkService.Flag(i.Parameter, female, i.Result) != "");
                var cats = string.Join(", ", o.Items.Where(i => i.Parameter != null).Select(i => i.Parameter.Category).Distinct());
                var order = o;
                Orders.Add(new OrderRowVm
                {
                    Name = cats,
                    KindText = "лаборатория",
                    StatusText = done ? (flag ? "есть отклонения" : "готово") : o.Status == LabOrderStatus.InProgress ? "в работе" : "назначено",
                    StatusKind = done ? (flag ? "flag" : "ok") : o.Status == LabOrderStatus.InProgress ? "wait" : "ordered",
                    Lab = o,
                    OpenCommand = new RelayCommand(() => OpenLabResult(order, female))
                });
            }
            foreach (var a in it.Instrumental)
            {
                bool done = OrderService.IsInstrumentalDone(a);
                var att = a;
                Orders.Add(new OrderRowVm
                {
                    Name = OrderService.InstrumentalName(a.DocumentType),
                    KindText = "инструментальное",
                    StatusText = done ? "готово" : "назначено",
                    StatusKind = done ? "ok" : "ordered",
                    Instrumental = a,
                    OpenCommand = new RelayCommand(() => OpenInstrumental(att))
                });
            }
            RaiseMany(nameof(HasOrders), nameof(NoOrders));
        }

        public string ResultTitle { get => _resultTitle; set => Set(ref _resultTitle, value); }
        public string ResultText { get => _resultText; set => Set(ref _resultText, value); }
        public bool ShowResult { get => _showResult; set => Set(ref _showResult, value); }
        public bool ResultWaiting { get => _resultWaiting; set => Set(ref _resultWaiting, value); }
        public bool ResultNeedsConclusion { get => _resultNeedsConclusion; set => Set(ref _resultNeedsConclusion, value); }
        public string InstrumentalConclusion { get => _instrumentalConclusion; set => Set(ref _instrumentalConclusion, value); }

        private void OpenLabResult(LabOrder o, bool female)
        {
            ResultRows.Clear();
            _resultAttachment = null;
            ResultNeedsConclusion = false;
            ResultText = "";
            ResultTitle = "Лаборатория · заказ от " + Format.Local(o.OrderedAt);
            foreach (var i in o.Items.Where(i => i.Parameter != null).OrderBy(i => i.Parameter.Category).ThenBy(i => i.LabParameterId))
            {
                ResultRows.Add(new ResultRowVm
                {
                    Name = $"{i.Parameter.Category} · {i.Parameter.Name}",
                    Value = string.IsNullOrWhiteSpace(i.Result) ? "—" : i.Result + (string.IsNullOrEmpty(i.Parameter.Units) ? "" : " " + i.Parameter.Units),
                    Ref = LabWorkService.RangeText(i.Parameter, female),
                    Flag = LabWorkService.Flag(i.Parameter, female, i.Result)
                });
            }
            ResultWaiting = o.Status != LabOrderStatus.Completed;
            ShowResult = true;
            GoStep(3);
        }

        private void OpenInstrumental(Attachment a)
        {
            ResultRows.Clear();
            ResultTitle = OrderService.InstrumentalName(a.DocumentType) + " · назначено в " + Format.Local(a.UploadedAt);
            bool done = OrderService.IsInstrumentalDone(a);
            ResultWaiting = false;
            if (done)
            {
                _resultAttachment = null;
                ResultNeedsConclusion = false;
                ResultText = !string.IsNullOrWhiteSpace(a.InterpretationText) ? a.InterpretationText!
                    : "Загружен файл: " + a.OriginalFileName + (string.IsNullOrWhiteSpace(a.Description) ? "" : "\n" + a.Description);
            }
            else
            {
                _resultAttachment = a;
                ResultText = "Рабочего места для этого исследования пока нет. Когда исследование выполнено, впишите заключение и отметьте его выполненным.";
                InstrumentalConclusion = "";
                ResultNeedsConclusion = true;
            }
            ShowResult = true;
            GoStep(3);
        }

        private async Task MarkInstrumentalDoneAsync()
        {
            if (_resultAttachment == null) return;
            await OrderService.CompleteInstrumentalAsync(_resultAttachment.Id, InstrumentalConclusion);
            Ui.Toast(OrderService.InstrumentalName(_resultAttachment.DocumentType) + ": заключение сохранено.");
            ShowResult = false;
            await ReloadAsync();
        }

        private async Task SendLabAsync()
        {
            if (Selected == null) return;
            var codes = LabProfiles.Where(p => p.IsSelected).SelectMany(p => ((LabProfile)p.Value!).Codes).Distinct().ToList();
            var names = string.Join(", ", LabProfiles.Where(p => p.IsSelected).Select(p => p.Text));
            await OrderService.SendLabAsync(Selected.Item.Patient.PatientId, codes);
            foreach (var p in LabProfiles) p.IsSelected = false;
            Ui.Toast($"Отправлено в лабораторию: {names}.");
            await ReloadAsync();
        }

        private async Task SendInstrumentalAsync(AttachmentType type)
        {
            if (Selected == null) return;
            var name = OrderService.InstrumentalName(type);
            if (!Ui.Confirm($"Направить пациента на {name}?")) return;
            try
            {
                await OrderService.SendInstrumentalAsync(Selected.Item.Patient.PatientId, Selected.AdmissionId, type);
                Ui.Toast($"{name}: направление отправлено.");
                await ReloadAsync();
            }
            catch (Exception ex) { Ui.ShowError("Не удалось отправить направление", ex); }
        }

        // ================= Решение =================
        public bool IsHospDecision => Exam?.Decision == DecisionKind.Hospitalize;
        public bool IsRefuseDecision => Exam?.Decision == DecisionKind.NotNeeded;
        public bool IsTransferDecision => Exam?.Decision == DecisionKind.TransferOut;
        public bool IsObserveDecision => Exam?.Decision == DecisionKind.Observe;
        public bool HasDecision => Exam != null && Exam.Decision != DecisionKind.None;

        public string PendingWarning
        {
            get
            {
                if (Selected == null || !IsHospDecision) return "";
                var pending = Orders.Where(o => o.StatusKind == "ordered" || o.StatusKind == "wait").Select(o => o.Name).ToList();
                return pending.Count == 0 ? "" : $"Не готовы: {string.Join(", ", pending)}. Можно отправить, но заведующий увидит, что результаты ещё ждут.";
            }
        }

        public string SubmitText => Exam?.Decision switch
        {
            DecisionKind.Hospitalize => "Отправить заведующему",
            DecisionKind.NotNeeded => "Оформить отказ",
            DecisionKind.TransferOut => "Оформить перевод",
            DecisionKind.Observe => "Оставить под наблюдением",
            _ => "Сохранить"
        };

        private void PickDecision(ChoiceVm c)
        {
            if (Exam == null) return;
            Exam.Decision = (DecisionKind)c.Value!;
        }

        private void PickReason(ChoiceVm c)
        {
            if (Exam == null) return;
            ChoiceVm.SelectSingle(Reasons, c);
            Exam.RefusalReason = c.Text;
        }

        private void RaiseDecisionFlags()
        {
            SyncDecisionChoices();
            RaiseMany(nameof(IsHospDecision), nameof(IsRefuseDecision), nameof(IsTransferDecision), nameof(IsObserveDecision),
                nameof(HasDecision), nameof(PendingWarning), nameof(SubmitText));
        }

        private void SyncDecisionChoices()
        {
            foreach (var d in DecisionOptions) d.IsSelected = Exam != null && Equals(d.Value, Exam.Decision);
            foreach (var r in Reasons) r.IsSelected = Exam != null && r.Text == Exam.RefusalReason;
            foreach (var d in Departments) d.IsSelected = Exam != null && Exam.DepartmentId == d.Id;
            RaiseMany(nameof(IsHospDecision), nameof(IsRefuseDecision), nameof(IsTransferDecision), nameof(IsObserveDecision),
                nameof(HasDecision), nameof(PendingWarning), nameof(SubmitText));
        }

        private void BuildDepartments()
        {
            Departments.Clear();
            int? blocked = IsReturned ? Selected?.Item.RejectedHospitalization?.DepartmentId : null;
            foreach (var (d, n) in _departments)
            {
                var id = d.DepartmentId;
                bool isBlocked = blocked == id;
                Departments.Add(new DeptVm
                {
                    Id = id,
                    Name = d.Name + (isBlocked ? " (отказало)" : ""),
                    PatientsText = "в отделении: " + n,
                    IsBlocked = isBlocked,
                    IsSelected = Exam?.DepartmentId == id,
                    PickCommand = new RelayCommand(() =>
                    {
                        if (Exam == null || isBlocked) return;
                        Exam.DepartmentId = id;
                        foreach (var x in Departments) x.IsSelected = x.Id == id;
                    })
                });
            }
        }

        private async Task SubmitAsync()
        {
            if (Exam == null || Selected == null) return;

            if (Exam.Decision == DecisionKind.Observe)
            {
                Ui.Toast($"{Selected.Fio}: оставлен под наблюдением в приёмном отделении.");
                return;
            }
            if (Exam.Decision == DecisionKind.Hospitalize && Exam.DepartmentId == null) { Ui.Info("Выберите отделение."); return; }
            if (Exam.Decision == DecisionKind.TransferOut && string.IsNullOrWhiteSpace(Exam.TransferTo)) { Ui.Info("Укажите учреждение, куда переводится пациент."); return; }
            if (!IsReturned && !Exam.Step2Done &&
                !Ui.Confirm("Не все разделы объективного осмотра заполнены. Всё равно сохранить?")) return;
            if (!Ui.Confirm($"{SubmitText}: {Selected.Fio}?")) return;

            string message = IsReturned && Selected.Item.RejectedHospitalization != null
                ? await InspectionService.ResolveReturnedAsync(Exam, Selected.Item.RejectedHospitalization.Id)
                : await InspectionService.SaveDecisionAsync(Exam);

            _exams.Remove(Selected.AdmissionId);
            ClearSelection();
            Ui.Toast(message);
            await ReloadAsync();
        }

        private void OpenPreview()
        {
            if (Exam == null || Selected == null) return;
            var a = Selected.Item.Admission;
            var smp = string.Join(". ", new[] { a.SourceDetails, string.IsNullOrWhiteSpace(a.DiagnosisOnAdmission) ? null : "Диагноз направившего: " + a.DiagnosisOnAdmission,
                                                 string.IsNullOrWhiteSpace(a.ProvidedCare) ? null : "Оказано: " + a.ProvidedCare }
                                       .Where(s => !string.IsNullOrWhiteSpace(s)));
            var protocol = Exam.BuildProtocol(smp);
            var ready = Orders.Where(o => o.StatusKind == "ok" || o.StatusKind == "flag").Select(o => o.Name + (o.StatusKind == "flag" ? " (отклонения)" : "")).ToList();
            var pending = Orders.Where(o => o.StatusKind == "ordered" || o.StatusKind == "wait").Select(o => o.Name).ToList();
            if (ready.Count + pending.Count > 0)
                protocol.Add(("Исследования", (ready.Count > 0 ? "Выполнены: " + string.Join(", ", ready) + ". " : "") + (pending.Count > 0 ? "Ожидаются: " + string.Join(", ", pending) + "." : "")));
            PreviewRequested?.Invoke($"{Selected.Fio} · {PatientMeta}", protocol);
        }
    }
}
