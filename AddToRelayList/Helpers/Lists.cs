using System;
using System.Collections.Generic;
using System.Linq;

namespace AddToRelayList.Helpers
{
    class Lists
    {
        internal static List<EntityIpDomain> GetDifference(List<EntityIpDomain> baseList, List<EntityIpDomain> toCheckList)
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>();
            Array toCheckArray = ToArray(toCheckList);
            Array baseArray = ToArray(baseList);

            foreach(string ip in baseArray)
            {
                if(Array.IndexOf(toCheckArray, ip) < 0)
                {
                    list.Add(new EntityIpDomain { IpDomain = ip });
                }
            }

            return list;
        }

        internal static List<EntityIpDomain> RemoveNotUnique(List<EntityIpDomain> baseList, List<EntityIpDomain> toCheckList)
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>();
            Array toCheckArray = ToArray(toCheckList);
            Array baseArray = ToArray(baseList);

            foreach (string ip in baseArray)
            {
                if (Array.IndexOf(toCheckArray, ip) < 0)
                {
                    list.Add(new EntityIpDomain { IpDomain = ip });
                }
            }

            return list;
        }

        internal static List<EntityIpDomain> Zip(List<EntityIpDomain> baseList, List<EntityIpDomain> toCmpList)
        {
            Array baseArray = ToArray(baseList);
            Array imsArray = ToArray(toCmpList);

            foreach (string ip in imsArray)
            {
                if (Array.IndexOf(baseArray, ip) < 0)
                {
                    if (!string.IsNullOrEmpty(ParseAddress.GetLine(ip)))
                    {
                        baseList.Add(new EntityIpDomain { IpDomain = ip });
                    }
                }
            }

            return baseList;
        }

        internal static Array ToArray(List<EntityIpDomain> ipList)
        {
            List<string> list = new List<string>();

            foreach (var ip in ipList)
            {
                list.Add(ip.IpDomain);
            }

            return list.Distinct().ToList().ToArray();
        }

        internal static List<EntityIpDomain> FromArray(Array aData)
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>(aData.Length);

            foreach (String item in aData)
            {
                list.Add(new EntityIpDomain { IpDomain = item });
            }

            return list;
        }

        internal static string ToString(List<EntityIpDomain> ipList)
        {
            string result = string.Empty;

            foreach (var ip in ipList)
            {
                result = string.Format("{0}, {1}", result, ip.IpDomain);
            }

            return result;
        }
    }
}
