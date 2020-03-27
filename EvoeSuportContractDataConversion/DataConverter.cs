using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EvoeSuportContractDataConversion
{
	class DataConverter
	{
		private int? currentNameID;
		private int currentNameIDIndex = -1;
		// Static Variables
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly static string CONTRACT_CASE_TYPE = "CONTR";
		private readonly static string NON_MON_APPLICATION = "NONE";
		private readonly static string AGENCY_PARTY_TYPE = "AGN";
		// Non-Static Variables
		string downloadRootDirectory;
		List<string> NameIDList;
		bool testsSucceeded = false;
		// Constructors
		public DataConverter()
		{
			downloadRootDirectory = ConfigurationManager.AppSettings["DownloadRootDirectory"];
			NameIDList = new List<string>();
		}
		// Execute
		public void ExecuteConversion()
		{
			if(testsSucceeded)
			{
				EmptyRootFolder();
				logger.Info("Beginning Conversion...");
				if (NameIDList != null)
				{
					do
					{
						ConvertCurrentNameID();
					} while (LoadNextNameID() != null);
					logger.Info("Conversion Complete for {} NameIDs", NameIDList.Count);
				}
				else
				{
					logger.Info("Conversion failed, NameIDs not loaded");
				}
			} else
			{
				logger.Info("Tests failed. Please try again later.");
			}
		}
		private void ConvertCurrentNameID()
		{
			try
			{
				var persons = VerifyNameIDExistsInEsupport(currentNameID);
				if(persons == null)
				{
					logger.Info("Failed to load data from eSupport for NameID {NameID}, skipping...", currentNameID);
				}
				else if (persons.Count > 1)
				{
					logger.Info("Multiple names found for NameID {NameID}:{lastName}, skipping...", currentNameID, persons[0].lastName);
					TrackPersonError(persons[0].id, "Multiple names found for NameID");
				}
				else if (persons.Count < 1)
				{
					logger.Info("NameID {NameID} not found in eSupport, skipping...", currentNameID);
					TrackPersonError(persons[0].id, "NameID not found in eSupport");
				}
				else
				{
					logger.Info("NameID {NameID} found for {lastName}, converting...", currentNameID, persons[0].lastName);
					var CaseID = CreateContractCase(persons.First());
					if(CaseID > 0)
					{
						DownloadFilesFromEvo(currentNameID);
						UploadFilesToEsupportAndArchive(persons.First(), CaseID);
					}
					logger.Info("{lastName} Conversion Complete!", persons[0].lastName);
				}

			}
			catch (Exception e)
			{
				logger.Error(e);
			}
		}
		// Testing Connections
		public void TestConnections()
		{
			if(TestEvoConnection() && TestEsupportConnection())
			{
				testsSucceeded = true;
			}
		}
		private bool TestEvoConnection()
		{
			EvoInterface evo = new EvoInterface();
			int evoNameID = evo.TestApiConnection();
			if (evoNameID > 0)
			{
				logger.Info("Connected to Evolution Successfully");
				return true;
			}
			else
			{
				logger.Error("Failed to load NameID for logged in user!");
				return false;
			}
		}
		private bool TestEsupportConnection()
		{
			try
			{
				eSupportInterface eSupport = new eSupportInterface();
				var success = eSupport.TestConnection();
				if (success)
				{
					logger.Info("Connected to eSupport Successfully");
					return true;
				}
				else
				{
					logger.Error("Failed to connect to eSupport!");
					return false;
				}
			} 
			catch (Exception e)
			{
				logger.Error("Failed to connect to eSupport!");
				logger.Error(e);
				return false;
			}
			
		}
		public void LoadNameIDsFromFile()
		{
			try
			{
				NameIDList = new List<string>(File.ReadAllLines(ConfigurationManager.AppSettings["NameIDListPath"]));
				logger.Debug("NameID's loaded...");
				LoadNextNameID();
			}
			catch (Exception e)
			{
				logger.Error(e);
			}
		}
		private int? LoadNextNameID()
		{
			if(NameIDList != null && NameIDList.Count > 0)
			{
				currentNameIDIndex++;
				if (currentNameIDIndex < NameIDList.Count)
				{
					string NameID = NameIDList[currentNameIDIndex];
					int NameIDInt = 0;
					if (!Int32.TryParse(NameID, out NameIDInt))
					{
						currentNameID = -1;
						logger.Error("NameID: {NameID} isn't a valid integer.", NameID);
					}
					else
					{
						currentNameID = NameIDInt;
						logger.Debug("NameID: {NameID} loaded...", currentNameID);
					}
				}
				else
				{
					currentNameID = null;
				}
			}
			else
			{
				logger.Error("Name ID List is not populated!");
			}
			return currentNameID;
		}
		// Evolution Methods
		private void DownloadFilesFromEvo(int? NameID)
		{
			try
			{
				CreateRootDirectoryIfNotExists();
				using (EvoInterface evo = new EvoInterface())
				{
					try
					{
						logger.Debug("Getting Name info for NameID {NameID}", NameID);
						if(NameID != null)
						{
							var nameEntity = evo.GetNameWithDocuments(NameID ?? default(int));
							logger.Info("Downloading files for {AgencyName}:{NameID}", nameEntity.FullName, nameEntity.ID);
							foreach (var nameDoc in nameEntity.NameDocuments)
							{
								var downloadFilePath = Path.Combine(downloadRootDirectory, nameEntity.ID.ToString(), nameDoc.FileName);
								var downloadDirectory = Path.GetDirectoryName(downloadFilePath);
								CreateNamedDirectory(downloadDirectory);
								try
								{
									logger.Debug("Downloading file {fileName}", nameDoc.FileName);
									evo.DownloadNameFile(nameDoc, downloadFilePath);
								}
								catch (Exception e)
								{
									logger.Error(e);
									TrackFileError(nameDoc.FileName, nameEntity.FullName, e.Message);
								}

							}
							logger.Info("Download complete for {AgencyName}:{NameID}", nameEntity.FullName, nameEntity.ID);
						}
						else
						{
							logger.Error("NameID cannot be null to download files.");
						}
						
					}
					catch (Exception e)
					{
						logger.Error(e);
						
					}
					
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to Downloading files from EVO for NameID {NameID}. Message: {ErrorMessage}", NameID, e.Message);
			}
			
		}
		// eSupport Methods
		private List<Person> VerifyNameIDExistsInEsupport(int? NameID)
		{
			try
			{
				eSupportInterface eSupport = new eSupportInterface();
				return JsonConvert.DeserializeObject<List<Person>>(eSupport.GetPersonsFromPersonSource(NameID));
			}
			catch
			{
				TrackPersonError(NameID.ToString(), "Failed to load person via NameID from eSupport.");
				return null;
			}
		}
		private int CreateContractCase(Person agency)
		{
			try
			{
				
				// create case
				Case theCase = new Case();
				theCase.caseName = agency.lastName;
				theCase.caseType = CONTRACT_CASE_TYPE;
				theCase.description = "Conversion data from Evolution";
				// create agency party
				Party agencyParty = new Party();
				agencyParty.person = agency.id;
				agencyParty.nonMonApplication = NON_MON_APPLICATION;
				agencyParty.partyType = AGENCY_PARTY_TYPE;
				theCase.parties.Add(agencyParty);
				// submit case
				eSupportInterface eSupport = new eSupportInterface();
				var caseJson = JsonConvert.SerializeObject(theCase);
				logger.Trace(caseJson);
				var result = eSupport.CreateCaseWithParty(caseJson);
				theCase = JsonConvert.DeserializeObject<Case>(result);
				// get result
				int CaseID;
				if(Int32.TryParse(theCase.id, out CaseID))
				{
					logger.Info("Case created with ID {CaseID}", CaseID);
					return CaseID;
				}
				else
				{
					logger.Error("Failed to create contract case");
					return -1;
				}
			}
			catch (Exception e)
			{
				logger.Error("Failed to create contract case!!");
				logger.Error(e);
			}


			return -1;
		}
		private void UploadFilesToEsupportAndArchive(Person agency, int CaseID)
		{
			if(agency != null)
			{
				var nameDirectoryPath = Path.Combine(downloadRootDirectory, agency.personSource);
				if(Directory.Exists(nameDirectoryPath))
				{
					var nameDirectory = Directory.GetFiles(nameDirectoryPath, "*.*", SearchOption.AllDirectories);
					logger.Info("Uploading files to eSupport...");
					foreach (var filePath in nameDirectory)
					{
						try
						{
							logger.Debug("Uploading file {filePath}", filePath);
							if(PathContainsText(filePath, "sales"))
							{
								CreateAndUploadDocument(filePath, CaseID.ToString());
							}
							else
							{
								//CreateAndUploadPersonDocument(filePath, agency);
							}
						}
						catch(Exception e)
						{
							logger.Error("Failed to upload file!");
							logger.Error(e);
							TrackFileError(Path.GetFileName(filePath), 
											String.Format("[NameID: {0}]", agency.personSource), 
											"Failed to upload file to eSupport");
						}
					}
					archiveFolder(nameDirectoryPath);
					logger.Info("Uploads complete!");
				}
				else
				{
					logger.Info("Directory not created, skipping.");
				}
			}
		}
		private void CreateAndUploadPersonDocument(string filePath, Person person)
		{
			var doc = CreatePersonDocument(filePath, person);
			eSupportInterface eSupport = new eSupportInterface();
			var documentJson = JsonConvert.SerializeObject(doc,
														   Formatting.None,
														   new JsonSerializerSettings
														   {
															NullValueHandling = NullValueHandling.Ignore
														   });
			logger.Trace(documentJson);
			var result = eSupport.CreatePersonDocument(documentJson);
			doc = JsonConvert.DeserializeObject<PersonDocument>(result);
			// upload doc
			eSupport.UploadPersonDocument(doc.id.ToString(), filePath);
		}
		private PersonDocument CreatePersonDocument(string filePath, Person person)
		{
			PersonDocument doc = new PersonDocument();
			doc.docDef = determineDocDef(default(string));
			doc.person = person;
			doc.nameExact = Path.GetFileName(filePath);

			return doc;
		}
		private void CreateAndUploadDocument(string filePath, string CaseID)
		{
			FileInfo info = new FileInfo(filePath);
			if(info.Extension == ".db")
			{
				logger.Debug("File extension is .db, skipping.");
			}
			else if (info.Extension == ".efx")
			{
				logger.Debug("file extension is .efx, skipping");
				TrackFileError(info.Name, "[]", "file extension is .efx and was skipped.");
			}
			else
			{
				var doc = CreateDocument(filePath, CaseID.ToString());
				eSupportInterface eSupport = new eSupportInterface();
				var documentJson = JsonConvert.SerializeObject(doc,
															   Formatting.None,
															   new JsonSerializerSettings
															   {
																   NullValueHandling = NullValueHandling.Ignore
															   });
				logger.Trace(documentJson);
				var result = eSupport.CreateDocument(documentJson);
				doc = JsonConvert.DeserializeObject<Document>(result);
				// upload doc
				eSupport.UploadDocument(doc.id.ToString(), filePath);
			}
		}
		private Document CreateDocument(string filePath, string CaseID)
		{
			Document doc = new Document();
			doc.docDef = determineDocDef(filePath);
			doc.theCase = new Case(CaseID.ToString());
			doc.nameExact = Path.GetFileName(filePath);

			return doc;
		}
		// Helper Methods
		private void CreateRootDirectoryIfNotExists()
		{
			if (!Directory.Exists(downloadRootDirectory))
			{
				Directory.CreateDirectory(downloadRootDirectory);
			}
		}
		private void CreateNamedDirectory(string path)
		{
			try
			{
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}
			catch (Exception e)
			{
				logger.Error(e);
			}

		}
		private void archiveFolder(string folderPath)
		{
			var folderName = "Complete";
			// create root folder
			var archivePath = Path.Combine(downloadRootDirectory, folderName);
			CreateNamedDirectory(archivePath);
			// move folder
			Directory.Move(folderPath, Path.Combine(archivePath, new DirectoryInfo(folderPath).Name));
		}
		private DocDef determineDocDef(string filePath)
		{
			DocDef docDef = new DocDef();
			if(PathContainsText(filePath, "initial"))
			{
				docDef.id = DocDefNumbers.LICENSE_SERICE_AGREEMENT;
			}
			else if(PathContainsText(filePath, "change request") || PathContainsText(filePath, "support renewal"))
			{
				docDef.id = DocDefNumbers.ADDENDUM;
			}
			else if(PathContainsText(filePath, "sales"))
			{
				docDef.id = DocDefNumbers.ADD_ON_SALES_CONTRACT;
			}
			else
			{
				docDef.id = DocDefNumbers.UNKNOWN;
			}
			return docDef;
		}
		private bool PathContainsText(string path, string text)
		{
			if(path == null || text == null)
			{
				return false;
			}

			return (path.ToLower().Contains(text.ToLower()));
		}
		private void EmptyRootFolder()
		{
			if (Directory.Exists(downloadRootDirectory))
			{
				logger.Info("Clearing downloads folder...");
				DirectoryInfo di = new DirectoryInfo(downloadRootDirectory);

				foreach (FileInfo file in di.EnumerateFiles())
				{
					file.Delete();
				}
				foreach (DirectoryInfo dir in di.EnumerateDirectories())
				{
					dir.Delete(true);
				}
				logger.Info("Downloads folder cleared!");
			}
		}
		// Error Tracking
		private void TrackFileError(string fileName, string agencyName, string message)
		{
			var errorsFilePath = Path.Combine(downloadRootDirectory, "!ERRORS", "error-files.txt");
			var errorsDirectory = Path.GetDirectoryName(errorsFilePath);
			CreateNamedDirectory(errorsDirectory);

			using (var file = File.AppendText(errorsFilePath))
			{
				file.WriteLine("Error Transfering File {0} for agency {1}. Error Message: {2}", fileName, agencyName, message);
			}
		}
		private void TrackPersonError(string NameID, string message)
		{
			var errorsFilePath = Path.Combine(downloadRootDirectory, "!ERRORS", "error-files.txt");
			var errorsDirectory = Path.GetDirectoryName(errorsFilePath);
			CreateNamedDirectory(errorsDirectory);

			using (var file = File.AppendText(errorsFilePath))
			{
				file.WriteLine("Error transfering files for NameID {0}. Error Message: {1}", NameID, message);
			}
		}
	}
}
