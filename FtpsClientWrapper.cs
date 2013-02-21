using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AlexPilotti.FTPS.Client;

namespace ConsoleApplication1
{
	public class FtpsClientWrapper : IFtpClient
	{
		public FTPSClient _client;

		public string Connect(string address, NetworkCredential credentials, ESSLSupportMode esslSupportMode)
		{
			_client = new FTPSClient();
			return _client.Connect(address, credentials, esslSupportMode);
		}

		public void MakeDir(string dir)
		{
			_client.MakeDir(dir);
		}

		public void SetCurrentDirectory(string dir)
		{
			_client.SetCurrentDirectory(dir);
		}

		public string GetCurrentDirectory()
		{
			return _client.GetCurrentDirectory();
		}

		public void PutFiles(string localDirPath, string remotePath, string filePattern, EPatternStyle patternStyle, bool recursive, FileTransferCallback transferCallback)
		{
			_client.PutFiles(localDirPath, remotePath, filePattern, patternStyle, recursive, transferCallback);
		}

		public void PutFile(string localFileName, string remoteFilename, FileTransferCallback transferCallback)
		{
			_client.PutFile(localFileName, remoteFilename, transferCallback);
		}

		public List<string> GetDirectoryNames(string remotePath)
		{
			return _client.GetDirectoryList(remotePath)
							.Where(x => x.IsDirectory)
							.Select(x => remotePath + "/" + x.Name).ToList();
		}
		public List<string> GetFileNames(string remotePath, string searchString)
		{
			return _client.GetDirectoryList(remotePath)
							.Where(x => !x.IsDirectory && x.Name.Contains(searchString))
							.Select(x => remotePath+"/"+ x.Name).ToList();
		} 
		public void Dispose()
		{
			if (_client != null)
			{
				_client.Close();
				_client.Dispose();
			}
		}
	}
}
