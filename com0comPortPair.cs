using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainPower.Com0com.Redirector
{
    public enum CommsMode
    {
        TCPClient,
        UDP,
        RFC2217,
    }

    public enum CommsStatus
    {
        Running, 
        Idle,
    }

    public class Com0comPortPair : INotifyPropertyChanged
    {
        #region Fields
        private string _portConfigStringA = "";
        private string _portConfigStringB = "";
        private Process _p;
        private CommsStatus _commsStatus = CommsStatus.Idle;
        private CommsMode _commsMode = CommsMode.RFC2217;
        private string _outputData = "";
        private string _remoteIP = "";
        private string _remotePort = "";
        private string _localPort = "";
        
        #endregion

        #region Properties

        public int PairNumber { get; private set; }
        public string PortNameA { get; private set; }
        public string PortNameB { get; private set; }

        public string RemotePort
        {
            get
            {
                return _remotePort;
            }
            set
            {
                _remotePort = value;
                OnPropertyChanged("RemotePort");
            }
        }

        public string RemoteIP
        {
            get
            {
                return _remoteIP;
            }
            set
            {
                _remoteIP = value;
                OnPropertyChanged("RemoteIP");
            }
        }

        public string LocalPort
        {
            get
            {
                return _localPort;
            }
            set
            {
                _localPort = value;
                OnPropertyChanged("LocalPort");
            }
        }
        public string OutputData
        {
            get { return _outputData; }
            private set 
            {
                _outputData = value;
                OnPropertyChanged("OutputData");
            }
        }
        public CommsMode CommsMode
        {
            get { return _commsMode; }
            set
            {
                _commsMode = value;
                OnPropertyChanged("CommsMode");
            }
        }
        public CommsStatus CommsStatus
        {
            get { return _commsStatus; }
            set
            {
                _commsStatus = value;
                OnPropertyChanged("CommsStatus");
            }
        }

        public string PortConfigStringA
        {
            get { return _portConfigStringA; }
            set
            {
                Regex regex = new Regex(@"(?<=PortName=)\w+(?=,)");
                _portConfigStringA = value;
                PortNameA = regex.Match(value).Value;
                
                OnPropertyChanged("PortNameA");
                OnPropertyChanged("PortConfigStringA");
                
            }
        }
        public string PortConfigStringB 
        {
            get { return _portConfigStringB; }
            set
            {
                Regex regex = new Regex(@"(?<=PortName=)\w+(?=,)");
                _portConfigStringB = value;
                PortNameB = regex.Match(value).Value;
                
                OnPropertyChanged("PortNameB");
                OnPropertyChanged("PortConfigStringB");
                
            }
        }

        #endregion

        public Com0comPortPair(int number)
        {
            RemoteIP = "192.168.7.1";
            RemotePort = "8882";
            LocalPort = "8883";
            PairNumber = number;
        }

        #region Static Functions
      
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
              ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch
            {
                //we might get exceptions here, as parent might auto exit once their children are terminated
            }
        }

        #endregion

        public void StartComms()
        {
            if (CommsStatus == CommsStatus.Running)
                return;
            string program = "";
            string arguments = "";

            switch (CommsMode)
            {
                case CommsMode.RFC2217:
                    program = "com2tcp-rfc2217.bat";
                    arguments = string.Format("\\\\.\\{0} {1} {2}", PortNameB, RemoteIP, RemotePort);
                    break;
                case CommsMode.TCPClient:
                    program = "com2tcp.exe";
                    arguments = string.Format("\\\\.\\{0} {1} {2}", PortNameB, RemoteIP, RemotePort);
                    break;
                case CommsMode.UDP:
                    program = "com2tcp.exe";
                    arguments = string.Format("--udp \\\\.\\{0} {1} {2} {3}", PortNameB, RemoteIP, RemotePort, LocalPort);
                    break;

            }

            _p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\com0com\" + program,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            _p.EnableRaisingEvents = true;
            _p.Exited += _p_Exited;
            _p.OutputDataReceived += _p_OutputDataReceived;
            _p.ErrorDataReceived += _p_ErrorDataReceived;

            OutputData = "";
            _p.Start();
            _p.BeginOutputReadLine();
            _p.BeginErrorReadLine();
            
            CommsStatus = CommsStatus.Running;
        }

        public void StopComms()
        {
            if (_p == null)
            {
                CommsStatus = CommsStatus.Idle;
                return;
            }
            if (_p.HasExited)
                return;
            KillProcessAndChildren(_p.Id);
        }

        private void _p_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputData += e.Data + Environment.NewLine;
        }

        private void _p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OutputData += e.Data + Environment.NewLine;
        }

        private void _p_Exited(object sender, EventArgs e)
        {
            CommsStatus = CommsStatus.Idle;
        }

        #region INotifyPropertyChangedMembers
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                handler(this, e);
            }
        }

        public void VerifyPropertyName(string propertyName)
        {
            //Verify that the property name matches a real,
            //public instance property on this object
            //an empty property name is ok, used to refresh all properties
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail( "Invalid property name: " + propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        
    }
}
