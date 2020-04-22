////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                    ibprovider.com. 19.04.2020
using System;
using lcpi.data.oledb;

//nuget package: lcpi.sdk.ibprovider.v05
using ibp_sdk=lcpi.sdk.ibprovider.v05;

namespace Sample_0029{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
  ="provider=LCPI.IBProvider.5;"
  +"location=localhost:d:\\database\\ram\\ibp_test_fb30_d3.gdb;"
  +"dbclient_type=fb.direct;"
  +"user id=GAMER;"
  +"password=vermut;";

 //----------------------------------------------------------------------
 static int Main(string[] args)
 {
  int resultCode=0;

  OleDbConnection  cn=null;
  OleDbTransaction tr=null;
  OleDbCommand     cmd=null;

  try
  {
   cn=new OleDbConnection(c_cn_str);

   cn.Open();

   tr=cn.BeginTransaction();

   cmd=new OleDbCommand(null,cn,tr);

   //--------------------
   cmd.CommandText
    ="select ID from DUAL where ID=1";

   cmd.Prepare();

   object stmtPlan
    =cmd.Properties.ReadByID
      (ibp_sdk.IBP_DBPROPSET.STATEMENTINFO.ID,
       ibp_sdk.IBP_DBPROPSET.STATEMENTINFO.PROPID.PLAN);

   Console.WriteLine("Stmt Plan: {0}",stmtPlan);

   //--------------------
   tr.Commit();
  }
  catch(Exception e)
  {
   resultCode=1;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch
  finally
  {
   Helper__Dispose(ref cmd);
   Helper__Dispose(ref tr);
   Helper__Dispose(ref cn);
  }//finally

  return resultCode;
 }//Main

 //Helper interface ------------------------------------------------------
 private static void Helper__Dispose<T>(ref T obj) where T:class, IDisposable
 {
  var x=System.Threading.Interlocked.Exchange(ref obj,null);

  Helper__Dispose(x);
 }//Helper__Dispose

 //-----------------------------------------------------------------------
 private static void Helper__Dispose(IDisposable obj)
 {
  if(!Object.ReferenceEquals(obj,null))
   obj.Dispose();
 }//Helper__Dispose
}//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0029
