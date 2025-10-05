using System.Collections.Concurrent;
using System.Text.Json;
using HandlebarsDotNet;
using HandlebarsDotNet.Extension.Json;

namespace CdcBridge.Application.Receivers.CdcWebhookReceiver;

public class HandlebarsUrlTemplateRenderer : IUrlTemplateRenderer<JsonElement>
{
    
    private static readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> TemplatesCache = new ();
    
    public string RenderUrlTemplate(string urlTemplate, JsonElement data)
    {
        var template = TemplatesCache.GetOrAdd(urlTemplate, Handlebars.Compile);
        var handlebars = Handlebars.Create();
        handlebars.Configuration.UseJson();

        return template(data);

    }
}