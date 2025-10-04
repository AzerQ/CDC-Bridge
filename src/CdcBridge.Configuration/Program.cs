// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;
using CdcBridge.Configuration;

string baseFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!;
string configPath = Path.Combine(baseFolderPath, "example/exampleSettingsFormat.yaml");

var cdcConfigurationContext = new CdcConfigurationContextBuilder()
    .AddConfigurationFromFile(configPath)
    .Build();

var expr = new { expression = "" };

var parameters = cdcConfigurationContext.CdcSettings.Filters.First().Parameters.Deserialize(expr.GetType());

var resolvedReceiverPipeline = cdcConfigurationContext.GetReceiverPipeline("AnalyticsWebHook");

Console.ReadLine();