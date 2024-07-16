using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Net.ScottyDoesKnow.DsBackup.Views
{
    // http://www.thomaslevesque.com/2011/03/21/wpf-how-to-bind-to-data-when-the-datacontext-is-not-inherited/
    public class BindingProxy<T> : Freezable
    {
        public T Data
        {
            get => (T)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(T), typeof(BindingProxy<T>), new UIPropertyMetadata(null));

        protected override Freezable CreateInstanceCore() => new BindingProxy<T>();
    }
}
