using System.Windows.Input;
using MisWpf.Infrastructure;

namespace MisWpf.ViewModels
{
    /// <summary>Переключатель-кнопка с выбором (профиль анализов, источник поступления, причина отказа и т.п.).</summary>
    public class ChoiceVm : ObservableObject
    {
        private bool _isSelected;
        public string Text { get; }
        public string Description { get; init; } = "";
        public object? Value { get; }
        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public ICommand PickCommand { get; }

        public ChoiceVm(string text, object? value, Action<ChoiceVm> pick)
        {
            Text = text;
            Value = value;
            PickCommand = new RelayCommand(() => pick(this));
        }

        /// <summary>Группа «выбрать один».</summary>
        public static void SelectSingle(IEnumerable<ChoiceVm> all, ChoiceVm picked)
        {
            foreach (var c in all) c.IsSelected = ReferenceEquals(c, picked);
        }
    }

    /// <summary>Маленькая метка с текстом и видом (ok / wait / flag / neutral) — для карточек.</summary>
    public class TagVm
    {
        public string Text { get; init; } = "";
        public string Kind { get; init; } = "neutral";
    }
}
