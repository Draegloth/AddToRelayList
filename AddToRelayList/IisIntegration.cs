using AddToRelayList.Helpers;
using AddToRelayList.Model;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Reflection;

namespace AddToRelayList
{
    public enum Result
    {
        Failure = -1,
        NotExist = 0,
        Exist = 1,
        OK = 2
    }

    public static class IisIntegration
    {
        private static object _oIpSecurity;
        private static Type _typeIpSecurityType;

        public static readonly string METABASE = "IIS://zhs-mxr-vm01.ispatcee.com/smtpsvc/1";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sets IP|Domain into SMTP Server
        /// This method is used to insert one IP or DNS entry into the Relay Restriction List in IIS
        /// </summary>
        /// <param name="sMetabasePath">IIS://localhost/smtpsvc/1</param>
        /// <param name="sMethodName">Get|Put</param>
        /// <param name="sMethodArgument">IPSecurity|RelayIPList</param>
        /// <param name="sMember">IPGrant|IPDeny|DomainGrant|DomainDeny</param>
        /// <param name="item">IP|Domain</param>
        /// <param name="listCurrent">List of current IP(s)|Domain(s)</param>
        /// <param name="aListNew">List of new IP(s)|Domain(s)</param>
        /// <returns></returns>
        public static Result SetIpSecurityPropertySingle(String sMetabasePath, String sMethodName, String sMethodArgument, String sMember, String item, out List<EntityIpDomain> listCurrent, out List<EntityIpDomain> aListNew)
        {
            aListNew = null;
            Result res = Result.Failure;

            using (DirectoryEntry directoryEntry = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    directoryEntry.RefreshCache();

                    _oIpSecurity = directoryEntry.Invoke(sMethodName, new object[] { sMethodArgument });
                    _typeIpSecurityType = _oIpSecurity.GetType();
                    Array aDataCurrent = GetIpSecurityData(_oIpSecurity, _typeIpSecurityType, sMember);
                    res = ListIpDomainAndCheckIfNewExists(aDataCurrent, item);
                    listCurrent = Lists.FromArray(aDataCurrent);

                    if (res == Result.NotExist)
                    {
                        Object[] oNewData = new object[aDataCurrent.Length + 1];
                        aDataCurrent.CopyTo(oNewData, 0);
                        oNewData.SetValue(item, aDataCurrent.Length);
                        _typeIpSecurityType.InvokeMember(sMember, BindingFlags.SetProperty, null, _oIpSecurity, new object[] { oNewData });
                        directoryEntry.Invoke(MethodName.Put, new[] { sMethodArgument, _oIpSecurity });
                        directoryEntry.CommitChanges();
                        directoryEntry.RefreshCache();
                        _oIpSecurity = directoryEntry.Invoke(MethodName.Get, new[] { sMethodArgument });
                        Array aDataNew = (Array)_typeIpSecurityType.InvokeMember(sMember, BindingFlags.GetProperty, null, _oIpSecurity, null);
                        aListNew = Lists.FromArray(aDataNew);

                        return (ListIpDomainAndCheckIfNewExists(aDataNew, item) == Result.Exist) ? Result.OK : Result.Failure;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    log.Error(string.Format("Błąd podczas importowania adresu \"{0}\" : {1}", item, ex.Message));
                    listCurrent = null;
                    return Result.Failure;
                }
            }

            return res;
        }

        /// <summary>
        /// Set IP(s)|Domain(s) into SMTP Server
        /// This method is used to insert multiple IPs or DNS entries into the Relay Restriction List in IIS
        /// </summary>
        /// <param name="sMetabasePath">IIS://localhost/smtpsvc/1</param>
        /// <param name="sMethodName">Get|Put</param>
        /// <param name="sMethodArgument">IPSecurity|RelayIPList</param>
        /// <param name="sMember">IPGrant|IPDeny|DomainGrant|DomainDeny</param>
        /// <param name="list">List of IP(s)\Domain(s)</param>
        /// 
        public static Result SetIpSecurityPropertyArray(String sMetabasePath, String sMethodName, String sMethodArgument, String sMember, List<EntityIpDomain> list)
        {
            List<EntityIpDomain> relayList = IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant);

            using (DirectoryEntry directoryEntry = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    directoryEntry.RefreshCache();
                    _oIpSecurity = directoryEntry.Invoke(sMethodName, new[] { sMethodArgument });
                    Type typeIpSecurityType = _oIpSecurity.GetType();
                    Object[] newList = new object[list.Count + relayList.Count];
                    Int32 iCounter = 0;

                    foreach (EntityIpDomain item in list)
                    {
                        newList[iCounter] = item.IpDomain;
                        iCounter++;
                    }

                    foreach (EntityIpDomain item in relayList)
                    {
                        newList[iCounter] = item.IpDomain;
                        iCounter++;
                    }

                    // add the updated list back to the IPSecurity object
                    typeIpSecurityType.InvokeMember(sMember, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, _oIpSecurity, new object[] { newList });

                    directoryEntry.Properties[sMethodArgument][0] = _oIpSecurity;

                    // commit the changes
                    directoryEntry.CommitChanges();
                    directoryEntry.RefreshCache();

                    return Result.OK;
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Błąd podczas importowania listy adresów z serwera SMTP: \"{0}\"", ex.Message));
                    return Result.Failure;
                }
            }
        }

        public static Result UpdateIpSecurityPropertyArray(String sMetabasePath, String sMethodName, String sMethodArgument, String sMember, List<EntityIpDomain> list)
        {
            using (DirectoryEntry directoryEntry = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    directoryEntry.RefreshCache();
                    _oIpSecurity = directoryEntry.Invoke(sMethodName, new[] { sMethodArgument });
                    Type typeIpSecurityType = _oIpSecurity.GetType();
                    Object[] newList = new object[list.Count];
                    Int32 iCounter = 0;

                    foreach (EntityIpDomain item in list)
                    {
                        newList[iCounter] = item.IpDomain;
                        iCounter++;
                    }

                    // add the updated list back to the IPSecurity object
                    typeIpSecurityType.InvokeMember(sMember, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, _oIpSecurity, new object[] { newList });

                    directoryEntry.Properties[sMethodArgument][0] = _oIpSecurity;

                    // commit the changes
                    directoryEntry.CommitChanges();
                    directoryEntry.RefreshCache();

                    return Result.OK;
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Błąd podczas update listy adresów na serwerze: \"{0}\"", ex.Message));
                    Mail.Send("dariusz.pieczara@arcelormittal.com", "", "", "ERROR!!! Błąd podczas update adresów.", string.Format("Błąd podczas update adresów.\n{0}", ex.Message));
                    return Result.Failure;
                }
            }
        }

        /// <summary>
        /// Retrieves the IP(s)|Domain(s) from the SMTP Server in an Array
        /// This method retrieves teh IPs/DNS entries from the Relay Restriction List in IIS
        /// </summary>
        /// <param name="sMetabasePath">IIS://localhost/smtpsvc/1</param>
        /// <param name="sMethodName">Get|Put</param>
        /// <param name="sMethodArgument">IPSecurity|RelayIPList</param>
        /// <param name="sMember">IPGrant|IPDeny|DomainGrant|DomainDeny</param>
        /// <returns></returns>
        /// 
        public static List<EntityIpDomain> GetIpSecurityPropertyArray(String sMetabasePath, String sMethodName, String sMethodArgument, String sMember)
        {
            List<EntityIpDomain> list = new List<EntityIpDomain>();

            using (DirectoryEntry directoryEntry = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    directoryEntry.RefreshCache();

                    object oIpSecurity = directoryEntry.Invoke(sMethodName, new[] { sMethodArgument });
                    Type typeIpSecurityType = oIpSecurity.GetType();
                    Array data = GetIpSecurityData(oIpSecurity, typeIpSecurityType, sMember);

                    for (int i = 0; i < data.Length; i++)
                    {
                        EntityIpDomain entityIpDomain = new EntityIpDomain { IpDomain = data.GetValue(i).ToString() };
                        list.Add(entityIpDomain);
                    }
                }
                catch (Exception ex)
                {
                    string ErrorMessage = string.Format("Błąd podczas pobierania listy adresów z serwera SMTP.", ex.Message);
                    Console.WriteLine(ex.Message);
                    log.Error(ErrorMessage);
                    Mail.Send("dariusz.pieczara@arcelormittal.com", "", "", "ERROR!!! Błąd podczas pobierania listy adresów z serwera SMTP", ErrorMessage);
                }
            }

            return list;
        }

        /// <summary>
        /// Retrieves a list of IPs or Domains
        /// //This is a helper method that actually returns an array of IPs/DNS entries from the Relay Restricton List in IIS
        /// </summary>
        /// <param name="oIpSecurity">Result of directoryEntry.Invoke</param>
        /// <param name="tIpSecurityType">Type of oIpSecurity</param>
        /// <param name="sMember">IPGrant|IPDeny|DomainGrant|DomainDeny</param>
        /// <returns>Array of IP(s)|Domain(s)</returns>
        private static Array GetIpSecurityData(object oIpSecurity, Type tIpSecurityType, String sMember)
        {
            return (Array)tIpSecurityType.InvokeMember(sMember, BindingFlags.GetProperty, null, oIpSecurity, null);
        }

        /// <summary>
        /// Lists the IP(s)|Domain(s)
        /// </summary>
        /// <param name="aData">Array of IP(s)|Domain(s)</param>
        /// <param name="sItem"></param>
        /// <returns>Stringbuilder of the list</returns>
        private static Result ListIpDomainAndCheckIfNewExists(Array aData, String sItem)
        {
            //StringBuilder stringBuilder = new StringBuilder();

            foreach (object oDataItem in aData)
            {
                //stringBuilder.Append(oDataItem + Environment.NewLine);
                if (oDataItem.ToString().StartsWith(sItem))
                {
                    return Result.Exist;
                }
            }

            return Result.NotExist;
        }

        enum StatusVirtualServerSMTP
        {
            Started = 2,
            Stopped = 4
        }

        public static void RestartSmtpServer(String sMetabasePath)
        {
            StopSmtpServer(sMetabasePath);
            System.Threading.Thread.Sleep(5000);
            StartSmtpServer(sMetabasePath);
        }

        public static bool ServerStarted
        {
            get
            {
                using (DirectoryEntry dir = new DirectoryEntry(METABASE))
                {
                    try
                    {
                        if (Convert.ToInt32(dir.Properties["SERVERSTATE"].Value) == (int)StatusVirtualServerSMTP.Started)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        log.Error(string.Format("Błąd podczas odczytu stanu usługi SMTP.", ex.Message));
                        return false;
                    }
                }
            }
        }

        public static bool StopSmtpServer(String sMetabasePath)
        {
            Console.Write("\nZatrzymywanie serwera SMTP..");

            using (DirectoryEntry dir = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    if (ServerStarted)
                    {
                        dir.Properties["SERVERSTATE"].Value = (int)StatusVirtualServerSMTP.Stopped;
                        dir.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    log.Error(string.Format("Błąd podczas zatrzymywania serwera SMTP.", ex.Message));
                    return false;
                }
            }

            Console.WriteLine(".zatrzymano.");
            log.Info(string.Format("Zatrzymano serwer SMTP."));

            return true;
        }

        public static bool StartSmtpServer(String sMetabasePath)
        {
            Console.Write("\nUruchamianie serwera SMTP..");

            using (DirectoryEntry dir = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    if (!ServerStarted)
                    {
                        dir.Properties["SERVERSTATE"].Value = (int)StatusVirtualServerSMTP.Started;
                        dir.CommitChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    log.Error(string.Format("Błąd podczas uruchamiania serwera SMTP.", ex.Message));
                    return false;
                }
            }

            Console.WriteLine(".uruchomiono.");
            log.Info(string.Format("Uruchomiono serwer SMTP."));

            return true;
        }

        public static bool CheckBeforeSynchronization
        {
            get
            {
                if (!IisIntegration.ServerStarted)
                {
                    Mail.Send("dariusz.pieczara@arcelormittal.com", "szymon.zygma@arcelormittal.com", "", "WARNING!!! Usługa SMTP na ZHS-MXR-VM01 jest zatrzymana - trwa próba uruchomienia....", "Na wszelki wypadek zaloguj się na serwer i sprawdź co się dzieje.");
                    IisIntegration.StartSmtpServer(IisIntegration.METABASE);

                    if (!IisIntegration.ServerStarted)
                    {
                        Mail.Send("dariusz.pieczara@arcelormittal.com", "szymon.zygma@arcelormittal.com", "", "ERROR!!! Usługa SMTP na ZHS-MXR-VM01 jest zatrzymana i nie udało się jej uruchomić.", "Lepiej zaloguj się na serwer i sprawdź co się dzieje bo będzie afera.");
                        return false;
                    }
                }
                return true;
            }
        }
        public static void PrintSmtpServerStatus(String sMetabasePath)
        {
            using (DirectoryEntry dir = new DirectoryEntry(sMetabasePath))
            {
                try
                {
                    switch (ServerStarted)
                    {
                        case false:
                            {
                                Console.WriteLine("Usługa SMTP jest zatrzymana.");
                                break;
                            }
                        case true:
                            {
                                Console.WriteLine("Usługa SMTP jest uruchomiona.");
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    log.Error(string.Format("Błąd podczas odczytu stanu usługi SMTP.", ex.Message));
                }
            }
        }
    }
}
