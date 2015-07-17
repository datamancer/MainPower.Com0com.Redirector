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
using System.Windows.Shapes;

namespace MainPower.Com0com.Redirector
{
    /// <summary>
    /// Interaction logic for PortConfig.xaml
    /// </summary>
    public partial class PortConfigWindow : Window
    {
        public PortConfig Result { get; private set; }

        public PortConfigWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = new PortConfig();
            Result.PortA = IsComAValid() ? txtPortAName.Text : "-";
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void txtPortAName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsComAValid())
            {
                txtPortAName.Background = SystemColors.WindowBrush;
            }
            else
            {
                txtPortAName.Background = Brushes.Red;
            }
        }

        private bool IsComAValid()
        {
            if (txtPortAName.Text.StartsWith("COM", StringComparison.CurrentCultureIgnoreCase))
            {
                int i;
                if (int.TryParse(txtPortAName.Text.Substring(3, txtPortAName.Text.Length - 3), out i))
                {
                    return true;

                }
            }
            return false;
        }
    }
}
