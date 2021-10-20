using FluentIL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Collections.ObjectModel;
using DojoManagerApi.Entities;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Collections;

namespace DojoManagerApi
{
    public class LinkedObservableCollection<T> : ObservableCollection<T>
    {
        IList<T> Origin;

        public LinkedObservableCollection(IList<T> li) : base()
        {
            Origin = li;
            foreach(var it in li)
                this.Add(it);
        }

        protected override void ClearItems()
        {
            Origin.Clear();
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {

            Origin.Insert(index, item);
            var wrapped = (T)EntityWrapper.Wrap(item);
            base.InsertItem(index, wrapped);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new NotImplementedException(); 
        }

        protected override void RemoveItem(int index)
        {
            Origin.RemoveAt(index);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            Origin[index] = item;
            var wrapped = (T)EntityWrapper.Wrap(item);
            base.SetItem(index, wrapped);
        }
    }
    public interface IWrapper
    {
        object Origin { get; }
    }
    /// <summary>
    /// Creates a new class that inherit from the wrapped type
    /// Internally the wrapper stores a reference to the wrapped instance ( origin )
    ///     - All of the propoerties get/set are intercepted and redirected to the origin 
    ///     - PropertyChanged event is reaised when setters are called
    ///     - When a property type should is decorated by WrapMe then a wrapped instance is returned
    ///     - Collection properties return linked collections of wrapped objects ( instead of original collection )
    /// Todo  
    ///     wrapper instance needs to create an nameof(EntityCollectionWrapper) for each collection of instances and return that one
    /// </summary>
    public class EntityWrapper
    {
        private const string WrapperTypeSuffix = "!Wrapper";
        static Dictionary<object, object> WrappedObjects = new Dictionary<object, object>();
        static Dictionary<string, Type> TypesCreated = new Dictionary<string, Type>();


        static void AddPropertyChanged(PropertyChangedEventHandler field, PropertyChangedEventHandler handler)
        {
            field += handler;
        }

        private bool NeedsWrapping(object obj)
        {
            Type ot = obj.GetType();
            return NeedsWrapping(ot);
        }

        private static bool NeedsWrapping(Type ot)
        {
            return TypeUtils.HasAttribute<WrapMeAttribute>(ot, true) && !ot.Name.Contains(WrapperTypeSuffix);
        }

        public static object Wrap(object entity)
        {
            //DebugOutput.Output = new ConsoleOutput(); 
            Type ot = entity.GetType();

            if (!NeedsWrapping(ot))
                return entity;

            if (WrappedObjects.ContainsKey(entity))
                return WrappedObjects[entity];

            Debug.WriteLine($"Wrapping object {entity.GetType().Name} {entity.GetHashCode()} ");
            while (!TypeUtils.HasAttribute<WrapMeAttribute>(ot))
                ot = ot.BaseType;

            Type newType = EmitType(ot);

            //create new instance now and register it ( before cascading replacements to avoid endless loop )
            object inst = Activator.CreateInstance(newType, entity);
            if (inst == null)
                throw new Exception("Unable to create");
            WrappedObjects[entity] = inst;


            //replace members of origin
            //here we need to substitute both objects and collections of objects
            foreach (var prop in ot.GetProperties())
            {
                var isEntyty = TypeUtils.HasAttribute<WrapMeAttribute>(prop.PropertyType);
                var isCollection = prop.PropertyType.IsAssignableTo(typeof(IEnumerable<object>));
                if (isEntyty)
                {
                    //var oldVal = prop.GetValue(entity, null);
                    //prop.SetValue(entity, EntityWrapper.Wrap(oldVal));
                }
                else if (isCollection)
                {
                    //Type basicType = TypeUtils.GetElementType(prop.PropertyType);
                    //isEntyty = TypeUtils.HasAttribute<WrapMeAttribute>(basicType);

                    //Type destListType = typeof(ObservableCollection<>).MakeGenericType(new Type[] { basicType });
                    //var source = (IEnumerable<object>)prop.GetValue(entity);
                    //var dest = Activator.CreateInstance(destListType);
                    //var destAdd = destListType.GetMethod("Add", new Type[] { basicType });
                    //foreach (var obj in source)
                    //{
                    //    var toAdd = obj;
                    //    //if (isEntyty)
                    //    //    toAdd = EntityWrapper.Wrap(obj);

                    //    destAdd.Invoke(dest, new[] { toAdd });
                    //}
                    //prop.SetValue(entity, dest);

                }
            }

            return inst;

        }

        private static Type EmitType(Type ot)
        {
            //Check if wrapping is needed
            if (!NeedsWrapping(ot))
                return ot;

            //emit Type 
            //definitions 
            var delegateCombine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
            var delegateRemove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });
           
            var getHashCodeBase = ot.GetMethod("GetHashCode");
            var newTypeName = $"{ot.FullName}{WrapperTypeSuffix}";

            if (TypesCreated.ContainsKey(newTypeName))
                return TypesCreated[newTypeName];

            //type not yet created, let's create it
            var builder = TypeFactory
                            .Default
                            .NewType(newTypeName).InheritsFrom(ot)
                            .Implements<INotifyPropertyChanged>();


            //add PropertyChanged event
            var propertyChangedEvent = builder.NewEvent("PropertyChanged", typeof(PropertyChangedEventHandler));
            var propertyChangedField = builder.NewField("PropertyChanged", typeof(PropertyChangedEventHandler));

            //add_PropertyChanged
            var eventAdd = builder.NewMethod("add_PropertyChanged")
                    .MethodAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot)
                    .CallingConvention(CallingConventions.Standard | CallingConventions.HasThis)
                    .Param<PropertyChangedEventHandler>()
                    .Body(b => b
                        .LdArg0()
                        .LdArg0()
                        .LdFld(propertyChangedField)
                        .LdArg1()
                        .Call(delegateCombine)
                        .CastClass<PropertyChangedEventHandler>()
                        .StFld(propertyChangedField)
                        .Ret()
                    );
            propertyChangedEvent.Define().SetAddOnMethod(eventAdd.Define());

            //remove_PropertyChanged
            var eventRemove = builder.NewMethod("remove_PropertyChanged")
                    .MethodAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot)
                    .CallingConvention(CallingConventions.Standard | CallingConventions.HasThis)
                    .Param<PropertyChangedEventHandler>()
                    .Body(b => b
                        .LdArg0()
                        .LdArg0()
                        .LdFld(propertyChangedField)
                        .LdArg1()
                        .Call(delegateRemove)
                        .CastClass<PropertyChangedEventHandler>()
                        .StFld(propertyChangedField)
                        .Ret()
                    );
            propertyChangedEvent.Define().SetRemoveOnMethod(eventRemove.Define());

            //add Origin field
            var originField = builder.NewField("Origin", ot).Public();
            builder.NewDefaultConstructor(System.Reflection.MethodAttributes.Public);

            builder.NewConstructor()
                .Public()
                .Param(ot, "origin")
                .Body()
                .LdArg0()
                .LdArg1()
                .StFld(originField)
                .Ret();

            //override get hash code
            var getHashCodeMethod = builder.NewMethod<int>("GetHashCode")
                .MethodAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig)
                .CallingConvention(CallingConventions.Standard | CallingConventions.HasThis)
                .Body()
                .LdFld(originField)
                .Call(getHashCodeBase)
                .Ret();
            //builder.Define().DefineMethodOverride(getHashCodeBase, getHashCodeBase);    

            //intercept properties
            foreach (var prop in ot.GetProperties().Where(p => p.GetMethod.IsVirtual))
            {
                bool shouldWrapIt = NeedsWrapping(prop.PropertyType);
                bool isCollection = prop.PropertyType.IsAssignableTo(typeof(ICollection));
                var itemType = TypeUtils.GetElementType(prop.PropertyType);
                var newProperty = builder.NewProperty(prop.Name, prop.PropertyType);
                var getter = newProperty.Getter()
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Public()
                        .Returns(prop.PropertyType)
                        .Body(m =>
                        {
                            m
                           .LdArg0()
                           .LdFld(originField)
                           .Call(prop.GetMethod);
                       
                            if (isCollection)
                            {
                                //returns new LinkedObservableCollection
                                var linkedCollectionCtor = typeof(LinkedObservableCollection<>).MakeGenericType(itemType).GetConstructors().First();
                                m.Newobj(linkedCollectionCtor);
                            }
                            else if (shouldWrapIt)
                            {
                                //return wrapped objec if the type is decorated by WrapMe
                                m.Call(typeof(EntityWrapper).GetMethod("Wrap"));
                            } 
                            m.Ret();
                        }); ;

                var eventArgsCtor = typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) });

                var setter = newProperty.Setter()
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Params(prop.PropertyType)

                        .Body(m => m
                            .DeclareLocal<PropertyChangedEventArgs>(out ILocal local)
                            .DefineLabel(out ILabel retLabel)
                            .LdArg0()
                            .LdFld(originField)
                            .LdArg1()
                            .Call(prop.SetMethod)
                            //return if null
                            .LdArg0()
                            .LdFld(propertyChangedField)
                            .BrFalseS(retLabel) //if not null 
                                                // create instance of PropertyChangedEventArgs 
                                .LdStr(prop.Name)
                                .Newobj(eventArgsCtor)
                                .StLoc(local)
                                //invoke event
                                .LdArg0()
                                .LdFld(propertyChangedField) //instance to invoke on
                                .LdArg0() //first parameter - sender
                                .LdLoc(local)
                                //call
                                .CallVirt(typeof(PropertyChangedEventHandler).GetMethod("Invoke"))
                            .MarkLabel(retLabel)
                            .Ret());


                newProperty.Define().SetGetMethod(getter.Define());
                newProperty.Define().SetSetMethod(setter.Define());




                //builder.Define().DefineMethodOverride(getter.Define(), prop.GetMethod);
                //builder.Define().DefineMethodOverride(setter.Define(), prop.SetMethod);
            }

            var type = builder.CreateType();
            TypesCreated[newTypeName] = type;
            Debug.WriteLine($"Emitting type {type.Name} from {ot.Name}    ");
            return type;
        }

        public static IList<T> SubstituteCollection<T>(IList<T> baseCollection)
        {
            return new ObservableCollection<T>(baseCollection);
        }
    }
}
