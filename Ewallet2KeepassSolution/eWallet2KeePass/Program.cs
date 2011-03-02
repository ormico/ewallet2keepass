using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace ZacksFiasco.eWallet2KeePass
{
	class Program
	{
		static void Main(string[] args)
		{
			string inFileName = "ewallet.txt";
			string outFileName = "KeePass.kdbx";

			// check commandline
			if (args.Length > 0)
			{
				inFileName = args[0];

				if (args.Length > 1)
				{
					outFileName = args[1];
				}
			}

			if (ReadEWalletFile(inFileName))
			{
				WriteKeePassFile(outFileName);
			}
			else
			{
				Console.WriteLine("Unable to open input file '{0}'", inFileName);
			}
		}

		static CardGroup _root = null;

		static bool ReadEWalletFile(string filePath)
		{
			CardGroup root = new CardGroup();
			CardGroup current = root;
			Card card = null;

			bool rc = false;

			try
			{
				using (StreamReader sr = File.OpenText(filePath))
				{
					while (sr.EndOfStream == false)
					{
						string line = sr.ReadLine();

						if (string.IsNullOrWhiteSpace(line))
						{
							// new card
							// need to guard against adding empty cards
							if (card != null &&
								(
								card.Name != null ||
								card.UserName != null ||
								card.Password != null ||
								card.Url != null ||
								card.PublicKey != null ||
								card.PrivateKey != null ||
								card.Notes != null
								)
							)
							{
								current.Entries.Add(card);
							}
							card = new Card();
						}
						else if (line.StartsWith("Category:"))
						{
							current = new CardGroup();
							current.Name = line.Substring(9).Trim();

							root.Entries.Add(current);
						}
						else if (line.StartsWith("Card Name"))
						{
							card.Name = line.Substring(9).Trim();
						}
						else if (line.StartsWith("Card Type"))
						{
							card.CardType = line.Substring(9).Trim();
						}
						else if (line.StartsWith("URL"))
						{
							card.Url = line.Substring(3).Trim();
						}
						//else if (line.StartsWith("Site Name"))
						//{
						//    card.Url = line.Substring(9).Trim();
						//}
						else if (line.StartsWith("User Name"))
						{
							card.UserName = line.Substring(9).Trim();
						}
						else if (line.StartsWith("Public Key"))
						{
							card.PublicKey = line.Substring(10).Trim();
						}
						else if (line.StartsWith("Private Key"))
						{
							card.PrivateKey = line.Substring(11).Trim();
						}
						else if (line.StartsWith("Password"))
						{
							card.Password = line.Substring(8).Trim();
						}
						else
						{
							StringBuilder sb;
							if (card.Notes == null)
							{
								sb = new StringBuilder();
							}
							else
							{
								sb = new StringBuilder(card.Notes);
							}

							sb.AppendLine(line);
							card.Notes = sb.ToString();
						}
					}
				}

				_root = root;

				rc = true;
			}
			catch (FileNotFoundException)
			{
				rc = false;
			}

			return rc;
		}

		static void WriteKeePassFile(string filePath)
		{
			XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
			XElement xKeyPassFile = new XElement("KeePassFile");
			XElement xRoot = new XElement("Root");
			XElement xGroup = new XElement("Group",
				new XElement("UUID", "Pysc/OPNvEOBrCYOGuQJ8g=="), 
				new XElement("Name", "Passwords"),
				new XElement("Notes"));

			xDoc.Add(xKeyPassFile);
			xKeyPassFile.Add(xRoot);
			xRoot.Add(xGroup);

			List<IEntry> entries = _root.Entries;

			AddEntriesToXml(entries, xGroup);

			MemoryStream ms = new MemoryStream();
			var docXmlWriter = new System.Xml.XmlTextWriter(ms, System.Text.Encoding.UTF8);
			docXmlWriter.Formatting = System.Xml.Formatting.Indented;
			xDoc.WriteTo(docXmlWriter);
			docXmlWriter.Flush();
			ms.Flush();

			ms.Seek(0, SeekOrigin.Begin);

			StreamReader sr = new StreamReader(ms);
			string xmlResult = sr.ReadToEnd();

			File.WriteAllText(filePath, xmlResult);
		}

		private static void AddEntriesToXml(List<IEntry> entries, XElement xTop)
		{
			foreach (var i in entries)
			{
				if (i is CardGroup)
				{
					CardGroup cg = (CardGroup)i;
					XElement xGroup = new XElement("Group", 
						new XElement("UUID", CreateUUID()),
						new XElement("Name", cg.Name)
						);
					xTop.Add(xGroup);

					AddEntriesToXml(cg.Entries, xGroup);
				}
				else
				{
					Card c = (Card)i;
					XElement xCard = new XElement("Entry",
						new XElement("UUID", CreateUUID()),
						new XElement("String", new XElement("Key", "Notes"), new XElement("Value", c.Notes)),
						new XElement("String", new XElement("Key", "Password"), new XElement("Value", c.Password)),
						new XElement("String", new XElement("Key", "Title"), new XElement("Value", c.Name)),
						new XElement("String", new XElement("Key", "URL"), new XElement("Value", c.Url)),
						new XElement("String", new XElement("Key", "UserName"), new XElement("Value", c.UserName))
					);

					if (string.IsNullOrWhiteSpace(c.PublicKey) == false)
					{
						xCard.Add(new XElement("String", new XElement("Key", "PublicKey"), new XElement("Value", c.PublicKey)));
					}

					if (string.IsNullOrWhiteSpace(c.PrivateKey) == false)
					{
						xCard.Add(new XElement("String", new XElement("Key", "PrivateKey"), new XElement("Value", c.PrivateKey)));
					}

					XElement xAutoType = new XElement("AutoType",
						new XElement("Enabled", "true"),
						new XElement("DataTransferObfuscation", 0),
						new XElement("Association",
							new XElement("Window", "Target Window"),
							new XElement("KeystrokeSequence", "{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}")	
								),
						new XElement("History")
						);
					xCard.Add(xAutoType);

					xTop.Add(xCard);
				}
			}
		}

		static string CreateUUID()
		{
			return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
		}
	}

	interface IEntry
	{
		string Name { get; set; }
	}

	class CardGroup: IEntry	
	{
		public CardGroup()
		{
			this.Entries = new List<IEntry>();
		}

		public string Name { get; set; }
		public List<IEntry> Entries { get; protected set; }
	}

	class Card: IEntry 
	{
		public string Name { get; set; }

		public string UserName { get; set; }
		public string Password { get; set; }
		public string Url { get; set; }
		public string Notes { get; set; }
		public string CardType { get; set; }
		public string PublicKey { get; set; }
		public string PrivateKey { get; set; }
	}
}
