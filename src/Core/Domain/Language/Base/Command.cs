namespace Cleanic.Core
{
    /// <summary>
    /// Represents an intent to change something in the domain.
    /// </summary>
    public abstract class Command : Message { }

    /// <summary>
    /// Command for entity creation.
    /// </summary>
    public abstract class InitialCommand : Command { }

    /// <summary>
    /// Command that not supposed to be used by external actors.
    /// </summary>
    public interface IInternalCommand { }
}