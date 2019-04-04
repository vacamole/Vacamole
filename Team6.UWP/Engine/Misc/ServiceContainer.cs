using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Team6.Engine.Components;
using Team6.Engine.MemoryManagement;

namespace Team6.Engine.Misc
{
    public class ServiceContainer<T> where T : class
    {
        private readonly Dictionary<Type, HashSet<T>> components = new Dictionary<Type, HashSet<T>>();
        private readonly Dictionary<string, T> nameIndex = new Dictionary<string, T>();
        private readonly HashSet<T> flatList = new HashSet<T>();
        private readonly HashSet<T> emptySet = new HashSet<T>();
        private readonly Func<T, string> nameExtractor;

        public ServiceContainer(Func<T, string> nameExtractor = null)
        {
            this.nameExtractor = nameExtractor;
        }

        public void Add(T service)
        {
            if (flatList.Add(service))
            {
                if (nameExtractor != null)
                {
                    string key = nameExtractor(service);
                    if (!string.IsNullOrEmpty(key))
                        nameIndex.Add(nameExtractor(service), service);
                }

                // [FOREACH PERFORMANCE] [not high frequency code] ALLOCATES GARBAGE but is fine
                foreach (Type t in GetAllTypes(service))
                {
                    AddConcreteService(t, service);
                }
            }
        }

        private void AddConcreteService(Type t, T service)
        {
            if (!components.ContainsKey(t))
                components.Add(t, new HashSet<T>());

            components[t].Add(service);
        }

        private IEnumerable<Type> GetAllTypes(T service)
        {
            Type currentType = service.GetType();

            while (currentType != typeof(T))
            {
                var info = currentType.GetTypeInfo();
                yield return currentType;

                // [FOREACH PERFORMANCE] Should not allocate garbage 
                foreach (var i in currentType.GetInterfaces())
                    yield return i;

                currentType = info.BaseType;
            }
        }

        public TS Get<TS>() where TS : class
        {
            return GetAll<TS>().FirstOrDefault();
        }

        public SafeSelectHashSetEnumerable<TS, T> GetAll<TS>() where TS : class
        {
            HashSet<T> list;

            var safeEnumerable = components.TryGetValue(typeof(TS), out list) ? list.ToSaveEnumerable() : emptySet.ToSaveEnumerable();

            return safeEnumerable.Cast<TS, T>();
        }

        public void RemoveComponent(T component)
        {
            if (component == null)
                return;
            // [FOREACH PERFORMANCE]  [not high frequency code] ALLOCATES GARBAGE (because of ToList())
            foreach (var entry in components.ToList())
            {
                if (entry.Value.Remove(component) && entry.Value.Count == 0)
                {
                    components.Remove(entry.Key);
                }
            }
            flatList.Remove(component);

            if (nameExtractor != null)
            {
                string key = nameExtractor(component);
                if (key != null)
                    nameIndex.Remove(key);
            }
        }

        public TS GetByName<TS>(string name) where TS : class
        {
            T component;
            if (nameIndex.TryGetValue(name, out component))
                return component as TS;

            return default(TS);
        }

        public void RemoveAllComponents<TS>() where TS : class
        {
            foreach (var component in GetAll<TS>().ToList())
            {
                RemoveComponent(component as T);
            }
        }

        public SafeHashSetEnumerable<T> AllServices
        {
            get { return flatList.ToSaveEnumerable(); }
        }



    }
}
