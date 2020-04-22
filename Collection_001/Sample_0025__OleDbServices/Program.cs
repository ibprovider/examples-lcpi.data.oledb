////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   07.05.2017.
using System;
using System.Data;
using System.Diagnostics;
using lcpi.data.oledb;

using structure_lib=lcpi.lib.structure;

namespace Sample_0025{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 //OLE DB Services
 // - MSDASC.MSDAINITIALIZE.1
 // - LCPI.OleDbServices.DataInitManager.Global.1
 // - LCPI.OleDbServices.DataInitManager.Local.1
 private const string c_ProgID_OleDbServices
  ="LCPI.OleDbServices.DataInitManager.Global.1";

 //Direct connection to Firebird DBMS
 private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\fb_03_0_0\\ibp_test_fb30_d3.gdb;" //any FB database
   +"dbclient_type=fb.direct;"
   +"user id=SYSDBA;"
   +"password=masterkey;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try //[catch]
  {
   using(OleDbServices services=new OleDbServices(c_ProgID_OleDbServices))
   {
    for(uint pass=0;pass!=3;)
    {
     ++pass;

     Console.WriteLine("----------------------- pass: {0}",pass);

     for(uint nCns=1;nCns!=4;++nCns)
     {
      Console.WriteLine("------- nCns: {0}",nCns);

      var cns=new OleDbConnection[nCns];

      try //finally
      {
       for(uint iCn=0;iCn!=nCns;++iCn)
       {
        cns[iCn]=services.CreateConnection(c_cn_str); //throw

        cns[iCn].Open(); //throw

        Console.WriteLine("{0}. {1}",iCn+1,Helper__GetCnSign(cns[iCn])); //throw
       }
      }
      finally
      {
       for(uint iCn=0;iCn!=nCns;++iCn)
        structure_lib.DisposeUtils.Exec(ref cns[iCn]);
      }//finally
     }//for nCn
    }//for pass
   }//using services
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch

  return resultCode;
 }//main

 //-----------------------------------------------------------------------
 private static string Helper__GetCnSign(OleDbConnection cn)
 {
  string result;

  using(var tr=cn.BeginTransaction())
  {
   using(var cmd=new OleDbCommand(null,cn,tr))
   {
    cmd.CommandText
     ="select CURRENT_CONNECTION, CURRENT_TRANSACTION from RDB$DATABASE";
    
    using(var reader=cmd.ExecuteReader())
    {
     if(!reader.Read())
      throw new ApplicationException("No record!");
    
     result=string.Format("connection: {0}, transaction: {1}",
                          reader[0],
                          reader[1]);
    }//using reader
   }//using cmd

   tr.Commit();
  }//using tr

  return result;
 }//Helper__GetCnSign
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//nms Sample_0025
