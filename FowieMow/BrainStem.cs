using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.IO.Ports;

namespace FowieMow
{
    /// <summary>
    /// The brain stem controls the basic motor functions of the robot
    /// Movement, obstace detection, awareness of surroundings, etc...
    /// That is what the Arduino is for.
    /// 
    /// </summary>
    class BrainStem
    {
        SerialPort Arduino;
        Thread StemThread;
        private static Mutex GPSDataMutex = new Mutex();
        private static Mutex BatteryDataMutex = new Mutex();
        private Boolean ThreadRunning = false;

        //!!!These values should only be accessed after obtaining the GPSDataMutex!!!
        double Latitude;
        double Longitude;
        DateTime UTC;
        double Speed;
        double Course;
        double BatteryVoltage;

        public BrainStem()
        {
            // Initialize the connection
            Arduino = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            try
            {
                Arduino.Open();
                Console.WriteLine("Successfully connected.");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Could not open port.  Is another application using the port already?");
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR connecting to Arduino!");
                Console.WriteLine(e.GetType());
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine("Starting Arduino communication thread");
            ThreadRunning = true;
            StemThread = new Thread(new ParameterizedThreadStart(BrainStemThread));
            StemThread.Start(Arduino);
        }

        ~BrainStem()
        {
            ThreadRunning = false;

            StemThread.Join();
        }

        void BrainStemThread(object ard)
        {
            SerialPort Arduino = (SerialPort)ard;

            // Main threadloop
            while (ThreadRunning)
            { 
                // Verify port is still open
                if(!Arduino.IsOpen)
                {
                    // die?
                    break;
                }

                // wait for data
                while (Arduino.BytesToRead <= 0 && ThreadRunning)
                {
                    Thread.Sleep(50);
                }

                string data = Arduino.ReadLine();
                Console.WriteLine("Got data: " + data);
                // parse data
                int commandEnd = data.LastIndexOf("~~") + 2;
                if (commandEnd > 0)
                {
                    var parts = data.Split(',');
                    string command = parts[0];
                    // update variables
                    if (command == "~~GPRMC~~")
                    {
                        //data == 052930.000,A,4744.198215,N,12157.797884,W,0.00,0.00,300515,,E,A
                        //        1          2 3           4 5            6 7    8    9
                        bool status = parts[2] == "A" ? true : false;

                        if (status)
                        {
                            //Console.WriteLine("Latitude: " + parts[3]);
                            //Console.WriteLine("Longitude: " + parts[5]);
                            //Console.WriteLine("Speed: " + parts[7]);
                            //Console.WriteLine("Course: " + parts[8]);
                            //Console.WriteLine("Date: " + parts[9]);                            

                            GPSDataMutex.WaitOne();
                            {
                                // Protected code
                                Latitude = Convert.ToDouble(parts[3]);
                                Longitude = Convert.ToDouble(parts[5]);
                                DateTime attempt = new DateTime();
                                bool success = DateTime.TryParseExact(parts[1] + " " + parts[9], "HHmmss.000 ddMMyy", CultureInfo.CurrentCulture,
                                    DateTimeStyles.AllowInnerWhite | DateTimeStyles.AssumeUniversal, out attempt);
                                if (success)
                                {
                                    //Console.WriteLine("Got UTC: " + UTC.ToLongDateString() + " " + UTC.ToLongTimeString());
                                    UTC = attempt;
                                }
                                Speed = Convert.ToDouble(parts[7]);
                                Course = Convert.ToDouble(parts[8]);
                            }
                            GPSDataMutex.ReleaseMutex();
                        }
                    }
                    else if (command == "~~STATUS~~")
                    {
                        Console.WriteLine(parts[1]);
                    }
                    else if (command == "~~BATT~~")
                    {
                        Console.WriteLine("Voltage: " + parts[1]);
                        BatteryDataMutex.WaitOne();
                        {
                            BatteryVoltage = Convert.ToDouble(parts[1]);
                        }
                        BatteryDataMutex.ReleaseMutex();
                    }
                    else
                    {
                        Console.WriteLine("Got command: " + command);
                    }
                }
            }

            Console.WriteLine("Thread terminating.");
            if (Arduino.IsOpen)
            {
                Console.WriteLine("Closing Arduino comm");
                Arduino.Close();
            }
        }
    }
}
