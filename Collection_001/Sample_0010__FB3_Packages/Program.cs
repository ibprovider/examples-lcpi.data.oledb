////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    14.10.2013
using System;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using lcpi.data.oledb;

namespace Sample_0010{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 //----------------------------------------- connection string
 private const string c_cn_str
  ="provider=LCPI.IBProvider.3;"
  +"location=vxp-fb30:e:\\database\\ibp_test_fb30_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;"
  +"exec_sp_named_param=true;" //use named parameters in "exec SP"
  +"dbclient_library=fbclient_30.dll";

 //----------------------------------------- DDL for test package
 private const string c_sql_package_ddl
  ="recreate package MATH as\n"
  +"begin\n"
  +" procedure sp_add(a integer,b integer) returns (r integer);\n"
  +" procedure sp_sub(a integer,b integer) returns (r integer);\n"
  +"end;\n"
  +"\n"
  +"recreate package body MATH as\n"
  +"begin\n"
  +" procedure sp_add(a integer,b integer) returns (r integer) as\n"
  +" begin\n"
  +"  r=a+b;\n"
  +" end\n"
  +" procedure sp_sub(a integer,b integer) returns (r integer) as\n"
  +" begin\n"
  +"  r=a-b;\n"
  +" end\n"
  +"end;";

 //----------------------------------------------------------------------
 static int Main(string[] args)
 {
  int resultCode=0;

  try
  {
   using(var cn=new OleDbConnection(c_cn_str))
   {
    Console.WriteLine("Connect to database ...");
    cn.Open();

    {
     //check server name and version
     var ds_info=cn.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);

     var dbms_name=ds_info.Rows[0][DbMetaDataColumnNames.DataSourceProductName];
     var dbms_verN=ds_info.Rows[0][DbMetaDataColumnNames.DataSourceProductVersionNormalized];

     Console.WriteLine("DBMS Name    : {0}",Helper_DBValueToStr(dbms_name));
     Console.WriteLine("DBMS Version : {0}",Helper_DBValueToStr(dbms_verN));

     if(!(dbms_name is string))
      throw new ApplicationException("DBMS name is not avaliable");

     if(((string)dbms_name)!="Firebird")
      throw new ApplicationException("Not supported DBMS");

     if(!(dbms_verN is string))
      throw new ApplicationException("Normalized version of DBMS is not available");

     //check the DBMS version format [I am RegExp Hero! :)]
     if(!Regex.IsMatch((string)dbms_verN,@"^([0-9]{2}\.){3}[0-9]{6}$"))
      throw new ApplicationException("Wrong DBMS version format");

     if(System.StringComparer.Ordinal.Compare(((string)dbms_verN),"03")<0)
      throw new ApplicationException("Not supported DBMS version");
    }//local

    //-------------------------------------- RECREATE PACKAGE
    Console.WriteLine("");
    Console.WriteLine("Start transaction ...");
    using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
    {
     using(var cmd=new OleDbCommand(c_sql_package_ddl,cn,tr))
     {
      Console.WriteLine("Recreate test package ...");

      cmd.ExecuteNonQuery();
     }//using cmd

     Console.WriteLine("Commit...");
     tr.Commit();
    }//using tr

    //--------------------------------------- WORK
    Console.WriteLine("");
    Console.WriteLine("Start transaction ...");
    using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
    {
     using(var cmd=new OleDbCommand("",cn,tr))
     {
      //------------------------------------ ADD
      cmd.CommandText="exec MATH.SP_ADD";

      cmd["a"].Value=2;
      cmd["b"].Value=3;

      cmd.ExecuteNonQuery();

      Console.WriteLine("{0}+{1}={2}",
                        Helper_DBValueToStr(cmd["a"].Value),
                        Helper_DBValueToStr(cmd["b"].Value),
                        Helper_DBValueToStr(cmd["r"].Value));

      //------------------------------------ SUB
      cmd.CommandText="exec MATH.SP_SUB";

      cmd["a"].Value=2;
      cmd["b"].Value=3;

      cmd.ExecuteNonQuery();

      Console.WriteLine("{0}-{1}={2}",
                        Helper_DBValueToStr(cmd["a"].Value),
                        Helper_DBValueToStr(cmd["b"].Value),
                        Helper_DBValueToStr(cmd["r"].Value));
     }//using cmd

     Console.WriteLine("Commit...");
     tr.Commit();
    }//using tr
   }//using cn
  }
  catch(Exception e)
  {
   ++resultCode;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch

  return resultCode;
 }//Main

  //Helper methods -------------------------------------------------------
 private static string Helper_DBValueToStr(object v)
 {
  if(Object.ReferenceEquals(v,null))
   return "#NULL";

  if(DBNull.Value.Equals(v))
   return "#DBNULL";

  return v.ToString();
 }//Helper_DBValueToStr
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0010