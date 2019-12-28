using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace Cluckeys
{
    internal static class AutoStartupHelper
    {
        private static readonly string AppPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");

        private static readonly string StartupFullPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                Path.ChangeExtension(Path.GetFileName(AppPath), ".lnk"));

        internal static bool CreateAutoRunShortcut()
        {
            try
            {
                RemoveAutoRunShortcut();

                var lnk = ShellLinkFactory.GetNewInstance();

                lnk.SetPath(AppPath);
                lnk.SetArguments("/autorun"); // silent
                lnk.SetIconLocation(AppPath, 0);
                lnk.SetWorkingDirectory(Path.GetDirectoryName(AppPath) ?? "");
                // ReSharper disable once SuspiciousTypeConversion.Global
                (lnk as IPersistFile)?.Save(StartupFullPath, false);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TrayIconManager.ShowNotification("", "Failed to add Cluckeys to Startup folder.");
            }

            return false;
        }

        internal static bool RemoveAutoRunShortcut()
        {
            try
            {
                File.Delete(StartupFullPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                TrayIconManager.ShowNotification("", "Failed to delete Cluckeys startup shortcut.");
            }

            return false;
        }

        internal static bool IsAutoRun()
        {
            return File.Exists(StartupFullPath);
        }
    }
}