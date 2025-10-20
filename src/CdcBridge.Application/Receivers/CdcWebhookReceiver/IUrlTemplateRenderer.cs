namespace CdcBridge.Application.Receivers.CdcWebhookReceiver;

public interface IUrlTemplateRenderer<in TData>
{
    string RenderUrlTemplate(string urlTemplate, TData data);
}