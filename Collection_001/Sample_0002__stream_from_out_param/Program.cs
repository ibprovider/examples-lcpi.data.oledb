////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    28.05.2013
using System;
using System.Data;
using lcpi.data.oledb;

namespace Sample_0002
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
     bytes[i]=(byte)(i%256);

    //------
    var cn=new OleDbConnection(c_cn_str);

    cn.Open();

    var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

    var cmd=new OleDbCommand("",cn,tr);

    //INSERT binary data into binary blob through System.IO.MemoryStream object
    cmd.CommandText="insert into BIN_BLOB_TABLE (BIN_DATA) values (:in)\n"
                   +"returning TEST_ID, BIN_DATA\n"
                   +"into :id, :out";

    //implicit generation of command parameters
    cmd["in"].Value=new System.IO.MemoryStream(bytes);

    cmd["out"].OutputBinding.Set(OleDbType.IUnknown,typeof(System.IO.Stream));

    cmd.ExecuteNonQuery();

    Console.WriteLine("record_id: {0}",cmd["id"].Value);

    var blob_stream=(System.IO.Stream)cmd["out"].Value;

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
}//Sample_0002
