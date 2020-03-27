using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;

namespace EvoeSuportContractDataConversion
{
	class eSupportInterface
	{
		string Username, Password, authToken, apiUrl;
		public eSupportInterface()
		{
			Username = ConfigurationManager.AppSettings["eSupportUsername"];
			Password = ConfigurationManager.AppSettings["eSupportPassword"];
			authToken = eRestClient.GetUserAuthToken(Username, Password);
			apiUrl = ConfigurationManager.AppSettings["eSupportBaseURL"] + "/ws/rest/ecourt";
		}
		public bool TestConnection()
		{
			var url = apiUrl + "/entities";
			eRestRequest request = new eRestRequest(url, authToken);
			try
			{
				var result = eRestClient.SubmitRequest(request);
				if (result != null)
				{
					return true;
				}
			} catch (Exception e)
			{
				throw e;
			}
			
			return false;
		}
		public string GetPersonsFromPersonSource(int? personSource)
		{
			if(personSource != null)
			{
				var url = apiUrl + "/search/person/personSource/" + personSource;
				eRestRequest request = new eRestRequest(url, authToken);
				var result = eRestClient.SubmitRequest(request);

				return result;
			}
			return null;
		}
		public string CreateCaseWithParty(string caseJson)
		{
			var url = apiUrl + "/entities/case";
			eRestRequest request = new eRestRequest(url, authToken, eRestRequestTypes.POST, caseJson);
			return eRestClient.SubmitRequest(request);
		}
		public string CreateDocument(string documentJson)
		{
			var url = apiUrl + "/entities/document";
			eRestRequest request = new eRestRequest(url, authToken, eRestRequestTypes.POST, documentJson);
			return eRestClient.SubmitRequest(request);
		}
		public string UploadDocument(string documentID, string filePath)
		{
			var url = apiUrl + "/upload/document/" + documentID;
			eRestRequest request = new eRestRequest(url, authToken, eRestRequestTypes.POST);
			request.FilePathToUpload = filePath;
			return eRestClient.SubmitRequest(request);
		}
		public string CreatePersonDocument(string documentJson)
		{
			var url = apiUrl + "/entities/personDocument";
			eRestRequest request = new eRestRequest(url, authToken, eRestRequestTypes.POST, documentJson);
			return eRestClient.SubmitRequest(request);
		}
		public string UploadPersonDocument(string documentID, string filePath)
		{
			var url = apiUrl + "/upload/personDocument/" + documentID;
			eRestRequest request = new eRestRequest(url, authToken, eRestRequestTypes.POST);
			request.FilePathToUpload = filePath;
			return eRestClient.SubmitRequest(request);
		}
	}
}
