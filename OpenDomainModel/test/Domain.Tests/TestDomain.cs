namespace OpenDomainModel
{
    public class EmptyAggregate : IAggregateRoot { }

    public class SimpleAggregate : IAggregateRoot
    {
        public void DoFirst(FirstCommand cmd) { }
    }

    public class ComplexAggregate : IAggregateRoot
    {
        public void DoFirst(FirstCommand cmd) { }
        public void DoSecond(SecondCommand cmd) { }
    }

    public class FirstCommand : ICommand { }

    public class SecondCommand : ICommand { }

    public class ListProjection : IProjection { }
}