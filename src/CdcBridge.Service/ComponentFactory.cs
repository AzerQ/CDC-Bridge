using Microsoft.Extensions.DependencyInjection;

namespace CdcBridge.Service;

/// <summary>
/// Фабрика для получения экземпляров компонентов (источников, фильтров, и т.д.)
/// по их имени типа, используя DI-контейнер.
/// </summary>
public class ComponentFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ComponentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Получает экземпляр компонента указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип интерфейса компонента (e.g., IReceiver).</typeparam>
    /// <param name="typeName">Имя типа реализации (e.g., "WebhookReceiver") или поля Name в классе.</param>
    /// <returns>Экземпляр компонента.</returns>
    /// <exception cref="InvalidOperationException">Если компонент с таким именем не найден.</exception>
    public T GetInstance<T>(string typeName) where T : class
    {
        // Получаем все зарегистрированные реализации интерфейса T
        var services = _serviceProvider.GetServices<T>();
        
        // Находим нужную по имени класса или по полю Name
        var service = services.FirstOrDefault(s =>
        {
            string serviceName = s.GetType().GetProperty("Name")?.GetValue(s) as string ?? s.GetType().Name;
            return serviceName.Equals(typeName, StringComparison.OrdinalIgnoreCase);
        });
            

        return service ?? throw new InvalidOperationException($"Component of type '{typeName}' for interface '{typeof(T).Name}' not found or not registered in DI container.");
    }
}