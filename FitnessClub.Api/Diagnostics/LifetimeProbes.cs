namespace FitnessClub.Api.Diagnostics;
public interface ILifetimeProbe
{
    Guid InstanceId { get; }
}
public interface ITransientProbe : ILifetimeProbe { }
public interface IScopedProbe : ILifetimeProbe { }
public interface ISingletonProbe : ILifetimeProbe { }
public class LifetimeProbe : ITransientProbe, IScopedProbe, ISingletonProbe
{
    public Guid InstanceId { get; } = Guid.NewGuid();
}
