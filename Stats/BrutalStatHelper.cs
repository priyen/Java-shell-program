using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Brutal.Dev.StatHelper
{
  class Program
  {
    static void Main(string[] args)
    {
      const string BRANCHES_CONFIG = "_branches.config";
      const string EXCLUDE_USERS_CONFIG = "_excludeusers.config";
      const string EXCLUDE_PATHS_CONFIG = "_excludepaths.config";
      const string LOG_FILE = "svn.log";

      try
      {
        IEnumerable<string> excludedUsers = new string[0];
        IEnumerable<string> excludedPaths = new string[0];

        // Do some basic checks and import settings from external files.
        if (File.Exists(EXCLUDE_USERS_CONFIG))
        {
          excludedUsers = File.ReadAllLines(EXCLUDE_USERS_CONFIG).Where(s => !String.IsNullOrEmpty(s) && !s.StartsWith("#"));
        }
        if (File.Exists(EXCLUDE_PATHS_CONFIG))
        {
          excludedPaths = File.ReadAllLines(EXCLUDE_PATHS_CONFIG).Where(s => !String.IsNullOrEmpty(s) && !s.StartsWith("#"));
        }

        // Go through each branch folder and extract the log info.
        foreach (var branchFolder in File.ReadAllLines(BRANCHES_CONFIG).Where(s => !String.IsNullOrEmpty(s) && !s.StartsWith("#")))
        {
          Console.WriteLine("Performing work for {0}...", branchFolder);

          // Generate the log file by passing the branch folder into the batch file provided.
          Process.Start(new ProcessStartInfo("GenerateLog.bat", string.Format("\"{0}\"", branchFolder))
          {
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
          }).WaitForExit();

          // Crack the XML and filter out files and users (heavy mode).
          var xml = XDocument.Parse(File.ReadAllText(Path.Combine(branchFolder, LOG_FILE)));

          int entryCount = 0;
          var sb = new StringBuilder();
          sb.AppendLine("<?xml version=\"1.0\"?><log>");

          foreach (var logEntry in xml.Descendants("logentry"))
          {
            // Do some basic filtering.
            if (!excludedUsers.Contains(logEntry.Element("author").Value, StringComparer.OrdinalIgnoreCase))
            {
              bool valid = true;
              foreach (var path in excludedPaths)
              {
                // Look for excluded paths.
                if (logEntry.Element("paths").Value.IndexOf(path, StringComparison.OrdinalIgnoreCase) != -1)
                {
                  valid = false;
                  break;
                }
              }

              if (valid)
              {
                entryCount++;
                sb.AppendLine(logEntry.ToString());
              }
            }
          }

          sb.AppendLine("</log>");

          File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_FILE), sb.ToString());

          // Check if the filters were a little too strict.
          if (entryCount == 0)
          {
            throw new Exception("All SVN log file entries have been filtered out!! Please relax the settings for excluding users and/or paths.");
          }

          // Generate the stats by calling the batch file provided with the correct arguments.
          Process.Start(new ProcessStartInfo("GenerateStats.bat", String.Format("\"{0}\" \"{1}\" \"{2}\"", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, new DirectoryInfo(branchFolder).Name), branchFolder, LOG_FILE))
          {
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
          }).WaitForExit();
        }

        Console.WriteLine("All done!");
      }
      catch (Exception ex)
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(ex.ToString());
      }
      finally
      {
        Console.ResetColor();

        // Get rid of the temp log export file.
        if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_FILE)))
        {
          File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LOG_FILE));
        }
      }
    }
  }
}
