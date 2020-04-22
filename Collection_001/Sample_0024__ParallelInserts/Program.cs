////////////////////////////////////////////////////////////////////////////////
//LCPI ADO.NET Provider Sample. Parallel insertion to database.
//                                                                 17.03.2017.
using System;
using System.IO;
using System.Threading;

using structure_lib=lcpi.lib.structure;
using xdb=lcpi.data.oledb;

namespace Sample_0024{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 public const string c_prog_arg__cn_str       = "cn_str";
 public const string c_prog_arg__thread_count = "thread_count";
 public const string c_prog_arg__rec_num      = "rec_num";
 public const string c_prog_arg__log_file     = "log_file";
 public const string c_prog_arg__summary_file = "summary_file";

 //-----------------------------------------------------------------------
 private static int Main()
 {
  int result = 0;

  try
  {
   result=Exec();
  }
  catch(Exception e)
  {
   ++result;
   Console.WriteLine("ERROR: {0}",e.Message);
  }//catch

  return result;
 }//Main

 //-----------------------------------------------------------------------
 private static int Exec()
 {
  int errCount=0;

  var cmdArgs = new structure_lib.CommandLineParser('/');

  cmdArgs.RegArg
   (c_prog_arg__cn_str,
    structure_lib.CommandLineArgumentFlags.WithValue);

  cmdArgs.RegArg
   (c_prog_arg__thread_count,
    structure_lib.CommandLineArgumentFlags.WithValue);

  cmdArgs.RegArg
   (c_prog_arg__rec_num,
    structure_lib.CommandLineArgumentFlags.WithValue);

  cmdArgs.RegArg
   (c_prog_arg__log_file,
    structure_lib.CommandLineArgumentFlags.WithValue);

  cmdArgs.RegArg
   (c_prog_arg__summary_file,
    structure_lib.CommandLineArgumentFlags.WithValue);

  //-----------------------------------
  cmdArgs.Parse(Environment.CommandLine);

  //-----------------------------------
  string Arg_log_file
   =cmdArgs[c_prog_arg__log_file].Value;

  var log=new ProgramLog(Arg_log_file);
 
  try
  {
   log.Send("Hello from test!");

   //-----------------------------------
   var startupData=new StartupData(cmdArgs);

   log.Send("CommandLine: {0}",Environment.CommandLine);
   log.Send("Threads    : {0}",startupData.m_arg__thread_count);
   log.Send("Records    : {0}",startupData.m_arg__rec_num);

   //-----------------------------------
   log.Send("Try to connect to database ...");

   using(var cn=new xdb.OleDbConnection(startupData.m_arg__cn_str))
   {
    cn.Open();

    using(var tr=cn.BeginTransaction())
    {
     var cmd=new xdb.OleDbCommand("delete from TEST_SEQUENTIAL_MOVE",cn,tr);

     cmd.ExecuteNonQuery();
  
     tr.Commit();
    }//using tr

    log.Send("OK!");
   }//using cn

   //---------------------------------- create workers
   log.Send("Create workers ...");

   var workers=new ThreadWorker[startupData.m_arg__thread_count];
  
   {
    var generator=new GeneratorSeq(startupData);

    for(uint i=0;i!=startupData.m_arg__thread_count;++i)
     workers[i]=new ThreadWorker(startupData,i,generator);
   }//local

   //----------------------------------
   log.Send("Create threads ...");

   var threads=new Thread[startupData.m_arg__thread_count];
   
   DateTime startTS;

   try
   {
    for(uint i=0;i!=startupData.m_arg__thread_count;++i)
    {
     var t=new Thread(workers[i].Run); //throw

     t.Start();

     workers[i].m_InitEvent.WaitOne(); //throw?

     threads[i]=t;
    }//for i

    startTS=DateTime.Now;
   }
   finally
   {
    //Begin thread executions
    startupData.m_StartEvent.Set(); //throw?

    //Wait of completion
    for(uint i=0;i!=startupData.m_arg__thread_count;++i)
    {
     if(Object.ReferenceEquals(threads[i],null))
      continue;

     log.Send("Wait thread {0}",i+1);

     threads[i].Join();

     if(Object.ReferenceEquals(workers[i].m_exc,null))
      continue;

     ++errCount;

     log.Send("ERROR: {0} - {1}",
              workers[i].m_exc.Source,
              workers[i].m_exc.Message);
    }//for i
   }//finally

   var endTS=DateTime.Now;

   long duration=(long)(endTS-startTS).Duration().TotalSeconds;

   log.Send("Duration: {0} second(s).",duration);

   //----------------------------------
   {
    ulong n=0;

    for(uint i=0;i!=startupData.m_arg__thread_count;++i)
    {
     n+=workers[i].m_cRecords;

     log.Send("Thread [{0}]: {1:###} record(s)",i,workers[i].m_cRecords);
    }//for
   
    if(n!=(ulong)startupData.m_arg__rec_num)
    {
     throw new ApplicationException
                (string.Format("Wrong work. Inserted {0}. Expected {1}",
                               n,
                               startupData.m_arg__rec_num));
    }//if
   }//local
    
   //----------------------------------
   log.Send("Check database state ...");

   using(var cn=new xdb.OleDbConnection(startupData.m_arg__cn_str))
   {
    cn.Open();

    using(var tr=cn.BeginTransaction())
    {
     var cmd=new xdb.OleDbCommand("select count(*) from TEST_SEQUENTIAL_MOVE",cn,tr);

     var n=cmd.ExecuteScalar();
  
     if(System.DBNull.Value.Equals(n))
      throw new ApplicationException("count is NULL!");

     if((long)n!=startupData.m_arg__rec_num)
     {
      throw new ApplicationException
                 (string.Format("Wrong database state. Inserted {0}. Expected {1}",
                                n,
                                startupData.m_arg__rec_num));
     }//if

     tr.Commit();
    }//using tr

    log.Send("OK!");
   }//using cn

   //----------------------------------
   using(var sf=new StreamWriter(cmdArgs[c_prog_arg__summary_file].Value,true))
   {
    sf.WriteLine("{0}\t{1}\t{2}",
                 startupData.m_arg__rec_num,
                 startupData.m_arg__thread_count,
                 duration);

    sf.Close();
   }
  }
  finally
  {
   log.Close();
  }//finally

  return errCount;
 }//Exec
}//class Program

////////////////////////////////////////////////////////////////////////////////
//class ProgramLog

class ProgramLog
{
 public ProgramLog(string logFileName)
 {
  m_log_file=new StreamWriter(logFileName,false);
 }

 public void Send(string s,params object[] args)
 {
  var ts=DateTime.Now;

  var tss=ts.ToString("[dd.mm.yyyy HH:mm:ss]");

  string line=string.Format(s,args);

  line=tss+' '+line;

  Console.WriteLine(line);

  m_log_file.WriteLine(line);
 }//Send

 public void Close()
 {
  m_log_file.Close();
 }//Close

 //-----------------------------------------------------------------------
 private StreamWriter m_log_file;
};//class ProgramLog

////////////////////////////////////////////////////////////////////////////////
//class StartupData

class StartupData
{
 public readonly string m_arg__cn_str;
 public readonly uint   m_arg__thread_count;
 public readonly long   m_arg__rec_num;

 public readonly ManualResetEvent m_StartEvent;

 public StartupData(structure_lib.CommandLineParser cmdArgs)
 {
  m_arg__cn_str
   =cmdArgs[Program.c_prog_arg__cn_str].Value;

  //-----------------------------------
  m_arg__thread_count
   =uint.Parse(cmdArgs[Program.c_prog_arg__thread_count].Value);

  if(m_arg__thread_count==0)
   throw new ApplicationException("Incorrect number of threads");

  //-----------------------------------
  m_arg__rec_num
   =long.Parse(cmdArgs[Program.c_prog_arg__rec_num].Value);
 
  if(m_arg__rec_num<=m_arg__thread_count)
   throw new ApplicationException("Incorrect number of records");

  //-----------------------------------
  m_StartEvent=new ManualResetEvent(false);
 }//StartupData

 //------------------------------------
 public bool GenNum(out long v)
 {
  v=Interlocked.Increment(ref m_rec_num);

  if(m_arg__rec_num<v)
   return false;

  return true;
 }//GenNum
   
 //------------------------------------
 private long m_rec_num=0;
};//class StartupData

////////////////////////////////////////////////////////////////////////////////
//interface Generator

interface Generator
{
 bool Gen(out long v);
};//interface Generator

 ////////////////////////////////////////////////////////////////////////////////
//class GeneratorSeq

class GeneratorSeq:Generator
{
 public GeneratorSeq(StartupData startupData)
 {
  m_StartupData=startupData;
 }

 //interface -------------------------------------------------------------
 public bool Gen(out long v)
 {
  return m_StartupData.GenNum(out v);
 }//Gen

 private readonly StartupData m_StartupData;
};//class GeneratorSeq

////////////////////////////////////////////////////////////////////////////////
//class ThreadWorker

class ThreadWorker
{
 public Exception m_exc=null;

 public readonly StartupData m_StartupData;

 public readonly Generator m_Generator;

 public readonly ManualResetEvent m_InitEvent;

 public readonly uint m_ThreadIndex;

 public ulong m_cRecords=0;

 public ThreadWorker(StartupData startupData,
                     uint        index,
                     Generator   generator)
 {
  m_StartupData=startupData;

  m_Generator=generator;

  m_InitEvent=new ManualResetEvent(false);

  m_ThreadIndex=index;
 }//ThreadWorker

 public void Run()
 {
  try
  {
   using(var cn=new xdb.OleDbConnection(m_StartupData.m_arg__cn_str)) //throw
   {
    cn.Open(); //throw

    using(var tr=cn.BeginTransaction()) //throw
    {
     using(var cmd=new xdb.OleDbCommand(null,cn,tr)) //throw
     {
      cmd.CommandText=
       "insert into TEST_SEQUENTIAL_MOVE (ID) values(:id)";

      cmd.Prepare(); //throw

      m_InitEvent.Set(); //throw

      m_StartupData.m_StartEvent.WaitOne(); //infinite!

      long v;
  
      while(m_Generator.Gen(out v))
      {
       cmd[0].Value=v;

       cmd.ExecuteNonQuery(); //throw

       ++m_cRecords;
      }//while
     }//using cmd

     tr.Commit(); //throw
    }//using tr
   }//using cn
  }
  catch(Exception e)
  {
   m_exc=e;
  }//catch
 }//Run
};//class ThreadWorker

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0024
