//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;

//namespace StackOverflowSoru
//{
//    public delegate void();
//    public interface Iperson
//    {

//        void Work(string message);

//        event SomeththingHappenedEventHandler SomeththingHappened;
//    }
//    public class person : Iperson
//    {
//        public event SomeththingHappenedEventHandler SomeththingHappened;

//        public void Work(string thework)
//        {
//            Console.WriteLine("Working on: {0}", thework);
//        }
//    }

//    class Boss
//    {
//        public void HeardIt()
//        {
//            Console.WriteLine("I Heard you buddy");
//        }
//    }
//    class Program
//    {
//        static Type CreateDynamicProxyType()
//        {
//            var assemblyName = new AssemblyName("MyProxies");
//            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
//                   assemblyName,
//                   AssemblyBuilderAccess.Run);
//            var modBuilder = assemblyBuilder.DefineDynamicModule("MyProxies");

//            // public class person
//            // {
//            var typeBuilder = modBuilder.DefineType(
//                "mypersonproxy",
//                TypeAttributes.Public | TypeAttributes.Class,
//                typeof(object),
//                new[] { typeof(Iperson) });

//            //      private person _realObject;
//            var fieldBuilder = typeBuilder.DefineField(
//                "_realObject",
//                typeof(person),
//                FieldAttributes.Private);
//            //event
//            var field = typeBuilder.DefineField("SomeththingHappened", typeof(SomeththingHappenedEventHandler), FieldAttributes.Private);
//            var eventInfo = typeBuilder.DefineEvent("SomeththingHappened", EventAttributes.None, typeof(SomeththingHappenedEventHandler));

//            //add
//            var addMethod = typeBuilder.DefineMethod("add_SomeththingHappened",
//        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
//        CallingConventions.Standard | CallingConventions.HasThis,
//        typeof(void),
//        new[] { typeof(SomeththingHappenedEventHandler) });
//            var generator = addMethod.GetILGenerator();
//            var combine = typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) });
//            generator.Emit(OpCodes.Ldarg_0);
//            generator.Emit(OpCodes.Ldarg_0);
//            generator.Emit(OpCodes.Ldfld, field);
//            generator.Emit(OpCodes.Ldarg_1);
//            generator.Emit(OpCodes.Call, combine);
//            generator.Emit(OpCodes.Castclass, typeof(SomeththingHappenedEventHandler));
//            generator.Emit(OpCodes.Stfld, field);
//            generator.Emit(OpCodes.Ret);
//            eventInfo.SetAddOnMethod(addMethod);

//            //remove
//            var removeMethod = typeBuilder.DefineMethod("remove_SomeththingHappened",
//        MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
//        CallingConventions.Standard | CallingConventions.HasThis,
//        typeof(void),
//        new[] { typeof(SomeththingHappenedEventHandler) });
//            var remove = typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) });
//            var removator = removeMethod.GetILGenerator();
//            removator.Emit(OpCodes.Ldarg_0);
//            removator.Emit(OpCodes.Ldarg_0);
//            removator.Emit(OpCodes.Ldfld, field);
//            removator.Emit(OpCodes.Ldarg_1);
//            removator.Emit(OpCodes.Call, remove);
//            removator.Emit(OpCodes.Castclass, typeof(SomeththingHappenedEventHandler));
//            removator.Emit(OpCodes.Stfld, field);
//            removator.Emit(OpCodes.Ret);
//            eventInfo.SetRemoveOnMethod(removeMethod);

//            //      public mypersonproxy(person arg1)
//            //      {
//            var constructorBuilder = typeBuilder.DefineConstructor(
//                MethodAttributes.Public,
//                CallingConventions.HasThis,
//                new[] { typeof(person) });
//            var contructorIl = constructorBuilder.GetILGenerator();
//            contructorIl.Emit(OpCodes.Ldarg_0);
//            contructorIl.Emit(OpCodes.Ldarg_1);
//            contructorIl.Emit(OpCodes.Stfld, fieldBuilder);
//            contructorIl.Emit(OpCodes.Ret);
//            //      }

//            //      public void Work(string arg1)
//            //      {
//            var methodBuilder = typeBuilder.DefineMethod("Work",
//                MethodAttributes.Public | MethodAttributes.Virtual,
//                typeof(void),
//                new[] { typeof(string) });
//            typeBuilder.DefineMethodOverride(methodBuilder,
//                typeof(Iperson).GetMethod("Work"));
//            var tweetIl = methodBuilder.GetILGenerator();

//            //          Console.WriteLine("Hello before!");
//            tweetIl.Emit(OpCodes.Ldstr, "Hello before!");
//            tweetIl.Emit(OpCodes.Call, typeof(Console)
//                .GetMethod("WriteLine", new[] { typeof(string) }));

//            //raise probably something wrong here
//            var eventmethodBuilder = typeBuilder.DefineMethod("OnSomeththingHappened",
//        MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
//        typeof(void),
//        new[] { typeof(string) });
//            var methodgenerator = eventmethodBuilder.GetILGenerator();

//            var returnLabel = methodgenerator.DefineLabel();

//            var eventArgsCtor = typeof(PropertyChangedEventArgs).GetConstructor(new[] { typeof(string) });

//            methodgenerator.DeclareLocal(typeof(PropertyChangedEventHandler));

//            methodgenerator.Emit(OpCodes.Ldarg_0);
//            methodgenerator.Emit(OpCodes.Ldfld, field);
//            methodgenerator.Emit(OpCodes.Stloc_0);
//            methodgenerator.Emit(OpCodes.Ldloc_0);
//            methodgenerator.Emit(OpCodes.Brfalse, returnLabel);
//            methodgenerator.Emit(OpCodes.Ldloc_0);
//            methodgenerator.Emit(OpCodes.Ldarg_0);
//            methodgenerator.Emit(OpCodes.Ldarg_1);

//            methodgenerator.Emit(OpCodes.Newobj, eventArgsCtor);
//            methodgenerator.Emit(OpCodes.Callvirt, typeof(PropertyChangedEventHandler).GetMethod("Invoke"));
//            methodgenerator.MarkLabel(returnLabel);
//            methodgenerator.Emit(OpCodes.Ret);

//            eventInfo.SetRaiseMethod(eventmethodBuilder);


//            //          _realObject.Work(arg1);
//            tweetIl.Emit(OpCodes.Ldarg_0);
//            tweetIl.Emit(OpCodes.Ldfld, fieldBuilder);
//            tweetIl.Emit(OpCodes.Ldarg_1);
//            tweetIl.Emit(OpCodes.Call,
//                fieldBuilder.FieldType.GetMethod("Work"));

//            //          Console.WriteLine("Hello after!");
//            tweetIl.Emit(OpCodes.Ldstr, "Hello after!");
//            tweetIl.Emit(OpCodes.Call, typeof(Console)
//                .GetMethod("WriteLine", new[] { typeof(string) }));
//            tweetIl.Emit(OpCodes.Ret);
//            //      }
//            // }


//            return typeBuilder.CreateType();
//        }

//        static void Main(string[] args)
//        {
//            Boss boss1 = new Boss();

//            var type = CreateDynamicProxyType();

//            var dynamicProxy = (Iperson)Activator.CreateInstance(
//                type, new object[] { new person() });

//            dynamicProxy.SomeththingHappened += boss1.HeardIt;

//            dynamicProxy.Work("fill the excel documents");
//        }
//    }
//}