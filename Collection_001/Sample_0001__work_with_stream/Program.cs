////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    28.05.2013
using System;
using System.Data;
using lcpi.data.oledb;

namespace Sample_0001
{
 class Program
 {
  private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
   +"user id=gamer;"
   +"password=vermut;";

  //----------------------------------------------------------------------
  private static byte GenByte(int i)
  {
   return (byte)(i%256);
  }//GenByte

  //----------------------------------------------------------------------
  static int Main(string[] args)
  {
   int resultCode=0;

   try
   {
    const int c_bytes=8*1024*1024;//8MB

    var bytes=new byte[c_bytes];

    for(int i=0;i!=c_bytes;++i)
     bytes[i]=GenByte(i);

    //------
    var cn=new OleDbConnection(c_cn_str);

    cn.Open();

    var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

    var cmd=new OleDbCommand("",cn,tr);

    //INSERT binary data into binary blob through System.IO.MemoryStream object
    cmd.CommandText="insert into BIN_BLOB_TABLE (BIN_DATA) values (:x)\n"
                   +"returning TEST_ID\n"
                   +"into :id";

    //implicit generation of command parameters
    cmd["x"].Value=new System.IO.MemoryStream(bytes);

    cmd.ExecuteNonQuery();

    var record_id=cmd["id"].Value;

    Console.WriteLine("record_id: {0}",record_id);

    //-------
    cmd.CommandText="select BIN_DATA from BIN_BLOB_TABLE where test_id=:id";

    //provider removes old implicit descriptions of parameters and
    //obtains parameters from new sql
    cmd["id"].Value=record_id;

    //hint to provider: command with one result
    var reader=cmd.ExecuteReader(CommandBehavior.SingleResult);

    if(!reader.Read())
     throw new ApplicationException("record not found!");

    //access to BIN_DATA through stream object
    var blob_stream=reader.GetStream(0);

    //check blob_data
    for(int i=0;i!=c_bytes;++i)
    {
     int b=blob_stream.ReadByte();

     if(b!=GenByte(i))
      throw new ApplicationException("Wrong blob data!");
    }//for

    //check EOF
    if(blob_stream.ReadByte()!=-1)
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
}//Sample_0001
