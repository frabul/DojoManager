using DojoManagerApi.Entities; 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui
{ 
    public class EntitiesViewModelProxy<T> : DispatchProxy, INotifyPropertyChanged

    {
        public T? _decorated { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
                return null;
            var ret = targetMethod.Invoke(_decorated, args);
            if (targetMethod.Name.StartsWith("set_"))
            {
                RaisePropertyChanged(targetMethod.Name.Remove(3));
            }
            return targetMethod.Invoke(_decorated, args);
        }

        public static T Create(T decorated)
        {
            T ob = Create<T, EntitiesViewModelProxy<T>>();
            EntitiesViewModelProxy<T>? proxy = ob as EntitiesViewModelProxy<T>;
            proxy?.SetParameters(decorated);
            return ob;
        }
        private void SetParameters(T decorated)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            _decorated = decorated;
        }
    }
}
