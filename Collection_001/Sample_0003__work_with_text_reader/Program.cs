////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    28.05.2013
using System;
using System.Data;
using lcpi.data.oledb;

namespace Sample_0003
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
    const int c_chars=8*1024*1024;//8MB

    var chars=new System.Text.StringBuilder(c_chars);

    for(int i=0;i!=c_chars;++i)
     chars.Append(GenChar(i));

    //------
    var cn=new OleDbConnection(c_cn_str);

    cn.Open();

    var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

    var cmd=new OleDbCommand("",cn,tr);

    //INSERT text data into text blob through System.IO.StringReader object
    cmd.CommandText="insert into TBL_CS__ASCII (COL_BLOB) values (:x)\n"
                   +"returning TEST_ID\n"
                   +"into :id";

    //implicit generation of command parameters
    cmd["x"].Value=new System.IO.StringReader(chars.ToString());

    cmd.ExecuteNonQuery();

    var record_id=cmd["id"].Value;

    Console.WriteLine("record_id: {0}",record_id);

    //-------
    cmd.CommandText="select COL_BLOB from TBL_CS__ASCII where test_id=:id";

    //provider removes old implicit descriptions of parameters and
    //obtains parameters from new sql
    cmd["id"].Value=record_id;

    //hint to provider: command with one result
    var reader=cmd.ExecuteReader(CommandBehavior.SingleResult);

    if(!reader.Read())
     throw new ApplicationException("record not found!");

    //access to BIN_DATA through stream object
    var blob_stream=reader.GetTextReader(0);

    //check blob_data
    for(int i=0;i!=c_chars;++i)
    {
     int c=blob_stream.Read();

     if(c!=GenChar(i))
      throw new ApplicationException("Wrong blob data!");
    }//for

    //check EOF
    if(blob_stream.Read()!=-1)
     throw new ApplicationException("Wrong blob data!");

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
}//Sample_0003
