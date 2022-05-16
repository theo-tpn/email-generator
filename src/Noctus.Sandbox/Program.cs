using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Noctus.Sandbox
{
    class Program
    {
        private const string LoginManageAccountQueryString =
            "wa=wsignin1.0&wp=SA_20MIN&wreply=https://account.live.com/proofs/manage/additional?refd=account.microsoft.com&refp=security&client_flight=m365.suiteheader";

        static async Task Main(string[] args)
        {
            var mail = "gaoooel@outlook.com";
            var usrname = new Regex("([a-z])([a-z])\\*{5}");
            var z = usrname.Match(body).Value;
            var s = z.Substring(0, 2);
            var u = mail.Substring(0, 2);
            if (string.Equals(u, s))
                Console.WriteLine("oui");
        }

        private static string body =>
            "Microsoft account\r\nSecurity code\r\nPlease use the following security code for the Microsoft account ga*****@outlook.com.\r\nSecurity code: 4152\r\nIf you didn't request this code, you can safely ignore this email. Someone else might have typed your email address by mistake.\r\nThanks,\r\nThe Microsoft account team";
    }

    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RetryCount { get; set; }
    }
}
