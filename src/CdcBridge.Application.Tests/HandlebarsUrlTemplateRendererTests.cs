using CdcBridge.Application.Receivers.CdcWebhookReceiver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace CdcBridge.Application.Tests;

[TestClass]
public class HandlebarsUrlTemplateRendererTests
{
    [TestMethod]
    public void RenderUrlTemplate_WithNoPlaceholders_ReturnsOriginalUrl()
    {
        // Arrange
        var renderer = new HandlebarsUrlTemplateRenderer();
        var template = "https://api.example.com/webhooks/static";
        var data = JsonSerializer.SerializeToElement(new
        {
            id = 123
        });

        // Act
        var result = renderer.RenderUrlTemplate(template, data);

        // Assert
        Assert.AreEqual("https://api.example.com/webhooks/static", result);
    }

    [TestMethod]
    public void RenderUrlTemplate_WithData_ReturnsNonEmptyString()
    {
        // Arrange
        var renderer = new HandlebarsUrlTemplateRenderer();
        var template = "https://api.example.com/webhooks/{{id}}";
        var data = JsonSerializer.SerializeToElement(new
        {
            id = 123
        });

        // Act
        var result = renderer.RenderUrlTemplate(template, data);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StartsWith("https://api.example.com/webhooks/"));
    }
}
