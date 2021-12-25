using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.IO.Compression;

namespace WinBM.PowerShell.Manifest
{
    class Project
    {
        private ProjectInfo _debug { get; set; }
        private ProjectInfo _release { get; set; }
        private ProjectInfo _target { get; set; }

        public Project()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this._debug = new ProjectInfo() { Mode = Mode.Debug };
            this._release = new ProjectInfo() { Mode = Mode.Release };

            DateTime lastWriteDebug = File.GetLastWriteTime(_debug.DllFile);
            DateTime lastWriteRelease = File.GetLastWriteTime(_release.DllFile);

            this._target = lastWriteDebug > lastWriteRelease ? _debug : _release;
        }

        public void CreateManifestFile()
        {
            Psd1.Create(_debug);
            Psm1.Create(_debug);
            Psd1.Create(_release);
            Psm1.Create(_release);
        }

        public void CopyProjectDir()
        {
            if (Directory.Exists(_target.TargetDir))
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.Arguments = string.Format(
                        "\"{0}\" \"{1}\" /COPY:DAT /MIR /E /XJD /XJF /XF *.log *.json", _target.TargetDir, _target.ModuleDir);
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.Start();
                    proc.WaitForExit();
                }
            }
        }

        public void CopyScriptDir()
        {
            if (Directory.Exists(_target.ScriptDir))
            {
                foreach (string fileName in Directory.GetFiles(_target.ScriptDir))
                {
                    File.Copy(fileName, Path.Combine(_target.ModuleDir, Path.GetFileName(fileName)), true);
                }
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    foreach (string dirName in Directory.GetDirectories(_target.ScriptDir))
                    {
                        proc.StartInfo.Arguments = string.Format(
                            "\"{0}\" \"{1}\" /COPY:DAT /MIR /E /XJD /XJF",
                            dirName,
                            Path.Combine(_target.ModuleDir, Path.GetFileName(dirName)));
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }
        }

        public void CopyFormatDir()
        {
            if (Directory.Exists(_target.FormatDir))
            {
                foreach (string fileName in Directory.GetFiles(_target.FormatDir, "*.ps1xml"))
                {
                    File.Copy(fileName, Path.Combine(_target.ModuleDir, Path.GetFileName(fileName)), true);
                }
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    foreach (string dirName in Directory.GetDirectories(_target.FormatDir))
                    {
                        proc.StartInfo.Arguments = string.Format(
                            "\"{0}\" \"{1}\" /COPY:DAT /MIR /E /XJD /XJF",
                            dirName,
                            Path.Combine(_target.ModuleDir, Path.GetFileName(dirName)));
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }
        }

        public void CopyHelpDir()
        {
            if (Directory.Exists(_target.HelpDir))
            {
                foreach (string fileName in Directory.GetFiles(_target.HelpDir, "*.dll-Help.xml"))
                {
                    File.Copy(fileName, Path.Combine(_target.ModuleDir, Path.GetFileName(fileName)), true);
                }
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = "robocopy.exe";
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    foreach (string dirName in Directory.GetDirectories(_target.HelpDir))
                    {
                        proc.StartInfo.Arguments = string.Format(
                            "\"{0}\" \"{1}\" /COPY:DAT /MIR /E /XJD /XJF",
                            dirName,
                            Path.Combine(_target.ModuleDir, Path.GetFileName(dirName)));
                        proc.Start();
                        proc.WaitForExit();
                    }
                }
            }
        }
    }
}
