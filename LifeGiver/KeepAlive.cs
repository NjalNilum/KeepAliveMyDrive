using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LifeGiver
{
  public class KeepAlive
  {
    #region Members
    private const int MEBIBYTE = 1048576;
    private const int DEFAULTCLUSTER = 4096;
    private string PathToLogFile;
    private string PathToConfigFile;
    private ConfigPathes configPathes;
    private bool endItAll;

    // Those locks are most important, because the config allows to multible acces on the same file. If
    // this happens within a large amount of different tasks/threads accessing on the same file at the same
    // time, the propability for a crash (access violation) is very high.
    // Try it. Remove 'dummyFileLock' and add multible equal entries to the config file with a cyclytime of a second.
    private object loggingLock = new object();
    private object dummyFileLock = new object();
    #endregion Members


    #region Ctor
    public KeepAlive()
    {
      PathToLogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logfile.txt");
      PathToConfigFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.xml");

      GetConfiguration();
    }
    #endregion Ctor


    #region Methods
    public void Start()
    {
      try
      {
        LogStart("Start service workers.");
        configPathes.PathAndTime.ToList().ForEach(item =>
        {
          StartHddWriterAsync(item.FullPath, item.CycleTime);
        });
      }
      catch (Exception ex)
      {
        LogError($"Start()  --  Exception: {ex.Message}");
        Stop();
      }

    }

    public void Stop()
    {
      endItAll = true;
      LogEnd("All tasks were ended. No drive kept alive anymore.");
    }

    /// <summary>
    /// Read the configuration from the configuration file.
    /// </summary>
    private void GetConfiguration()
    {
      try
      {
        if (!File.Exists(PathToConfigFile))
        {
          LogError($"GetConfiguration()  --  Configuration missing");
          Stop();
          return;
        }

        XmlSerializer reader = new XmlSerializer(typeof(ConfigPathes));
        using (FileStream file = new FileStream(PathToConfigFile, FileMode.Open))
        {
          configPathes = (ConfigPathes)reader.Deserialize(file);
          file.Close();
        }
      }
      catch (Exception ex)
      {
        LogError($"GetConfiguration()  --  Exception: {ex.Message}");
        Stop();
      }
    }

    /// <summary>
    /// Async task that loops every 'cycleTime' seconds and appends a dot into the file 'path'.
    /// File is created if not existing.
    /// </summary>
    /// <param name="pathToDummyFile">Full path to the dummy file that keeps alive the hdd.</param>
    /// <param name="cycleTime">The cycle time in seconds. Every 'cycleTime' seconds the file will be changed.</param>
    private void StartHddWriterAsync(string pathToDummyFile, int cycleTime)
    {
      // local func that creates (or overwrites) the given file
      Action<string> local_ReCreateDummyFile = (path2File) =>
      {
        File.Delete(path2File); // Unfortunately a necessary evil. File.Create() can't creat or override if the stream is still open.
        using (FileStream fs = File.Create(path2File)) { };
        File.SetAttributes(path2File, FileAttributes.Hidden);
        LogInfo($"File {path2File} was created.");
      };

      Task.Run(() =>
      {
        LogInfo($"LifeGiver startet for {pathToDummyFile}");
        while (!endItAll)
        {
          try
          {
            lock (dummyFileLock)
            {
              // Default cluster size of a hdd is 4096 Byte. Every dot we add means 1 byte. So this may help to spare the hdd. Relatively.
              if (!File.Exists(pathToDummyFile) || new FileInfo(pathToDummyFile).Length >= DEFAULTCLUSTER - 6)
                local_ReCreateDummyFile(pathToDummyFile);

              using (TextWriter sWriter = File.AppendText(pathToDummyFile))
              {
                sWriter.Write(".");
                LogInfo($"Wrote dot in {pathToDummyFile}");
              }
            }
            // CycleTime stored in config in seconds
            Thread.Sleep(cycleTime * 1000);
          }
          catch (Exception ex)
          {
            LogError($"StartHddWriterAsync()  --  Exception: {ex.Message}");
            LogError($"LifeGiver aborted for {pathToDummyFile}. {Path.GetPathRoot(pathToDummyFile)} is no longer kept alive.");
            return;
          }
        }
      });
    }
    #endregion Methods


    #region Logging
    // self-explanatory
    private void LogInfo(string message)
    {
      LogRaw(message, "INFO ", ConsoleColor.White);
    }

    // self-explanatory
    private void LogStart(string message)
    {
      LogRaw(message, "START", ConsoleColor.Green);
    }

    // self-explanatory
    private void LogEnd(string message)
    {
      LogRaw(message, "END  ", ConsoleColor.Yellow);
    }

    // self-explanatory
    private void LogError(string message)
    {
      LogRaw(message, "ERROR", ConsoleColor.Red);
    }

    /// <summary>
    /// Self-explanatory.
    /// 
    /// If log file becomes greater than 10 MB, it will be deleted.
    /// </summary>
    private void LogRaw(string message, string kindOf, ConsoleColor color)
    {
      try
      {
        lock (loggingLock)
        {
          if (!File.Exists(PathToLogFile) || new FileInfo(PathToLogFile).Length >= 10 * MEBIBYTE)
            using (FileStream fs = File.Create(PathToLogFile)) { };

          using (TextWriter sWriter = File.AppendText(PathToLogFile))
          {
            string s = $"{kindOf} - {DateTime.Now} - Thread {Thread.CurrentThread.ManagedThreadId:00}: {message}";
            sWriter.WriteLine(s);
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.White;
          }
        }
      }
      catch (Exception ex)
      {
        if (!EventLog.SourceExists("KeepAliveMyDriveService"))
          EventLog.CreateEventSource("KeepAliveMyDriveService", "Application");

        EventLog.WriteEntry(
          "KeepAliveMyDriveService",
          $"Error when accessing on logfile.{Environment.NewLine}Exception Message: {ex.Message}",
          EventLogEntryType.Error,
          166,
          666);
      }
    }
    #endregion Logging
  }
}
