////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    06.06.2013
using System;
using System.Diagnostics;
using System.Data;
using lcpi.data.oledb;

namespace Sample_0009{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
  ="provider=LCPI.IBProvider.3;"
  +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;";

 //----------------------------------------------------------------------
 private static byte GenByte(long i)
 {
  return (byte)(i%253);
 }//GenByte

 //----------------------------------------------------------------------
 static int Main(string[] args)
 {
  int resultCode=0;

  try
  {
   const int c_bytes_count=8*1024*1024+1;//8MB+1

   const int c_block_size=64*1024; //64K

   //--------------------------------------- connect to database
   Console.WriteLine("Connect to database ...");

   var cn=new OleDbConnection(c_cn_str);

   cn.Open();

   //-----
   Console.WriteLine("Start transaction ...");

   var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

   //--------------------------------------- insert test bytes to binary blob
   Console.WriteLine("Create and prepare insert command ...");

   var cmd=new OleDbCommand("",cn,tr);

   cmd.CommandText="insert into TBL_CS__OCTETS (COL_BLOB) values(:blob)\n"
                  +"returning TEST_ID\n"
                  +"into :id";

   {
    var test_bytes=new byte[c_bytes_count];

    for(int i=0;i!=c_bytes_count;++i)
     test_bytes[i]=GenByte(i);

    cmd["blob"].Value=test_bytes;
   }//local

   //----
   Console.WriteLine("Insert test record ...");

   cmd.ExecuteNonQuery();

   var rec_id=cmd["id"].Value;

   Console.WriteLine("Test record ID: {0}",rec_id);

   //--------------------------------------- select test record
   Console.WriteLine("Prepare select command ...");

   cmd.CommandText="select COL_BLOB from TBL_CS__OCTETS where TEST_ID=:x";

   cmd["x"].Value=rec_id;

   //----
   Console.WriteLine("Select test record ...");

   var reader=cmd.ExecuteReader();

   if(!reader.Read())
    throw new ApplicationException("Test record not found!");

   if(reader.IsDBNull(0))
    throw new ApplicationException("Select NULL value!");

   //---- detect blob length
   long column_data_length=reader.GetBytes(/*COL_BLOB*/0,0,null,0,0);

   Console.WriteLine("BLOB length: {0}",column_data_length);

   if(column_data_length!=c_bytes_count)
   {
    ++resultCode;

    Console.WriteLine("ERROR: Get wrong blob length: {0}. Expected: {1}.",
                      column_data_length,
                      c_bytes_count);
   }//if

   //---- read blob data
   Console.WriteLine("Check BLOB data ...");

   var buf=new byte[c_block_size];

   long dataOffset=0;

   long nBlocks=0;

   for(;;)
   {
    long l=reader.GetBytes(/*COL_BLOB*/0,dataOffset,buf,0,buf.Length);

    Debug.Assert(l>=0);
    Debug.Assert(l<=buf.Length);

    if(l==0)
     break;  //no more data

    ++nBlocks;

    for(int i=0;i!=l;++i,++dataOffset)
    {
     if(dataOffset==c_bytes_count)
      throw new ApplicationException("Wrong blob data [check point #1]");

     if(buf[i]!=GenByte(dataOffset))
      throw new ApplicationException("Wrong blob data [check point #2]");
    }//for i, dataOffset
   }//for[ever]

   Debug.Assert(dataOffset<=c_bytes_count);

   if(dataOffset!=c_bytes_count)
    throw new ApplicationException("Wrong blob data [check point #3]");

   Console.WriteLine("OK. Block count: {0}",nBlocks);

   //---------------------------------------
   Console.WriteLine("Commit transaction ...");

   tr.Commit();
  }
  catch(Exception e)
  {
   ++resultCode;

   Console.WriteLine("");
   Console.WriteLine("ERROR: {0} - {1}",e.Source,e.Message);
  }//catch

  return resultCode;
 }//Main
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0009