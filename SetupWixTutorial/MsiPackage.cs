using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Forms;

namespace SetupWixTutorial
{
    internal class MsiPackage
    {
        internal const string ProductId = "ProductPackageId";
        internal const string ProductName = "My Very Awesome App";
        private const string Description = "Моя супер крутая программа";
        internal const string CompanyName = "My Company";

        internal static readonly Guid Guid = new Guid("4FEFE8E1-F19C-40A7-8562-FDAF8D12EC63");
        internal static readonly Guid UpgradeCode = new Guid("2F380BC9-BFC3-479C-8B64-BA7DCA890636"); //DO NOT CHANGE UpgradeCode

        private static readonly string[] _defaultFilter = { "*.*" };

        /// <summary>
        /// Создать пакет MSI
        /// </summary>
        /// <param name="version">Версия продукта</param>
        /// <param name="sourceBaseDir">Путь до файлов для упаковки</param>
        /// <param name="filters">фильтр файлов для установки ["*.dll", "*.exe"]</param>
        /// <returns></returns>
        public static ManagedProject CreateProject(Version version, string sourceBaseDir, IEnumerable<string> filters)
        {
            var itemsDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            var project = new ManagedProject(ProductName)
            {
                GUID = Guid,
                Version = version,
                Description = Description,
                ProductId = GuidHelper.GenerateGuid(ProductName, version),
                Codepage = "1251",
                UpgradeCode = UpgradeCode,
                MajorUpgrade = new MajorUpgrade
                {
                    AllowSameVersionUpgrades = true,
                    Schedule = UpgradeSchedule.afterInstallInitialize,
                    DowngradeErrorMessage = "A later version of [ProductName] is already installed. Setup will now exit."
                },
                Language = "ru-ru",
                InstallScope = InstallScope.perMachine,
                Encoding = Encoding.UTF8,
                LocalizationFile = Path.Combine(itemsDir, "Resources", "WixUI_ru-ru.wxl"),
                BackgroundImage = Path.Combine(itemsDir, "Resources", "banner_left.png"),
                BannerImage = Path.Combine(itemsDir, "Resources", "banner_up.png"),
                LicenceFile = Path.Combine(itemsDir, "Resources", "licence.rtf"),
    #if (DEBUG)
                PreserveTempFiles = true
    #endif
            };

            //custom set of standard UI dialogs
            project.ManagedUI = new ManagedUI();
            project.ManagedUI.InstallDialogs.Add(Dialogs.Welcome)
                .Add(Dialogs.Licence)
                .Add(Dialogs.InstallDir)
                .Add(Dialogs.Progress)
                .Add(Dialogs.Exit);

            project.ManagedUI.ModifyDialogs.Add(Dialogs.MaintenanceType)
                .Add(Dialogs.Features)
                .Add(Dialogs.Progress)
                .Add(Dialogs.Exit);

            if (filters == null) filters = _defaultFilter;

            //файлы для установки, ставим все *.* и все подпапки
            var installDir = new Dir(ProductName);

            installDir.AddDirFileCollections(filters.Select(f => new DirFiles(f)).ToArray());
            //recursive all folders
            foreach (var subDir in Directory.GetDirectories(sourceBaseDir))
                ProcessDir(installDir, "", subDir, filters);

            project.AddDir(new Dir("%ProgramFiles%", new Dir(CompanyName, installDir)));
            project.SourceBaseDir = sourceBaseDir;


            return project;

        }

        private static void ProcessDir(Dir installDir, string parentPath, string dirPath, IEnumerable<string> filters)
        {
            var dirName = Path.GetFileName(dirPath);
            if (string.IsNullOrEmpty(dirName))
                return;

            var dir = new Dir(dirName);
            dir.AddDirFileCollections(filters.Select(f => new DirFiles(Path.Combine(parentPath, dirName, f))).ToArray());

            installDir.AddDir(dir);

            foreach (var subDir in Directory.GetDirectories(dirPath))
                ProcessDir(dir, Path.Combine(parentPath, dirName), subDir, filters);
        }
    }
}
