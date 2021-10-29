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

    public interface IEntityWrapper<out T>
    {
        T Origin { get; }
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
            if (entity == null)
                return null;
            //DebugOutput.Output = new ConsoleOutput(); 
            Type ot = entity.GetType();

            if (!NeedsWrapping(ot))
                return entity;

            if (WrappedObjects.ContainsKey(entity))
                return WrappedObjects[entity];

            Debug.WriteLine($"Wrapping object {entity.GetType().Name} {entity.GetHashCode()} ");
            while (!TypeUtils.HasAttribute<WrapMeAttribute>(ot))
                ot = ot.BaseType;

            // get the new type
            Type newType = BuildNewType(ot);

            //create new instance now and register it ( before cascading replacements to avoid endless loop )
            object inst = Activator.CreateInstance(newType, entity);
            if (inst == null)
                throw new Exception("Unable to create");
            WrappedObjects[entity] = inst;
            return inst;

        }

        private static Type BuildNewType(Type ot)
        {
            //Check if wrapping is needed
            if (!NeedsWrapping(ot))
                return ot;

            //emit Type 
            //definitions 
            var delegateCombine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
            var delegateRemove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });


            var newTypeName = $"{ot.FullName}{WrapperTypeSuffix}";

            if (TypesCreated.ContainsKey(newTypeName))
                return TypesCreated[newTypeName];

            //type not yet created, let's create it
            var builder = TypeFactory
                            .Default
                            .NewType(newTypeName)
                            .InheritsFrom(ot)
                            .Implements(typeof(IEntityWrapper<>).MakeGenericType(ot))
                            .Implements<INotifyPropertyChanged>();

            //add  fields
            var propertyChangedEvent = builder.NewEvent("PropertyChanged", typeof(PropertyChangedEventHandler));
            var propertyChangedField = builder.NewField("PropertyChanged", typeof(PropertyChangedEventHandler));
            var originField = builder.NewField("_Origin", ot).Public();

            //-- constructors
            builder.NewDefaultConstructor(System.Reflection.MethodAttributes.Public);
            builder.NewConstructor()
                .Public()
                .Param(ot, "origin")
                .Body()
                .LdArg0()
                .LdArg1()
                .StFld(originField)
                .Ret();

            // implemente IEntityWrapper
            var OriginProperty = builder
                .NewProperty("Origin", ot);
            OriginProperty.Getter()
                .MethodAttributes(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual)
                .CallingConvention(CallingConventions.HasThis | CallingConventions.Standard)
                .Body()
                    .LdArg0()
                    .LdFld(originField)
                    .Ret();
            OriginProperty.Define();




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


            //override get hash code
            //var oldGetHashCode = ot.GetMethod("GetHashCode");
            //var getHashCodeMethod = builder.NewMethod("GetHashCode")
            //    .Returns(oldGetHashCode.ReturnType)
            //    .MethodAttributes(oldGetHashCode.Attributes)
            //    .CallingConvention(oldGetHashCode.CallingConvention)
            //        .Body()
            //            .LdArg0()
            //            .LdFld(originField)
            //            .CallVirt(oldGetHashCode)
            //            .Ret();
            ////override equals  
            //var oldEquals = ot.GetMethod("Equals", new[] { ot });
            //if (oldEquals != null)
            //{
            //    var equalsMethod = builder.NewMethod("Equals")
            //        .Returns(oldEquals.ReturnType)
            //        .MethodAttributes(MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig)
            //        .CallingConvention(oldEquals.CallingConvention)
            //        .Params(oldEquals.GetParameters().Select(p => p.ParameterType).ToArray())
            //            .Body()
            //                .DefineLabel(out ILabel lab1)
            //                //fist instance is always Origin
            //                .LdArg0()
            //                .LdFld(originField)
            //                //if arg1 is of type wrapper, load origin, otherwise load itself
            //                .LdArg1()
            //                .IsInst(builder.Define())
            //                .BrFalseS(lab1) //if cast is null jump 
            //                    .LdArg1()
            //                    .IsInst(builder.Define())
            //                    .Call(oldEquals)
            //                    .Ret()
            //                .MarkLabel(lab1)//arg1 is of base type
            //                .LdArg1()
            //                .Call(oldEquals)
            //                .Ret();
            //}

            //intercept properties
            var properties = ot.GetProperties(BindingFlags.Instance /*| BindingFlags.NonPublic*/ | BindingFlags.Public).Where(p => p.GetMethod.IsVirtual).ToList();
            foreach (var prop in properties)
            {
                if (prop.Name == nameof(IEntityWrapper<Person>.Origin))
                    continue;
                bool shouldWrapIt = NeedsWrapping(prop.PropertyType);
                var itemType = TypeUtils.GetElementType(prop.PropertyType);
                bool isCollection = prop.PropertyType.IsAssignableTo(typeof(IList<>).MakeGenericType(itemType));
                var newProperty = builder.NewProperty(prop.Name, prop.PropertyType);
                var likedCollectionType = typeof(LinkedObservableCollection<>).MakeGenericType(itemType);
                IFieldBuilder linkedCollectionField = null;
                if (isCollection)
                {
                    //allocate a field to keep the linked collection instance
                    linkedCollectionField = builder.NewField($"_{prop.Name}Linked", likedCollectionType);
                }
                var getter = newProperty.Getter()
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Public()
                        .Returns(prop.PropertyType)
                        .Body(m =>
                        {


                            if (isCollection)
                            {
                                //returns new LinkedObservableCollection
                                var linkedCollectionCtor = likedCollectionType.GetConstructors().First();
                                m
                                .DefineLabel(out ILabel fieldNotNull)
                                .LdArg0()
                                .LdFld(linkedCollectionField)
                                .BrTrueS(fieldNotNull) // if field is null
                                    .LdArg0() //this will be used later by StFld 
                                    .LdArg0()
                                    .LdFld(originField)
                                    .CallVirt(prop.GetMethod)
                                    .Newobj(linkedCollectionCtor)
                                    .StFld(linkedCollectionField)
                                //if not null end
                                .MarkLabel(fieldNotNull)
                                .LdArg0()
                                .LdFld(linkedCollectionField);
                            }
                            else
                            {
                                m
                                .LdArg0()
                                .LdFld(originField)
                                .CallVirt(prop.GetMethod);
                                if (shouldWrapIt)
                                {
                                    //return wrapped objec if the type is decorated by WrapMe 
                                    m.Call(typeof(EntityWrapper).GetMethod("Wrap"));
                                }
                            }
                            m.Ret();
                        }); ;

                var eventArgsCtor = typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) });
                if (prop.SetMethod != null)
                {
                    var setter = newProperty.Setter()
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Params(prop.PropertyType)

                        .Body(m => m
                            .DeclareLocal<PropertyChangedEventArgs>(out ILocal local)
                            .DefineLabel(out ILabel retLabel)
                            .LdArg0()
                            .LdFld(originField)
                            .LdArg1()
                            .CallVirt(prop.SetMethod)
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
                }



                newProperty.Define();




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
