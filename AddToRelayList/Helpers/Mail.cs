using AddToRelayList.Model;
using System;
using System.Net.Mail;

namespace AddToRelayList.Helpers
{
    class Mail : EventLogging
    {
        public static bool Send(string recipient, string copy, string bcopy, string subject, string body)
        {
            bool retValue = false;
            SmtpClient client = new SmtpClient("zhs-mxr-vm01.ispatcee.com");
            MailAddress from = new MailAddress("AddToRelayList@arcelormittal.com", "AddToRelayList (" + System.Environment.MachineName + ")", System.Text.Encoding.ASCII);

            MailAddress to = new MailAddress(recipient);
            MailMessage message = new MailMessage(from, to);

            if (copy.Length > 0)
            {
                MailAddress cc = new MailAddress(copy);
                message.CC.Add(cc);
            }

            if (bcopy.Length > 0)
            {
                MailAddress bcc = new MailAddress(bcopy);
                message.Bcc.Add(bcc);
            }

            message.BodyEncoding = System.Text.Encoding.UTF8;
            message.Body = body;
            message.Subject = subject;

            try
            {
                client.Send(message);
                retValue = true;
            }
            catch (Exception e)
            {
                log.Error(string.Format("Błąd podczas wysyłania wiadomości e-mail", e.Message));
                retValue = false;
            }
            message.Dispose();

            return retValue;
        }
    }
}
