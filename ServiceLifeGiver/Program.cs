using System;
using System.Collections;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ServiceLifeGiver
{
  static class Program
  {
    private const string SERVICE_NAME = "KeepAliveMyDriveService";

    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        // Run your service normally.
        ServiceBase[] ServicesToRun = new ServiceBase[] { new KeepAliveMyDriveService() };
        ServiceBase.Run(ServicesToRun);
      }

      else if (args.Length == 1)
      {
        switch (args[0])
        {
          case "-install":
            InstallService();
            Console.WriteLine("Service successfully installed");
            StartService();
            Console.WriteLine("Service successfully started");
            break;
          case "-uninstall":
            StopService();
            Console.WriteLine("Service successfully stopped");
            UninstallService();
            Console.WriteLine("Service successfully uninstalled");
            break;
          case "-restart":
            StopService();
            StartService();
            break;
          default:
            throw new NotImplementedException();
        }
      }
    }


    // "http://stackoverflow.com/questions/1195478/how-to-make-a-net-windows-service-start-right-after-the-installation/1195621#1195621"
    private static bool IsInstalled()
    {
      using (ServiceController controller =
          new ServiceController(SERVICE_NAME))
      {
        try
        {
          ServiceControllerStatus status = controller.Status;
        }
        catch
        {
          return false;
        }
        return true;
      }
    }

   
    private static bool IsRunning()
    {
      using (ServiceController controller =
          new ServiceController(SERVICE_NAME))
      {
        if (!IsInstalled()) return false;
        return (controller.Status == ServiceControllerStatus.Running);
      }
    }

    private static AssemblyInstaller GetInstaller()
    {
      AssemblyInstaller installer = new AssemblyInstaller(
          typeof(KeepAliveMyDriveService).Assembly, null);
      installer.UseNewContext = true;
      return installer;
    }

    private static void InstallService()
    {
      if (IsInstalled()) return;

      try
      {
        using (AssemblyInstaller installer = GetInstaller())
        {
          IDictionary state = new Hashtable();
          try
          {
            installer.Install(state);
            installer.Commit(state);
          }
          catch
          {
            try
            {
              installer.Rollback(state);
            }
            catch { }
            throw;
          }
        }
      }
      catch
      {
        throw;
      }
    }

    private static void UninstallService()
    {
      if (!IsInstalled()) return;
      try
      {
        using (AssemblyInstaller installer = GetInstaller())
        {
          IDictionary state = new Hashtable();
          try
          {
            installer.Uninstall(state);
          }
          catch
          {
            throw;
          }
        }
      }
      catch
      {
        throw;
      }
    }

    private static void StartService()
    {
      if (!IsInstalled()) return;

      using (ServiceController controller =
          new ServiceController(SERVICE_NAME))
      {
        try
        {
          if (controller.Status != ServiceControllerStatus.Running)
          {
            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running,
                TimeSpan.FromSeconds(10));
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.Message);
          throw;
        }
      }
    }

    private static void StopService()
    {
      if (!IsInstalled()) return;
      using (ServiceController controller =
          new ServiceController(SERVICE_NAME))
      {
        try
        {
          if (controller.Status != ServiceControllerStatus.Stopped)
          {
            controller.Stop();
            controller.WaitForStatus(ServiceControllerStatus.Stopped,
                 TimeSpan.FromSeconds(10));
          }
        }
        catch
        {
          throw;
        }
      }
    }
  }
}
