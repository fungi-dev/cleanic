using Cleanic.Application;
using Cleanic.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cleanic.TestFramework
{
    public static class BddTestHelper
    {
        public static void Init(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _app = _serviceProvider.GetService<ApplicationFacade>();
        }

        public static void DoNothing(this String _) { }

        public static String Do(this String stepName, Command command)
        {
            _app.Do(command).GetAwaiter().GetResult();

            return stepName;
        }

        public static String Validate<T>(this String stepName, Query query, Action<T> validator)
            where T : Projection
        {
            var projection = (T)_app.Get(query).GetAwaiter().GetResult();
            validator.Invoke(projection);

            return stepName;
        }

        private static IServiceProvider _serviceProvider;
        private static ApplicationFacade _app;
    }
}