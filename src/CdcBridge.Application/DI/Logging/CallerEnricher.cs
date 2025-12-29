using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace CdcBridge.Application.DI.Logging;

public class CallerEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var stackTrace = new StackTrace(fNeedFileInfo: true);
        
        // Пропускаем стандартные кадры Serilog
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            var method = frame?.GetMethod();
            var declaringType = method?.DeclaringType;
            
            if (declaringType == null) 
                continue;
                
            // Пропускаем методы Serilog и System
            if (declaringType.FullName?.StartsWith("Serilog.") == true ||
                declaringType.FullName?.StartsWith("System.") == true ||
                declaringType.FullName?.Contains("LoggerExtensions") == true ||
                declaringType.FullName?.Contains("CallerEnricher") == true ||
                declaringType.FullName?.Contains("Microsoft") == true) 
                continue;
                
            var (typeName, methodName) = GetMethodName(declaringType, method);
            var fileName = frame?.GetFileName();
            var lineNumber = frame?.GetFileLineNumber();
            
            var callerMethodProperty = propertyFactory.CreateProperty("CallerMethod", $"{typeName}.{methodName}");
            var sourceLocationProperty = propertyFactory.CreateProperty("SourceLocation", $"{fileName}:{lineNumber}");
            
            logEvent.AddPropertyIfAbsent(callerMethodProperty);
            logEvent.AddPropertyIfAbsent(sourceLocationProperty);
            
            return;
        }
        
        var fallbackProperty = propertyFactory.CreateProperty("CallerMethod", "unknown");
        logEvent.AddPropertyIfAbsent(fallbackProperty);
    }

    private (string TypeName, string MethodName) GetMethodName(Type declaringType, MethodBase? method)
    {
        bool isAsyncMethod = typeof(System.Runtime.CompilerServices.IAsyncStateMachine).IsAssignableFrom(declaringType);
        string sourceType = declaringType.Name;
        
        if (!isAsyncMethod)
            return (declaringType.FullName ?? declaringType.Name, method?.Name ?? "unknown");
        
        string typeName = declaringType.DeclaringType?.FullName ?? "unknown";

        string methodName = Regex.Match(sourceType, "<([A-Za-z]+)>").Groups[1].Value;
        
        return (typeName, methodName);
    }
    
}

public static class LoggerCallerEnrichmentConfiguration
{
    public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration, IConfiguration configuration)
    {
        return enrichmentConfiguration.With<CallerEnricher>();
    }
}

