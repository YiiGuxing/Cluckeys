using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Cluckeys
{
    internal static class Updater
    {
        private static bool _isChecking;

        private const string UpdatesUrl = "https://api.github.com/repos/YiiGuxing/Cluckeys/releases/latest";
        private const string ReleasesPageUrl = "https://github.com/YiiGuxing/Cluckeys/releases/latest";

        public static void CheckForUpdates(bool silent = true)
        {
            if (_isChecking)
                return;

            _isChecking = true;

            Task.Run(() =>
            {
                try
                {
                    var result = HttpRequest(UpdatesUrl);
                    var version = (string) result["tag_name"];

                    if (new Version(version.Substring(1)) <= Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        Application.Current.Dispatcher?.Invoke(() =>
                        {
                            _isChecking = false;
                            if (!silent)
                                TrayIconManager.ShowNotification("", "You are now on the latest version.");
                        });
                    }
                    else
                    {
                        Application.Current.Dispatcher?.Invoke(() =>
                        {
                            _isChecking = false;
                            TrayIconManager.ShowUpdatesAvailable(version);
                        });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        _isChecking = false;
                        if (!silent)
                            TrayIconManager.ShowNotification("",
                                $"Error occured when checking for updates: {e.Message}");
                    });
                }
            });
        }

        public static void OpenReleasesPageUrl()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ReleasesPageUrl,
                UseShellExecute = true
            });
        }

        private static dynamic HttpRequest(string url)
        {
            var web = new WebClient()
            {
                Proxy = WebRequest.DefaultWebProxy,
                Credentials = CredentialCache.DefaultCredentials
            };
            web.Headers.Add(HttpRequestHeader.UserAgent, "Wget/1.9.1");

            var response = new MemoryStream(web.DownloadData(url));
            var json = JsonConvert.DeserializeObject<dynamic>(new StreamReader(response).ReadToEnd());
            return json;
        }
    }
}