////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                    28.05.2013
using System;
using System.Diagnostics;
using System.Data;
using lcpi.data.oledb;

using com_lib=lcpi.lib.com;
using oledb_lib=lcpi.lib.oledb;

namespace Sample_0007{
////////////////////////////////////////////////////////////////////////////////
//class Generator

static class Generator
{
 public static byte exec(long i)
 {
  return (byte)(i%253);
 }//GenByte
};//class Generator

////////////////////////////////////////////////////////////////////////////////
//class VirtualComStream

unsafe class VirtualComStream:oledb_lib.native.ISequentialStream
{
 public VirtualComStream(long size)
 {
  Debug.Assert(size>=0);

  m_size=size;
  m_pos=0;
 }//VirtualComStream

 //ISequentialStream interface -------------------------------------------
 public com_lib.HResultCode Read(void*   pv,
                                 UInt32  cb,
                                 UInt32* pcbRead)
 {
  com_lib.ObjectUtils.ClearErrorInfo();

  if(pv==null && cb!=0)
   return com_lib.HResultCode.E_INVALIDARG;

  if(pcbRead!=null)
   (*pcbRead)=0;

  long pos=m_pos;

  byte* dest_beg=(byte*)pv;
  byte* dest_pos=dest_beg;
  byte* dest_end=dest_beg+cb;

  for(;dest_pos!=dest_end && pos<m_size;++dest_pos,++pos)
   (*dest_pos)=Generator.exec(pos);

  m_pos=pos;

  var writed=(dest_pos-dest_beg);

  Debug.Assert(writed<=UInt32.MaxValue);

  if(writed==0 && m_pos==m_size)
   return com_lib.HResultCode.S_FALSE;

  if(pcbRead!=null)
   lcpi.lib.structure.NumericCast.Exec(out (*pcbRead),writed); //no problem

  return com_lib.HResultCode.S_OK;
 }//Read

 //-----------------------------------------------------------------------
 public com_lib.HResultCode Write(void*   pv,
                                  UInt32  cb,
                                  UInt32* pcbWrited)
 {
  com_lib.ObjectUtils.ClearErrorInfo();

  if(pv==null && cb!=0)
   return com_lib.HResultCode.E_INVALIDARG;

  if(pcbWrited!=null)
   (*pcbWrited)=0;

  return com_lib.HResultCode.E_NOTIMPL;
 }//Write

 //private data ----------------------------------------------------------
 private readonly long m_size;

 private long m_pos;
};//class VirtualComStream

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
 static int Main(string[] args)
 {
  int resultCode=0;

  try
  {
   const long c_size=8L*1024L*1024L*1024L; //8GB

   //---------------------------------------
   var cn=new OleDbConnection(c_cn_str);

   cn.Open();

   var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead);

   var cmd=new OleDbCommand("",cn,tr);

   //--------------------------------------INSERT
   Console.WriteLine("Insert BLOB ... ");

   cmd.CommandText="insert into BIN_BLOB_TABLE (BIN_DATA) values(:x)\n"
                  +"returning TEST_ID\n"
                  +"into :rec_id";

   //define INPUT parameter
   cmd["x"].Value=new VirtualComStream(c_size);

   cmd.ExecuteNonQuery();

   var rec_id=cmd.Parameters["rec_id"].Value;

   Console.WriteLine("TEST_ID={0}",
                     rec_id);

   //--------------------------------------SELECT OCTET_LENGTH
   Console.Write("Query length of our BLOB ... ");

   try
   {
    cmd.CommandText="select OCTET_LENGTH (BIN_DATA) from BIN_BLOB_TABLE\n"
                   +"where test_id=:id";

    cmd["id"].Value=rec_id;

    var length=cmd.ExecuteScalar();

    if(c_size.Equals(length))
     Console.Write("OK. Length: {0}",length);
    else
     Console.Write("PROBLEM. Length: {0}",length);
   }
   catch
   {
    Console.Write("FAILED!");
    throw;
   }
   finally
   {
    Console.WriteLine("");
   }//finally

   //---------------------------------------Select data
   Console.WriteLine("Read and check our BLOB ... ");

   cmd.CommandText="select BIN_DATA from BIN_BLOB_TABLE where\n"
                  +"test_id=:id";

   cmd["id"].Value=rec_id;

   using(var reader=cmd.ExecuteReader(CommandBehavior.SingleResult))
   {
    if(!reader.Read())
     throw new ApplicationException("test record not found!");

    using(var blob_stream=reader.GetStream(0))
    {
     const int c_block_sz=1024*1024+1;

     var buffer=new byte[c_block_sz];

     long total_sz=0;

     for(;;)
     {
      var sz=blob_stream.Read(buffer,0,c_block_sz);

      if(sz==0)
       break;

      int i=0;

      for(;i<sz && total_sz<c_size;++i,++total_sz)
      {
       if(buffer[i]!=Generator.exec(total_sz))
        throw new ApplicationException("Bad BLOB data!");
      }//for

      if(i!=sz)
       throw new ApplicationException("Bad BLOB data!");
     }//for[ever]

     if(total_sz!=c_size)
      throw new ApplicationException("Bad BLOB data!");
    }//using blob_stream

    if(reader.Read())
     throw new ApplicationException("More that one test row!");
   }//using reader

   //---------------------------------------
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

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0007
