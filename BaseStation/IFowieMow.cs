using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace BaseStation
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IFowieMow
    {

        [OperationContract]
        [WebInvoke(Method ="GET", ResponseFormat = WebMessageFormat.Json, BodyStyle =WebMessageBodyStyle.Wrapped,UriTemplate ="/json/{id}")]
        string GetJsonData(string id);

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/xml/{id}")]
        string GetXmlData(string id);

        [OperationContract]
        [WebInvoke(Method ="POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, UriTemplate = "/FowieMow")]
        FowieMowCommand PostJsonData(FowieMowCommand composite);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class FowieMowCommand
    {
        int commandId = 0;
        string commandData = "Empty";

        [DataMember]
        public int CommandId
        {
            get { return commandId; }
            set { commandId = value; }
        }

        [DataMember]
        public string CommandData
        {
            get { return commandData; }
            set { commandData = value; }
        }
    }
}
