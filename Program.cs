using System.Diagnostics;
using System.IO;

namespace WinZoPNG
{
  internal static class Program
  {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      // To customize application configuration such as set high DPI settings or default font,
      // see https://aka.ms/applicationconfiguration.

      if (!ExistsZopflipngExe()) return;

      ApplicationConfiguration.Initialize();
      Application.Run(new FrmMain());
    }

    public static bool ExistsZopflipngExe()
    {
      string path = Path.GetDirectoryName(Application.ExecutablePath) ?? "Unknown Path";
      if (File.Exists(path + "\\zopflipng.exe")) return true;
      if (DialogResult.OK.Equals(MessageBox.Show($"zopflipng.exe not found.\nPlease put it on same directory of this app:\n{path}\n\nClick OK to open this directory.", null, MessageBoxButtons.OKCancel))) {
        // Kick explorer or filer at directory of this app.
        _ = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
      }
      return false;
    }
  }
}