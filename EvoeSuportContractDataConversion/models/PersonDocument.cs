using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoeSuportContractDataConversion
{
	class PersonDocument
	{
		public DocDef docDef;

		public Person person;
		public string nameExact;
		public string id;


		public PersonDocument()
		{
			docDef = new DocDef();
			person = new Person();
		}
	}
}
