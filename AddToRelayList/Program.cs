using AddToRelayList.Helpers;
using AddToRelayList.Model;
using System;
using System.Collections.Generic;

namespace AddToRelayList
{
    partial class Program
    {
        
        private static readonly string METABASE = "IIS://zhs-mxr-vm01.ispatcee.com/smtpsvc/1";
        private static void PrintHelp()
        {
            Console.WriteLine();
            Console.WriteLine(string.Format("{0} [--add \"IP, mask\"] [--export filename] [--exportFromIms filename] [--compare filename] [--import filename] [--status] [--start] [--stop] [--restart]", Environment.GetCommandLineArgs()[0]));
            Console.WriteLine("Przykłady użycia programu:");
            Console.WriteLine(string.Format("\t--add \"10.9.121.210, 255.255.255.255\"              Dodanie pojedyńczego adresu"));
            Console.WriteLine(string.Format("\t--export myBackup.txt                              Eksport adresów dodanych do relaya"));
            Console.WriteLine(string.Format("\t--exportFromIms imsBackup.txt                      Eksport adresów z IMS"));
            Console.WriteLine(string.Format("\t--import IpList.txt                                Import adresów z pliku tekstowego"));
            Console.WriteLine(string.Format("\t--synchronize                                      Synchronizacja (zaciągnięcie) adresów z IMS i zaktualizowanie wpisów w Connection na serwerze SMTP."));
            Console.WriteLine(string.Format("\t--status                                           Status usługi SMTP"));
            Console.WriteLine(string.Format("\t--stop                                             Zatrzymanie usługi SMTP"));
            Console.WriteLine(string.Format("\t--start                                            Uruchomienie usługi SMTP"));
            Console.WriteLine(string.Format("\t--restart                                          Restart usługi SMTP"));
            Console.WriteLine(string.Format("\t--compareImsToRelay ImsToRelayDifference.txt       Eksport różnicy między SMTP relay a IMS (adresy których brakuje na serwerze SMTP)"));
            Console.WriteLine(string.Format("\t--compareRelayToIms RelayToImsDifference.txt       Eksport różnicy między IMS a SMTP relay (adresy których brakuje w IMS, tak możesz utworzyć listę Permanent)"));
            Console.WriteLine(string.Format("\t--help                                             Wyświetlenie powyższej pomocy."));
        }
        static int Main(string[] args)
        {
            if (args.Length == 1)
            {
                switch (Environment.GetCommandLineArgs()[1].ToLower())
                {
                    case "--status":
                        {
                            IisIntegration.PrintSmtpServerStatus(METABASE);
                            break;
                        }
                    case "--stop":
                        {
                            IisIntegration.StopSmtpServer(METABASE);
                            break;
                        }
                    case "--start":
                        {
                            IisIntegration.StartSmtpServer(METABASE);
                            break;
                        }
                    case "--restart":
                        {
                            IisIntegration.RestartSmtpServer(METABASE);
                            break;
                        }
                    case "--synchronize":
                        {                            
                            FileSupport.Synchronize();
                            break;
                        }
                    case "--help":
                        {
                            PrintHelp();
                            break;
                        }
                    default:
                        {
                            PrintHelp();
                            return 0;
                        }
                }
            }
            else if (args.Length == 2)
            {
                switch (Environment.GetCommandLineArgs()[1].ToLower())
                {
                    case "--exportfromims":
                        {
                            List<EntityIpDomain> imsList = Ims.GetList();
                            FileSupport.ExportToFile(Environment.GetCommandLineArgs()[2].Trim(), imsList, true);
                            break;
                        }
                    case "--export":
                        {
                            List<EntityIpDomain> relayList = IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant);
                            FileSupport.ExportToFile(Environment.GetCommandLineArgs()[2].Trim(), relayList, true);
                            break;
                        }
                    case "--import":
                        {
                            FileSupport.ImportFromFile(Environment.GetCommandLineArgs()[2].Trim());
                            break;
                        }
                    case "--add":
                        {
                            FileSupport.AddFromArg(Environment.GetCommandLineArgs()[2].Trim());
                            break;
                        }
                    case "--compareimstorelay":
                        {
                            List<EntityIpDomain> diffList = Lists.GetDifference(Ims.GetList(), IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant));
                            FileSupport.ExportToFile(Environment.GetCommandLineArgs()[2].Trim(), diffList, true);
                            break;
                        }
                    case "--comparerelaytoims":
                        {
                            List<EntityIpDomain> diffList = Lists.GetDifference(IisIntegration.GetIpSecurityPropertyArray(IisIntegration.METABASE, MethodName.Get, MethodArgument.IPSecurity, Member.IPGrant), Ims.GetList());
                            FileSupport.ExportToFile(Environment.GetCommandLineArgs()[2].Trim(), diffList, true);
                            break;
                        }
                    default:
                        {
                            PrintHelp();
                            return 0;
                        }
                }
            }
            else
            {
                PrintHelp();
                return 0;
            }

            return 0;
        }
    }
}
