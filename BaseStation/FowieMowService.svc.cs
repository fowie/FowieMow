using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using FowieMow;

namespace BaseStation
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FowieMowService : IFowieMow
    {
        ~FowieMowService()
        {
            // This gets called for each thread.  Can't stop the communicator here.... ArduinoCommunicator.Stop();
        }

        public string GetJsonData(string id)
        {
            String response = "";
            if (id == "GPS")
            {
                // Make sure we're connected to the Arduino
                if(!ArduinoCommunicator.Start())
                {
                    // False means we just connected, so no data is available yet
                    response = "No data available";
                }
                else
                {
                    // True means we were already connected
                    response = String.Format("Lat: {0}, Lon: {1}", ArduinoCommunicator.GetLatitude(), ArduinoCommunicator.GetLongitude());
                }
                return response;
            }

            return string.Format("You entered id {0}", id);
        }

        public string GetXmlData(string id)
        {
            return string.Format("You entered id {0}", id);
        }

        public FowieMowCommand PostJsonData(FowieMowCommand composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.CommandId == 0)
            {
                composite.CommandData = "NEW COMMAND";
            }
            return composite;
        }
    }
}
