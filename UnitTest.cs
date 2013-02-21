using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using AlexPilotti.FTPS.Client;
using NUnit.Framework;

namespace ConsoleApplication1
{
	[TestFixture]
	public class UnitTest
	{

		[Test]
		public void FtpTest()
		{
			var ftpClient = new FtpsClientWrapper();
			ftpClient.Connect("encvweb.upload.akamai.com", new NetworkCredential("encvweb-dev", "OfCl9FHj9zMl6ZXo9KLX"),
			                  ESSLSupportMode.ClearText);
			//var lst = ftpClient.GetDirectoryNames("/187855/dev/v3/smil");
			var lst = ftpClient.GetFileNames("/187855/dev/v3/smil/AG1245678900",".smil");

			var fs = ftpClient._client.GetFile(lst.FirstOrDefault());
			var xmlDocument = new XmlDocument{XmlResolver = null};
			var ss = new StreamReader(fs).ReadToEnd();
			xmlDocument.LoadXml(ss);
			var videos = xmlDocument.DocumentElement.ChildNodes.Cast<XmlNode>()
				.FirstOrDefault(x => x.Name == "body").ChildNodes.Cast<XmlNode>()
				.FirstOrDefault(x=> x.Name == "switch").ChildNodes.Cast<XmlNode>().Select(x=> x as XmlElement);

		}
	}
}
