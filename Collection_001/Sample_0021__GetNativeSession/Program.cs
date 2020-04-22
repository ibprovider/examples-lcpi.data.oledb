////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   26.03.2015.
using System;
using System.Data;
using System.Diagnostics;
using lcpi.data.oledb;

using com_lib=lcpi.lib.com;
using adodb_lib=lcpi.lib.adodb;

namespace Sample_0021{
////////////////////////////////////////////////////////////////////////////////
//
// LCPI.IBP.Samples.IBGenManager.1
//  - Sample COM-object (writed on C++) from IBProvider installation kit.
//    Works with OLEDB provider through native OLEDB interfaces.
//
////////////////////////////////////////////////////////////////////////////////
//
// Scenario:
// - ADO.NET provider creates the connection to database
// - ADODB connection initiates the transaction
// - Sample COM object (GenManager) executes "GEN_ID" statement for generation of record identifier
// - ADODB connection commits the transaction
//
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="provider=LCPI.IBProvider.5;"
   +"location=localhost:d:\\database\\fb_03_0_0\\employee.fdb;"
   +"dbclient_type=fb.direct;"
   +"user id=SYSDBA;"
   +"password=masterkey;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  dynamic adodbCn=null;

  dynamic genMng=null;

  try //[catch] [finally]
  {
   using(var oledbCn=new OleDbConnection(c_cn_str))
   {
    Console.WriteLine("[ADO.NET] Connection to database...");

    oledbCn.Open();

    //--------------------------------------
    Console.WriteLine("Creation of ADODB.Connection object ...");

    adodbCn
      =com_lib.ObjectUtils.CreateInstance
        ("ADODB.Connection",
         com_lib.ClsCtxCode.CLSCTX_INPROC_SERVER).GetObject();

    Console.WriteLine("Connect ADODB to ADO.NET");

    adodb_lib.AdoDbConstructor.attach_adodb_cn_to_oledb_session
     (adodbCn,
      oledbCn.GetNativeSession());

    //--------------------------------------
    Console.WriteLine("[ADODB] Start transaction...");

    adodbCn.BeginTrans();

    //--------------------------------------
    Console.WriteLine("Creation of LCPI.IBP.Samples.IBGenManager.1");

    genMng
      =com_lib.ObjectUtils.CreateInstance
         ("LCPI.IBP.Samples.IBGenManager.1",
          com_lib.ClsCtxCode.CLSCTX_INPROC_SERVER).GetObject();

    //--------------------------------------
    Console.WriteLine("Connect GenMng to ADODB");

    genMng.Connection=adodbCn;

    //--------------------------------------
    Console.WriteLine("[GenMng] Generate ID: {0}",
                      genMng.GenID("CUST_NO_GEN"));

    //--------------------------------------
    Console.WriteLine("[ADODB] Commit transaction...");

    adodbCn.CommitTrans();
   }//using oledbCn
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch
  finally
  {
   Helper__ReleaseComObject(ref adodbCn);
   Helper__ReleaseComObject(ref genMng);
  }//finally

  return resultCode;
 }//Main

 //Helper interface ------------------------------------------------------
 private static void Helper__ReleaseComObject<T>(ref T obj) where T:class
 {
  var x=System.Threading.Interlocked.Exchange(ref obj,null);

  Helper__ReleaseComObject(x);
 }//Helper__ReleaseComObject

 //-----------------------------------------------------------------------
 private static void Helper__ReleaseComObject(object obj)
 {
  if(!Object.ReferenceEquals(obj,null))
   System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
 }//Helper__ReleaseComObject
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//nms Sample_0021
