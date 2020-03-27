using System;
using System.IO;

namespace EvoeSuportContractDataConversion
{
	public class FileTypes
	{
		// if list is expanded, add branch to SetContentType
		public const string JPG = "image/jpeg";
		public const string JPEG = "image/jpeg";
		public const string MS_DOC = "application/msword";
		public const string MS_DOCX = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
		public const string PNG = "image/png";
		public const string PDF = "application/pdf";
		public const string RTF = "application/rtf";
		public const string XLSM = "application/vnd.ms-excel.sheet.macroEnabled.12";
		public const string DB = "text/plain"; // Unknown mime type.
		public const string MSG = "application/vnd.ms-outlook";
		public const string PCF = "application/x-font-pcf";
		public const string EFX = "image/efax";
		public const string RDL = "text/xml";
		public const string ZIP = "application/zip";
		public const string TXT = "text/plain";
	}

	public enum eRestRequestTypes
	{
		GET,
		POST,
		PUT,
		DELETE
	}

	public class eRestRequest
	{
		public string Url { get; set; }
		public string AuthToken { get; set; }
		public eRestRequestTypes RequestType { get; set; }
		public string Json { get; set; }

		public string FilePathToUpload { get; set; }
		public string ContentType { get; set; }


		public eRestRequest() { }
		public eRestRequest(string url, string authToken, eRestRequestTypes requestType = eRestRequestTypes.GET, string json = null)
		{
			Url = url;
			AuthToken = authToken;
			RequestType = requestType;
			Json = json;
		}

		public void SetContentType(FileInfo fi)
		{
			switch (fi.Extension.ToLower())
			{
				case ".jpg":
				case ".jpeg":
					ContentType = FileTypes.JPEG;
					break;

				case ".doc":
					ContentType = FileTypes.MS_DOC;
					break;

				case ".docx":
					ContentType = FileTypes.MS_DOCX;
					break;

				case ".png":
					ContentType = FileTypes.PNG;
					break;

				case ".pdf":
					ContentType = FileTypes.PDF;
					break;

				case ".rtf":
					ContentType = FileTypes.RTF;
					break;
				case ".xlsm":
					ContentType = FileTypes.XLSM;
					break;
				case ".db":
					ContentType = FileTypes.DB;
					break;
				case ".msg":
					ContentType = FileTypes.MSG;
					break;
				case ".pcf":
					ContentType = FileTypes.PCF;
					break;
				case ".efx":
					ContentType = FileTypes.EFX;
					break;
				case ".rdl":
					ContentType = FileTypes.RDL;
					break;
				case ".zip":
					ContentType = FileTypes.ZIP;
					break;
				case ".txt":
					ContentType = FileTypes.TXT;
					break;
				default:
					throw new Exception("File extension " + fi.Extension.ToLower() + " is unknown, cannot set content type");
			}
		}
	}
}
