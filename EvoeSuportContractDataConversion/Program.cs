using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoeSuportContractDataConversion
{
	class Program
	{
		static void Main(string[] args)
		{
			DataConverter dataConverter = new DataConverter();
			dataConverter.TestConnections();
			dataConverter.LoadNameIDsFromFile(); // TODO: make variable
			dataConverter.ExecuteConversion();	
			

			Console.ReadLine();
		}
	}
}
