using System;
using System.Data;
using System.Data.Common;
using lcpi.data.oledb;

using structure_lib=lcpi.lib.structure;

namespace Sample_0014{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str__fb
  ="provider=LCPI.IBProvider.3;"
  +"location=vxp-fb30:e:\\database\\ibp_test_fb30_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;"
  +"dbclient_library=fbclient_30.dll";

 //usage of different clients for 32bit (gds32) and 64bit (ibclient64)
 private const string c_cn_str__ib
  ="provider=LCPI.IBProvider.3;"
  +"location=vxpsp2-ib11-0-3:e:\\database\\ibp_test_ib11-0-3_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;"
  +"dbclient_library=d:\\users\\dima\\IB_FB_YA\\IB110\\gds32.dll;"
  +"dbclient_library_64=d:\\users\\dima\\IB_FB_YA\\IB110\\ibclient64.dll";

 //-----------------------------------------------------------------------
 //need change this constant for switch between servers
 private const string c_cn_str=c_cn_str__ib;

 //-----------------------------------------------------------------------
 const string c_TestTableName="OLEDB_NET_SMPL_0014";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=1;

  try
  {
   using(var cn=new OleDbConnection(c_cn_str))
   {
    cn.Open();

    Helper__CheckServerID(cn);

    Helper__CreateTestTable(cn);

    var values=new object[]{true,false,DBNull.Value,"false","true",1,0,null};

    using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
    using(var cmd=new OleDbCommand(null,cn,tr))
    {
     cmd.CommandText
      =string.Format("select GEN_ID(GEN_ID_{0},{1}) from RDB$DATABASE",
                     c_TestTableName,
                     values.Length);

     var baseID=(Int64)cmd.ExecuteScalar();

     //----------
     cmd.CommandText
      =string.Format("insert into {0} (ID,COL_BOOL) values(:id,:data)",
                     c_TestTableName);

     for(int i=0;i!=values.Length;++i)
     {
      var recID=baseID+i;
      var val=values[i];

      Console.WriteLine("insert: {0}, {1} [{2}]",
                         recID,
                         Helper__ToStr(val),
                         Helper__GetTypeName(val));

      cmd["id"].Value=baseID+i;
      cmd["data"].Value=values[i];

      cmd.ExecuteNonQuery();
     }//for i

     Console.WriteLine("---------");

     cmd.CommandText
      =string.Format("select ID,COL_BOOL from {0} where ID>={1} and ID<{2} order by ID",
                     c_TestTableName,baseID,baseID+values.Length);

     using(var reader=cmd.ExecuteReader(CommandBehavior.SingleResult))
     {
      while(reader.Read())
      {
       Console.WriteLine("select: {0}, {1} [{2}]",
                         reader.GetValue(0),
                         Helper__ToStr(reader.GetValue(1)),
                         Helper__GetTypeName(reader.GetValue(1)));
      }//while
     }//using reader

     tr.Commit();
    }//using cmd, tr
   }//using cn
  }
  catch(Exception e)
  {
   resultCode=1;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch

  return resultCode;
 }//Main

 //Helper methods --------------------------------------------------------
 private static void Helper__CheckServerID(OleDbConnection cn)
 {
  var info_table=cn.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);

  var info_row=info_table.Rows[0];

  var dbmsName
   =(string)info_row[DbMetaDataColumnNames.DataSourceProductName];

  var dbmsVer
   =new structure_lib.Version((string)info_row[DbMetaDataColumnNames.DataSourceProductVersion]);

  Console.WriteLine("DBMS: {0} {1}",dbmsName,dbmsVer);

  if(string.CompareOrdinal(dbmsName,"Firebird")==0)
  {
   if(dbmsVer.ComparePrefix(new structure_lib.Version("3.0"))<0)
   {
    var errMsg=string.Format("Not supported version of Firebird: {0}",dbmsVer.ToString());

    throw new ApplicationException(errMsg);
   }//if

   return; //ok
  }//if Firebird

  if(string.CompareOrdinal(dbmsName,"InterBase")==0)
  {
   if(dbmsVer.ComparePrefix(new structure_lib.Version("11.0"))<0)
   {
    var errMsg=string.Format("Not supported version of InterBase: {0}",dbmsVer.ToString());

    throw new ApplicationException(errMsg);
   }//if

   return; //ok
  }//if InterBase

  {
   var errMsg=string.Format("Unexpected name of DBMS: {0}",dbmsName);

   throw new ApplicationException(errMsg);
  }//local
 }//Helper__CheckServerID

 //-----------------------------------------------------------------------
 private static void Helper__CreateTestTable(OleDbConnection cn)
 {
  using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
  {
   var schema=cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                                     new object[]{null,null,c_TestTableName});
   if(schema.Rows.Count==1)
   {
    tr.Commit();

    Console.WriteLine("Test table already exists");

    return;
   }//if

   if(schema.Rows.Count>1)
   {
    tr.Commit();

    var errMsg
     =string.Format("Unexpected problem. Found {0} test tables.",schema.Rows.Count);

    throw new ApplicationException(errMsg);
   }//if

   Console.WriteLine("Create new test table");

   var sql=string.Format
    ("create table {0} (ID INTEGER NOT NULL primary key,COL_BOOL BOOLEAN);\n"
     +"create generator GEN_ID_{0};",
     c_TestTableName);

   using(var cmd=new OleDbCommand(sql,cn,tr))
   {
    cmd.ExecuteNonQuery();
   }//using cmd

   tr.Commit();
  }//using tr
 }//Helper__CreateTestTable

 //-----------------------------------------------------------------------
 private static string Helper__ToStr(object v)
 {
  if(Object.ReferenceEquals(v,null))
   return "#NULL";

  if(v==DBNull.Value)
   return "#DBNULL";

  return v.ToString();
 }//Helper__ToStr

 //-----------------------------------------------------------------------
 private static string Helper__GetTypeName(object v)
 {
  if(Object.ReferenceEquals(v,null))
   return "#NULL";

  return v.GetType().Name;
 }//Helper__GetTypeName
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0014
