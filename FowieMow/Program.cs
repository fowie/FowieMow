using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FowieMow
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Brainstem...");
            ArduinoCommunicator.Start();
            DataLogging Logger = new DataLogging("C:\\Users\\Fowie\\OneDrive\\Documents\\Side Projects\\FowieMow\\Data");
            Console.WriteLine("Done!");

            double[] PreviousGPS = { 0.0, 0.0, 0.0, 0.0, 0.0 };
            double[] CurrentGPS = { 0.0, 0.0, 0.0, 0.0, 0.0 };

            while(true)
            {
                ConsoleKey pressedKey = ConsoleKey.Escape;
                if (Console.KeyAvailable)
                {
                   pressedKey = Console.ReadKey().Key;
                }

                if(pressedKey == ConsoleKey.Q)
                {
                    break;
                }
                else if(pressedKey == ConsoleKey.C)
                {
                    Console.WriteLine("Sending test 'Move' command");
                    ArduinoCommunicator.IssueCommand("1,10,10");
                }

                CurrentGPS[0] = ArduinoCommunicator.GetLatitude();
                CurrentGPS[1] = ArduinoCommunicator.GetLongitude();
                CurrentGPS[2] = ArduinoCommunicator.GetSpeed();
                CurrentGPS[3] = ArduinoCommunicator.GetCourse();

                if(CurrentGPS[0] != PreviousGPS[0] || 
                    CurrentGPS[1] != PreviousGPS[1] ||
                    CurrentGPS[2] != PreviousGPS[2] ||
                    CurrentGPS[3] != PreviousGPS[3])
                {
                    Console.WriteLine("Writing new GPS coordinate to path.");
                    Logger.WriteGPSCoordinate(CurrentGPS[0], CurrentGPS[1]);
                }

                PreviousGPS[0] = CurrentGPS[0];
                PreviousGPS[1] = CurrentGPS[1];
                PreviousGPS[2] = CurrentGPS[2];
                PreviousGPS[3] = CurrentGPS[3];

                Thread.Sleep(500);
            }

            Console.WriteLine("ENDING");
            ArduinoCommunicator.Stop();
        }
    }
}
