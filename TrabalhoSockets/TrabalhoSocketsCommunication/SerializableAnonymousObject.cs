using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoSocketsCommunication
{
    [Serializable]
    public class SerializableAnonymousObject
    {
        public object Obj { get; set; }

        public SerializableAnonymousObject(object obj)
        {
            this.Obj = obj;
        }
    }
}
