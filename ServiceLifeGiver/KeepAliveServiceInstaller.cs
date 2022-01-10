using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServiceLifeGiver
{
  [RunInstaller(true)]
  public partial class KeepAliveServiceInstaller : Installer
  {
    public KeepAliveServiceInstaller()
    {
      ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
      ServiceInstaller serviceInstaller = new ServiceInstaller();

      //# Service Account Information
      serviceProcessInstaller.Account = ServiceAccount.LocalSystem;

      //# Service Information
      serviceInstaller.DisplayName = "KeepAliveMyDriveService";
      serviceInstaller.StartType = ServiceStartMode.Automatic;
      serviceInstaller.DelayedAutoStart = true;

      // This must be identical to the WindowsService.ServiceBase name
      // set in the constructor of WindowsService.cs
      serviceInstaller.ServiceName = "KeepAliveMyDriveService";
      serviceInstaller.Description = "Mein Gott etz lauf halt Du Glomb.";

      this.Installers.Add(serviceProcessInstaller);
      this.Installers.Add(serviceInstaller);
    }
  }
}
