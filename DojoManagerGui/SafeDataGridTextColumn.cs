using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace DojoManagerGui
{
    public class SafeDataGridTextColumn : DataGridTextColumn
    {
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            cell.SetCurrentValue(UIElement.IsEnabledProperty, HasSetter(dataItem));
            return base.GenerateEditingElement(cell, dataItem);
        }

        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            cell.SetCurrentValue(UIElement.IsEnabledProperty, HasSetter(dataItem));
     
            return base.GenerateElement(cell, dataItem);
        }

        private bool HasSetter(object dataItem)
        {
            if (dataItem == null || Binding == null)
                return false;

            var propName = (Binding as Binding)?.Path.Path;
            if (propName == null)
                return false;

            var prop = dataItem.GetType().GetProperty(propName);
            return prop?.CanWrite ?? false;
        }
    }
}
