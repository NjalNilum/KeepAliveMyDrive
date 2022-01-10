using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LifeGiver;

namespace TestLifeGiver
{
  class Program
  {


    static void Main(string[] args)
    {
      KeepAlive myKeeper = new KeepAlive();
      myKeeper.Start();

      bool alive = true;
      while (alive)
      {
        String s = Console.ReadLine();

        switch (s)
        {
          case "exit":
          case "end":
          case "cancel":
          case "abort":
          case "reset":
            myKeeper.Stop();
            alive = false;
            break;
          default:
            break;
        }
      }

      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine("End Test - Press return");
      Console.ReadLine();
    }

   
  }
}
