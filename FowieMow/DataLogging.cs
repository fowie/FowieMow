using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpKml.Engine;
using SharpKml.Base;
using SharpKml.Dom;
using System.IO;

namespace FowieMow
{
    /// <summary>
    /// Logs data from the mower to the cloud so it is accessible anywhere
    /// </summary>
    class DataLogging
    {
        SharpKml.Dom.GX.Track GPSTrack = new SharpKml.Dom.GX.Track();
        string GPSFilePath;

        public DataLogging(string GPSfilePath)
        {
            GPSFilePath = GPSfilePath;
        }

        ~DataLogging()
        {
            SaveToKML();
        }

        public void SaveToKML()
        {
            //var kml = new Kml();
            //kml.AddNamespacePrefix(KmlNamespaces.GX22Prefix, KmlNamespaces.GX22Namespace);
            //kml.Feature = GPSTrack;
            KmlFile kmlFile = KmlFile.Create(GPSTrack, true);
            using (FileStream stream = File.OpenWrite(GPSFilePath + Path.DirectorySeparatorChar + DateTime.Now.ToFileTimeUtc().ToString() + ".kml"))
            {
                kmlFile.Save(stream);
            }
        }

        public void WriteGPSCoordinate(double lat, double lon)
        {
            GPSTrack.AddCoordinate(new Vector(lat, lon));
        }
    }
}
