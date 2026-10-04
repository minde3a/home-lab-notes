using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

/// <summary>
/// Windows wrapper: closing the console (X / Ctrl+C / logoff) must not kill the
/// child process with a hard terminate. Instead we send a localhost admin command
/// so the child can persist state, then wait for a clean exit.
///
/// Usage:
///   ConsoleGuard --port 12345 --pass SECRET --cmd "shutdown 1" -- java.exe -jar app.jar
///
/// Password is a CLI argument so it is not hardcoded. Bind the admin listener to
/// 127.0.0.1 only.
/// </summary>
internal static class ConsoleGuard
{
	private const int CTRL_C = 0;
	private const int CTRL_BREAK = 1;
	private const int CTRL_CLOSE = 2;
	private const int CTRL_LOGOFF = 5;
	private const int CTRL_SHUTDOWN = 6;

	private static Process _child;
	private static int _port;
	private static string _pass = "";
	private static string _cmd = "shutdown 1";
	private static volatile bool _closing;

	private delegate bool Handler(int sig);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleCtrlHandler(Handler h, bool add);

	private static readonly Handler _handler = OnCtrl;

	private static int Main(string[] args)
	{
		int i = 0;
		while (i < args.Length)
		{
			string a = args[i];
			if (a == "--port" && i + 1 < args.Length) { _port = int.Parse(args[++i]); }
			else if (a == "--pass" && i + 1 < args.Length) { _pass = args[++i]; }
			else if (a == "--cmd" && i + 1 < args.Length) { _cmd = args[++i]; }
			else if (a == "--") { i++; break; }
			else { break; }
			i++;
		}
		if (i >= args.Length)
		{
			Console.WriteLine("ConsoleGuard --port N --pass P --cmd \"shutdown 1\" -- child args...");
			return 1;
		}

		string file = args[i];
		string childArgs = "";
		if (i + 1 < args.Length)
			childArgs = QuoteArgs(args, i + 1);

		SetConsoleCtrlHandler(_handler, true);
		Console.WriteLine("Close this window with X for a graceful shutdown.");

		int code = 0;
		while (true)
		{
			_child = StartChild(file, childArgs);
			_child.WaitForExit();
			code = _child.ExitCode;
			_child.Dispose();
			_child = null;
			if (_closing) break;
			// Child exit codes 2/5/6 mean "please restart me".
			if (code == 2 || code == 5 || code == 6)
			{
				Console.WriteLine("Restart requested (" + code + ").");
				Thread.Sleep(2000);
				continue;
			}
			break;
		}
		return code;
	}

	private static Process StartChild(string file, string arguments)
	{
		ProcessStartInfo psi = new ProcessStartInfo();
		psi.FileName = file;
		psi.Arguments = arguments;
		psi.UseShellExecute = false;
		psi.RedirectStandardOutput = true;
		psi.RedirectStandardError = true;
		psi.RedirectStandardInput = false;
		psi.CreateNoWindow = true;
		psi.WorkingDirectory = Environment.CurrentDirectory;
		Process p = Process.Start(psi);
		p.OutputDataReceived += delegate(object s, DataReceivedEventArgs e)
		{
			if (e.Data != null) Console.WriteLine(e.Data);
		};
		p.ErrorDataReceived += delegate(object s, DataReceivedEventArgs e)
		{
			if (e.Data != null) Console.Error.WriteLine(e.Data);
		};
		p.BeginOutputReadLine();
		p.BeginErrorReadLine();
		return p;
	}

	private static bool OnCtrl(int sig)
	{
		if (sig != CTRL_C && sig != CTRL_BREAK && sig != CTRL_CLOSE && sig != CTRL_LOGOFF && sig != CTRL_SHUTDOWN)
			return false;
		if (_closing) return true;
		_closing = true;
		Console.WriteLine("");
		Console.WriteLine("Window closing — requesting graceful shutdown...");
		try { SendLocalAdmin(); }
		catch (Exception ex) { Console.WriteLine("Local admin shutdown failed: " + ex.Message); }
		try
		{
			if (_child != null && !_child.HasExited)
				_child.WaitForExit(180000);
		}
		catch { }
		return true;
	}

	private static void SendLocalAdmin()
	{
		if (_port <= 0) return;
		using (TcpClient c = new TcpClient())
		{
			c.ReceiveTimeout = 8000;
			c.SendTimeout = 8000;
			IAsyncResult ar = c.BeginConnect("127.0.0.1", _port, null, null);
			if (!ar.AsyncWaitHandle.WaitOne(4000, false) || !c.Connected)
				throw new IOException("admin not listening on " + _port);
			NetworkStream ns = c.GetStream();
			StreamReader r = new StreamReader(ns, Encoding.ASCII);
			StreamWriter w = new StreamWriter(ns, Encoding.ASCII);
			w.NewLine = "\r\n";
			w.AutoFlush = true;
			string acc = "";
			DateTime until = DateTime.Now.AddSeconds(6);
			while (DateTime.Now < until)
			{
				if (!ns.DataAvailable) { Thread.Sleep(50); continue; }
				char[] buf = new char[256];
				int n = r.Read(buf, 0, buf.Length);
				if (n <= 0) break;
				acc += new string(buf, 0, n);
				if (acc.IndexOf("Password", StringComparison.OrdinalIgnoreCase) >= 0)
					break;
			}
			if (_pass.Length > 0)
				w.WriteLine(_pass);
			Thread.Sleep(200);
			w.WriteLine(_cmd);
			Thread.Sleep(300);
		}
	}

	private static string QuoteArgs(string[] args, int from)
	{
		StringBuilder sb = new StringBuilder();
		for (int i = from; i < args.Length; i++)
		{
			if (sb.Length > 0) sb.Append(' ');
			string a = args[i];
			if (a.IndexOf(' ') >= 0 || a.IndexOf('\t') >= 0)
				sb.Append('"').Append(a).Append('"');
			else
				sb.Append(a);
		}
		return sb.ToString();
	}
}
