using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace MainPower.Com0com.Redirector
{
    public static class Com0comSetup
    {

        private static string _com0comSetupc = @"C:\Program Files (x86)\com0com\setupc.exe";
        private static string _com0comSetupg = @"C:\Program Files (x86)\com0com\setupg.exe";

        /// <summary>
        /// Get all com0com Port Pairs currently installed in the system.
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<Com0comPortPair> GetPortPairs()
        {

            ObservableCollection<Com0comPortPair> ports = new ObservableCollection<Com0comPortPair>();
            //get the output from setupc --detail-prms list
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _com0comSetupc,
                    Arguments = "--detail-prms list",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                try
                {
                    string line = proc.StandardOutput.ReadLine();
                    //get port number
                    Regex regex = new Regex(@"(?<=CNC[A,B])\d+(?=\s)");
                    int portnum = int.Parse(regex.Match(line).Value);
                    
                    Com0comPortPair pair = ports.FirstOrDefault(d => d.PairNumber == portnum);

                    if (pair == null)
                    {
                        pair = new Com0comPortPair(portnum);
                        ports.Add(pair);
                    }
                    regex = new Regex(@"(?<=CNC)[A,B](?=\d+\s)");
                    string portAB = regex.Match(line).Value;
                    if (portAB == "A")
                    {
                        pair.PortConfigStringA = line;
                    }
                    else if (portAB == "B")
                    {
                        pair.PortConfigStringB = line;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return ports;
        }

        public static bool CreatePortPair(string portNameA)
        {
            if (UacHelper.IsUacEnabled)
                if (!UacHelper.IsProcessElevated)
                    return false;
            if (!UacHelper.IsAdministrator())
                return false;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_com0comSetupc),
                    FileName = _com0comSetupc,
                    Arguments = portNameA == "-" ? "install - -" : "install PortName=" + portNameA + " -",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Verb = "runas"
                }
            };
            proc.Start();

            //TODO: add a timeout here
            while (!proc.HasExited) { }
            return proc.ExitCode == 0;
        }

        public static void LaunchSetupg()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_com0comSetupc),
                    FileName = _com0comSetupg,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Verb = "runas"
                }
            };
            proc.Start();
        }

        public static bool DeletePortPair(int n)
        {
            if (UacHelper.IsUacEnabled)
                if (!UacHelper.IsProcessElevated)
                    return false;
            if (!UacHelper.IsAdministrator())
                return false;

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(_com0comSetupc),
                    FileName = _com0comSetupc,
                    Arguments = "remove " + n.ToString(),
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    Verb = "runas"
                }
            };
            proc.Start();
            //TODO: add a timeout here
            while (!proc.HasExited) { }
            return proc.ExitCode == 0;
        }

    }
}
