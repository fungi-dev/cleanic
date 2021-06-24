namespace Cleanic.Application
{
    using System;

    public class User
    {
        public String Name { get; }
        public UserRole Role { get; }

        public User(String name, UserRole role)
        {
            Name = name;
            Role = role;
        }
    }

    public class UserRole { }
}