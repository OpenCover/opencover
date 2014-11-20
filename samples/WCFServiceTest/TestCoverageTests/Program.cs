using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestCoverageTests.ServiceReference1;

namespace TestCoverageTests
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Service1Client srv = new Service1Client();
                var contact = srv.GetDataUsingDataContract(new CompositeType() { BoolValue = true, StringValue = "xxx" });
                Console.WriteLine(contact.StringValue);
                srv.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
