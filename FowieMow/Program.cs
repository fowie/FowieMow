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
            BrainStem Stem = new BrainStem();
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
                    Stem.IssueCommand("1,10,10");
                }

                CurrentGPS[0] = Stem.GetLatitude();
                CurrentGPS[1] = Stem.GetLongitude();
                CurrentGPS[2] = Stem.GetSpeed();
                CurrentGPS[3] = Stem.GetCourse();

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
            Stem.Stop();
        }
    }
}
