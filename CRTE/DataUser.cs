using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CRTE
{
    [DataContract]

    public class DataUser
    {
        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string publicip { get; set; }
    }
}
