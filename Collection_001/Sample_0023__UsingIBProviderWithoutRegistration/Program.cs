using System;
using lcpi.data.oledb;

namespace Sample_0023{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
   ="location=localhost:d:\\database\\employee.fdb;"
   +"user id=gamer;"
   +"password=vermut;";

 //-----------------------------------------------------------------------
 static int Main()
 {
  int resultCode=0;

  try
  {
   var cnsb=new OleDbConnectionStringBuilder(c_cn_str);

   switch(IntPtr.Size)
   {
    case 4: cnsb.Provider="LCPI.IBProvider.3.32bit.Private"; break;
    case 8: cnsb.Provider="LCPI.IBProvider.3.64bit.Private"; break;

    default:
     throw new ApplicationException("Unexpected platform!");
   }//switch

   using(var cn=new OleDbConnection(cnsb.ConnectionString))
   {
    Console.WriteLine("Try to connect ...");

    cn.Open();

    Console.WriteLine("OK!");

    var t=cn.GetSchema(OleDbMetaDataCollectionNames.DataSourceInformation);

    Console.WriteLine
     ("provider: {0}",
      t.Rows[0][OleDbMetaDataCollectionColumnNames
                 .DataSourceInformation
                 .LCPI_OleDbProviderFriendlyName]);
   }//using cn
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
}//namespace Sample_0023
