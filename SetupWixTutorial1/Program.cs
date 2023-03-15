using System;
using System.IO;
using System.Windows.Forms;
using WixSharp;
using static WixSharp.Nsis.Compressor;

namespace SetupWixTutorial
{
    internal class Program
    {
        static void Main()
        {
            Compiler.EmitRelativePaths = false;
            Compiler.LightOptions += "-reusecab "; //doesn't delete them..      

            Run();
        }

        private static void Run()
        {
            Version version = new Version(1, 0, 0, 0);

            var msiProject = MsiPackage.CreateProject(version, Path.Combine(Directory.GetCurrentDirectory(), "..", "bin"), 
                new string[] { "*.json", "*.dll", "*.exe", "*.txt" });

            msiProject.OutFileName = MsiPackage.ProductName;
            msiProject.OutDir = Directory.GetCurrentDirectory();
            msiProject.BuildMsi();
        }
    }
}