using Core;
using Moq;
using Plugin.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests
{
    public class CoreServiceTests
    {
        [Fact]
        public async Task LoadSourcePluginAsync_LoadsPluginSuccessfully()
        {
            // Arrange
            var mockLoader = new Mock<PluginLoader>();
            mockLoader.Setup(l => l.LoadSourcePluginAsync("TestSource")).ReturnsAsync(Mock.Of<ISourcePlugin>());
            var service = new CoreService(mockLoader.Object);

            // Act
            await service.LoadSourcePluginAsync("TestSource");

            // Assert
            mockLoader.Verify(l => l.LoadSourcePluginAsync("TestSource"), Times.Once);
            Assert.Single(service.GetLoadedSources());
        }

        [Fact]
        public async Task LoadSinkPluginAsync_LoadsPluginSuccessfully()
        {
            // Arrange
            var mockLoader = new Mock<PluginLoader>();
            mockLoader.Setup(l => l.LoadSinkPluginAsync("TestSink")).ReturnsAsync(Mock.Of<ISinkPlugin>());
            var service = new CoreService(mockLoader.Object);

            // Act
            await service.LoadSinkPluginAsync("TestSink");

            // Assert
            mockLoader.Verify(l => l.LoadSinkPluginAsync("TestSink"), Times.Once);
            Assert.Single(service.GetLoadedSinks());
        }

        [Fact]
        public void GetLoadedSources_ReturnsLoadedSources()
        {
            // Arrange
            var service = new CoreService(new PluginLoader());
            // Assume some sources are loaded

            // Act
            var sources = service.GetLoadedSources();

            // Assert
            Assert.NotNull(sources);
        }
    }
}