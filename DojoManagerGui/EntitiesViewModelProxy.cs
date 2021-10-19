using DojoManagerApi;
using DojoManagerApi.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerGui
{

    public class EntitiesViewModelProxy<T> : DispatchProxy, INotifyPropertyChanged
    {
        public T? origin { get; private set; }

        public HashSet<MethodInfo> GettersToReplace { get; private set; } = new HashSet<MethodInfo>();

        public HashSet<MethodInfo> Adders { get; private set; }
        public HashSet<MethodInfo> Removers { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private (CollectionModifier[] adders, CollectionModifier[] removers) GetCollectionModifiers()
        {
            var properties = typeof(T).GetProperties();
            var methods = typeof(T).GetMethods();
            List<CollectionModifier> adders = new List<CollectionModifier>();
            List<CollectionModifier> removers = new List<CollectionModifier>();
            foreach (var property in properties)
            {
                //remove the final s from the name
                if (property.Name.EndsWith('s'))
                {
                    var collectionElemType = TypeSystem.GetElementType(property.PropertyType);
                    if (collectionElemType != null)
                    {
                        var adderName = "Add" + property.Name.Substring(property.Name.Length - 1);
                        var removerName = "Add" + property.Name.Substring(property.Name.Length - 1);

                        var addersAndRemovers = from m in methods
                                                let pars = m.GetParameters()
                                                where pars.Length == 1 && pars[0].ParameterType.IsSubclassOf(collectionElemType)
                                                select m;

                        var remover = addersAndRemovers.Where(m => m.Name == removerName).FirstOrDefault();
                        var adder = addersAndRemovers.Where(m => m.Name == adderName).FirstOrDefault();
                        if (adder != null)
                            adders.Add(new CollectionModifier(adder, property));
                        if (remover != null)
                            removers.Add(new CollectionModifier(remover, property));
                    }

                }
            }
            return (adders.ToArray(), removers.ToArray());
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {

            if (targetMethod == null)
                return null;

            if (GettersToReplace.Contains(targetMethod))
            {
                var originObject = targetMethod.Invoke(origin, args);
                var baseType = targetMethod.ReturnType.GetInterfaces().First();
                var proxyType = typeof(EntitiesViewModelProxy<>).MakeGenericType(baseType);
                var proxyInstance = proxyType
                    ?.GetMethod("Create", BindingFlags.Static | BindingFlags.Public)
                    ?.Invoke(null, new[] { originObject });
                return proxyInstance;
            }
            else
            {

            }

            var ret = targetMethod.Invoke(origin, args);
            if (targetMethod.Name.StartsWith("set_"))
            {
                RaisePropertyChanged(targetMethod.Name.Remove(3));
            }
            return targetMethod.Invoke(origin, args);
        }

        public static T Create(T decorated)
        {
            T ob = Create<T, EntitiesViewModelProxy<T>>();
            EntitiesViewModelProxy<T>? proxy = ob as EntitiesViewModelProxy<T>;
            proxy?.SetParameters(decorated);
            return ob;
        }

        private void SetParameters(T decorated)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            origin = decorated;

            var substitutions = typeof(T).GetProperties().Where(p => p.PropertyType.IsClass && p.PropertyType.HasAttribute<EntityInterfaceAttribute>());
            foreach (var p in typeof(T).GetProperties())
            {
                var propType = p.PropertyType;
                if (propType.IsClass)
                {
                    var entityInterface = (from enti in propType.GetInterfaces()
                                           where enti.HasAttribute<EntityInterfaceAttribute>()
                                           select enti)
                                          .FirstOrDefault();
                    if (entityInterface != null   )
                    {
                        var getMet = p.GetGetMethod();
                        if(getMet != null )
                            this.GettersToReplace.Add(getMet);
                    }


                }
            }
     
        }

        class CollectionModifier
        {
            public MethodInfo Method;
            public PropertyInfo Property;

            public CollectionModifier(MethodInfo method, PropertyInfo property)
            {
                this.Method = method;
                Property = property;
            }
        }
    }
}


internal static class TypeSystem
{
    public static bool HasAttribute<T>(this Type declaringType) where T : Attribute
    {
        return declaringType.GetCustomAttribute<T>() != null;
    }
    internal static Type? GetElementType(Type seqType)
    {
        Type ienum = FindIEnumerable(seqType);
        if (ienum == null)
            return seqType;

        return ienum.GetGenericArguments()[0];

    }
    private static Type? FindIEnumerable(Type seqType)
    {
        if (seqType == null || seqType == typeof(string))
            return null;
        if (seqType.IsArray)
            return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
        if (seqType.IsGenericType)
        {
            foreach (Type arg in seqType.GetGenericArguments())
            {
                Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                if (ienum.IsAssignableFrom(seqType))
                {
                    return ienum;
                }
            }
        }
        Type[] ifaces = seqType.GetInterfaces();
        if (ifaces != null && ifaces.Length > 0)
        {
            foreach (Type iface in ifaces)
            {
                Type ienum = FindIEnumerable(iface);
                if (ienum != null) return ienum;
            }
        }
        if (seqType.BaseType != null && seqType.BaseType != typeof(object))
        {
            return FindIEnumerable(seqType.BaseType);
        }
        return null;
    }
}