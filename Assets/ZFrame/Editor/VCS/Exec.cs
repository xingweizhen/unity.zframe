using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ZFrame.Editors
{
    class Exec
    {
        public static string DoExec(string appName, string args)
        {
            Process process = new Process();
            process.StartInfo.FileName = appName;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            StreamReader reader = null;
            try
            {
                process.Start();
                reader = process.StandardOutput;
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                {
                    try
                    {
                        process.Close();
                    }
                    catch
                    {
                    }
                    try
                    {
                        reader.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
