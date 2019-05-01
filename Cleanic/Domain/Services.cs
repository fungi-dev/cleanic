namespace Cleanic.Domain
{
    /// <summary>
    /// Stateless domain service.
    /// </summary>
    public interface IService { }

    /// <summary>
    /// Description of some process logic.
    /// </summary>
    public interface ISaga : IService { }

    /// <summary>
    /// Facade of some service external to domain.
    /// </summary>
    public interface IExternalService : IService { }
}