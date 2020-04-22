////////////////////////////////////////////////////////////////////////////////
//author: Kovalenko Dmitry. IBProvider Team.
//date  : 2013-11-15
using System;
using System.Data;
using System.Data.Common;

using System.Threading;

namespace Sample_0016{
////////////////////////////////////////////////////////////////////////////////
//NOTE.
// Please, install all providers into GAC and register they in "machine.config".
//
////////////////////////////////////////////////////////////////////////////////
//class ThreadWorker

class ThreadWorker
{
 private readonly DbCommand m_cmd;

 public Exception m_exc=null;

 //-----------------------------------------------------------------------
 public ThreadWorker(DbCommand cmd)
 {
  m_cmd=cmd;
 }//ThreadWorker

 //-----------------------------------------------------------------------
 public void ExecuteNonQuery()
 {
  Console.WriteLine("Enter to ThreadWorker.ExecuteNonQuery");

  try
  {
   m_cmd.ExecuteNonQuery();
  }
  catch(Exception e)
  {
   Console.WriteLine("Catch exception in ThreadWorker.ExecuteNonQuery");

   m_exc=e;
  }//catch

  Console.WriteLine("Exit from ThreadWorker.ExecuteNonQuery");
 }//ExecuteNonQuery
};//class ThreadWorker

/////////////////////////////////////////////////////////////////////////////////
//class ConnectionData

class ConnectionData
{
 public string dbfactory;
 public string cn_str;
};//class ConnectionData

/////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 //-----------------------------------------------------------------------
 //LCPI .NET Provider for OleDb.

 private static readonly ConnectionData sm_cn_data__oledb
  =new ConnectionData
   {
    dbfactory="lcpi.data.oledb",
    cn_str="provider=LCPI.IBProvider.3;"
          +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
          +"user id=gamer;"
          +"password=vermut;"
          +"dbclient_library=fbclient.dll"
   };//sm_cn_data__oledb

 //-----------------------------------------------------------------------
 //Original .Net Provider for FireBird. Welcome to world of industrial programming.

 private static readonly ConnectionData sm_cn_data__fb_net_client
  =new ConnectionData
   {
    dbfactory="FirebirdSql.Data.FirebirdClient",
    cn_str="DataSource=localhost;"
          +"Database=d:\\database\\ibp_test_fb25_d3.gdb;"
          +"user=gamer;"
          +"password=vermut;"
   };//sm_cn_data__fb_net_client

 //-----------------------------------------------------------------------
 //change this data for switch between providers

 private static readonly ConnectionData sm_cn_data
  =sm_cn_data__oledb;

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try
  {
   var dbfactory=DbProviderFactories.GetFactory(sm_cn_data.dbfactory);

   using(var cn=dbfactory.CreateConnection())
   {
    {
     var cn_assembly_name=cn.GetType().Assembly.GetName();

     Console.WriteLine("Connection Assembly: {0}, {1}",
                        cn_assembly_name.Name,
                        cn_assembly_name.Version);
    }//local

    cn.ConnectionString=sm_cn_data.cn_str;
    cn.Open();

    using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
    {
     using(DbCommand cmd1=cn.CreateCommand())
     using(DbCommand cmd2=cn.CreateCommand())
     {
      cmd1.Transaction=tr;
      cmd1.CommandText="execute procedure SP_EXEC_DUMMY_COUNTER(100000000)";

      var threadWorker=new ThreadWorker(cmd1);

      cmd2.Transaction=tr;
      cmd2.CommandText="select * from RDB$DATABASE";

      cmd2.Prepare();

      Thread thread=new Thread(threadWorker.ExecuteNonQuery);

      try
      {
       thread.Start();

       while(thread.IsAlive)
       {
        Thread.Sleep(2000);

        Console.WriteLine("Cancel");

        cmd2.Cancel();
       }//while

       Console.WriteLine("threadWorker was stopped");

       if(Object.ReferenceEquals(threadWorker.m_exc,null))
       {
        Console.WriteLine("No exception");
       }
       else
       {
        Console.WriteLine("Thread exception: {0} - {1}",
                          threadWorker.m_exc.Source,
                          threadWorker.m_exc.Message);
       }//else
      }
      finally
      {
       thread.Join();
      }//finally
     }//using cmd1,cmd2
    }//using tr
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
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0016