using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;

namespace Org.Mentalis.Files
{
public class IniReader
{
	private string m_Filename;

	private string m_Section;

	private const int MAX_ENTRY = 0x8000;

	public string Filename
	{
		get
		{
			return this.m_Filename;
		}
		set
		{
			this.m_Filename = value;
		}
	}

	public string Section
	{
		get
		{
			return this.m_Section;
		}
		set
		{
			this.m_Section = value;
		}
	}

	public IniReader(string file)
	{
		this.Filename = file;
	}

	public bool DeleteKey(string section, string key)
	{
		return IniReader.WritePrivateProfileString(section, key, null, this.Filename) != 0;
	}

	public bool DeleteKey(string key)
	{
		return IniReader.WritePrivateProfileString(this.Section, key, null, this.Filename) != 0;
	}

	public bool DeleteSection(string section)
	{
		return IniReader.WritePrivateProfileSection(section, null, this.Filename) != 0;
	}

	[DllImport("KERNEL32.DLL", CharSet=CharSet.Ansi)]
	private static extern int GetPrivateProfileInt(string lpApplicationName, string lpKeyName, int nDefault, string lpFileName);

	[DllImport("KERNEL32.DLL", CharSet=CharSet.Ansi)]
	private static extern int GetPrivateProfileSectionNames(byte[] lpszReturnBuffer, int nSize, string lpFileName);

	[DllImport("KERNEL32.DLL", CharSet=CharSet.Ansi)]
	private static extern int GetPrivateProfileString(string lpApplicationName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

	public ArrayList GetSectionNames()
	{
		ArrayList arrayLists;
		try
		{
			byte[] numArray = new byte[0x8000];
			IniReader.GetPrivateProfileSectionNames(numArray, 0x8000, this.Filename);
			char[] chrArray = new char[1];
			char[] chrArray1 = new char[1];
			string[] strArrays = Encoding.ASCII.GetString(numArray).Trim(chrArray).Split(chrArray1);
			arrayLists = new ArrayList(strArrays);
		}
		catch
		{
			return null;
		}
		return arrayLists;
	}

	public bool ReadBoolean(string section, string key, bool defVal)
	{
		return bool.Parse(this.ReadString(section, key, defVal.ToString()));
	}

	public bool ReadBoolean(string section, string key)
	{
		return this.ReadBoolean(section, key, false);
	}

	public bool ReadBoolean(string key, bool defVal)
	{
		return this.ReadBoolean(this.Section, key, defVal);
	}

	public bool ReadBoolean(string key)
	{
		return this.ReadBoolean(this.Section, key);
	}

	public byte[] ReadByteArray(string section, string key)
	{
		byte[] numArray;
		try
		{
			numArray = Convert.FromBase64String(this.ReadString(section, key));
		}
		catch
		{
			return null;
		}
		return numArray;
	}

	public byte[] ReadByteArray(string key)
	{
		return this.ReadByteArray(this.Section, key);
	}

	public int ReadInteger(string section, string key, int defVal)
	{
		return IniReader.GetPrivateProfileInt(section, key, defVal, this.Filename);
	}

	public int ReadInteger(string section, string key)
	{
		return this.ReadInteger(section, key, 0);
	}

	public int ReadInteger(string key, int defVal)
	{
		return this.ReadInteger(this.Section, key, defVal);
	}

	public int ReadInteger(string key)
	{
		return this.ReadInteger(key, 0);
	}

	public long ReadLong(string section, string key, long defVal)
	{
		return long.Parse(this.ReadString(section, key, defVal.ToString()));
	}

	public long ReadLong(string section, string key)
	{
		return this.ReadLong(section, key, (long)0);
	}

	public long ReadLong(string key, long defVal)
	{
		return this.ReadLong(this.Section, key, defVal);
	}

	public long ReadLong(string key)
	{
		return this.ReadLong(key, (long)0);
	}

	public string ReadString(string section, string key, string defVal)
	{
		StringBuilder stringBuilder = new StringBuilder(0x8000);
		IniReader.GetPrivateProfileString(section, key, defVal, stringBuilder, 0x8000, this.Filename);
		return stringBuilder.ToString();
	}

	public string ReadString(string section, string key)
	{
		return this.ReadString(section, key, "");
	}

	public string ReadString(string key)
	{
		return this.ReadString(this.Section, key);
	}

	public bool Write(string section, string key, int value)
	{
		return this.Write(section, key, value.ToString());
	}

	public bool Write(string key, int value)
	{
		return this.Write(this.Section, key, value);
	}

	public bool Write(string section, string key, string value)
	{
		return IniReader.WritePrivateProfileString(section, key, value, this.Filename) != 0;
	}

	public bool Write(string key, string value)
	{
		return this.Write(this.Section, key, value);
	}

	public bool Write(string section, string key, long value)
	{
		return this.Write(section, key, value.ToString());
	}

	public bool Write(string key, long value)
	{
		return this.Write(this.Section, key, value);
	}

	public bool Write(string section, string key, byte[] value)
	{
		if (value != null)
		{
			return this.Write(section, key, value, 0, (int)value.Length);
		}
		else
		{
			return this.Write(section, key, string.Empty);
		}
	}

	public bool Write(string key, byte[] value)
	{
		return this.Write(this.Section, key, value);
	}

	public bool Write(string section, string key, byte[] value, int offset, int length)
	{
		if (value != null)
		{
			return this.Write(section, key, Convert.ToBase64String(value, offset, length));
		}
		else
		{
			return this.Write(section, key, string.Empty);
		}
	}

	public bool Write(string section, string key, bool value)
	{
		return this.Write(section, key, value.ToString());
	}

	public bool Write(string key, bool value)
	{
		return this.Write(this.Section, key, value);
	}

	[DllImport("KERNEL32.DLL", CharSet=CharSet.Ansi)]
	private static extern int WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

	[DllImport("KERNEL32.DLL", CharSet=CharSet.Ansi)]
	private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
}
}