namespace Cleanic.Core
{
    using System;

    public class LogicException : Exception
    {
        public LogicException(String message) : base(message) { }
    }

    public class LogicSchemaException : LogicException
    {
        public LogicSchemaException(String message) : base(message) { }
    }

    public class MisusingLogicException : LogicException
    {
        public MisusingLogicException(String message) : base(message) { }
    }
}