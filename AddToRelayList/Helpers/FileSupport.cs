using AddToRelayList.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddToRelayList.Helpers
{
    class FileSupport : EventLogging
    {
        public static string ErrorDescription { get; private set; }
        readonly static string permanentFileName = @"PermanentList(do not remove).txt";
        internal static void ImportFromFile(string myFileName)
        {
            int validLines = 0;
            int counter = 0;

            List<EntityIpDomain> list = GetListFromFile(myFileName);

            if (list.Count > 0)
            {
                ErrorDescription = string.Empty;
                List<EntityIpDomain> newList = new List<EntityIpDomain>();
                List<EntityIpDomain> currentList = IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant);

                foreach (var ip in list)
                {
                    counter++;
                    Result result = IisIntegration.SetIpSecurityPropertySingle(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant, ip.IpDomain, out currentList, out newList);

                    switch (result)
                    {
                        case Result.Failure:
                            Console.WriteLine(ErrorDescription = string.Format("Wiersz nr {0} - błąd podczas importu adresu IP: \"{1}\"", counter, ip.IpDomain));
                            log.Error(ErrorDescription);
                            break;
                        case Result.NotExist:
                            break;
                        case Result.Exist:
                            Console.WriteLine(ErrorDescription = string.Format("Wiersz nr {0} - urządzenie o adresie IP: \"{1}\" już jest dodana do listy relay.", counter, ip.IpDomain));
                            log.Info(ErrorDescription);
                            break;
                        case Result.OK:
                            Console.WriteLine(ErrorDescription = string.Format("Wiersz nr {0} - pomyślnie zaimportowano adres IP: \"{1}\" do listy relay.", counter, ip.IpDomain));
                            log.Info(ErrorDescription);
                            validLines++;
                            break;
                    }
                }
            }

            Console.WriteLine(string.Format("Wykonano import z pliku: {0}", myFileName));
            Console.WriteLine(string.Format("      Odczytano wierszy: {0}", counter));
            Console.WriteLine(string.Format("Zaimportowanych adresów: {0}", validLines));
            log.Info(string.Format("Odczytanych wierszy: {0}. Zaimportowanych adresów: {1}", counter, validLines));
        }

        private static List<EntityIpDomain> GetListFromFile(string myFileName)
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>();
            string line;

            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(myFileName))
                {
                    while ((line = file.ReadLine()) != null)
                    {
                        string ip = ParseAddress.GetLine(line.Replace("\"", "").Replace(" ", ""));

                        if (!string.IsNullOrEmpty(ip))
                        {
                            list.Add(new EntityIpDomain { IpDomain = ip });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string ErrorDescription = string.Format("\nBłąd podczas odczytu pliku!!!\n{0}", ex.Message);
                Console.WriteLine(ErrorDescription);
                log.Error(ErrorDescription);
                Mail.Send("dariusz.pieczara@arcelormittal.com", "", "", string.Format("ERROR!!! Błąd podczas pobierania listy z pliku {0}.", myFileName), string.Format("Błąd podczas pobierania listy z pliku {0}.\n{1}", myFileName, ex.Message));
            }

            return list;
        }

        internal static bool Synchronize()
        {
            log.Info("Uruchomiono program z opcją \"synchronize\"");
            if (!IisIntegration.CheckBeforeSynchronization)
            {
                return false;
            }

            UpdatePermanentList();
            List<EntityIpDomain> compareList = Lists.Zip(GetListFromFile(permanentFileName), Ims.GetList());

            List<EntityIpDomain> iisList = IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant);
            List<EntityIpDomain> compToIms = Lists.GetDifference(compareList, iisList);
            List<EntityIpDomain> imsToComp = Lists.GetDifference(iisList, compareList);
            
            if ((compToIms.Count > 0) || (imsToComp.Count > 0))
            {
                IisIntegration.UpdateIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant, compareList);
                string ErrorDescription = string.Format("Dodane do IMS adresy: {0}. Usunięte w IMS adresy: {1}", Lists.ToString(compToIms), Lists.ToString(imsToComp));
                Console.WriteLine(ErrorDescription);
                log.Info(ErrorDescription);
            }
            log.Info("Zakończono synchronizację.");

            return true;
        }

        private static void UpdatePermanentList()
        {            
            List<EntityIpDomain> permanentList = GetListFromFile(permanentFileName);
            List<EntityIpDomain> diffList = Lists.RemoveNotUnique(permanentList, Ims.GetList());
            ExportToFile(permanentFileName, diffList, false);
        }

        internal static void AddFromArg(string hostIp)
        {
            try
            {
                ErrorDescription = string.Empty;
                List<EntityIpDomain> newList = new List<EntityIpDomain>();
                List<EntityIpDomain> currentList = IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant);

                string ip = ParseAddress.GetLine(hostIp.Replace("\"", "").Replace(" ", ""));

                if (!string.IsNullOrEmpty(ip))
                {
                    Result result = IisIntegration.SetIpSecurityPropertySingle(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant, ip, out currentList, out newList);

                    switch (result)
                    {
                        case Result.Failure:
                            Console.WriteLine(ErrorDescription = string.Format("Błąd podczas importu adresu IP: {0}", ip));
                            log.Error(ErrorDescription);
                            break;
                        case Result.NotExist:
                            break;
                        case Result.Exist:
                            Console.WriteLine(ErrorDescription = string.Format("Adres IP: \"{0}\" już jest dodany do listy relay.", ip));
                            log.Info(ErrorDescription);
                            break;
                        case Result.OK:
                            Console.WriteLine(ErrorDescription = string.Format("Zaimportowano adres IP: \"{0}\"", ip));
                            log.Info(ErrorDescription);
                            break;
                    }
                }
                else
                {                    
                    Console.WriteLine(ErrorDescription = string.Format("Nieprawidłowy format adresu: \"{0}\"", ip));
                    log.Error(ErrorDescription);
                }
            }
            catch (Exception ex)
            {
                ErrorDescription = string.Format("Nie udało się dodać adresu IP: {0}", ex.Message);
                Console.WriteLine(ErrorDescription);
                log.Error(ErrorDescription);
            }
        }

        internal static bool ExportToFile(string myFileName, List<EntityIpDomain> ipList, bool verbose = true)
        {
            int counter = 0;

            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(myFileName))
                {
                    foreach (var ip in ipList)
                    {
                        file.WriteLine(ip.IpDomain);
                        counter++;
                    }
                }
                if (verbose)
                {
                    Console.WriteLine(string.Format("Wykonano eksport do pliku: {0}", myFileName));
                    Console.WriteLine(string.Format("       Zapisanych wierszy: {0}", counter));
                    log.Info(string.Format("Wykonano eksport do pliku: {0}. Zapisanych adresów: {1}", myFileName, counter));
                }
            }
            catch (Exception ex)
            {
                ErrorDescription = string.Format("Błąd podczas próby zapisu listy do pliku: {0}, {1}", myFileName, ex.Message);
                Console.WriteLine(ErrorDescription);
                log.Error(ErrorDescription);
                Mail.Send("dariusz.pieczara@arcelormittal.com", "", "", string.Format("ERROR!!! Błąd podczas zapisu listy do pliku {0}.", myFileName), string.Format("Błąd podczas zapisu do pliku {0}.\n{1}", myFileName, ex.Message));

                return false;
            }

            return true;
        }
    }
}
