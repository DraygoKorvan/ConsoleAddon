using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;
using VRage.Plugins;
using VRage.ModAPI;
using Sandbox.Engine.Multiplayer;

namespace ConsoleInput
{
	public class SEConsole : IDisposable, IPlugin
	{
		Thread m_chatHandleThread;
		bool m_running = false;
		StreamWriter m_out;
		MemoryStream m_stream;


		private void mainloop()
		{

			var outStream = Console.Out;
			ConsoleKeyInfo ckey = new ConsoleKeyInfo();
			int m_history = 0;
			int m_byte = 0;
			List<string> history = new List<string>();
			string emptyline = "".PadRight(Console.WindowWidth -1);
			string input = "";
			while (m_running)
			{
				if(Console.KeyAvailable == true)
				{
					
					//Console.Write(m_byte.ToString());
					Console.SetOut(m_out);
					do
					{
						ckey = Console.ReadKey();
						
						if (m_byte == 10) break;
						switch (ckey.Key)
						{
							case ConsoleKey.UpArrow:
								m_history--;
								if (m_history <= 0)
									m_history = history.Count;
								input = "";
								Console.SetOut(outStream);
								Console.SetCursorPosition(0, Console.CursorTop);
								Console.Write(emptyline);
								Console.SetCursorPosition(0, Console.CursorTop);
								if(m_history-1 >= 0)
									input = history.ElementAt(m_history-1);
								Console.Write(input);
								Console.SetOut(m_out);
								break;

							case ConsoleKey.DownArrow:
								m_history++;
								if (m_history > history.Count)
									m_history = 1;
								input = "";
								Console.SetOut(outStream);
								Console.SetCursorPosition(0, Console.CursorTop);
								Console.Write(emptyline);
								Console.SetCursorPosition(0, Console.CursorTop);
								if (m_history - 1 >= 0 && history.Count > 0)
									input = history.ElementAt(m_history - 1);
								Console.Write(input);
								Console.SetOut(m_out);
								break;
							case ConsoleKey.Backspace:
								if (input.Length > 0)
								{
									input = input.Remove(input.Length - 1);
									Console.SetOut(outStream);
									Console.SetCursorPosition(0, Console.CursorTop);
									Console.Write(emptyline);
									Console.SetCursorPosition(0, Console.CursorTop);
									Console.Write(input);
									Console.SetOut(m_out);
								}
								break;
							case ConsoleKey.Enter:
								Console.SetOut(outStream);
								Console.SetCursorPosition(0, Console.CursorTop);
								Console.Write(emptyline);
								Console.SetCursorPosition(0, Console.CursorTop);
								Console.SetOut(m_out);
								break;
							default:
								
								input += ckey.KeyChar;
								outStream.Write(ckey.KeyChar);
								break;

						}
					}
					while (ckey.Key != ConsoleKey.Enter || input.Length == 0);

					Console.SetOut(outStream);
					flushCache();
					if (input.Length > 0)
					{
						Console.SetCursorPosition(0, Console.CursorTop);
						Console.Write(emptyline);
						Console.SetCursorPosition(0, Console.CursorTop);
						input = input.Trim();
						if( Sandbox.MySandboxGame.IsGameReady)
							MyMultiplayer.Static.SendChatMessage("Server: " + input);
						//ChatManager.Instance.SendPublicChatMessage(input);
						history.Add(input);
						m_history = history.Count;
					}
						
					input = "";
				}
	
				Thread.Sleep(250);
			}
		}
		public void flushCache()
		{
			m_out.Flush();
			string text = "";
			int m_byte = 0;
			m_out.BaseStream.Position = 0;
			while ((m_byte = m_out.BaseStream.ReadByte()) != -1)
			{
				if (m_byte == 13) continue;
				if( m_byte == 10)
				{
					Console.WriteLine(text);
					text = "";
					continue;
				}
				text += (char)m_byte;
			}
			if (text != "")
				Console.WriteLine(text);
			m_out = new StreamWriter(new MemoryStream());
		}


		public void Dispose()
		{
			m_running = false;
			m_chatHandleThread.Abort();
		}

		public void Init(object gameInstance)
		{
			Console.WriteLine("Initializing Console Additions ....");
			if (m_chatHandleThread != null)
				m_chatHandleThread.Abort();
			m_running = true;
			m_stream = new MemoryStream();
			m_out = new StreamWriter(m_stream);
			m_chatHandleThread = new Thread(mainloop);
			m_chatHandleThread.Priority = ThreadPriority.BelowNormal;
			m_chatHandleThread.Start();
		}

		public void Update()
		{
			
		}
	}
}
