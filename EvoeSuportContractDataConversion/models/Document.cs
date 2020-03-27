using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EvoeSuportContractDataConversion
{
	class Document
	{
		public DocDef docDef;

		[JsonProperty(PropertyName = "case")]
		public Case theCase;
		public string nameExact;
		public string id;


		public Document() {
			docDef = new DocDef();
			theCase = new Case();
		}
	}
}
