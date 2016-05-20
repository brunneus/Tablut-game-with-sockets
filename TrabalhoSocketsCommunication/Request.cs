using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrabalhoSocketsEngine;

namespace TrabalhoSocketsCommunication
{
    [Serializable]
    public class Request
    {
       public eRequestType Type { get; set; }
       public object ClientParameterValue { get; set; }
       public object ReplyServerValue { get; set; }
    }
}
