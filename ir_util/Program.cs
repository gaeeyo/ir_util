using System;
using System.Net;
using System.Configuration;
using static IrUtilApp.IrUtil;

namespace IrUtilApp {
    class Program {

        static void MainFunc() {
            var settings = ConfigurationManager.AppSettings;

            int port = Convert.ToInt32(settings["listenPort"]);
            string csvPath = settings["csvPath"];

            try {
                new IrUtil(port, csvPath).Start();
            } catch (HttpListenerException e) {
                Log(e.StackTrace);
                Log("参照: https://stackoverflow.com/questions/4019466/httplistener-access-denied");
                Log($"netsh http add urlacl url = http://+:{port}/ user=Everyone");
            }
        }

        static void Main(string[] args) {
            MainFunc();
        }
    }
}
