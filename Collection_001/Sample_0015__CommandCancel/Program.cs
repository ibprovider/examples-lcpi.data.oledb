using System;
using System.Data;
using lcpi.data.oledb;

using System.Threading;
using System.Globalization;

namespace Sample_0015{
////////////////////////////////////////////////////////////////////////////////
//class ThreadWorker

class ThreadWorker
{
 private readonly OleDbCommand m_cmd;

 public Exception m_exc=null;

 //-----------------------------------------------------------------------
 public ThreadWorker(OleDbCommand cmd)
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
//class Program

class Program
{
 private const string c_cn_str
  ="provider=LCPI.IBProvider.3;"
  +"location=localhost:d:\\database\\ibp_test_fb25_d3.gdb;"
  +"user id=gamer;"
  +"password=vermut;"
  +"dbclient_library=fbclient.dll";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try
  {
   using(var cn=new OleDbConnection(c_cn_str))
   {
    cn.Open();

    using(var tr=cn.BeginTransaction(IsolationLevel.RepeatableRead))
    {
     using(var cmd=new OleDbCommand(null,cn,tr))
     {
      cmd.CommandText="execute procedure SP_EXEC_DUMMY_COUNTER(100000000)";

      var threadWorker=new ThreadWorker(cmd);

      Thread thread=new Thread(threadWorker.ExecuteNonQuery);

      try
      {
       thread.Start();

       while(thread.IsAlive)
       {
        Thread.Sleep(2000);

        Console.WriteLine("Cancel");

        cmd.Cancel();
       }//while

       Console.WriteLine("threadWorker was stopped");

       if(Object.ReferenceEquals(threadWorker.m_exc,null))
       {
        Console.WriteLine("No exception");
       }
       else
       {
        Console.WriteLine("Thread exception: {0}",
                          Helper__BuildExcText_InvariantCulture(threadWorker.m_exc));
       }//else
      }
      finally
      {
       thread.Join();
      }//finally
     }//using cmd
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

 //-----------------------------------------------------------------------
 //Get exception message with "invariant" UI-culture

 private static string Helper__BuildExcText_InvariantCulture(Exception e)
 {
  var sb=new System.Text.StringBuilder();

  var oldCulture=Thread.CurrentThread.CurrentUICulture;

  try
  {
   Thread.CurrentThread.CurrentUICulture=CultureInfo.InvariantCulture;

   sb.Append(e.Source).Append(" - ").Append(e.Message);
  }
  finally
  {
   if(!Object.ReferenceEquals(Thread.CurrentThread.CurrentUICulture,oldCulture))
    Thread.CurrentThread.CurrentUICulture=oldCulture;
  }//finally

  return sb.ToString();
 }//Helper__BuildExcText_InvariantCulture
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0015