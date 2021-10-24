using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DojoManagerGui
{
    /// <summary>
    /// Interaction logic for LabeledTextBox.xaml
    /// </summary>
    public partial class LabeledTextBox : UserControl
    {
        public LabeledTextBox()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
           "Key",
           typeof(string),
           typeof(LabeledTextBox),
           new PropertyMetadata("test", KeyPropertyChanged)
       );

        private static void KeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (LabeledTextBox)d;
            me.theLabel.Text = e.NewValue as string;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(string),
            typeof(LabeledTextBox),
            new FrameworkPropertyMetadata
            {
                DefaultValue = "TestVal",
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                PropertyChangedCallback = ValuePropertyChanged
            }
        );

        private static void ValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (LabeledTextBox)d;
            me.theTextBox.Text = e.NewValue as string;
        }

 
        public string Key
        {
            get
            {
                return (string)GetValue(KeyProperty);
            }
            set
            {
                SetValue(KeyProperty, value);
            }
        }

        public object Value
        {
            get
            {
                return GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }
    }
}
