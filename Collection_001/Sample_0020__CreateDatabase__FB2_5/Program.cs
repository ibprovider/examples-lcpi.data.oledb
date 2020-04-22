////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                                   03.03.2015.
using System;
using System.Data;
using System.Diagnostics;
using lcpi.data.oledb;

namespace Sample_0020{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 static int Main()
 {
  int resultCode=0;

  try //[catch]
  {
   var cnsb=new OleDbConnectionStringBuilder();

   cnsb.Provider="LCPI.IBProvider.3";

   cnsb.Location="localhost:d:\\database\\TEST_DB__SMPL_20_FB2_5.gdb";

   cnsb["user id"]="SYSDBA";

   cnsb["password"]="masterkey";

   cnsb["ctype"]="win1251";

   cnsb["auto_commit"]=true;

   //--------
   cnsb["IBP_NEWDB: Database Page Size"]=8192;

   cnsb["IBP_NEWDB: Database Dialect"]=3;
   
   cnsb["IBP_NEWDB: Default Charset"]="win1251";

   cnsb["IBP_NEWDB: Default Charset Collation"]="pxw_cyrl";

   //--------
   var cn=new OleDbConnection(cnsb.ToString());

   //--------
   Console.WriteLine("Create database ...");

   cn.CreateDatabase();

   //--------
   using(var cmd=new OleDbCommand("select RDB$CHARACTER_SET_NAME\n"
                                 +"from RDB$DATABASE",
                                  cn))
   {
    Console.WriteLine("\nDB.def_charset: \"{0}\"",cmd.ExecuteScalar());
   }
   
   //--------
   using(var cmd=new OleDbCommand("select RDB$DEFAULT_COLLATE_NAME\n"
                                 +"from RDB$CHARACTER_SETS\n"
                                 +"where RDB$CHARACTER_SET_NAME='WIN1251'",
                                  cn))
   {
    Console.WriteLine("\nWIN1251.def_collation: \"{0}\"",cmd.ExecuteScalar());
   }
   
   //--------
   Console.WriteLine("\nCreate DUAL table ...");

   using(var cmd=new OleDbCommand
              ("set transaction;\n"
               +"create TABLE DUAL (ID INTEGER NOT NULL PRIMARY KEY);\n"
               +"commit retain;\n"
               +"insert into DUAL VALUES(0);\n"
               +"commit retain;\n"
               +"create exception E_DUAL '********* DON''T TOUCH THAT TABLE! ************';"
               +"create trigger BIUD_DUAL for DUAL BEFORE INSERT OR UPDATE OR DELETE AS BEGIN EXCEPTION E_DUAL; END;\n"
               +"commit;",
               cn))
   {
    cmd.ExecuteNonQuery();
   }

   //--------
   Console.WriteLine("\nSelect from DUAL table ...");

   using(var cmd=new OleDbCommand("select ID from DUAL",cn))
   {
    Console.WriteLine("\nDUAL.ID: {0}",cmd.ExecuteScalar());
   }//using cmd

   //--------
   Console.WriteLine("\nAttempt to delete from DUAL ...");

   var prevCulture=System.Threading.Thread.CurrentThread.CurrentUICulture;

   try
   {
    using(var cmd=new OleDbCommand("delete from DUAL;",cn))
    {
     cmd.ExecuteNonQuery(); //throw!
    }//using
   }
   catch(Exception exc)
   {
    //display error text on english.

    System.Threading.Thread.CurrentThread.CurrentUICulture
     =new System.Globalization.CultureInfo("EN");
   
    Console.WriteLine("\nOK: {0} - {1}",exc.Source,exc.Message);
   }
   finally
   {
    //restore culture
    System.Threading.Thread.CurrentThread.CurrentUICulture=prevCulture;
   }

   //--------
   Console.WriteLine("\nDrop database ...");

   cn.DropDatabase();

   //--------
   Console.WriteLine("\nOK. Run again!");
  }
  catch(Exception exc)
  {
   resultCode=1;

   Console.WriteLine("ERROR: {0} - {1}",exc.Source,exc.Message);
  }//catch

  return resultCode;
 }//Main
}//class Program

////////////////////////////////////////////////////////////////////////////////
}//nms Sample_0020
