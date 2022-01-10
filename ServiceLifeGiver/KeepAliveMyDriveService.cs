using LifeGiver;
using System.Diagnostics;
using System.ServiceProcess;

namespace ServiceLifeGiver
{
  public partial class KeepAliveMyDriveService : ServiceBase
  {
    KeepAlive myKeeper;

    public KeepAliveMyDriveService()
    {
      this.ServiceName = "KeepAliveMyDriveService";
      this.EventLog.Source = "KeepAliveMyDriveService Event Log Source";
      this.EventLog.Log = "KeepAliveMyDriveService Event Log";

      // These Flags set whether or not to handle that specific
      //  type of event. Set to true if you need it, false otherwise.
      this.CanHandlePowerEvent = true;
      this.CanHandleSessionChangeEvent = true;
      this.CanPauseAndContinue = true;
      this.CanShutdown = true;
      this.CanStop = true;

      if (!EventLog.SourceExists("KeepAliveMyDriveService"))
        EventLog.CreateEventSource("KeepAliveMyDriveService", "Application");

      myKeeper = new KeepAlive();
    }

    protected override void OnStart(string[] args)
    {
      myKeeper.Start();
      base.OnStart(args);
    }

    protected override void OnStop()
    {
      myKeeper.Stop();
      base.OnStop();
    }


    #region override base
    // "http://www.codeproject.com/Articles/14353/Creating-a-Basic-Windows-Service-in-C"

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
    }

    protected override void OnPause()
    {
      base.OnPause();
    }

    protected override void OnContinue()
    {
      base.OnContinue();
    }

    /// <summary>
    /// OnShutdown(): Called when the System is shutting down
    /// - Put code here when you need special handling
    ///   of code that deals with a system shutdown, such
    ///   as saving special data before shutdown.
    /// </summary>
    protected override void OnShutdown()
    {
      base.OnShutdown();
    }

    /// <summary>
    /// OnCustomCommand(): If you need to send a command to your
    ///   service without the need for Remoting or Sockets, use
    ///   this method to do custom methods.
    /// </summary>
    /// <param name="command">Arbitrary Integer between 128 & 256</param>
    protected override void OnCustomCommand(int command)
    {
      //  A custom command can be sent to a service by using this method:
      //#  int command = 128; //Some Arbitrary number between 128 & 256
      //#  ServiceController sc = new ServiceController("NameOfService");
      //#  sc.ExecuteCommand(command);

      base.OnCustomCommand(command);
    }

    /// <summary>
    /// OnPowerEvent(): Useful for detecting power status changes,
    ///   such as going into Suspend mode or Low Battery for laptops.
    /// </summary>
    /// <param name="powerStatus">The Power Broadcase Status (BatteryLow, Suspend, etc.)</param>
    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
    {
      return base.OnPowerEvent(powerStatus);
    }

    /// <summary>
    /// OnSessionChange(): To handle a change event from a Terminal Server session.
    ///   Useful if you need to determine when a user logs in remotely or logs off,
    ///   or when someone logs into the console.
    /// </summary>
    /// <param name="changeDescription"></param>
    protected override void OnSessionChange(SessionChangeDescription changeDescription)
    {
      base.OnSessionChange(changeDescription);
    }
    #endregion override base
  }
}
