////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    28.05.2013
using System;
using System.Data;
using lcpi.data.oledb;

namespace Sample_0006
{
 class Program
 {
  private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
   +"user id=gamer;"
   +"password=vermut;";

  //----------------------------------------------------------------------
  private static char GenChar(int i)
  {
   return (char)('a'+(i%26));
  }//GenChar

  //----------------------------------------------------------------------
  static int Main(string[] args)
  {
   int resultCode=0;

   try
   {
    const string c_test_str=".NET Provider Sample #0006";

    var cn=new OleDbConnection(c_cn_str);

    cn.Open();

    var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

    var cmd=new OleDbCommand("",cn,tr);

    //--------------------------------------INSERT
    cmd.CommandText="insert into TBL_CS__ASCII (VARCHAR__32) values(:x)\n"
                   +"returning TEST_ID\n"
                   +"into :record_id";

    //define INPUT parameter
    cmd.Parameters.Add("x",OleDbType.VarWChar,0,ParameterDirection.Input).Value=c_test_str;

    //define OUTPUT parameter
    cmd.Parameters.Add("record_id",OleDbType.Variant,0,ParameterDirection.Output);

    var RowsAffected=cmd.ExecuteNonQuery();

    var rec_id=cmd.Parameters["record_id"].Value;

    Console.WriteLine("RowsAffected: {0}, TEST_ID={1}",
                      RowsAffected,
                      rec_id);

    //--------------------------------------SELECT
    cmd.CommandText="select VARCHAR__32 from TBL_CS__ASCII where TEST_ID=:record_id";

    //Remove previous set of parameters
    cmd.Parameters.Clear();

    cmd.Parameters.Add("record_id",OleDbType.Variant,0,ParameterDirection.Input).Value=rec_id;

    using(var reader=cmd.ExecuteReader())
    {
     if(!reader.Read())
      throw new ApplicationException("Test record not found!");

     if(reader.GetString(/*VARCHAR__32*/0)!=c_test_str)
      throw new ApplicationException("Select wrong column data!");

     Console.WriteLine("OK. We select the correct field value.");

     if(reader.Read())
      throw new ApplicationException("Select more that one row!");
    }//using reader

    //Commit transaction
    tr.Commit();

    //-----
    Console.WriteLine("All is OK.");
   }
   catch(Exception e)
   {
    resultCode=1;

    Console.WriteLine("");
    Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
   }//catch

   return resultCode;
  }//Main
 }//class Program
}//Sample_0006
