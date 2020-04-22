using System;
using System.Data;
using System.Data.Common;
using lcpi.data.oledb;

using System.Security;
using System.Security.Permissions;

using System.Globalization;
using System.Threading;

namespace Sample_0013{
////////////////////////////////////////////////////////////////////////////////
// Attention:
//  You should install providers into GAC!

////////////////////////////////////////////////////////////////////////////////
//class TestObject

public class TestObject:MarshalByRefObject
{
 private bool   m_enter=false;
 private bool   m_try_open=false;
 private object m_rec_count=null;
 private bool   m_exit=false;

 public void Exec(string connectionString)
 {
  m_enter=true;

  using (var cn=new OleDbConnection(connectionString))
  {
   m_try_open=true;

   cn.Open();

   using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
   {
    using(var cmd=new OleDbCommand("select count(*) from rdb$database",cn,tr))
    {
     m_rec_count=cmd.ExecuteScalar();
    }//using cmd

    tr.Commit();
   }
  }//using cn

  m_exit=true;
 }//Exec

 //-----------------------------------------------------------------------
 public string State
 {
  get
  {
   return string.Format("enter:{0}, try_open:{1}, rec_count:{2}, exit:{3}",
                         m_enter,
                         m_try_open,
                         Helper__ToStr(m_rec_count),
                         m_exit);
  }//get
 }//State

 //-----------------------------------------------------------------------
 private static string Helper__ToStr(object x)
 {
  if(Object.ReferenceEquals(x,null))
   return "#NULL";

  if(DBNull.Value==x)
   return "#DBNULL";

  return x.ToString();
 }//Helper__ToStr
};//class TestObject

////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 //-----------------------------------------------------------------------
 private const string c_cn_str
  ="provider=LCPI.IBProvider.3;"
  +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;";

 //-----------------------------------------------------------------------
 private static int Main()
 {
  int resultCode=1;

  try
  {
   Main_Impl();
  }
  catch(Exception e)
  {
   resultCode=1;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch

  return resultCode;
 }//Main

 //-----------------------------------------------------------------------
 private static readonly Action[] sm_Tests=
 {
  Exec__no_OleDbPermission,
  Exec__ok,
  Exec__badCnStr,
  Exec__Unrestricted,
 };//sm_Tests

 //-----------------------------------------------------------------------
 private static void Main_Impl()
 {
  var OldCulture=Thread.CurrentThread.CurrentUICulture;

  try
  {
   Thread.CurrentThread.CurrentUICulture=CultureInfo.InvariantCulture;

   //--------------------------------------- TESTS
   for(int i=0,_c=sm_Tests.Length;i!=_c;++i)
   {
    Console.WriteLine("---------------------------- {0}. {1}",
                      (i+1),sm_Tests[i].Method.Name);

    sm_Tests[i]();

    Console.WriteLine("");
   }//for i
  }
  finally
  {
   if(!Object.ReferenceEquals(Thread.CurrentThread.CurrentUICulture,OldCulture))
    Thread.CurrentThread.CurrentUICulture=OldCulture;
  }//finally
 }//Main_Impl

 //-----------------------------------------------------------------------
 static void Exec__no_OleDbPermission()
 {
  var permissions
   =new PermissionSet(PermissionState.None);

  permissions.AddPermission(Helper__CreateExecutePermission());

  AppDomain testDomain=null;

  try
  {
   testDomain=Helper_CreateTestDomain("NoSqlPermissions",
                                      permissions);

   var testObject=Helper__CreateTestObject(testDomain);

   Console.WriteLine("Try call testObject.Exec ...");

   Helper__Exec_Failed(testObject,c_cn_str);

   Console.WriteLine("OK [{0}]",testObject.State);
  }
  finally
  {
   if(!Object.ReferenceEquals(testDomain,null))
    AppDomain.Unload(testDomain);
  }//finally
 }//Exec__no_OleDbPermission

 //-----------------------------------------------------------------------
 static void Exec__ok()
 {
  var permissions
   =new PermissionSet(PermissionState.None);

  permissions.AddPermission(Helper__CreateExecutePermission());

  permissions.AddPermission(Helper__CreateOleDbPermission(c_cn_str));

  AppDomain testDomain=null;

  try
  {
   testDomain=Helper_CreateTestDomain("Exec__ok",
                                      permissions);

   var testObject=Helper__CreateTestObject(testDomain);

   Helper__Exec(testObject,c_cn_str);

   Console.WriteLine("OK [{0}]",testObject.State);
  }
  finally
  {
   if(!Object.ReferenceEquals(testDomain,null))
    AppDomain.Unload(testDomain);
  }//finally
 }//Exec__ok

 //-----------------------------------------------------------------------
 static void Exec__badCnStr()
 {
  var permissions
   =new PermissionSet(PermissionState.None);

  permissions.AddPermission(Helper__CreateExecutePermission());

  permissions.AddPermission(Helper__CreateOleDbPermission(c_cn_str));

  AppDomain testDomain=null;

  try
  {
   testDomain=Helper_CreateTestDomain("Exec__badCnStr",
                                      permissions);

   var testObject=Helper__CreateTestObject(testDomain);

   Helper__Exec_Failed(testObject,"prop1=val1;"+c_cn_str);

   Console.WriteLine("OK [{0}]",testObject.State);
  }
  finally
  {
   if(!Object.ReferenceEquals(testDomain,null))
    AppDomain.Unload(testDomain);
  }//finally
 }//Exec__badCnStr

 //-----------------------------------------------------------------------
 static void Exec__Unrestricted()
 {
  var permissions
   =new PermissionSet(PermissionState.None);

  permissions.AddPermission(Helper__CreateExecutePermission());

  permissions.AddPermission(new OleDbPermission(PermissionState.Unrestricted));

  AppDomain testDomain=null;

  try
  {
   testDomain=Helper_CreateTestDomain("Exec__Unrestricted",
                                      permissions);

   var testObject=Helper__CreateTestObject(testDomain);

   Helper__Exec(testObject,"prop1=val1;"+c_cn_str);

   Console.WriteLine("OK [{0}]",testObject.State);
  }
  finally
  {
   if(!Object.ReferenceEquals(testDomain,null))
    AppDomain.Unload(testDomain);
  }//finally
 }//Exec__Unrestricted

 //helper methods --------------------------------------------------------
 private static AppDomain Helper_CreateTestDomain(string        name,
                                                  PermissionSet permissions)
 {
  Console.WriteLine("Create testDomain [{0}] ...",name);

  var appDomainSetup
   =new AppDomainSetup();

  appDomainSetup.ApplicationBase
   =AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

  //----------------------------------------
  return AppDomain.CreateDomain(name,
                                AppDomain.CurrentDomain.Evidence,
                                appDomainSetup,
                                permissions);
 }//Helper_CreateTestDomain

 //-----------------------------------------------------------------------
 private static TestObject Helper__CreateTestObject(AppDomain testDomain)
 {
  Console.WriteLine("Create testObject ...");

  var TestObjectType=typeof(TestObject);

  return (TestObject)testDomain.CreateInstanceAndUnwrap
                                           (TestObjectType.Assembly.FullName,
                                            TestObjectType.FullName);
 }//Helper__CreateTestObject

 //-----------------------------------------------------------------------
 private static SecurityPermission Helper__CreateExecutePermission()
 {
  const SecurityPermissionFlag spflags
   = 0
   //|SecurityPermissionFlag.SkipVerification //required for .NET 3.5
   |SecurityPermissionFlag.Execution
   ;

  return new SecurityPermission(spflags);
 }//Helper__CreateExecutePermission

 //-----------------------------------------------------------------------
 private static OleDbPermission Helper__CreateOleDbPermission(string cn_str)
 {
  var oledbPermission=new OleDbPermission(PermissionState.None);

  if(!Object.ReferenceEquals(cn_str,null))
  {
   oledbPermission.Add(c_cn_str,
                       null,
                       KeyRestrictionBehavior.AllowOnly);
  }//if

  return oledbPermission;
 }//Helper__CreateOleDbPermission

 //-----------------------------------------------------------------------
 private static void Helper__Exec_Failed(TestObject testObject,
                                         string     cn_str)
 {
  try
  {
   Helper__Exec(testObject,cn_str);
  }
  catch(SecurityException e)
  {
   Helper__PrintSecurityException(e);

   return;
  }//catch

  throw new ApplicationException("We Wait The Exception!");
 }//Helper__Exec_Failed

 //-----------------------------------------------------------------------
 private static void Helper__Exec(TestObject testObject,
                                  string     cn_str)
 {
  Console.WriteLine("Try call testObject.Exec(\"{0}\")...",cn_str);

  testObject.Exec(cn_str);
 }//Helper__Exec

 //-----------------------------------------------------------------------
 private static void Helper__PrintSecurityException(SecurityException e)
 {
  Console.WriteLine("Trap SecurityException");
  Console.WriteLine("  PermissionType:");
  Console.WriteLine("    {0}",e.PermissionType);
  Console.WriteLine("  Source:");
  Console.WriteLine("    {0}", e.Source);
  Console.WriteLine("  Message:");
  Console.WriteLine("    {0}", e.Message);
 }//Helper__PrintSecurityException
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0013
