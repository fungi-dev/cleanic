namespace Cleanic.Utils
{
    using Cleanic.Application;
    using System;

    public static class UserExtensions
    {
        public static UseCase Using<T>(this User user) where T : Client => throw new NotImplementedException();
    }
}