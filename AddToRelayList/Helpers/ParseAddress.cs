using System;
using System.Collections.Generic;
using System.Linq;

namespace AddToRelayList.Helpers
{
    partial class ParseAddress
    {   
        public static string GetLine(string line)
        {
            string[] subStrings = line.Split(',');

            if (subStrings.Length == 1)
            {
                if (!IPv4.IsValidIp(subStrings[0].Trim()))
                {
                    return string.Empty;
                }

                return string.Format("{0}, 255.255.255.255", subStrings[0].Trim());
            }

            if (subStrings.Length == 2)
            {
                if (!IPv4.IsValidIp(subStrings[0].Trim()))
                {
                    return string.Empty;
                }

                if (!IPv4.IsValidMask(subStrings[1].Trim()))
                {
                    return string.Empty;
                }

                return string.Format("{0}, {1}", subStrings[0].Trim(), subStrings[1].Trim());
            }

            return string.Empty;
        }

        internal static List<EntityIpDomain> GetImsIp(string ipString)
        {
            if (string.IsNullOrEmpty(ipString))
                return new List<EntityIpDomain>();

            List<string> ips = ipString.Split(',').ToList();
            List<EntityIpDomain> imsIp = new List<EntityIpDomain>();

            foreach (string ip in ips)
            {
                if (IPv4.IsValidIp(ip.Trim()))
                {
                    EntityIpDomain entityIpDomain = new EntityIpDomain
                    {
                        IpDomain = string.Format("{0}, 255.255.255.255", ip.Trim())
                    };
                    imsIp.Add(entityIpDomain);
                }
            }

            return imsIp;
        }
    }
}
