using McServerControlAPI.Models;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace McServerControlAPI.Services
{
    public class MinecraftServerService : IMinecraftServerService
    {
        private readonly ILogger<MinecraftServerService> _logger;

        private ServerStatus _ServerStatus;
        private object _StatusLock = new object();
        public ServerStatus ServerStatus
        {
            get
            {
                lock (_StatusLock)
                {
                    return _ServerStatus;
                }
            }
            private set
            {
                lock (_StatusLock)
                {
                    _ServerStatus = value;
                }
            }
        }

        public bool IsInputPossible { get; private set; }

        private Process _ServerProcess;

        public MinecraftServerService(ILogger<MinecraftServerService> logger)
        {
            _logger = logger;
        }

        public void StartServer()
        {
            if (ServerStatus != ServerStatus.Offline)
            {
                return;
            }

            //If this app crashed/shutdown and restarted while the server process did not, try to find it. (Or it was started from somewhere else)
            //While grabbing an already running process that wasn't started with this app, we don't have access to StandardOutput
            //But we can at least determine its status via the log-file, and monitor the Process.HasExited property.
            if (TryCheckForRunningServerProcess(out _ServerProcess))
            {
                IsInputPossible = false;

                WaitForLoggingStarted(@".\Content\MinecraftServerFiles\logs\latest.log");

                Task.Run(() =>
                {
                    using var fs = new FileStream(@".\Content\MinecraftServerFiles\logs\latest.log", FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    using var sr = new StreamReader(fs);

                    StartMonitoring(sr);


                }).ContinueWith(ex => { _logger.LogError($"An exception was thrown inside the monitorstatus-loop: {ex.Exception.Message}"); },
                TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                _ServerProcess = new Process();
                _ServerProcess.StartInfo = ConfigureInfo();

                _logger.LogInformation("Starting process");
                try
                {
                    _ServerProcess.Start();
                }
                catch (Win32Exception e)
                {
                    //TODO
                    _logger.LogCritical($"{e.Message}. Make sure Java runtime is installed on your system, and the path to the folder containing java.exe" +
                        $" is listed among the \"path\"-variables of your system environment variables");
                }

                IsInputPossible = true;

                //A check to make sure the process has really started, check 3 times before determining something went wrong
                for (int i = 0; i < 3; i++)
                {
                    if (HasServerProcessStarted())
                    {
                        Task.Run(() => StartMonitoring(_ServerProcess.StandardOutput))
                            .ContinueWith(ex => { _logger.LogError($"An exception was thrown inside the monitorstatus-loop, {ex.Exception.Message}"); },
                            TaskContinuationOptions.OnlyOnFaulted);

                        return;
                    }
                    else
                    {
                        Thread.Sleep(3000);
                    }
                }

                _logger.LogWarning("Something went wrong in trying to start the server process");
            }
        }

        public void StopServer()
        {
            if (ServerStatus == ServerStatus.Online)
            {
                if (_ServerProcess != null && !_ServerProcess.HasExited)
                {
                    _logger.LogDebug("Sending stop command");

                    _ServerProcess.StandardInput.WriteLine("/stop");
                }
            }
        }

        private bool TryCheckForRunningServerProcess(out Process outprocess)
        {
            if (!IsServerPortBusy())
            {
                outprocess = null;
                return false;
            }

            Process[] processes = Process.GetProcessesByName("java");

            if (processes.Length == 1)
            {
                _logger.LogWarning("An instance of this server is already running. Not able to send commands (like /stop) " +
                    "until the process has exited and started using this app");

                outprocess = processes[0];
                return true;
            }
            else if (processes.Length == 0)
            {
                _logger.LogError("The port specified in \"server.properties\" is busy, but no process matching \"java.exe\" could be found." +
                    "Make sure no other application is using the specified port");  
            }
            else if (processes.Length > 1)
            {
                _logger.LogError("The port specified in \"server.properties\" is busy, but multiple processes matching \"java.exe\" could be found. " +
                    "Cannot determine which is the correct process for this server-instance");
            }

            outprocess = null;
            return false;

            bool IsServerPortBusy()
            {
                //Try to start bind a listener to the port specified in server.properties
                if (TryGetPortProperty(out int port))
                {
                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    try
                    {
                        sock.Bind(new IPEndPoint(IPAddress.Any, port));
                    }
                    catch (SocketException)
                    {
                        return true;
                    }
                    finally
                    {
                        //Also invokes Dispose
                        sock.Close();
                    }
                }

                return false;

                bool TryGetPortProperty(out int port)
                {
                    const string propertiesfile = @".\Content\MinecraftServerFiles\server.properties";
                    if (File.Exists(propertiesfile))
                    {
                        using var f = new FileStream(propertiesfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        using var reader = new StreamReader(f);

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (line.Contains("server-port"))
                            {
                                var span = line.AsSpan();
                                var slice = span.Slice(line.IndexOf('=') + 1);

                                port = int.Parse(slice.ToString());
                                return true;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Couldn't find file \"server.properties\"");
                    }

                    port = -1;
                    return false;
                }
            }
        }

        private ProcessStartInfo ConfigureInfo()
        {
            var startinfo = new ProcessStartInfo
            {
                WorkingDirectory = @".\Content\MinecraftServerFiles",
                FileName = "cmd",
                Arguments = ConfigReader.GetConfigProperty("JavaArguments"),
                WindowStyle = ProcessWindowStyle.Normal,
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            return startinfo;
        }

        private bool HasServerProcessStarted()
        {
            //There are problaby dozens of way to determine this, but here we are just checking the if an id has been assigned by the system.
            try
            {
                var i = _ServerProcess.Id;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private void StartMonitoring(StreamReader stream)
        {
            _ServerStatus = ServerStatus.Loading;

            MessageToStatusConverter messageConverter = new MessageToStatusConverter();

            while (!_ServerProcess.HasExited)
            {
                if (_ServerStatus != ServerStatus.Online)
                {
                    //Note:  ReadLine returns null when the process exits while trying to read.
                    //Note: In the case the stream is a filestream, ReadLine returns null if there is nothing further to read, and thus WON'T block the thread -
                    //       significantly increasing CPU usage. Which is the main reason we only read until "Online"-status is achieved, and only if we can't access -
                    //       the standard output. (Reading a log that is already past its "done"-message is extremely fast on the other hand)

                    var r = stream.ReadLine();
                    messageConverter.CheckMessageForStatusUpdate(r, ref _ServerStatus);
                }
                else
                {
                    //We've already hit "Online" status, the only status-change left to happen is to "Offline", indicated by the process terminating
                    //We make a blocking call here to prevent the loop from eating CPU-power.
                    _logger.LogInformation("Server is online and ready");
                    _logger.LogDebug("Waiting for process exiting..");
                    _ServerProcess.WaitForExit();
                }
            }

            OnServerProcessExit();
        }

        private void OnServerProcessExit()
        {
            _ServerStatus = ServerStatus.Offline;

            _logger.LogInformation($"Server process has stopped. Exit time: {DateTime.Now}");

            //_ServerProcess.Dispose();
            _ServerProcess.Close();
            _ServerProcess = null;
        }

        private void WaitForLoggingStarted(string path)
        {
            FileInfo fi = new FileInfo(path);
            do
            {
                Thread.Sleep(500);
                fi.Refresh();

                //If the file exists and the last writetime is later than (after) starttime, it's the current log and we can go ahead with the reading
            } while (!fi.Exists && fi.LastWriteTime < _ServerProcess.StartTime);

            //NOTE: It seems like File.GetLastAccessTime or File.GetLastWriteTime actually puts a lock on the file-handle,
            //so if the process is starting up and preparing a new logfile, it wants to move the "latest.log" and rename it, delete "latest.log",
            //and then create a new "latest.log"-file
            //If we have a lock on the file, the logging framwork used (Log4J) by the minecraft process, will fail on moving/deleting, so the old
            //log is kept and appended to, creating scrambled logfiles.
            //But since we are only using log-monitoring if the process is already running when we try to start it, this is rarely an issue.
        }
    }
}