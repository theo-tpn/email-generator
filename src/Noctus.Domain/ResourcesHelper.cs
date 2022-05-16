using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Noctus.Domain.Models;
using Noctus.Domain.Models.Emails;
using System;
using System.Collections.Generic;
using System.IO;

namespace Noctus.Domain
{
    public class ResourcesHelper
    {
        public static string DefaultDocumentsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static string DefaultNoctusFolderPath = Path.Combine(DefaultDocumentsFolderPath, "Noctus");
        public static string DefaultGenwaveFolderPath = Path.Combine(DefaultNoctusFolderPath, "Genwave");
        public static string DefaultProxiesFolderPath = Path.Combine(DefaultGenwaveFolderPath, "Proxies");
        public static string DefaultAccountsFolderPath = Path.Combine(DefaultGenwaveFolderPath, "Mails");
        public static string DefaultMiscDataFolderPath = Path.Combine(DefaultGenwaveFolderPath, "Misc");
        public static string DefaultLogsFolderPath = Path.Combine(DefaultGenwaveFolderPath, "Logs");
        public static string DefaultUserSettingsFilePath => Path.Combine(DefaultGenwaveFolderPath, "config.json");
        public static string DefaultLocalhostProxiesFilePath => Path.Combine(DefaultProxiesFolderPath, "no-proxies.txt");
        public static string DefaultHarvestFilePath => Path.Combine(DefaultMiscDataFolderPath, "harvested_cookies.json");
        public static string DefaultRecoveryEmailsFilePath =>
            Path.Combine(DefaultMiscDataFolderPath, "recovery_emails.json");

        public static void EnsureDefaultResources()
        {
            EnsureFolder(DefaultNoctusFolderPath);
            EnsureFolder(DefaultGenwaveFolderPath);
            EnsureFolder(DefaultMiscDataFolderPath);
            EnsureFolder(DefaultLogsFolderPath);

            EnsureDefaultUserSettingsFile();
            EnsureFile(DefaultLocalhostProxiesFilePath);
            EnsureDefaultHarvestFile();
            EnsureDefaultRecoveryEmailsFile();
        }

        public static void SaveUserSettings(UserSettings settings)
        {
            File.WriteAllText(DefaultUserSettingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static void AddOrUpdateSetting<T>(string sectionPathKey, T value)
        {
            try
            {
                string json = File.ReadAllText(DefaultUserSettingsFilePath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);

                SetValueRecursively(sectionPathKey, jsonObj, value);

                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(DefaultUserSettingsFilePath, output);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing app settings | {0}", ex.Message);
            }
        }

        private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                // continue with the procress, moving down the tree
                var nextSection = remainingSections[1];
                jsonObj[currentSection] ??= new JObject();
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                var type = value.GetType();

                if (type.IsPrimitive || type == typeof(string))
                {
                    jsonObj[currentSection] = value;
                }
                else
                {
                    // we've got to the end of the tree, set the value
                    jsonObj[currentSection] = JObject.FromObject(value);
                }
            }
        }

        private static void EnsureFolder(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            if (!dirInfo.Exists) dirInfo.Create();
        }

        private static void EnsureDefaultUserSettingsFile()
        {
            var fileInfo = new FileInfo(DefaultUserSettingsFilePath);
            if (fileInfo.Exists) return;
            var fs = fileInfo.Create();
            using var sw = new StreamWriter(fs);
            sw.WriteLine(JsonConvert.SerializeObject(new UserSettings(), Formatting.Indented));
            sw.Dispose();
            fs.Dispose();
        }

        private static void EnsureDefaultHarvestFile()
        {
            var fileInfo = new FileInfo(DefaultHarvestFilePath);
            if (fileInfo.Exists) return;
            var fs = fileInfo.Create();
            using var sw = new StreamWriter(fs);
            sw.WriteLine(JsonConvert.SerializeObject(new List<HarvestedCookies>(), Formatting.Indented));
            sw.Dispose();
            fs.Dispose();
        }

        private static void EnsureDefaultRecoveryEmailsFile()
        {
            var fileInfo = new FileInfo(DefaultRecoveryEmailsFilePath);
            if (fileInfo.Exists) return;
            var fs = fileInfo.Create();
            using var sw = new StreamWriter(fs);
            sw.WriteLine(JsonConvert.SerializeObject(new Dictionary<string, RecoveryEmail>(), Formatting.Indented));
            sw.Dispose();
            fs.Dispose();
        }

        private static void EnsureFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists) return;
            var fs = fileInfo.Create();
            fs.Dispose();
        }
    }
}
