[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace AddToRelayList.Model
{
    public class EventLogging
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
