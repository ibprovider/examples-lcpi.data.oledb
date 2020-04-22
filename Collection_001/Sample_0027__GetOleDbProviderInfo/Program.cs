////////////////////////////////////////////////////////////////////////////////
//Samples for LCPI ADO.NET Data provider for OLEDB.
//                                                    ibprovider.com. 13.12.2019
using System;
using lcpi.data.oledb;

namespace Sample_0027{
////////////////////////////////////////////////////////////////////////////////
//class Program

class Program
{
 private const string c_cn_str
  ="provider=LCPI.IBProvider.5;"
  +"location=localhost:d:\\database\\ram\\ibp_test_fb30_d3.gdb;"
  +"dbclient_type=fb.direct;"
  +"user id=GAMER;"
  +"password=vermut;";

 //----------------------------------------------------------------------
 static int Main(string[] args)
 {
  int resultCode=0;

  try
  {
   using(var cn=new OleDbConnection(c_cn_str))
   {
    cn.Open();

    var dsinfo=cn.GetSchema(OleDbMetaDataCollectionNames.DataSourceInformation);

    var row=dsinfo.Rows[0];

    Console.WriteLine("OLE DB Provider Name    : {0}",row[OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderFriendlyName]);
    Console.WriteLine("OLE DB Provider FileName: {0}",row[OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderFileName]);
    Console.WriteLine("OLE DB Provider ClassID : {0}",row[OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderClassID]);
    Console.WriteLine("OLE DB Provider Version : {0}",row[OleDbMetaDataCollectionColumnNames.DataSourceInformation.LCPI_OleDbProviderVersion]);
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
}//class Program

////////////////////////////////////////////////////////////////////////////////
}//Sample_0027
