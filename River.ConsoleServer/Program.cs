﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using River.Http;
using River.ShadowSocks;
using River.Socks;

namespace River.ConsoleServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var cli = new ShadowSocksClient("abc");
			cli.Plug("RHOP2", 8338);
			cli.Route("10.7.1.1", 8338);
			var cli2 = new ShadowSocksClient("def");
			cli2.Plug(cli);
			cli2.Route("10.7.0.1", 80);

			cli2.Write(Encoding.ASCII.GetBytes("GET /\r\n\r\n"));
			var buf = new byte[1024 * 1024];
			var c = cli2.Read(buf, 0, buf.Length);
			var str = Encoding.UTF8.GetString(buf, 0, c);
			Console.WriteLine(str);
			if (str.Contains("THIS IS SUPER PRIVATE SITE"))
			{
				Console.WriteLine("THIS WORKS!!!");
			}
			Console.ReadLine();
		}

		static void Main22(string[] args)
		{
			var cli1 = new ShadowSocksClient("pwd");
			cli1.Plug("127.0.0.1", 8338);
			cli1.Route("httpbin.org", 80);

			// var cli2 = new Socks4Client();
			// cli2.Plug("127.0.0.1", 1079);
			// cli2.Route("httpbin.org", 80);

			var cli = cli1;

			void read()
			{
				var buf = new byte[1024 * 1024];
				var c = cli.Read(buf, 0, buf.Length);
				if (c > 0)
				{
					var str = Encoding.UTF8.GetString(buf, 0, c);
					Console.WriteLine(str);
					Console.WriteLine("\r\n==========");
					Task.Run(delegate
					{
						read();
					});
				}
			}
			Task.Run(delegate
			{
				read();
			});


			

			cli.Write(Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
Host: httpbin.org
Keep-Alive: true

"));


			Thread.Sleep(1000);
			Console.WriteLine("============");
			Console.ReadLine();

			cli.Write(Encoding.ASCII.GetBytes(@"GET / HTTP/1.1
Host: httpbin.org

"));

			Console.ReadLine();
		}

		static void Main2(string[] args)
		{
			// FireFox => SocksServer => RiverClient => Fiddler => RiverServer => Internet

			// Trace.Listeners.Add(new ConsoleTraceListener());

			
			var server = new SocksServer(new ServerConfig
			{
				EndPoints =
				{
					new IPEndPoint(IPAddress.IPv6Loopback, 1080),
					new IPEndPoint(IPAddress.Loopback, 1080),
				},
			})
			{
				Forwarder = new SocksForwarder("RHOP2", 1080)
				{
					NextForwarder = new SocksForwarder("10.7.1.1", 1080)
					{

					}
				}
			};
			

			// connect to this server and ask super secret web site behind 2 private lan
			var cli = new Socks4Client("127.0.0.1", 1080, "10.7.0.1", 80);
			var req = Encoding.ASCII.GetBytes("GET /\r\n\r\n");
			cli.Write(req);
			var buf = new byte[1024 * 1024];
			var c = cli.Read(buf, 0, buf.Length);

			var str = Encoding.UTF8.GetString(buf, 0, c);
			Console.WriteLine(str);
			if (str.Contains("THIS IS SUPER PRIVATE SITE"))
			{
				Console.WriteLine("THIS WORKS!!!");
			}


			Console.ReadLine();
		}
	}
}
