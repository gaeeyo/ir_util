using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace IrUtilApp {
    class IrUtil {

        public static void Log(string message) {
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} {message}");
        }

        public static List<RemoconData> LoadRemoconDataList(string filename) {
            var result = new List<RemoconData>();
            StreamReader reader = File.OpenText(filename);
            try {
                string line;
                int lineNumber = -1;
                while ((line = reader.ReadLine()) != null) {
                    lineNumber++;
                    MatchCollection matches = Regex.Matches(line, "\"(.*?)\"");
                    if (matches.Count > 3) {
                        const int dataStart = 3;
                        string name = matches[0].Groups[1].Value;
                        uint freq = uint.Parse(matches[1].Groups[1].Value);
                        uint dataSize = uint.Parse(matches[2].Groups[1].Value);
                        byte[] data = new byte[(matches.Count - dataStart) * 2];
                        int idx = 0;
                        for (int j = dataStart; j < matches.Count; j++) {
                            string valueStr = matches[j].Groups[1].Value;
                            ushort value;
                            if (valueStr.StartsWith("0x")) {
                                value = ushort.Parse(valueStr.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier);
                            } else {
                                value = ushort.Parse(valueStr);
                            }
                            data[idx++] = (byte)((value >> 8) & 0xff);
                            data[idx++] = (byte)(value & 0xff);
                        }
                        result.Add(new RemoconData(name, freq, dataSize, data));
                        Console.WriteLine($"登録({filename}:{lineNumber}): {name}");
                    } else {
                        Console.WriteLine($"無視({filename}:{lineNumber}): {line}");
                    }
                }
            } finally {
                reader.Close();
            }
            return result;
        }


        string remoconDataFilePath;
        int listenPort;
        IEnumerable<RemoconData> remoconDataList;


        public IrUtil(int listenPort, string remoconDataFilePath) {
            this.listenPort = listenPort;
            this.remoconDataFilePath = Path.Combine(Directory.GetCurrentDirectory(), remoconDataFilePath);
        }

        void LoadRemoconDataList() {
            remoconDataList = LoadRemoconDataList(remoconDataFilePath);
        }

        RemoconData QueryToCommand(string query) {
            return (from item in remoconDataList
                    where Regex.IsMatch(query, item.Name)
                    select item)
                .FirstOrDefault();
        }

        void WriteResponse(HttpListenerContext context, int statusCode, string message) {
            Log($" => {statusCode} {message}");

            HttpListenerResponse res = context.Response;
            res.ContentType = "text/plain; charset=utf-8";
            res.StatusCode = statusCode;

            byte[] content = Encoding.UTF8.GetBytes(message);
            res.OutputStream.Write(content, 0, content.Length);
            res.Close();
        }

        void HandleRequest(HttpListenerContext context) {
            Log($"{context.Request.HttpMethod} {context.Request.Url}");
            string query = context.Request.Url.ToString();
            RemoconData command = QueryToCommand(query);
            if (command == null) {
                WriteResponse(context, 200, "OK");
                return;
            }
            command.Execute();
            WriteResponse(context, 200, $"query: {query}, command: {command}");
        }

        void OnRemoconDataChanged(object source, FileSystemEventArgs args) {
            if (remoconDataFilePath == args.FullPath) {
                try {
                    LoadRemoconDataList();
                } catch (IOException e) {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void Start() {
            Log($"Port:{listenPort}");
            LoadRemoconDataList();

            var fsw = new FileSystemWatcher();

            var absPath = Path.Combine(Directory.GetCurrentDirectory(), remoconDataFilePath);

            fsw.Path += Directory.GetParent(absPath);
            fsw.Changed += new FileSystemEventHandler(OnRemoconDataChanged);
            fsw.EnableRaisingEvents = true;

            try {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add($"http://+:{listenPort}/");
                listener.Start();
                while (true) {
                    HandleRequest(listener.GetContext());
                }
            } finally {
                fsw.EnableRaisingEvents = false;
            }

        }
    }
}
