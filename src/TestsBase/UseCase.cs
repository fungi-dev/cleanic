namespace Cleanic.Utils
{
    using Cleanic.Application;
    using Cleanic.Core;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;

    public class UseCase
    {
        public List<Step> Steps;

        public UseCase Command(Command command) => throw new NotImplementedException();
        public UseCase See<T>(Action<T> viewValidator) where T : AggregateView => throw new NotImplementedException();
        public UseCase View<T>() where T : AggregateView => throw new NotImplementedException();
        public void Run() => throw new NotImplementedException();
    }
}