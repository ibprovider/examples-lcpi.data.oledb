////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   17.11.2014.
using System;
using System.Data;
using lcpi.data.oledb;

using structure_lib=lcpi.lib.structure;

namespace Sample_0018{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="provider=LCPI.IBProvider.3;"
   +"location=localhost:d:\\database\\employee.fdb;"
   +"user id=SYSDBA;"
   +"password=masterkey;"
   +"auto_commit=true;";

 //-----------------------------------------------------------------------
#if IGNORE_NEXT_SECTION
    set transaction;
    
    create generator GEN_ID__TEST_TABLE_018;
    
    create table TEST_TABLE_018
     (ID INTEGER NOT NULL PRIMARY KEY,
      DATA VARCHAR(64) CHARACTER SET UTF8);
    
    create trigger BI_TEST_TABLE_018 FOR TEST_TABLE_018
    before insert position 0
    as
    begin
     -- generate identifier
     if(new.ID is null) then new.ID=GEN_ID(GEN_ID__TEST_TABLE_018,1);
     /*make UPPER data*/
     new.DATA=UPPER(NEW.DATA);
    end;
    
    commit;
    
    -- insert test record
    insert into TEST_TABLE_018 (DATA) VALUES (:b) returning ID,DATA into :a,:b;
    
    set transaction;
    
    drop table TEST_TABLE_018;
    
    drop generator GEN_ID__TEST_TABLE_018;
    
    commit;
#endif
 
 private const string c_script_with_ddl
  ="set transaction;\n\n"
  +"create generator GEN_ID__TEST_TABLE_018;\n\n"
  +"create table TEST_TABLE_018\n"
  +" (ID INTEGER NOT NULL PRIMARY KEY,\n"
  +"  DATA VARCHAR(64) CHARACTER SET UTF8);\n\n"
  +"create trigger BI_TEST_TABLE_018 FOR TEST_TABLE_018\n"
  +"before insert position 0\n"
  +"as\n"
  +"begin\n"
  +" -- generate identifier\n"
  +" if(new.ID is null) then new.ID=GEN_ID(GEN_ID__TEST_TABLE_018,1);\n"
  +" /*make UPPER data*/\n"
  +" new.DATA=UPPER(NEW.DATA);\n"
  +"end;\n\n"
  +"commit;\n\n"
  +"-- insert test record\n"
  +"insert into TEST_TABLE_018 (DATA) VALUES (:b) returning ID,DATA into :a,:b;\n\n"
  +"set transaction;\n\n"
  +"drop table TEST_TABLE_018;\n\n"
  +"drop generator GEN_ID__TEST_TABLE_018;\n\n"
  +"commit;";
 
 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try // [catch]
  {
   OleDbConnection  cn=null;
   OleDbCommand     cmd=null;

   try // [finally]
   {
    Console.WriteLine("Connect...");
    cn=new OleDbConnection(c_cn_str);

    cn.Open();

    cmd=new OleDbCommand(c_script_with_ddl,cn);

    cmd.Parameters.Add
     ("a",
      OleDbType.Variant,
      0,
      ParameterDirection.Output);
    
    cmd.Parameters.Add
     ("b",
      OleDbType.Variant,
      0,
      ParameterDirection.InputOutput).Value="(russian text!) русский текст!";

    Console.WriteLine("ExecuteNonQuery...");
    cmd.ExecuteNonQuery();

    Console.WriteLine("a=[{0}]",cmd["a"].Value);
    Console.WriteLine("b=[{0}]",cmd["b"].Value);

    Console.WriteLine("OK!");
   }
   finally
   {
    structure_lib.DisposeUtils.Exec(ref cmd);
    structure_lib.DisposeUtils.Exec(ref cn);
   }//finally
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch

  return resultCode;
 }//Main
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0018
