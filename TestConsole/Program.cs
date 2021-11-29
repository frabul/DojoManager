using DojoManagerApi;
using DojoManagerApi.Entities;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DojoManager
{
    class Program
    {
        static void Main(string[] args)
        {  
            var Db = new DojoManagerApi.TestNHibernate();
            Db.LoadBlankDb();
            Db.Test1();
            Db.CloseAndClear();

            Db.LoadBlankDb();
            Db.Test2();
            Db.CloseAndClear();

            Db.LoadBlankDb();
            Db.Test3();
            Db.CloseAndClear();

            Db.LoadBlankDb();
            Db.Test_Deletions();
            Db.CloseAndClear(); 
        } 
    }
}
