using System;
using System.Collections.Generic;

namespace Questions
{
    public class DemoContainer
    {
        //https://ayende.com/blog/2886/building-an-ioc-container-in-15-lines-of-code
        public delegate object Creator(DemoContainer container);

        private readonly Dictionary<string, object> configuration
            = new Dictionary<string, object>();
        private readonly Dictionary<Type, Creator> typeToCreator
            = new Dictionary<Type, Creator>();

        public Dictionary<string, object> Configuration
        {
            get { return configuration; }
        }

        public void Register<T>(Creator creator)
        {
            typeToCreator.Add(typeof(T),creator);
        }

        public T Create<T>()
        {
            return (T) typeToCreator[typeof (T)](this);
        }

        public T GetConfiguration<T>(string name)
        {
            return (T) configuration[name];
        }
    }
}