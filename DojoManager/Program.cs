using System;

namespace DojoManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var test = new DojoManagerApi.TestNHibernate();
            test.Test();
        }
    }
}
