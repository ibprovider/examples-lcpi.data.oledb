////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   10.01.2015.
using System;
using System.Data;
using System.Diagnostics;
using lcpi.data.oledb;

using structure_lib=lcpi.lib.structure;

namespace Sample_0019{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
   +"user id=gamer;"
   +"password=vermut;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try //[catch]
  {
   Helper__Work();
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch

  return resultCode;
 }//Main

 //-----------------------------------------------------------------------
 private static void Helper__Work()
 {
  OleDbConnection  cn=null;
  OleDbTransaction tr=null;
  OleDbCommand     cmd=null;
  OleDbCommand     cmd_s=null;
  OleDbDataReader  reader=null;

  try // [finally with dispose]
  {
   cn=new OleDbConnection(c_cn_str);

   cn.Open();

   tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

   cmd=new OleDbCommand(null,cn,tr);
   cmd_s=new OleDbCommand(null,cn,tr);

   //-------------------
   if(cn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables,
                             new object[]{null,null,"NET_SMPL_0019"}).Rows.Count==0)
   {
    Console.WriteLine("create test table ...");

    cmd.CommandText
     ="create generator GEN_ID_NET_SMPL_0019;\n"
     +"create table NET_SMPL_0019(\n"
     +" ID INTEGER NOT NULL PRIMARY KEY,\n"
     +" ARR VARCHAR(128) [0:2,1:2]);\n"
     +"create trigger BI_NET_SMPL_0019 for NET_SMPL_0019\n"
     +" before insert\n"
     +" as\n"
     +" begin if(new.ID is NULL) then new.ID=GEN_ID(GEN_ID_NET_SMPL_0019,1); end;";

    cmd.ExecuteNonQuery();

    tr.CommitRetaining();
   }//if

   //-------------------
   Console.WriteLine("insert new array...");

   {
    var arr=Array.CreateInstance(typeof(string),
                                 /*lengths*/new int[]{3,2},
                                 /*lowerBounds*/new int[]{0,1});


    arr.SetValue("firebird"  ,0,1);
    arr.SetValue("2.5.3 SU1" ,0,2);

    arr.SetValue("interbase" ,1,1);
    arr.SetValue("XE7"       ,1,2);

    arr.SetValue("yaffil"    ,2,1);
    arr.SetValue("He flew away, but promised to return",2,2);

    cmd.CommandText
     ="insert into NET_SMPL_0019 (ARR) values(:arr) returning ID into :id;";

    cmd["arr"].Value=arr;

    cmd.ExecuteNonQuery();
   }//local

   var recID=cmd["id"].Value;

   Console.WriteLine("recID: {0}",recID);

   //-------------------
   Console.WriteLine("");
   Console.WriteLine("select array [ExecuteScalar] ...");

   cmd_s.CommandText="select ARR from NET_SMPL_0019 where ID=:id";

   cmd_s["id"].Value=recID;

   var Arr2=(Array)cmd_s.ExecuteScalar();

   Helper__PrintArr(Arr2);

   //-------------------
   Console.WriteLine("");
   Console.WriteLine("update array ...");

   Arr2.SetValue("FIREBIRD"  ,0,1);
   Arr2.SetValue("INTERBASE" ,1,1);
   Arr2.SetValue("YAFFIL"    ,2,1);

   cmd.CommandText="update NET_SMPL_0019 set ARR=:arr2 where ID=:recID";

   cmd["arr2"].Value=Arr2;
   cmd["recID"].Value=recID;

   var rowsAffected=cmd.ExecuteNonQuery();

   Console.WriteLine("rowsAffected: {0}",rowsAffected);

   //-------------------
   Console.WriteLine("");
   Console.WriteLine("select updated array [ExecuteReader]...");

   reader=cmd_s.ExecuteReader(CommandBehavior.SingleResult);

   if(!reader.Read())
    throw new ApplicationException("Test record not found!");

   Helper__PrintArr(reader.GetArray(0 /*ARR*/));

   reader.Close();

   //-------------------
   Console.WriteLine("");

   tr.Commit();
  }
  finally
  {
   structure_lib.DisposeUtils.Exec(ref reader);
   structure_lib.DisposeUtils.Exec(ref cmd);
   structure_lib.DisposeUtils.Exec(ref cmd_s);
   structure_lib.DisposeUtils.Exec(ref tr);
   structure_lib.DisposeUtils.Exec(ref cn);
  }//finally
 }//Helper__Work

 //-----------------------------------------------------------------------
 private static void Helper__PrintArr(Array Arr)
 {
  Debug.Assert(!Object.ReferenceEquals(Arr,null));

  Helper__PrintArrElem(Arr,0,1);
  Helper__PrintArrElem(Arr,0,2);
  Helper__PrintArrElem(Arr,1,1);
  Helper__PrintArrElem(Arr,1,2);
  Helper__PrintArrElem(Arr,2,1);
  Helper__PrintArrElem(Arr,2,2);
 }//Helper__PrintArr

 //-----------------------------------------------------------------------
 private static void Helper__PrintArrElem(Array Arr,int i1,int i2)
 {
  Debug.Assert(!Object.ReferenceEquals(Arr,null));

  Console.WriteLine("[{0},{1}]=\"{2}\"",i1,i2,Arr.GetValue(i1,i2));
 }//Helper__PrintArrElem
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//nms Sample_0019
