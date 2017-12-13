using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace Observer
{
    [RunInstaller(true)]
    public partial class InstallerForObserver : Installer
    {
        string serviceName = "Windows Observer";
        string serviceDesc= "Windows Observer v1.0.0.0";
        ServiceInstaller serviceInstaller;
        ServiceProcessInstaller processInstaller;

        public InstallerForObserver()
        {
            InitializeComponent();
            
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalService
            };
            serviceInstaller = new ServiceInstaller
            {
                StartType = ServiceStartMode.Automatic,
                ServiceName = serviceName,
                Description = serviceDesc,
                DelayedAutoStart = true,
                DisplayName = serviceName
            };
            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }
    }
}
