////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                    ibprovider.com. 22.04.2020
using System;
using lcpi.data.oledb;

//nuget package: ActiveScriptEngine
using ax_lib=ActiveXScriptLib;

using com_lib=lcpi.lib.com;
using adodb_lib=lcpi.lib.adodb;

namespace Sample_0030{
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

 //----------------------------------------------------------------------
 private const string c_vbCrLf="\r\n";

 private const string c_vbs_code=
 "option explicit" + c_vbCrLf +
 "" + c_vbCrLf +
 "function create_services()" + c_vbCrLf +
 " set create_services=new t_services" + c_vbCrLf +
 "end function" + c_vbCrLf +
 "" + c_vbCrLf +
 "class t_services" + c_vbCrLf +
 " private m_cn" + c_vbCrLf +
 "" + c_vbCrLf +
 " private sub class_initialize()" + c_vbCrLf +
 "  set m_cn=nothing" + c_vbCrLf +
 " end sub" + c_vbCrLf +
 "" + c_vbCrLf +
 " public property set connection(cn)" + c_vbCrLf +
 "  set m_cn=cn" + c_vbCrLf +
 " end property" + c_vbCrLf +
 "" + c_vbCrLf +
 " public function get_currency(country)" + c_vbCrLf +
 "  dim cmd" + c_vbCrLf +
 "  set cmd=createobject(\"ADODB.Command\")" + c_vbCrLf +
 "" + c_vbCrLf +
 "  set cmd.ActiveConnection=m_cn" + c_vbCrLf +
 "" + c_vbCrLf +
 "  cmd.CommandText=\"select currency from country where country=:country_name\"" + c_vbCrLf +
 "" + c_vbCrLf +
 "  cmd(\"country_name\")=country" + c_vbCrLf +
 "" + c_vbCrLf +
 "  dim rs" + c_vbCrLf +
 "  set rs=cmd.Execute()" + c_vbCrLf +
 "" + c_vbCrLf +
 "  if(rs.Eof)then" + c_vbCrLf +
 "    call err.raise(,-1,\"Country [\"&country&\"] not found!\")" + c_vbCrLf +
 "  end if" + c_vbCrLf +
 "" + c_vbCrLf +
 "  get_currency=rs(0).value" + c_vbCrLf +
 " end function" + c_vbCrLf +
 "end class";

 //----------------------------------------------------------------------
 static int Main(string[] args)
 {
  int resultCode=0;

  OleDbConnection  cn=null;
  OleDbTransaction tr=null;
  OleDbCommand     cmd=null;
  OleDbDataReader  rd=null;

  dynamic svcs=null;
  dynamic adodbCn=null;

  ax_lib.ActiveScriptEngine ax_engine=null;

  try
  {
   //Console.WriteLine("------------------- SCRIPT");
   //Console.WriteLine("{0}",c_vbs_code);
   //Console.WriteLine("------------------- /SCRIPT");
   //Console.WriteLine("");

   //--------------------
   cn=new OleDbConnection(c_cn_str);

   cn.Open();

   tr=cn.BeginTransaction();

   //--------------------
   Console.WriteLine("Attaching ADODB connection to OLEDB connection");

   adodbCn
    =com_lib.ObjectUtils.CreateInstance
      ("ADODB.Connection",
       com_lib.ClsCtxCode.CLSCTX_INPROC_SERVER).GetObject();
 
   adodb_lib.AdoDbConstructor.attach_adodb_cn_to_oledb_session
    (adodbCn,
     cn.GetNativeSession());

   //--------------------
   Console.WriteLine("Creation ActiveScriptEngine");

   ax_engine=new ax_lib.ActiveScriptEngine(ax_lib.VBScript.ProgId);

   ax_engine.AddCode(c_vbs_code);

   //--------------------
   Console.WriteLine("Creation object of scripted services");

   svcs=ax_engine.Evaluate("create_services()");

   svcs.connection=adodbCn;

   //--------------------
   Console.WriteLine("GO!");

   cmd=new OleDbCommand("select country from country",cn,tr);

   rd=cmd.ExecuteReader();

   while(rd.Read())
   {
    var countryName=rd["country"];

    Console.WriteLine("Currency of {0}: {1}",rd["country"],svcs.get_currency(countryName));
   }//while rd

   //--------------------
   tr.Commit();
  }
  catch(Exception e)
  {
   resultCode=1;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);

   if(!Object.ReferenceEquals(ax_engine,null))
   {
    var lastErr=ax_engine.LastError;

    if(!Object.ReferenceEquals(lastErr,null))
    {
     Console.WriteLine("AXSCR.POSITION : Line={0}, Column={1}",lastErr.LineNumber,lastErr.ColumnNumber);
     Console.WriteLine("AXSCR.LineText : {0}",lastErr.LineText);
     Console.WriteLine("AXSCR.DESCR    : {0}",lastErr.Description);
    }//if
   }//if
  }//catch
  finally
  {
   Helper__ReleaseComObject(ref svcs); // <--- Releasing object of scripted services

   Helper__Dispose(ref ax_engine);
   
   Helper__ReleaseComObject(ref adodbCn); // <--- Releasing ADODB connection object

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

 //-----------------------------------------------------------------------
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
}//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0030
