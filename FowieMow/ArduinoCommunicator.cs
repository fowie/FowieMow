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
    static class ArduinoCommunicator
    {
        private static SerialPort Arduino;
        private static Thread ArduinoThread;
        private static Mutex GeneralThreadMutex = new Mutex();
        private static Mutex GPSDataMutex = new Mutex();
        private static Mutex BatteryDataMutex = new Mutex();
        private static Mutex StatusDataMutex = new Mutex();
        private static Mutex CommandQueueMutex = new Mutex();

        //!!!This value should only be accessed after obtaining the GeneralThreadMutex!!!
        private static Boolean ThreadRunning = false;

        //!!!These values should only be accessed after obtaining the GPSDataMutex!!!
        private static double Latitude;
        private static double Longitude;
        private static DateTime UTC;
        private static double Speed;
        private static double Course;

        //!!!!Only access after obtaining BatteryDataMutex
        private static double BatteryVoltage;

        //!!!!Only access after obtaining StatusDataMutex
        private static string BrainStemLatestStatus = "";

        //!!!!Only access after obtaining CommandQueueMutex
        private static Queue<string> CommandQueue = new Queue<string>();

        static ArduinoCommunicator()
        {
            Connect();
            Console.WriteLine("Starting Arduino communication thread");
            StartThread();
        }

        public static bool Start()
        {
            if (!Arduino.IsOpen)
            {
                Connect();
                return false;
            }
            if (!ThreadRunning)
            {
                StartThread();
                return false;
            }
            return true;
        }

        public static void Stop()
        {
            GeneralThreadMutex.WaitOne();
            ThreadRunning = false;
            GeneralThreadMutex.ReleaseMutex();

            ArduinoThread.Join();
        }

        private static void Connect()
        {
            Arduino = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
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
        }

        private static void StartThread()
        {
            ArduinoThread = new Thread(new ParameterizedThreadStart(ArduinoCommThread));
            ArduinoThread.Start(Arduino);
        }

        public static double GetLatitude()
        {
            GPSDataMutex.WaitOne();
            double retVal = Latitude;
            GPSDataMutex.ReleaseMutex();
            return retVal;
        }
        public static double GetLongitude()
        {
            GPSDataMutex.WaitOne();
            double retVal = Longitude;
            GPSDataMutex.ReleaseMutex();
            return retVal;
        }
        public static double GetSpeed()
        {
            GPSDataMutex.WaitOne();
            double retVal = Speed;
            GPSDataMutex.ReleaseMutex();
            return retVal;
        }
        public static double GetCourse()
        {
            GPSDataMutex.WaitOne();
            double retVal = Course;
            GPSDataMutex.ReleaseMutex();
            return retVal;
        }
        public static double GetBatteryVoltage()
        {
            BatteryDataMutex.WaitOne();
            double retVal = BatteryVoltage;
            BatteryDataMutex.ReleaseMutex();
            return retVal;
        }
        public static string GetStatus()
        {
            StatusDataMutex.WaitOne();
            string retVal = BrainStemLatestStatus;
            StatusDataMutex.ReleaseMutex();
            return retVal;
        }
        public static int IssueCommand(string command)
        {
            CommandQueueMutex.WaitOne();
            CommandQueue.Enqueue(command);
            int len = CommandQueue.Count;
            CommandQueueMutex.ReleaseMutex();
            return len;
        }

        static void ArduinoCommThread(object ard)
        {

            SerialPort Arduino = (SerialPort)ard;

            // Main threadloop
            while (ThreadRunning)
            {
                // Verify port is still open
                if (!Arduino.IsOpen)
                {
                    // die?
                    break;
                }

                if (!ThreadRunning)
                {
                    break;
                }

                if (Arduino.BytesToRead > 0)
                {
                    ProcessData(Arduino.ReadLine());
                }

                Thread.Sleep(50);
            }
            Console.WriteLine("Thread terminating.");
            if (Arduino.IsOpen)
            {
                Console.WriteLine("Closing Arduino comm");
                Arduino.Close();
            }
        }

        private static Boolean TransmitCommand(string command)
        {
            int timeouts = 0;

            command = "CMD:" + command + ":CMDEND";

            if (Arduino.IsOpen)
            {
                Console.WriteLine("Transmitting: " + command);
                Arduino.WriteLine(command);
                while(Arduino.BytesToRead >= 0 && timeouts < 10)
                {
                    timeouts++;
                    Thread.Sleep(50);
                }
                if(Arduino.BytesToRead > 0)
                {
                    string retVal = Arduino.ReadLine();
                    if(retVal.Contains("ACK"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void ProcessData(string data)
        {
            Console.WriteLine("Got data: " + data);
            // parse data
            int commandEnd = data.LastIndexOf("~~") + 2;
            if (commandEnd > 0)
            {
                var parts = data.Split(',');
                string command = parts[0];
                //Console.WriteLine("Got command: [" + command + "]");
                // update variables
                if (command.Equals("~~GPRMC~~"))
                {
                    if (parts.Length < 3)
                    {
                        return;
                    }
                    //data == 052930.000,A,4744.198215,N,12157.797884,W,0.00,0.00,300515,,E,A
                    //        1          2 3           4 5            6 7    8    9
                    bool status = parts[2] == "A" ? true : false;

                    if (status && parts.Length >= 10)
                    {
                        //Console.WriteLine("Latitude: " + parts[3]);
                        //Console.WriteLine("Longitude: " + parts[5]);
                        //Console.WriteLine("Speed: " + parts[7]);
                        //Console.WriteLine("Course: " + parts[8]);
                        //Console.WriteLine("Date: " + parts[9]);                            

                        GPSDataMutex.WaitOne();
                        {
                            // Protected code
                            // Lat is returned in the format ddmm.mmmmmm
                            // Lon is returned in the format dddmm.mmmmmm
                            String LatDeg = parts[3].Substring(0, 2);
                            String LatMins = parts[3].Substring(2);
                            String LonDeg = parts[5].Substring(0, 3);
                            String LonMins = parts[5].Substring(3);
                            Latitude = Convert.ToDouble(LatDeg) + (Convert.ToDouble(LatMins) / 60.0);
                            if(parts[4].Equals("S"))
                            {
                                Latitude *= -1;
                            }
                            Longitude = Convert.ToDouble(LonDeg) + (Convert.ToDouble(LonMins) / 60.0);
                            if(parts[6].Equals("W"))
                            {
                                Longitude *= -1;
                            }
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
                else if (command.Equals("~~STATUS~~"))
                {
                    StatusDataMutex.WaitOne();
                    BrainStemLatestStatus = string.Copy(parts[1]);
                    StatusDataMutex.ReleaseMutex();
                    Console.WriteLine(parts[1]);
                }
                else if (command.Equals("~~BATT~~"))
                {
                    Console.WriteLine("Voltage: " + parts[1]);
                    BatteryDataMutex.WaitOne();
                    {
                        BatteryVoltage = Convert.ToDouble(parts[1]);
                    }
                    BatteryDataMutex.ReleaseMutex();
                }
                else if (command.Equals("~~RDY~~"))
                {
                    CommandQueueMutex.WaitOne();
                    if (CommandQueue.Count > 0)
                    {
                        Console.WriteLine("Issuing a command:");
                        if(TransmitCommand(CommandQueue.Dequeue()))
                        {
                            Console.WriteLine("Command received successfully");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Sending NOP");
                        if(TransmitCommand("99"))
                        {
                            Console.WriteLine("Command received successfully");
                        }
                    }
                    CommandQueueMutex.ReleaseMutex();
                }
                else
                {
                    //Console.WriteLine("Got command: " + command);
                }
            }
        }
    }
}
