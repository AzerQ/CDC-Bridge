// See https://aka.ms/new-console-template for more information

using System.Reflection;
using CdcBridge.Configuration;

var loader = new ConfigurationLoader();
string baseFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;
string configPath = Path.Combine(baseFolderPath, "example/exampleSettingsFormat.json");

var cdcSettings = loader.LoadConfiguration(configPath);

Console.ReadLine();