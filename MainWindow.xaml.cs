using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MainPower.Com0com.Redirector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Com0comPortPair> PortPairs { get; set; }

        public MainWindow()
        {
            
            PortPairs = Com0comSetup.GetPortPairs();
            InitializeComponent();
            cboCommsMode.ItemsSource = Enum.GetValues(typeof(CommsMode));
        }

        private void RefreshPortPairs()
        {
            ObservableCollection<Com0comPortPair> newpairs = Com0comSetup.GetPortPairs();

            //first we need to delete any ports that don't appear in the new list
            foreach (var expair in PortPairs.ToList())
            {
                var newpair = (from p in newpairs where p.PairNumber == expair.PairNumber select p).FirstOrDefault();
                if (newpair == null)
                {
                    expair.StopComms();
                    PortPairs.Remove(expair);
                }
            }

            //next we need to add any new pairs
            foreach (var newpair in newpairs)
            {
                var expair = (from p in PortPairs where p.PairNumber == newpair.PairNumber select p).FirstOrDefault();
                if (expair == null)
                {
                    PortPairs.Add(newpair);
                }
            }
        }

        private bool AllPortsCommsIdle()
        {
            foreach (var v in PortPairs)
            {
                if (v.CommsStatus == CommsStatus.Running)
                    return false;
            }
            return true;
        }

        #region UI Events

        private void mnuLaunchSetupg_Click(object sender, RoutedEventArgs e)
        {
            Com0comSetup.LaunchSetupg();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mnuRemovePair_Click(object sender, RoutedEventArgs e)
        {
            Com0comPortPair p;
            if ((p = listPorts.SelectedValue as Com0comPortPair) != null)
            {
                if (p.CommsStatus != CommsStatus.Idle)
                {
                    MessageBox.Show("Please stop the comms on this port first");
                    return;
                }
                if (Com0comSetup.DeletePortPair(p.PairNumber))
                {
                    RefreshPortPairs();
                }
                else
                {
                    MessageBox.Show("Failed to remove pair - do you have admin?");
                }
            }
        }

        private void mnuRefreshPairs_Click(object sender, RoutedEventArgs e)
        {
            RefreshPortPairs();
        }

        private void mnuAddPair_Click(object sender, RoutedEventArgs e)
        {
            PortConfigWindow w = new PortConfigWindow();
            if (w.ShowDialog() ?? false)
            {
                if (Com0comSetup.CreatePortPair(w.Result.PortA))
                {
                    RefreshPortPairs();
                }
                else
                {
                    MessageBox.Show("Failed to create pair - do you have admin?");
                }
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            Com0comPortPair port = listPorts.SelectedValue as Com0comPortPair;
            if (port != null)
            {
                port.StopComms();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (var pair in PortPairs)
            {
                pair.StopComms();
            }
        }

        private void btnPortSelect_Click(object sender, RoutedEventArgs e)
        {
            PortsDBSelect win = new PortsDBSelect();
            win.ShowDialog();
            if (win.Result != null)
            {
                Com0comPortPair p;
                if ((p = listPorts.SelectedValue as Com0comPortPair) != null)
                {
                    p.CommsMode = win.Result.Mode;
                    p.LocalPort = win.Result.LocalPort;
                    p.RemoteIP = win.Result.RemoteIP;
                    p.RemotePort = win.Result.RemotePort;
                }
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Com0comPortPair port = listPorts.SelectedValue as Com0comPortPair;
            if (port != null)
            {
                port.StartComms();
            }
        }

        #endregion

    }
}
