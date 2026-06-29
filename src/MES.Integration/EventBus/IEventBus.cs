namespace MES.Integration.EventBus;

/// <summary>
/// 事件订阅管理器，跟踪事件与处理器的映射关系。
/// 注意：IEvent 和 IEventBus 的规范定义在 MES.Application.Interfaces 中，
/// Integration 层通过实现 Application 层的接口来适配。
/// </summary>
public interface IIntegrationEventBus : MES.Application.Interfaces.IEventBus
{
    // 可在此扩展集成层特有方法（如订阅等）
}
