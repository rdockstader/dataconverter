using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EvoeSuportContractDataConversion
{
	class EvoInterface : EvoAPI.JustWareApiClient
	{
		string UserName, Password;
		public EvoInterface()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			UserName = ConfigurationManager.AppSettings["evoUsername"];
			Password = ConfigurationManager.AppSettings["evoPassword"];

			ClientCredentials.UserName.UserName = UserName;
			ClientCredentials.UserName.Password = Password; 
		}

		public int OverrideUsernameAndPassword(string NewUserName, string NewPassword)
		{
			int userNameID;

			UserName = NewUserName;
			Password = NewPassword;

			ClientCredentials.UserName.UserName = UserName;
			ClientCredentials.UserName.Password = Password;

			userNameID = TestApiConnection();

			return userNameID;
		}

		public int TestApiConnection()
		{
			int nNameID;
			try
			{
				nNameID = GetCallerNameID();
			}
			catch (Exception e)
			{
				throw e;
			}
			return nNameID;
		}

		public EvoAPI.Name GetNameWithDocuments(int NameID)
		{
			try
			{
				var name = GetName(NameID, null);
				if(name != null)
				{
					List<EvoAPI.NameDocument> returnedDocuments = new List<EvoAPI.NameDocument>();
					List<EvoAPI.NameDocument> nameDocuments = new List<EvoAPI.NameDocument>();
					int numSkip = 0;
					const int numTake = 5;
					string query = "NameID = " + NameID;
					String skipTake = ".OrderBy(AddedDate asc).Skip(" + numSkip + @").Take(" + numTake + @")";
					returnedDocuments = FindNameDocuments(query + skipTake, null);
					while (returnedDocuments != null && returnedDocuments.Count > 0)
					{
						numSkip += numTake;
						skipTake = ".OrderBy(AddedDate asc).Skip(" + numSkip + @").Take(" + numTake + @")";
						returnedDocuments = FindNameDocuments(query + skipTake, null);
						nameDocuments.AddRange(returnedDocuments);
					}
					name.NameDocuments = nameDocuments;
				}
				
				return name;
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		public void DownloadNameFile(EvoAPI.NameDocument nameDocument, string downloadTofilePath)
		{
			try
			{
				using (var webClient = new WebClient())
				{
					
					webClient.Credentials = new NetworkCredential(UserName, Password);
					var sourceFilePath = RequestFileDownload(nameDocument);
					// Download the file
					webClient.DownloadFile(sourceFilePath, downloadTofilePath);
				}
					
			}
			catch (Exception e)
			{
				throw e;
			}
		}
	}
}
