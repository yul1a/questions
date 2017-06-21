using System;

namespace Questions
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var container = new DemoContainer();
            container.Register<IConsole>(delegate
            {
                return new StandartConsoleWrapper();
            });

            container.Register<Profile>(delegate
            {
                return new Profile(
                    container.Create<IConsole>());
            });

            var profile  = container.Create<Profile>();
            profile.StartWork();
        }
    }
}