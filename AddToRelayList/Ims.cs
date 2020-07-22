using AddToRelayList.Helpers;
using AddToRelayList.Model;
using AddToRelayList.Model.Db;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddToRelayList
{
    class Ims : EventLogging
    {
        internal static List<EntityIpDomain> GetList()
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>();

            try
            {
                using (var db = new IMSEntities())
                {
                    var ips = (from p in db.v_SmtpInventoryIP where p.IP != null select new { p.IP }).Distinct().ToList();

                    foreach (var ip in ips)
                    {
                        try
                        {
                            List<EntityIpDomain> tmpList = ParseAddress.GetImsIp(ip.IP);

                            if(tmpList.Count > 0)
                                list.AddRange(tmpList);
                        }
                        catch (Exception ex)
                        {
                            string ErrorMessage = string.Format("Błąd odczytu bazy danych IMS: {0}", ex.Message);
                            Console.WriteLine(ErrorMessage);
                            log.Error(ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorMessage = string.Format("Brak połączenia z bazą danych IMS: {0}", ex.Message);
                Console.WriteLine(ErrorMessage);
                log.Error(ErrorMessage);
            }

            return list;
        }
    }
}
