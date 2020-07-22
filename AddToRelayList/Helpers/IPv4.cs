using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace AddToRelayList.Helpers
{
    public static class IPv4
    {
        public static bool IsValidIp(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            return IPAddress.TryParse(ipString, out IPAddress _);
        }

        internal static bool IsValidMask(string mask)
        {
            if (String.IsNullOrWhiteSpace(mask))
            {
                return false;
            }

            string[] splitValues = mask.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            for(int i = 0; i < 4; i++)
            {

                if (Int32.TryParse(splitValues[i], out int x))
                {
                    if (!(x >= 0 && x <= 255))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
