using System;
using System.Data;
using lcpi.data.oledb;

using structure_lib=lcpi.lib.structure;

namespace Sample_0017{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\employee.fdb;"
   +"user id=gamer;"
   +"password=vermut;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try // [catch]
  {
   OleDbConnection cn1=null;
   OleDbConnection cn2=null;

   OleDbTransaction tr1=null;
   OleDbTransaction tr2=null;

   try // [finally]
   {
    cn1=new OleDbConnection(c_cn_str);

    cn1.Open();

    cn2=cn1.CloneSession();

    //--------------------------------------
    tr1=cn1.BeginTransaction(IsolationLevel.RepeatableRead);

    tr2=cn2.BeginTransaction(IsolationLevel.RepeatableRead);

    //--------------------------------------
    Helper__PrintTrInfo("tr1",tr1);
    Helper__PrintTrInfo("tr2",tr2);

    Console.WriteLine("");

    //--------------------------------------
    tr1.Commit();
    tr2.Commit();
   }
   finally
   {
    structure_lib.DisposeUtils.Exec(ref tr1);
    structure_lib.DisposeUtils.Exec(ref tr2);

    structure_lib.DisposeUtils.Exec(ref cn1);
    structure_lib.DisposeUtils.Exec(ref cn2);
   }//finally
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch

  return resultCode;
 }//Main

 //-----------------------------------------------------------------------
 private static void Helper__PrintTrInfo(string           sign,
                                         OleDbTransaction tr)
 {
  OleDbCommand    cmd=null;
  OleDbDataReader reader=null;

  try // [finally]
  {
   cmd=new OleDbCommand("select CURRENT_CONNECTION as c,\n"
                              +"CURRENT_TRANSACTION as t\n"
                        +"from RDB$DATABASE",
                        tr.Connection,
                        tr);

   reader=cmd.ExecuteReader();

   reader.Read();

   Console.WriteLine("{0}: cn_id: {1}, tr_id: {2}",
                     sign,
                     reader["c"],
                     reader["t"]);
  }
  finally
  {
   structure_lib.DisposeUtils.Exec(ref reader);
   structure_lib.DisposeUtils.Exec(ref cmd);
  }//finally
 }//Helper__PrintTrInfo
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0017
