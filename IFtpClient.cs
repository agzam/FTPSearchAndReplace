using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Client;

namespace ConsoleApplication1
{
	public interface IFtpClient : IDisposable
	{
		string Connect(string address, NetworkCredential credentials, ESSLSupportMode esslSupportMode);

		void MakeDir(string dir);

		void SetCurrentDirectory(string dir);

		string GetCurrentDirectory();

		void PutFiles(string localDirPath, string remotePath, string filePattern, EPatternStyle patternStyle, bool recursive,
					  FileTransferCallback transferCallback);

		void PutFile(string localFileName, string remoteFilename, FileTransferCallback transferCallback);

		List<string> GetDirectoryNames(string remotePath);

		List<string> GetFileNames(string remotePath, string searchString);

		//void Close();
	}
}
