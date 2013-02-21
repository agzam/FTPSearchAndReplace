using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\smils");
			var fixer1 = new FtpSmilFixer("", new NetworkCredential("", ""));

			//var mfst = fixer1.GetVideosFromManifestAsync("/187855/dev/v3/smil/AG1245678911/ag1245678911.smil");
			var t = fixer1.ReadDirs("/187855/dev/v3/smil");
			Task.WaitAll(t);
			Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\smils",true);
		}
	}

	public class FtpSmilFixer
	{
		private readonly string _host;
		private readonly NetworkCredential _credential;
		private readonly FtpsClientWrapper _ftpClient;

		public FtpSmilFixer(string host, NetworkCredential credential)
		{
			_host = host;
			_credential = credential;
			Console.Write("Connecting to: " + host + "...");
			_ftpClient = new FtpsClientWrapper();
			_ftpClient.Connect(host, credential, ESSLSupportMode.ClearText);
			Console.WriteLine(" succesfully connected");
		}

		public async Task ReadDirs(string remotePath)
		{
			var lst = _ftpClient.GetDirectoryNames(remotePath);
			Console.WriteLine("Found {0} ISRCs", lst.Count);
			foreach (var dir in lst)
			{
				Console.WriteLine("Reading smills for {0}", dir.Split('/').Last());
				var smills = _ftpClient.GetFileNames(dir, ".smil");

				foreach (var smilFile in smills)
				{
					var vids = GetVideosFromManifest(smilFile);
					var srcs = vids.Select(x => x.Attributes[0].Value);
					var goFix = false;
					foreach (var src in srcs)
					{
						try
						{
							srcs.SingleOrDefault(x => src == x);
						}
						catch (InvalidOperationException)
						{
							goFix = true;
						}
						if (!goFix) continue;
							await FixTheSmilFileAsync(dir, smilFile);
							break;
					}
				}
			}
		}

		public async Task<bool> FixTheSmilFileAsync(string remotePath, string remoteSmilPath)
		{
			var xmlDocument = new XmlDocument { XmlResolver = null };
			var ftpCl = new FtpsClientWrapper();
			Console.WriteLine("Fixing smil {0}", remoteSmilPath);
			ftpCl.Connect(_host, _credential, ESSLSupportMode.ClearText);
			var noSmilFiles = ftpCl.GetFileNames(remotePath, "").Where(x => !x.Contains(".smil"));

			using (var fs = ftpCl._client.GetFile(remoteSmilPath))
			{
				using (var sr = new StreamReader(fs))
				{
					xmlDocument.LoadXml(sr.ReadToEnd());
					var vids = xmlDocument.DocumentElement.ChildNodes.Cast<XmlNode>()
										  .FirstOrDefault(x => x.Name == "body").ChildNodes.Cast<XmlNode>()
										  .FirstOrDefault(x => x.Name == "switch")
										  .ChildNodes.Cast<XmlNode>()
										  .Select(x => x as XmlElement);
					foreach (var xmlElement in vids)
					{
						var isrc = remotePath.Split('/').Last().ToLower();
						var bit = (int.Parse(xmlElement.GetAttribute("system-bitrate")) / 1000) + "k";
						var newfileName = noSmilFiles.FirstOrDefault(x => x.Contains(isrc + "_" + bit)).Split('/').Last();
						var previousValue = xmlElement.GetAttribute("src").Split('/').Last();
						if (newfileName != previousValue)
						{
							xmlElement.SetAttribute("src", xmlElement.GetAttribute("src").Replace(previousValue, newfileName));
						}
					}
					
				}
			}

			var lclFileName = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\smils\\" +
									  remoteSmilPath.Split('/').Last();
			xmlDocument.Save(lclFileName);

			await Task.Run(() =>
				{
					ftpCl._client.PutFile(lclFileName, remoteSmilPath);
					Console.WriteLine("Fixed {0}", remoteSmilPath);
				});
			return true;
		}

		public IEnumerable<XmlElement> GetVideosFromManifest(string remoteSmilFilePath)
		{
			using (var fs = _ftpClient._client.GetFile(remoteSmilFilePath))
			{
				var xmlDocument = new XmlDocument { XmlResolver = null };
				using (var sr = new StreamReader(fs))
				{
					xmlDocument.LoadXml(sr.ReadToEnd());
					return xmlDocument.DocumentElement.ChildNodes.Cast<XmlNode>()
						.FirstOrDefault(x => x.Name == "body").ChildNodes.Cast<XmlNode>()
						.FirstOrDefault(x => x.Name == "switch").ChildNodes.Cast<XmlNode>().Select(x => x as XmlElement);
				}
			}
		}
	}
}
