using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoeSuportContractDataConversion
{
	class Case
	{
		public string caseType;
		public string id;
		public string caseName;
		public string description;
		public List<Party> parties;

		public Case() : this(null) {}

		public Case(string CaseID)
		{
			id = CaseID;

			parties = new List<Party>();
		}

		public bool ShouldSerializeparties()
		{
			return (parties.Count > 0);
		}
	}
}
