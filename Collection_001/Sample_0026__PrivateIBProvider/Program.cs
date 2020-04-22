////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLE DB.
//                                                                   20.08.2018.
using System;

using xdb=lcpi.data.oledb;

namespace Sample_0026{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 static void Main(string[] args)
 {
  try
  {
   var cnsb=new xdb.OleDbConnectionStringBuilder();

   cnsb.Provider="LCPI.IBProvider.3";

   //Load IBProvider and read initialization properties
   Console.WriteLine("cnsb.Keys.Count={0}",cnsb.Keys.Count);

   cnsb.Location="inet4://localhost/d:\\database\\fb_03_0_0\\ibp_test_fb30_d3.gdb";

   cnsb.UserID="GAMER";

   cnsb.Password="vermut";

   cnsb.IBProvider.dbclient_type="fb.direct";

   for(uint n=0;n!=3;)
   {
    Console.WriteLine("-------------- pass: {0}",++n);

    using(var cn=new xdb.OleDbConnection(cnsb.ConnectionString))
    {
     Console.WriteLine("Open connection ...");

     //Create standard connection pool, which mapped to "LCPI OLE DB Services", and
     //open connection through IBProvider
     cn.Open();

     using(var tr=cn.BeginTransaction())
     {
      using(var cmd=new xdb.OleDbCommand("",cn,tr))
      {
       cmd.CommandText="select CURRENT_CONNECTION from RDB$DATABASE";

       Console.WriteLine("Connection ID: {0}",cmd.ExecuteScalar());
      }

      tr.Commit();
     }//using tr
    }//using cn
   }//for n
  }
  catch(Exception e)
  {
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }

  Console.WriteLine("");
  Console.WriteLine("[Press Any Key]");

  Console.ReadKey();
 }//Main
}//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0026
