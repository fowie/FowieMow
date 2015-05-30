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
            Console.WriteLine("Done!");

            while(Console.ReadKey().Key != ConsoleKey.Q)
            {
                Thread.Sleep(50);
            }
        }
    }
}
