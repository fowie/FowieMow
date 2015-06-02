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
        Kml myKml;

        public DataLogging(string GPSfilePath)
        {
            GPSFilePath = GPSfilePath;
            myKml = new Kml();
            myKml.AddNamespacePrefix(KmlNamespaces.GX22Prefix, KmlNamespaces.GX22Namespace);
            myKml.Feature = new Document();
        }

        ~DataLogging()
        {
            SaveToKML();
        }

        public void SaveToKML()
        {
            Placemark pm = new Placemark();
            pm.Name = "FowieMow Path";
            pm.Geometry = GPSTrack;
            ((Document)myKml.Feature).AddFeature(pm);
            
            KmlFile kmlFile = KmlFile.Create(myKml, true);
            
            using (FileStream stream = File.OpenWrite(GPSFilePath + Path.DirectorySeparatorChar + DateTime.Now.ToFileTimeUtc().ToString() + ".kml"))
            {
                kmlFile.Save(stream);
            }
        }

        public void WriteGPSCoordinate(double lat, double lon)
        {
            /*Point newPoint = new Point();
            newPoint.Coordinate = new Vector(lat, lon);
            Placemark placemark = new Placemark();
            placemark.Geometry = newPoint;
            ((Document)myKml.Feature).AddFeature(placemark);*/
            GPSTrack.AddCoordinate(new Vector(lat, lon));
        }
    }
}
