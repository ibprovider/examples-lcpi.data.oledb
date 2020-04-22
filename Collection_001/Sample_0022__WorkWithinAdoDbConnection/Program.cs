////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   26.03.2015.
using System;
using System.Data;
using System.Diagnostics;
using lcpi.data.oledb;

using com_lib=lcpi.lib.com;
using adodb_lib=lcpi.lib.adodb;

namespace Sample_0022{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="provider=LCPI.IBProvider.5;"
   +"location=localhost:d:\\database\\fb_03_0_0\\employee.fdb;"
   +"dbclient_type=fb.direct;"
   +"user id=gamer;"
   +"password=vermut;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  dynamic adodbCn=null;

  try //[catch] [finally]
  {
   adodbCn
    =com_lib.ObjectUtils.CreateInstance
      ("ADODB.Connection",
       com_lib.ClsCtxCode.CLSCTX_INPROC_SERVER).GetObject();

   adodbCn.ConnectionString=c_cn_str;

   adodbCn.Open();

   adodbCn.BeginTrans();

   using(var oledbCn=new OleDbConnection())
   {
    oledbCn.AttachToNativeSession(adodb_lib.AdoDbConstructor.get_oledb_session_from_adodb_cn(adodbCn));

    using(var cmd=oledbCn.CreateCommand())
    {
     cmd.CommandText="select c.CURRENCY from COUNTRY c where c.COUNTRY=:country";

     cmd["country"].Value="USA";

     Console.WriteLine("currency: {0}",cmd.ExecuteScalar());
    }
   }//using oledbCn

   adodbCn.CommitTrans();
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch
  finally
  {
   Helper__ReleaseComObject(ref adodbCn);
  }//finally

  return resultCode;
 }//main

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
}//nms Sample_0022