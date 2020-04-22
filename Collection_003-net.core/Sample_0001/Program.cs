////////////////////////////////////////////////////////////////////////////////
//Example for "LCPI ADO.NET Provider for OLE DB"
//                                                             LCPI. 14.11.2017
using System;
using System.Threading;

using xdb=lcpi.data.oledb;

namespace Sample_0001.vs2017{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 static void Main(string[] args)
 {
  try
  {
   Console.WriteLine("Initialization of COM [MTA] ...");

   Thread.CurrentThread.SetApartmentState(ApartmentState.MTA);

   var cnsb=new xdb.OleDbConnectionStringBuilder();

   cnsb.Provider="LCPI.IBProvider.3";

   cnsb.Location="inet4://localhost/d:\\database\\fb_03_0_0\\employee.fdb";

   cnsb.UserID="GAMER";

   cnsb.Password="vermut";

   cnsb.IBProvider.dbclient_type="fb.direct";

   using(var cn=new xdb.OleDbConnection(cnsb.ConnectionString))
   {
    Console.WriteLine("Connection to database ...");
    cn.Open();

    var ds_info=cn.GetSchema(xdb.OleDbMetaDataCollectionNames.DataSourceInformation);

    Console.WriteLine("");

    var R0=ds_info.Rows[0];

    Console.WriteLine
     ("DBMS Name   : {0}",
      Helper__ValueToStr(R0[xdb.OleDbMetaDataCollectionColumnNames.DataSourceInformation.DataSourceProductName]));

    Console.WriteLine
     ("DBMS Version: {0}",
      Helper__ValueToStr(R0[xdb.OleDbMetaDataCollectionColumnNames.DataSourceInformation.DataSourceProductVersion]));

    Console.WriteLine("");

    Console.WriteLine
     ("OLE DB Provider Descr  : {0}",
      Helper__ValueToStr(R0[xdb.OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderFriendlyName]));

    Console.WriteLine
     ("OLE DB Provider File   : {0}",
      Helper__ValueToStr(R0[xdb.OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderFileName]));

    Console.WriteLine
     ("OLE DB Provider Version: {0}",
      Helper__ValueToStr(R0[xdb.OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderVersion]));

    Console.WriteLine("");

    Console.WriteLine
     ("ADO.NET Provider File   : {0}",
      cn.GetType().Assembly.GetModules()[0].Name);

    Console.WriteLine
     ("ADO.NET Provider Version: {0}",
      cn.GetType().Assembly.GetName().Version);
   }//using cn
  }
  catch(Exception exc)
  {
   Console.WriteLine
    ("ERROR: {0} - {1}",
     exc.Source,
     exc.Message);
  }//catch

  Console.WriteLine("");
  Console.WriteLine("Force release of references to COM objects");

  GC.Collect();
  GC.WaitForPendingFinalizers();
 }//Main

 //----------------------------------------------------------------------
 private static string Helper__ValueToStr(object v)
 {
  if(Object.ReferenceEquals(v,null))
   return "#EMPTY";

  if(DBNull.Value.Equals(v))
   return "#NULL";

  return v.ToString();
 }//Helper__ValueToStr
};//class Program

////////////////////////////////////////////////////////////////////////////////
}//namespace Sample_0001.vs2017
