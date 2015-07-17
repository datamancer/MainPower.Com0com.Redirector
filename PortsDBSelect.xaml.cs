using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace MainPower.Com0com.Redirector
{
    public class PortData
    {
        public string Name { get; set; }
        public CommsMode Mode { get; set; }
        public string RemoteIP { get; set; }
        public string RemotePort { get; set; }
        public string LocalPort { get; set; }
    }

    /// <summary>
    /// Interaction logic for PortsDBSelect.xaml
    /// </summary>
    public partial class PortsDBSelect : Window
    {
        public ObservableCollection<PortData> PortsDatabase { get; set; }

        public PortData Result { get; set; }

        public PortsDBSelect()
        {
            PortsDatabase = new ObservableCollection<PortData>();
            ReadPorts();
            InitializeComponent();
            
        }

        private void ReadPorts()
        {
            var s = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly().GetName().CodeBase) + @"\portsdb.txt").LocalPath; ;
            var reader = new StreamReader(File.OpenRead(s));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                if (values.Length != 5)
                    continue;

                PortData p = new PortData();
                p.Name = values[0];
                CommsMode m;
                Enum.TryParse<CommsMode>(values[1], out m);
                p.Mode = m;
                p.RemoteIP = values[2];
                p.RemotePort = values[3];
                p.LocalPort = values[4];

                PortsDatabase.Add(p);
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Result = lstPorts.SelectedValue as PortData;
            this.Close();
        }
    }
}
