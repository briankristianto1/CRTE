using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CRTE
{
    [DataContract]

    public class User
    {
        [DataMember]
        public string success { get; set; }

        [DataMember]
        public DataUser data { get; set; }
    }
}
