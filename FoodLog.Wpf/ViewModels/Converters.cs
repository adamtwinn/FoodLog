using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Editors;

namespace FoodLog.Wpf.ViewModels
{
    public class DateEditChangedToDateConverter : EventArgsConverterBase<EditValueChangedEventArgs>
    {
        protected override object Convert(object sender, EditValueChangedEventArgs args)
        {
            return (DateTime)args.NewValue;
        }
    }
}
