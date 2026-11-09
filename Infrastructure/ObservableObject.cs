using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MisWpf.Infrastructure
{
    /// <summary>Базовый класс для ViewModel: уведомляет интерфейс об изменении свойств.</summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            Raise(name);
            return true;
        }

        protected void Raise([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected void RaiseMany(params string[] names)
        {
            foreach (var n in names) Raise(n);
        }
    }
}
