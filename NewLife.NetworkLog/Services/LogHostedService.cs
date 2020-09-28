using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NewLife.NetworkLog.Entities;
using NewLife.NetworkLog.Models;

namespace NewLife.NetworkLog.Services
{
    public class LogHostedService : IHostedService
    {
        private UdpClient _udpClient;
        private Dictionary<string, ProjectInfo> _projectInfos;
        private readonly IHubContext<LogHub> _hubContext;

        public LogHostedService(Dictionary<string, ProjectInfo> projectInfos, IHubContext<LogHub> hubContext)
        {
            _projectInfos = projectInfos;
            _hubContext = hubContext;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            LoadProjectInfo();
            GetLogAsync();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _udpClient.Dispose();
            return Task.CompletedTask;
        }

        private void GetLogAsync()
        {
            var localEp = new IPEndPoint(IPAddress.Any, 514);
            _udpClient = new UdpClient(localEp);

            Task.Factory.StartNew(Receive);
        }

        private async Task Receive()
        {
            IPEndPoint remoteEP = null;
            while (true)
            {
                var buffer = _udpClient.Receive(ref remoteEP);

                var str = Encoding.UTF8.GetString(buffer);

                var url = remoteEP.ToString();

                CheckProjectInfo(url, str);

                var log = SaveLog(url, str);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", remoteEP.ToString(), log);
            }
        }

        private void LoadProjectInfo()
        {
            // 如果数据库存在相同地址信息，则返回数据库记录
            var projectInfoList = ProjectInfo.Meta.Cache.Entities;
            foreach (var p in projectInfoList)
            {
                _projectInfos[p.IP] = p;
            }
        }

        private void CheckProjectInfo(string url, string str)
        {
            //lock (_projectInfos)
            {
                // 地址已存在则返回
                if (_projectInfos.TryGetValue(url, out var projectInfo))
                {
                    projectInfo.UpdateTime = DateTime.Now;
                    projectInfo.Save();
                    return;
                }

                var name = url;

                // 数据库记录不存在相同地址信息，解析日志头获取项目信息
                // 除了IIS托管的FX项目，其余项目类型均不是“#Software: w3wp”，特殊处理
                var exp = str.Contains("#Software: w3wp") ?
                    @"#OS: [\w\W]+(?=,),\s+[^/]+/([\w\W]+?)(?=\r\n)\b\r\n#CPU" :
                    @"#Software: ([\w\W]+?)(?=\r\n)\b\r\n#ProcessID";

                var m = Regex.Match(str, exp);
                if (m.Success)
                {
                    name = m.Groups[1].Value;
                    var p = _projectInfos.Values.FirstOrDefault(f => f.Name == name);
                    if (p != null)
                    {
                        _projectInfos.Remove(p.IP);
                    }
                }

                projectInfo = ProjectInfo.FindByName(name);
                if (projectInfo != null)
                {
                    projectInfo.IP = url;
                }
                else
                {
                    projectInfo = new ProjectInfo { Name = name, IP = url, AddTime = DateTime.Now };
                }

                projectInfo.UpdateTime = DateTime.Now;
                projectInfo.Save();

                _projectInfos[url] = projectInfo;
            }
        }

        private ProjectLog SaveLog(string url, string str)
        {
            var p = ProjectInfo.FindByIP(url);

            var log = new ProjectLog
            {
                ProjectID = p.ID,
            };

            var m = Regex.Match(str, @"^(\d{2}:\d{2}:\d{2}\.\d{3})\s+(\d+)\s([W|Y|N])\s+([^\s]+?)\s([\w\W]+)$");
            var now = DateTime.Now;

            if (m.Success)
            {
                log.Time = now.Date.Add(TimeSpan.Parse(m.Groups[1].Value)).Ticks;
                log.ThreadID = m.Groups[2].Value.ToInt();
                log.ThreadType = (ThreadType)Enum.Parse(typeof(ThreadType), m.Groups[3].Value);
                log.ThreadName = m.Groups[4].Value;
                log.Message = m.Groups[5].Value;
            }
            else
            {
                log.Time = now.Ticks;
                log.ThreadType = ThreadType.Y;
                log.ThreadName = "-";
                log.Message = str;
            }

            log.Insert();

            return log;
        }
    }
}
