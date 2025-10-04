// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using CdcBridge.Configuration;

var loader = new ConfigurationLoader();
string baseFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;
string configPath = Path.Combine(baseFolderPath, "example/exampleSettingsFormat.yaml");

var cdcSettings = loader.LoadConfiguration(configPath);

var expr = new { expression = "" };

var parameters = cdcSettings.Filters.First().Parameters.Deserialize(expr.GetType());

Console.ReadLine();