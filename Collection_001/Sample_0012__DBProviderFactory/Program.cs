using System;
using System.Data;
using System.Data.Common;

namespace Sample_0012{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 //Requirement:
 // - Install assemblies into GAC
 // - Define default provider for .NET Runtime

 //-----------------------------------------
 private const string c_dbfactory_name="lcpi.data.oledb";

 //-----------------------------------------
 private const string c_cn_str
  ="provider=LCPI.IBProvider.3;"
  +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;";

 static int Main()
 {
  int resultCode=0;

  try // [catch]
  {
   Console.WriteLine("Create factory \"{0}\"",
                     c_dbfactory_name);

   DbProviderFactory factory
    =DbProviderFactories.GetFactory(c_dbfactory_name);

   Console.WriteLine("Factory Assembly: \"{0}\"",
                     factory.GetType().Assembly.FullName);

   Console.WriteLine("Factory Type    : \"{0}\"",
                      factory.GetType().FullName);

   using(var cn=factory.CreateConnection())
   {
    cn.ConnectionString=c_cn_str;

    cn.Open();

    cn.Close();
   }//using

   Console.WriteLine("OK");
  }
  catch(Exception e)
  {
   resultCode=1;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch

  return resultCode;
 }//Main
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0012
