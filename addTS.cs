using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class LogProcessor
{
    private const int MinRequiredArgs = 3;
    private static bool IsQuietMode = false;
    // Default separator is a comma (,)
    private static string LogSeparator = ",";

    // Helper method to convert text input into the correct separator character
    private static string GetSeparatorChar(string separatorName)
    {
        switch (separatorName.ToLower())
        {
            case "tab":
            case "\t":
                return "\t";
            case "space":
            case " ":
                return " ";
            case "comma":
            case ",":
                return ",";
            case "pipe":
            case "|":
                return "|";
            default:
                // Use the input string as the separator if not a known keyword (e.g., user supplies '::')
                return separatorName;
        }
    }

    // Overloaded Console.WriteLine to respect quiet mode
    private static void LogMessage(string message)
    {
        if (!IsQuietMode)
        {
            Console.WriteLine(message);
        }
    }

    public static int Main(string[] args)
    {
        // --- 0. Initial Argument Check and Flag Processing ---
        
        var processedArgs = args.ToList();
        
        // 0a. Detect Quiet Mode
        if (processedArgs.RemoveAll(a => a.Equals("/q", StringComparison.OrdinalIgnoreCase) || a.Equals("--quiet", StringComparison.OrdinalIgnoreCase)) > 0)
        {
            IsQuietMode = true;
        }

        // 0b. Detect Separator Flag
        for (int i = 0; i < processedArgs.Count; i++)
        {
            if (processedArgs[i].Equals("/s", StringComparison.OrdinalIgnoreCase) || processedArgs[i].Equals("--separator", StringComparison.OrdinalIgnoreCase))
            {
                // Check if the next argument (the separator value) exists
                if (i + 1 < processedArgs.Count)
                {
                    LogSeparator = GetSeparatorChar(processedArgs[i + 1]);
                    
                    // Remove both the flag and the value from the list
                    processedArgs.RemoveAt(i + 1); // Remove value first
                    processedArgs.RemoveAt(i);     // Remove flag second
                    i = -1; // Restart loop to handle new indexing
                    break;
                }
                else
                {
                    Console.WriteLine("Error: Separator flag requires a value (e.g., tab, comma, or a character).");
                    return 1;
                }
            }
        }
        
        // Final minimum required arguments check
        if (processedArgs.Count < MinRequiredArgs)
        {
            Console.WriteLine("Error: Missing required positional arguments.");
            Console.WriteLine("Usage: LogProcessor.exe [/q] [/s SEPARATOR] [OutputLogFile] [CounterExe] [... Command Args including TempFile ...]");
            Console.WriteLine("Example: LogProcessor.exe /s tab metrics.log NetworkCountersWatch.exe /stabular data.tmp");
            return 1;
        }

        // --- 1. Map Arguments ---
        string outputLogFile = processedArgs[0];
        string counterExe = processedArgs[1];
        
        // Everything after the executable name are external command arguments.
        string[] externalArgsArray = processedArgs.Skip(2).ToArray();
        
        // The temporary file name is the LAST element of the external arguments array.
        string tempFile = externalArgsArray[externalArgsArray.Length - 1]; 
        
        // Build Quoted Command Arguments String
        string quotedCommandArgs = string.Empty;
        foreach (string arg in externalArgsArray)
        {
            // C# 5 compatible string.Format()
            quotedCommandArgs += string.Format("\"{0}\" ", arg);
        }
        string commandArgs = quotedCommandArgs.TrimEnd(); 

        try
        {
            // === 2. Execute External Command ===
            LogMessage(string.Format("Calling {0} with args: '{1}' (TempFile: {2})...", counterExe, commandArgs, tempFile));
            
            ProcessStartInfo startInfo = new ProcessStartInfo(counterExe, commandArgs);
            
            // Set the most robust suppression settings:
            startInfo.UseShellExecute = false; 
            startInfo.CreateNoWindow = true; 
            startInfo.WindowStyle = ProcessWindowStyle.Hidden; 

            Process process = Process.Start(startInfo);
            process.WaitForExit();

            if (!File.Exists(tempFile))
            {
                Console.WriteLine(string.Format("Error: Temporary file {0} was not created by the external program.", tempFile));
                return 1;
            }

            // === 3. Read, Process, and Log ===
            LogMessage(string.Format("Processing and logging data from {0} to {1} using separator '{2}'...", tempFile, outputLogFile, LogSeparator));
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string[] lines = File.ReadAllLines(tempFile, Encoding.Unicode);
            StringBuilder logEntry = new StringBuilder();
            
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                // --- APPLY SEPARATOR HERE ---
                // C# 5 compatible string.Format()
                logEntry.AppendLine(string.Format("{0}{1}{2}", timestamp, LogSeparator, line));
            }

            File.AppendAllText(outputLogFile, logEntry.ToString(), Encoding.ASCII);
            
            LogMessage("Successfully logged data.");
            
            // === 4. Cleanup ===
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            // Always output unexpected exceptions (FATAL errors)
            Console.WriteLine(string.Format("An unexpected error occurred: {0}", ex.Message));
            return 1;
        }
        
        return 0;
    }
}