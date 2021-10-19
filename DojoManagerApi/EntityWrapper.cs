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

namespace DojoManagerApi {
    /// <summary>
    /// Creates a new class that inherit from the wrapped type
    /// Internally the wrapper stores a reference to the wrapped instance ( origin )
    ///     - All of the propoerties get/set are intercepted and redirected to the origin 
    ///     - PropertyChanged event is reaised when setters are called
    ///     - IList 
    /// </summary>
    public class EntityWrapper
    {
        static Dictionary<object,object> WrappedObjects = new Dictionary<object,object>();
        
    

        static void AddPropertyChanged(PropertyChangedEventHandler field, PropertyChangedEventHandler handler)
        {
            field += handler;
        }

        

        public static object Wrap(object entity, Type et)
        {
            if(WrappedObjects.ContainsKey(entity))
                return WrappedObjects[entity];

            var builder = TypeFactory
                .Default
                .NewType("TestType").InheritsFrom(et)
                .Implements<INotifyPropertyChanged>()
                ;


            var delegateCombine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
            var delegateRemove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });

            var propertyChangedEvent = builder.NewEvent("PropertyChanged", typeof(PropertyChangedEventHandler));
            var propertyChangedField = builder.NewField("PropertyChanged", typeof(PropertyChangedEventHandler));
            //event add 
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
            //event remove 
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

            var origin = builder.NewField("Origin", et).Public();
            builder.NewDefaultConstructor(System.Reflection.MethodAttributes.Public);
            builder.NewConstructor()
                .Public()
                .Param(et, "origin")
                .Body()
                .LdArg0()
                .LdArg1()
                .StFld(origin)
                .Ret();

            foreach (var prop in et.GetProperties().Where(p => p.GetMethod.IsVirtual))
            {
                var newProperty = builder.NewProperty(prop.Name, prop.PropertyType);
                var getter = newProperty.Getter()
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Public()
                        .Returns(prop.PropertyType)
                        .Body(m => m
                            .LdArg0()
                            .LdFld(origin)
                            .Call(prop.GetMethod)
                            .Ret());

                var eventArgsCtor = typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) });
                DebugOutput.Output = new ConsoleOutput();
                var setter = newProperty.Setter()  
                        .MethodAttributes(MethodAttributes.Virtual | MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig)
                        .Params(prop.PropertyType)
                         
                        .Body(m => m
                            .DeclareLocal<PropertyChangedEventArgs>( out ILocal local)
                            .DefineLabel(out ILabel retLabel) 
                            .LdArg0()
                            .LdFld(origin)
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
            //dynamic inst = Activator.CreateInstance(type);
            //type.GetField("Origin").SetValue(inst, entity);
            object? inst = Activator.CreateInstance(type, entity);
            if (inst == null)
                throw new Exception("Unable to create");
            return inst;

        }
    }
}
