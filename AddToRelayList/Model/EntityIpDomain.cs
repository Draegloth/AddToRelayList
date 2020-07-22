﻿using System;
using System.Runtime.Serialization;

namespace AddToRelayList
{
    [DataContract]
    public class EntityIpDomain
    {
        /// <summary>
        /// Stores the value of the IP|Domain in a String
        /// </summary>
        [DataMember]
        public String IpDomain { get; set; }
    }
}
