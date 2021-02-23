using Cleanic.Application;
using Cleanic.Core;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Cleanic.Utils
{
    public static class BddTestHelper
    {
        public static void Init(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _writeApp = _serviceProvider.GetService<WriteApplicationFacade>();
            _readApp = _serviceProvider.GetService<ReadApplicationFacade>();
        }

        public static void DoNothing(this String _) { }

        public static String ExternalLogic(this String stepName, System.Action logic)
        {
            logic.Invoke();

            return stepName;
        }

        public static String Do(this String stepName, Command command)
        {
            _writeApp.Do(command).GetAwaiter().GetResult();

            return stepName;
        }

        public static String Validate<T>(this String stepName, Query query, Action<T> validator)
            where T : QueryResult
        {
            var result = (T)_readApp.Get(query).GetAwaiter().GetResult();
            validator.Invoke(result);

            return stepName;
        }

        private static IServiceProvider _serviceProvider;
        private static WriteApplicationFacade _writeApp;
        private static ReadApplicationFacade _readApp;
    }
}