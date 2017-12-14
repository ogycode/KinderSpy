using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.IO;

namespace KinderSpy
{
    public partial class MainWindow : Window
    {
        RegistryKey Key;
        string password;
        string serviceName = "Windows Observer";
        string servicePath = $@"{Environment.CurrentDirectory}\Observer\Observer.exe";
        string servicePathUSB = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Observer\Observer.exe";

        string from = $@"{Environment.CurrentDirectory}\Observer";
        string to = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\Observer";
        char s = ',';

        SolidColorBrush red = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        SolidColorBrush green = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        SolidColorBrush blue = new SolidColorBrush(Color.FromRgb(33, 150, 243));

        public MainWindow()
        {
            InitializeComponent();
        }

        void LoadData()
        {
            tbParentMail.Text = Key.GetValue("Arg0", string.Empty).ToString();
            tbSecondMail.Text = Key.GetValue("Arg1", string.Empty).ToString();
            tbSeconMailPass.Text = Key.GetValue("Arg2", string.Empty).ToString();
            tbKidName.Text = Key.GetValue("Arg3", string.Empty).ToString();
            tbCount.Text = Key.GetValue("Arg4", string.Empty).ToString();
            tbPeriod.Text = Key.GetValue("Arg5", string.Empty).ToString();

            password = Key.GetValue("Arg99", string.Empty).ToString();
            tbPassword.Text = password;
        }
        bool CheckData(bool OnlyApp, bool ShowMsg)
        {
            bool APP_Name = !string.IsNullOrWhiteSpace(tbKidName.Text);
            bool REG_Name = !string.IsNullOrWhiteSpace(Key.GetValue("Arg3", string.Empty).ToString());

            bool APP_Period = !string.IsNullOrWhiteSpace(tbPeriod.Text);
            bool REG_Period = !string.IsNullOrWhiteSpace(Key.GetValue("Arg5", string.Empty).ToString());

            bool APP_Count = !string.IsNullOrWhiteSpace(tbCount.Text);
            bool REG_Count = !string.IsNullOrWhiteSpace(Key.GetValue("Arg4", string.Empty).ToString());

            bool APP_Parent = !string.IsNullOrWhiteSpace(tbParentMail.Text);
            bool REG_Parent = !string.IsNullOrWhiteSpace(Key.GetValue("Arg0", string.Empty).ToString());

            bool APP_From = !string.IsNullOrWhiteSpace(tbSecondMail.Text);
            bool REG_From = !string.IsNullOrWhiteSpace(Key.GetValue("Arg1", string.Empty).ToString());

            bool APP_FromPass = !string.IsNullOrWhiteSpace(tbSeconMailPass.Text);
            bool REG_FromPass = !string.IsNullOrWhiteSpace(Key.GetValue("Arg2", string.Empty).ToString());

            bool REG_Temp = !string.IsNullOrWhiteSpace(Key.GetValue("Arg97", string.Empty).ToString());
            bool REG_Local = !string.IsNullOrWhiteSpace(Key.GetValue("Arg98", string.Empty).ToString());

            if (ShowMsg && !(APP_Name || REG_Name)) MessageBox.Show("Kid's name can not be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ShowMsg && !(APP_Period || REG_Period)) MessageBox.Show("Specify period!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ShowMsg && !(APP_Count || REG_Count)) MessageBox.Show("Specify count of reports for create general report!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ShowMsg && !(APP_Parent || REG_Parent)) MessageBox.Show("Enter the EMail where will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ShowMsg && !(APP_From || REG_From))
                MessageBox.Show("Enter the EMail from will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (ShowMsg && !(APP_FromPass || REG_FromPass))
                MessageBox.Show("Enter the password of EMail from will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return OnlyApp ? (APP_Name && APP_Period && APP_Count && APP_Parent && APP_From && APP_FromPass) :
                             (APP_Name && APP_Period && APP_Count && APP_Parent && APP_From && APP_FromPass &&
                              REG_Name && REG_Period && REG_Count && REG_Parent && REG_From && REG_FromPass && REG_Temp && REG_Local);
        }
        void SetStatus()
        {
            if (ServiceManager.ServiceIsInstalled(serviceName))
            {
                ServiceState status = ServiceManager.GetServiceStatus(serviceName);

                if (status == ServiceState.Running)
                {
                    tbStatus.Text = "Status: Running";
                    tbStatus.Background = green;
                }
                else
                {
                    tbStatus.Text = $"Status: {status.ToString()}";
                    tbStatus.Background = green;
                }

                btnRemove.IsEnabled = true;
            }
            else
            {
                tbStatus.Text = "Status: not installed";
                tbStatus.Background = red;
                btnRemove.IsEnabled = false;
            }
        }
        void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            if (copySubDirs)
                foreach (DirectoryInfo subdir in dirs)
                    DirectoryCopy(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs);
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            Key = Registry.LocalMachine.CreateSubKey($"SOFTWARE\\Windows Login\\Services data");
            LoadData();
            gridPassword.Visibility = string.IsNullOrWhiteSpace(password) ? Visibility.Collapsed : Visibility.Visible;

            tbKidName.TextChanged += tbTextChanged;
            tbParentMail.TextChanged += tbTextChanged;
            tbSecondMail.TextChanged += tbTextChanged;
            tbSeconMailPass.TextChanged += tbTextChanged;
            tbCount.TextChanged += tbTextChanged;
            tbPeriod.TextChanged += tbTextChanged;
            tbPassword.TextChanged += tbTextChanged;

            SetStatus();
            btnSetup.IsEnabled = CheckData(true, false);
        }
        private void tbTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            btnSave.Content = "Save*";
            btnSetup.IsEnabled = false;
        }
        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                MessageBox.Show("If you are not set a password for application your kid can take access to this spy-tool and remove them! Be careful!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (CheckData(true, true))
            {
                Key.SetValue("Arg0", tbParentMail.Text);
                Key.SetValue("Arg1", tbSecondMail.Text);
                Key.SetValue("Arg2", tbSeconMailPass.Text);
                Key.SetValue("Arg3", tbKidName.Text);
                Key.SetValue("Arg4", tbCount.Text);
                Key.SetValue("Arg5", tbPeriod.Text);

                Key.SetValue("Arg97", Path.GetTempPath());
                Key.SetValue("Arg98", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                Key.SetValue("Arg99", tbPassword.Text);

                btnSave.Content = "Save";
                btnSetup.IsEnabled = true;

                MessageBox.Show("Settings was saved!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void btnEnterClick(object sender, RoutedEventArgs e)
        {
            if (pbPassword.Password == password)
                gridPassword.Visibility = Visibility.Collapsed;
            else
                Environment.Exit(1);
        }
        private void btnRemoveClick(object sender, RoutedEventArgs e)
        {
            if (ServiceManager.ServiceIsInstalled(serviceName))
            {
                try { ServiceManager.StopService(serviceName); }
                catch { }

                ServiceManager.Uninstall(serviceName);
                MessageBox.Show("Service will be removed after close this window!", "Information!", MessageBoxButton.OK, MessageBoxImage.Information);

                btnRemove.IsEnabled = false;
            }
            else
                MessageBox.Show("Service is not installed!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);

            SetStatus();
        }
        private void btnSetupClic(object sender, RoutedEventArgs e)
        {
            if (CheckData(false, true))
            {
                if (ServiceManager.ServiceIsInstalled(serviceName))
                    ServiceManager.StartService(serviceName);
                else
                {
                    if (cbFromUSB.IsChecked.Value)
                    {
                        DirectoryCopy(from, to, true);
                        ServiceManager.InstallAndStart(serviceName, serviceName, servicePathUSB);
                        MessageBox.Show($"Service was installed!\n({to})\n\nYour next actions:\n1) Close this window\n2) Wait for report from this PC\n3) Read them in ReportViewer\n4) ...draw conclusions...\n\nI hope its help to you and your kid", "Information!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        ServiceManager.InstallAndStart(serviceName, serviceName, servicePath);
                        MessageBox.Show($"Service was installed!\n({from})\n\nYour next actions:\n1) Close this window\n2) Wait for report from this PC\n3) Read them in ReportViewer\n4) ...draw conclusions...\n\nI hope its help to you and your kid", "Information!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
                MessageBox.Show("Service do not installed! Fill all needed information and try now", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            SetStatus();
            btnSetup.IsEnabled = false;
        }
        private void btnClearClick(object sender, RoutedEventArgs e)
        {
            Key.SetValue("Arg0", string.Empty);
            Key.SetValue("Arg1", string.Empty);
            Key.SetValue("Arg2", string.Empty);
            Key.SetValue("Arg3", string.Empty);
            Key.SetValue("Arg4", string.Empty);
            Key.SetValue("Arg5", string.Empty);

            Key.SetValue("Arg97", string.Empty);
            Key.SetValue("Arg98", string.Empty);
            Key.SetValue("Arg99", string.Empty);

            LoadData();
            btnSave.Content = "Save";

            if (ServiceManager.ServiceIsInstalled(serviceName))
                MessageBox.Show("Now you are need to remove service, push the button: Remove", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void btnSavePrefabClick(object sender, RoutedEventArgs e)
        {
            string str = $"{tbParentMail.Text}{s}{tbSecondMail.Text}{s}{tbSeconMailPass.Text}{s}{tbKidName.Text}{s}{tbCount.Text}{s}{tbPeriod.Text}";

            if (CheckData(true, true))
            {
                using (StreamWriter sw = File.CreateText($@"{Environment.CurrentDirectory}\prefab.txt"))
                    sw.Write(str);

                MessageBox.Show("Prefab was saved!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void btnLoadPrefabClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists($@"{Environment.CurrentDirectory}\prefab.txt"))
                using (StreamReader sr = File.OpenText($@"{Environment.CurrentDirectory}\prefab.txt"))
                {
                    string[] str = sr.ReadToEnd().Split(s);

                    try
                    {
                        tbParentMail.Text = str[0];
                        tbSecondMail.Text = str[1];
                        tbSeconMailPass.Text = str[2];
                        tbKidName.Text = str[3];
                        tbCount.Text = str[4];
                        tbPeriod.Text = str[5];

                        MessageBox.Show("Prefab was loaded!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Prefab file is corrupted!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
        }
    }
}
