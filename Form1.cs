using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NAudio.Wave;
using System.Media;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Security.Principal;
using System.Management;

namespace SirTool
{
    public partial class Form1 : Form
    {
        int n = 4000; // number of x-axis pints
        //Stopwatch time = new Stopwatch();
        WaveIn wi;
        Queue<double> myQ;
        public Form1()
        {
            InitializeComponent();

            uint CurrVol = 0;
            waveOutGetVolume(IntPtr.Zero, out CurrVol);
            ushort CalcVol = (ushort)(CurrVol & 0x0000ffff);
            guna2TrackBar1.Value = CalcVol / (ushort.MaxValue / 10);

            myQ = new Queue<double>(Enumerable.Repeat(0.0, n).ToList()); // fill myQ w/ zeros
            chart1.ChartAreas[0].AxisY.Minimum = -10000;
            chart1.ChartAreas[0].AxisY.Maximum = 10000;
        }
        FilterInfoCollection Cihazlar;
        VideoCaptureDevice kameram;

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume
        (IntPtr hwo, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume
        (IntPtr hwo, uint dwVolume);

        private void label1_MouseMove(object sender, MouseEventArgs e)
        {
            label1.BackColor = Color.Gray;
        }

        private void label1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label1_MouseLeave(object sender, EventArgs e)
        {
            label1.BackColor = Color.Transparent;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label3_MouseMove(object sender, MouseEventArgs e)
        {
            label3.BackColor = Color.Gray;
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            label3.BackColor = Color.Transparent;
        }

        public static string GetCPUName() // Getting name of CPU
        {
            try
            {
                string CPU = string.Empty;
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject mObject in mSearcher.Get())
                {
                    CPU = mObject["Name"].ToString();
                }
                return CPU;
            }
            catch (Exception)
            {
                return "Error";
            }
        }
        public static string GetRAM() // Getting RAM
        {
            /*try
            {
                int RamAmount = 0;
                using (ManagementObjectSearcher MOS = new ManagementObjectSearcher("Select * From Win32_ComputerSystem"))
                {
                    foreach (ManagementObject MO in MOS.Get())
                    {
                        double Bytes = Convert.ToDouble(MO["TotalPhysicalMemory"]);
                        RamAmount = (int)(Bytes / 1048576) - 1;
                        break;
                    }
                }
                return RamAmount.ToString() + " MB";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error";
            }*/
            // UP MB / DOWN GB

            string ramSizeInfo = null;
            ManagementObjectSearcher ramSearcher = new ManagementObjectSearcher("Select * From Win32_ComputerSystem");

            foreach (ManagementObject mObject in ramSearcher.Get())
            {
                double Ram_Bytes = (Convert.ToDouble(mObject["TotalPhysicalMemory"]));
                double ramgb = Ram_Bytes / 1073741824;
                double ramSize = Math.Ceiling(ramgb);
                ramSizeInfo = ramSize.ToString() + " GB";
            }
            return ramSizeInfo;
        }
        public static string GetGpuName() // Getting GPU Name
        {
            try
            {
                ManagementObjectSearcher mSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
                foreach (ManagementObject mObject in mSearcher.Get())
                    return mObject["Name"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "Unknown";
        }
        public static string GetSystemVersion() // Getting Windows version 
        {
            return GetWindowsVersionName() + " " + GetBitVersion();
        }
        public static string GetWindowsVersionName()// Version Windows
        {
            string sData = "Unknown System";
            try
            {
                using (ManagementObjectSearcher mSearcher = new ManagementObjectSearcher(@"root\CIMV2", " SELECT * FROM win32_operatingsystem"))
                {
                    foreach (ManagementObject tObj in mSearcher.Get())
                        sData = Convert.ToString(tObj["Name"]);
                    sData = sData.Split(new char[] { '|' })[0];
                    int iLen = sData.Split(new char[] { ' ' })[0].Length;
                    sData = sData.Substring(iLen).TrimStart().TrimEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return sData;
        }
        private static string GetBitVersion() // Getting bits
        {
            try
            {
                if (Registry.LocalMachine.OpenSubKey(@"HARDWARE\Description\System\CentralProcessor\0")
                    .GetValue("Identifier")
                    .ToString()
                    .Contains("x86"))
                    return "(32 Bit)";
                else
                    return "(64 Bit)";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return "(Unknown)";
        }
        public void GetDisk()
        {
            /*long totalSize = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                totalSize += drive.TotalSize / 1024 / 1024 / 1024;
            }
            label17.Text = "Disk: " + totalSize.ToString() + " GB";*/

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

            foreach (ManagementObject disk in searcher.Get())
            {
                label17.Text = "Disk Model: " + disk["Model"];
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //system info
            label13.Text = "CPU: " + GetCPUName();
            label14.Text = "RAM: " + GetRAM();
            label15.Text = "GPU: " + GetGpuName();
            label16.Text = "OS: " + GetSystemVersion();
            GetDisk();
            //systeminfo
            Cihazlar = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo cihaz in Cihazlar)
            {
                cmbKamera.Items.Add(cihaz.Name);
            }
            cmbKamera.SelectedIndex = 0;
            kameram = new VideoCaptureDevice();

            wi = new WaveIn();
            wi.StartRecording();
            wi.WaveFormat = new WaveFormat(4, 16, 1); // (44100, 16, 1);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);
            timer1.Enabled = true;
            //CleanTemporaryFolders();
        }
        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                myQ.Enqueue(BitConverter.ToInt16(e.Buffer, i));
                myQ.Dequeue();
            }
        }
        private void tabPage1_Click(object sender, EventArgs e)
        {

        }


        bool tutus;
        int FareX, FareY;
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (tutus)
            {
                this.Left = Cursor.Position.X - FareX;
                this.Top = Cursor.Position.Y - FareY;
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            tutus = false;
            FareX = 0;
            FareY = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {   //Temp
            if (tempFiles.Checked)
            {
                string[] folder = Directory.GetFiles(Path.GetTempPath());
                string temp = Path.GetTempPath();
                var Dir = new DirectoryInfo(temp);
                foreach (string _file in folder)
                {
                    try
                    {
                        File.Delete(_file);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir in Dir.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }

                string[] folder2 = Directory.GetFiles(@"C:\Windows\Temp");
                string temp2 = @"C:\Windows\Temp";
                var Dir2 = new DirectoryInfo(temp2);
                foreach (string _file2 in folder2)
                {
                    try
                    {
                        File.Delete(_file2);
                        Directory.Delete(_file2);
                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir2 in Dir2.GetDirectories())
                {
                    try
                    {
                        dir2.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Temp

            //Recent
            /*if (recentFiles.Checked)
            {
                string[] folder = Directory.GetFiles(@"C:\Users\" + Environment.UserName + "\\Recent");
                string recent = @"C:\Users\" + Environment.UserName + "\\Recent";
                var Dir = new DirectoryInfo(recent);
                try
                {
                    foreach (string _file in folder)
                    {
                        try
                        {
                            File.Delete(_file);

                        }
                        catch (Exception)
                        {
                        }
                    }
                    foreach (DirectoryInfo dir in Dir.GetDirectories())
                    {
                        try
                        {
                            dir.Delete(true);
                        }

                        catch (Exception)
                        {
                        }
                    }
                }
                catch (Exception) { }
            }*/
            //Recent

            //Prefetch
            if (prefetchFiles.Checked)
            {
                string[] folder = Directory.GetFiles(@"C:\Windows\Prefetch");
                string temp = @"C:\Windows\Prefetch";
                var Dir = new DirectoryInfo(temp);
                foreach (string _file in folder)
                {
                    try
                    {
                        File.Delete(_file);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir in Dir.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Prefetch

            //Windows.old
            if (windowsOld.Checked)
            {
                try
                {
                    string windowsold = @"C:\Windows.old";
                    Directory.Delete(windowsold, true);
                }
                catch (Exception) {}
            }
            //Windows.old

            //Software Distribution
            if (softwareDist.Checked)
            {
                string[] folder5 = Directory.GetFiles(@"C:\Windows\SoftwareDistribution\Download");
                string softwareDist = @"C:\Windows\SoftwareDistribution\Download";
                var Dir5 = new DirectoryInfo(softwareDist);
                foreach (string _file5 in folder5)
                {
                    try
                    {
                        File.Delete(_file5);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir5 in Dir5.GetDirectories())
                {
                    try
                    {
                        dir5.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Software Distribution

            //NVIDIA Cache
            try
            {
                if (nvidiaCache.Checked)
                {
                    string[] folder5 = Directory.GetFiles(@"C:\Users\" + Environment.UserName + @"\AppData\Local\NVIDIA\GLCache");
                    string softwareDist = @"C:\Users\" + Environment.UserName + @"\AppData\Local\NVIDIA\GLCache";
                    var Dir5 = new DirectoryInfo(softwareDist);
                    foreach (string _file5 in folder5)
                    {
                        try
                        {
                            File.Delete(_file5);

                        }
                        catch (Exception)
                        {
                        }
                    }
                    foreach (DirectoryInfo dir5 in Dir5.GetDirectories())
                    {
                        try
                        {
                            dir5.Delete(true);
                        }

                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch
            {

            }
            //NVIDIA Cache

            //Recycle Bin
            if(recycleBin.Checked)
            {
                string[] folder5 = Directory.GetFiles(@"C:\$Recycle.Bin");
                string softwareDist = @"C:\$Recycle.Bin";
                var Dir5 = new DirectoryInfo(softwareDist);
                foreach (string _file5 in folder5)
                {
                    try
                    {
                        File.Delete(_file5);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir5 in Dir5.GetDirectories())
                {
                    try
                    {
                        dir5.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Recycle Bin

            //Delivery Optimization Files
            if (deliveryFiles.Checked)
            {
                string[] folder5 = Directory.GetFiles(@"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache");
                string softwareDist = @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache";
                var Dir5 = new DirectoryInfo(softwareDist);
                foreach (string _file5 in folder5)
                {
                    try
                    {
                        File.Delete(_file5);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir5 in Dir5.GetDirectories())
                {
                    try
                    {
                        dir5.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Delivery Optimization Files

            //Downloads
            if(downloadsFiles.Checked)
            {
                string[] folder5 = Directory.GetFiles(@"C:\Users\" + Environment.UserName + @"\Downloads");
                string softwareDist = @"C:\Users\" + Environment.UserName + @"\Downloads";
                var Dir5 = new DirectoryInfo(softwareDist);
                foreach (string _file5 in folder5)
                {
                    try
                    {
                        File.Delete(_file5);

                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (DirectoryInfo dir5 in Dir5.GetDirectories())
                {
                    try
                    {
                        dir5.Delete(true);
                    }

                    catch (Exception)
                    {
                    }
                }
            }
            //Downloads

            MessageBox.Show("Unnecessary files have been cleared.");
        }
        private void CleanTemporaryFolders()
        {
            String tempFolder = Environment.ExpandEnvironmentVariables("%TEMP%");
            String recent = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + "\\Recent";
            String prefetch = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%") + "\\Prefetch";
            EmptyFolderContents(tempFolder);
            EmptyFolderContents(recent);
            EmptyFolderContents(prefetch);
        }

        private void EmptyFolderContents(string folderName)
        {
            foreach (var folder in Directory.GetDirectories(folderName))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception excep)
                {
                    System.Diagnostics.Debug.WriteLine(excep);
                }
            }
            foreach (var file in Directory.GetFiles(folderName))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception excep)
                {
                    System.Diagnostics.Debug.WriteLine(excep);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (discord.Checked)
                Process.Start("https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x86");
            if (visualStudio.Checked)
                Process.Start("https://visualstudio.microsoft.com/tr/thank-you-downloading-visual-studio/?sku=Community&channel=Release&version=VS2022&source=VSLandingPage&cid=2030&passive=false");
            if (kmspico.Checked)
                Process.Start("https://cdn.discordapp.com/attachments/863831449282674692/1012315562203955260/KMSpico.rar");
            if (java.Checked)
                Process.Start("https://javadl.oracle.com/webapps/download/AutoDL?BundleId=246778_424b9da4b48848379167015dcc250d8d");
            if (chrome.Checked)
                Process.Start("https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7B3B37402B-B172-6F18-2A1B-585CF57C6F51%7D%26lang%3Dtr%26browser%3D4%26usagestats%3D0%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26brand%3DYTUH%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe");
            if (msi.Checked)
                Process.Start("https://indir.gezginler.net/i/34776/33343737365f323032322d30382d3235/");
            if (steam.Checked)
                Process.Start("https://cdn.cloudflare.steamstatic.com/client/installer/SteamSetup.exe");
            if (epicGames.Checked)
                Process.Start("https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi");
            if (dcontrol.Checked)
                Process.Start("https://cdn.discordapp.com/attachments/863831449282674692/1001751526261399652/dControl.rar");
            if (quickcpu.Checked)
                Process.Start("https://coderbag.com/assets/downloads/cpm/currentversion/QuickCpuSetup64.zip");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            kameram = new VideoCaptureDevice(Cihazlar[cmbKamera.SelectedIndex].MonikerString);
            kameram.NewFrame += VideoCaptureDevice_NewFrame;
            kameram.Start();
        }
        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            guna2PictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (kameram.IsRunning)
            {
                kameram.Stop();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                chart1.Series["Series1"].Points.DataBindY(myQ);
                //chart1.ResetAutoValues();
            }
            catch(Exception)
            {
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            kameram.Stop();
            guna2PictureBox1.Image = null;
        }

        bool bip = false;
        private void button5_Click(object sender, EventArgs e)
        {
            SoundPlayer simpleSound = new SoundPlayer(Application.StartupPath + "\\Other\\bip.wav");
            if (bip == false)
            {
                simpleSound.Play();
                bip = true;
                button5.Text = "";
            }
            else
            {
                simpleSound.Stop();
                bip = false;
                button5.Text = "►";
            }
        }

        private void guna2TrackBar1_Scroll(object sender, ScrollEventArgs e)
        {
            int NewVolume = ((ushort.MaxValue / 100) * guna2TrackBar1.Value);
            uint NewVolumeAllChannels = (((uint)NewVolume & 0x0000ffff) | ((uint)NewVolume << 16));
            waveOutSetVolume(IntPtr.Zero, NewVolumeAllChannels);
        }

        private void disableUpdate_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("wuauserv");
            if (disableUpdate.Checked)
            { 
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                       sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label4.Text = "Enable Windows Update";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label4.Text = "Disable Windows Update";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disableDefender_CheckedChanged(object sender, EventArgs e)
        {
            if(disableDefender.Checked)
            {
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return;

                RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0"); //4
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "1"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "1"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "1"); //0

                CheckDefender();
                label5.Text = "Enable Windows Defender";
            }
            else
            {
                if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) return;

                RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "4"); //4
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "0"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "0"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "0"); //0
                RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "0"); //0

                CheckDefender();
                label5.Text = "Disable Windows Defender";
            }
        }

        private static void RegistryEdit(string regPath, string name, string value)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (key == null)
                    {
                        Registry.LocalMachine.CreateSubKey(regPath).SetValue(name, value, RegistryValueKind.DWord);
                        return;
                    }
                    if (key.GetValue(name) != (object)value)
                        key.SetValue(name, value, RegistryValueKind.DWord);
                }
            }
            catch { }
        }
        private static void CheckDefender()
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "Get-MpPreference -verbose",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();

                if (line.StartsWith(@"DisableRealtimeMonitoring") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableRealtimeMonitoring $true"); //real-time protection

                else if (line.StartsWith(@"DisableBehaviorMonitoring") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableBehaviorMonitoring $true"); //behavior monitoring

                else if (line.StartsWith(@"DisableBlockAtFirstSeen") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableBlockAtFirstSeen $true");

                else if (line.StartsWith(@"DisableIOAVProtection") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableIOAVProtection $true"); //scans all downloaded files and attachments

                else if (line.StartsWith(@"DisablePrivacyMode") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisablePrivacyMode $true"); //displaying threat history

                else if (line.StartsWith(@"SignatureDisableUpdateOnStartupWithoutEngine") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true"); //definition updates on startup

                else if (line.StartsWith(@"DisableArchiveScanning") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableArchiveScanning $true"); //scan archive files, such as .zip and .cab files

                else if (line.StartsWith(@"DisableIntrusionPreventionSystem") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $true"); // network protection 

                else if (line.StartsWith(@"DisableScriptScanning") && line.EndsWith("False"))
                    RunPS("Set-MpPreference -DisableScriptScanning $true"); //scanning of scripts during scans

                else if (line.StartsWith(@"SubmitSamplesConsent") && !line.EndsWith("2"))
                    RunPS("Set-MpPreference -SubmitSamplesConsent 2"); //MAPSReporting 

                else if (line.StartsWith(@"MAPSReporting") && !line.EndsWith("0"))
                    RunPS("Set-MpPreference -MAPSReporting 0"); //MAPSReporting 

                else if (line.StartsWith(@"HighThreatDefaultAction") && !line.EndsWith("6"))
                    RunPS("Set-MpPreference -HighThreatDefaultAction 6 -Force"); // high level threat // Allow

                else if (line.StartsWith(@"ModerateThreatDefaultAction") && !line.EndsWith("6"))
                    RunPS("Set-MpPreference -ModerateThreatDefaultAction 6"); // moderate level threat

                else if (line.StartsWith(@"LowThreatDefaultAction") && !line.EndsWith("6"))
                    RunPS("Set-MpPreference -LowThreatDefaultAction 6"); // low level threat

                else if (line.StartsWith(@"SevereThreatDefaultAction") && !line.EndsWith("6"))
                    RunPS("Set-MpPreference -SevereThreatDefaultAction 6"); // severe level threat
            }
        }

        private static void RunPS(string args)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = args,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }
            };
            proc.Start();
        }

        private void disableWinsearch_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("WSearch");
            if (disableWinsearch.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label9.Text = "Enable Windows Search";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label9.Text = "Disable Windows Search";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disablePrint_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("Spooler");
            if (disablePrint.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label6.Text = "Enable Print Spooler";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label6.Text = "Disable Print Spooler";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disableRestore_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("swprv");
            if (disableRestore.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label8.Text = "Enable System Restore";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label8.Text = "Disable System Restore";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disableSysmain_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("SysMain");
            if (disableSysmain.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label8.Text = "Enable SysMain";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label8.Text = "Disable SysMain";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disableServer_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("LanmanServer");
            if (disableServer.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label10.Text = "Enable Server";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label8.Text = "Disable Server";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void disableip_CheckedChanged(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("iphlpsvc");
            if (disableip.Checked)
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.Stop();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    sc.Close();
                    label11.Text = "Enable IP Helper";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    if (sc != null && sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                    sc.WaitForStatus(ServiceControllerStatus.StartPending);
                    sc.Close();
                    label11.Text = "Disable IP Helper";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (runcore.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\Core Temp\\Core Temp.exe");
                if (runvc.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\VC_redist.x64.exe");
                if (runfurmark.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\FurMark\\FurMark.exe");
                if (runbattery.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\BatteryBarSetup-3.6.6.exe");
                if (runhdsen.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\Hard Disk Sentinel\\HDSentinel.exe");
                if (runwinrar.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\winrarsetup.exe");
                if(gpuz.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\FurMark\\gpuz.exe");
                if (cpuburner.Checked)
                    Process.Start(Application.StartupPath + "\\Other\\FurMark\\cpuburner.exe");
            }
            catch (Exception) { }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            tutus = true;
            FareX = Cursor.Position.X - this.Left;
            FareY = Cursor.Position.Y - this.Top;
        }
    }
}
