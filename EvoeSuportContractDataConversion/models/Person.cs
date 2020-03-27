using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoeSuportContractDataConversion
{
	class Person
	{
		public string id;
		public string personSource;

		public string personType;
		public string personSubType;

		public string lastName;
		public string firstName;

		public string lastUpdateUserRealName;
		public string lastUpdated;
		
		public string dateCreated;
		public string createUsername;
		
		public string email;

		public Person(): this(null) { }

		public Person(string NameID)
		{
			id = NameID;
		}

	}
}
