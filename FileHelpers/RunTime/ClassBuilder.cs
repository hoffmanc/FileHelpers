using System;
using System.Collections;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Xml;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

namespace FileHelpers.RunTime
{
	
	//-> ADD: Sealed !!!
	//-> ADD: Visibility !!!
	
	//-> REGIONS !!!!

	/// <summary>The MAIN class to work with runtime defined records.</summary>
    public abstract class ClassBuilder
    {
		//---------------------
		//->  STATIC METHODS

    	#region LoadFromString

		/// <summary>Compiles the source code passed and returns the FIRST Type of the assembly. (Code in C#)</summary>
		/// <param name="classStr">The Source Code of the class in C#</param>
		/// <returns>The Type generated by runtime compilation of the class source.</returns>
		public static Type ClassFromString(string classStr)
		{
			return ClassFromString(classStr, string.Empty);
		}

		/// <summary>Compiles the source code passed and returns the FIRST Type of the assembly.</summary>
		/// <param name="classStr">The Source Code of the class in the specified language</param>
		/// <returns>The Type generated by runtime compilation of the class source.</returns>
		/// <param name="lang">One of the .NET Languages</param>
		public static Type ClassFromString(string classStr, NetLanguage lang)
		{
			return ClassFromString(classStr, string.Empty, lang);
		}

		/// <summary>Compiles the source code passed and returns the Type with the name className. (Code in C#)</summary>
		/// <param name="classStr">The Source Code of the class in C#</param>
		/// <param name="className">The Name of the Type that must be returned</param>
		/// <returns>The Type generated by runtime compilation of the class source.</returns>
		public static Type ClassFromString(string classStr, string className)
		{
			return ClassFromString(classStr, className, NetLanguage.CSharp);
		}
		
		/// <summary>Compiles the source code passed and returns the Type with the name className.</summary>
		/// <param name="classStr">The Source Code of the class in the specified language</param>
		/// <param name="className">The Name of the Type that must be returned</param>
		/// <returns>The Type generated by runtime compilation of the class source.</returns>
		/// <param name="lang">One of the .NET Languages</param>
		public static Type ClassFromString(string classStr, string className, NetLanguage lang)
		{
			ICodeCompiler comp = null;

			switch(lang)
			{
				case NetLanguage.CSharp:
					comp = (new CSharpCodeProvider()).CreateCompiler();
					break;

				case NetLanguage.VbNet:
					comp = (new VBCodeProvider()).CreateCompiler();
					break;
			}

			CompilerParameters cp = new CompilerParameters();
			cp.ReferencedAssemblies.Add("system.dll");
			cp.ReferencedAssemblies.Add("system.data.dll");
			cp.ReferencedAssemblies.Add(typeof(ClassBuilder).Assembly.GetModules()[0].FullyQualifiedName);
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;

			StringBuilder code = new StringBuilder();

			switch(lang)
			{
				case NetLanguage.CSharp:
					code.Append("using System; \n");
					code.Append("using FileHelpers; \n");
					code.Append("using System.Data; \n\n");
					break;

				case NetLanguage.VbNet:
					code.Append("Imports System \n");
					code.Append("Imports FileHelpers \n");
					code.Append("Imports System.Data \n\n");
					break;
			}

			code.Append(classStr);

			CompilerResults cr = comp.CompileAssemblyFromSource(cp, code.ToString());

			if (cr.Errors.HasErrors)
			{
				StringBuilder error = new StringBuilder();
				error.Append("Error Compiling Expression: " + StringHelper.NewLine);
				foreach (CompilerError err in cr.Errors)
				{
					error.AppendFormat("Line {0}: {1}\n", err.Line, err.ErrorText);
				}
				throw new RunTimeCompilationException(error.ToString(), classStr, cr.Errors);
			}
            
			//            Assembly.Load(cr.CompiledAssembly.);
			if (className != string.Empty)
				return cr.CompiledAssembly.GetType(className, true, true);
			else
			{
				Type[] ts = cr.CompiledAssembly.GetTypes();
				if (ts.Length > 0)
					foreach (Type t in ts)
					{
						if (t.FullName.StartsWith("My.My") == false)
							return t;
					}

				throw new BadUsageException("The Compiled assembly don�t have any Type inside.");
			}
		}

		#endregion

		#region CreateFromFile

		/// <summary>
		/// Create a class from a source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromSourceFile(string filename)
		{
			return ClassFromSourceFile(filename, string.Empty);
		}

		/// <summary>
		/// Create a class from a source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <param name="lang">The languaje used to compile the class.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromSourceFile(string filename, NetLanguage lang)
		{
			return ClassFromSourceFile(filename, string.Empty, lang);
		}

		/// <summary>
		/// Create a class from a source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <param name="className">The name of the class to return.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromSourceFile(string filename, string className)
		{
			return ClassFromSourceFile(filename, className, NetLanguage.CSharp);
		}

		/// <summary>
		/// Create a class from a source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <param name="className">The name of the class to return.</param>
		/// <param name="lang">The languaje used to compile the class.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromSourceFile(string filename, string className, NetLanguage lang)
		{
			StreamReader reader = new StreamReader(filename);
			string classDef = reader.ReadToEnd();
			reader.Close();

			return ClassFromString(classDef, className, lang);
		}



		/// <summary>
		/// Create a class from a encripted source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromBinaryFile(string filename)
		{
			return ClassFromBinaryFile(filename, string.Empty, NetLanguage.CSharp);
		}

		/// <summary>
		/// Create a class from a encripted source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <param name="lang">The languaje used to compile the class.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromBinaryFile(string filename, NetLanguage lang)
		{
			return ClassFromBinaryFile(filename, string.Empty, lang);
		}
    	
		/// <summary>
		/// Create a class from a encripted source file.
		/// </summary>
		/// <param name="filename">The filename with the source of the class.</param>
		/// <param name="lang">The languaje used to compile the class.</param>
		/// <param name="className">The name of the class to return.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromBinaryFile(string filename, string className, NetLanguage lang)
		{
			
			StreamReader reader = new StreamReader(filename);
			string classDef = reader.ReadToEnd();
			reader.Close();
			
			classDef = Decrypt(classDef, "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");

			return ClassFromString(classDef, className, lang);
		}


		/// <summary>
		/// Create a class from a Xml file generated with the Wizard or saved using the SaveToXml Method.
		/// </summary>
		/// <param name="filename">The filename with the Xml definition.</param>
		/// <returns>The compiled class.</returns>
		public static Type ClassFromXmlFile(string filename)
		{
			ClassBuilder cb = LoadFromXml(filename);
			return cb.CreateRecordClass();
		}

		/// <summary>
		/// Encript the class source code and write it to a file.
		/// </summary>
		/// <param name="filename">The file name to write to.</param>
		/// <param name="classSource">The source code for the class.</param>
		public static void ClassToBinaryFile(string filename, string classSource)
		{
			classSource = Encrypt(classSource, "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");
			
			StreamWriter writer = new StreamWriter(filename);
			writer.Write(classSource);
			writer.Close();
		}


		#endregion

		#region SaveToFile


		/// <summary>Write the source code of the current class to a file. (In C#)</summary>
		/// <param name="filename">The file to write to.</param>
		public void SaveToSourceFile(string filename)
		{
			SaveToSourceFile(filename, NetLanguage.CSharp);
		}

		/// <summary>Write the source code of the current class to a file. (In the especified language)</summary>
		/// <param name="filename">The file to write to.</param>
		/// <param name="lang">The .NET Language used to write the source code.</param>
		public void SaveToSourceFile(string filename, NetLanguage lang)
    	{
    		StreamWriter writer = new StreamWriter(filename);
    		writer.Write(GetClassSourceCode(lang));
    		writer.Close();
    	}

		/// <summary>Write the ENCRIPTED source code of the current class to a file. (In C#)</summary>
		/// <param name="filename">The file to write to.</param>
		public void SaveToBinaryFile(string filename)
		{
			SaveToBinaryFile(filename, NetLanguage.CSharp);
		}

		/// <summary>Write the ENCRIPTED source code of the current class to a file. (In C#)</summary>
		/// <param name="filename">The file to write to.</param>
		/// <param name="lang">The .NET Language used to write the source code.</param>
		public void SaveToBinaryFile(string filename, NetLanguage lang)
		{
			StreamWriter writer = new StreamWriter(filename);
			writer.Write(GetClassBinaryCode(lang));
			writer.Close();
		}

		#endregion

    	internal ClassBuilder(string className)
		{
    		className = className.Trim();
    		if (ValidIdentifier(className) == false)
    			throw new FileHelperException(string.Format(sInvalidIdentifier, className));
    		
    		mClassName = className;
		}
    	
		/// <summary>Generate the runtime record class to be used by the engines.</summary>
		/// <returns>The generated record class</returns>
    	public Type CreateRecordClass()
    	{
    		string classCode = GetClassSourceCode(NetLanguage.CSharp);
    		return ClassFromString(classCode, NetLanguage.CSharp);
    	}

    	   	
		//--------------
		//->  Fields 

		#region Fields
    	
		/// <summary>Removes all the Fields of the current class.</summary>
		public void ClearFields()
		{
			mFields.Clear();
		}
		
		/// <summary></summary>
		protected ArrayList mFields = new ArrayList();

		/// <summary></summary>
		/// <param name="field"></param>
		protected void AddFieldInternal(FieldBuilder field)
		{
			field.mFieldIndex = mFields.Add(field);
			field.mClassBuilder = this;
		}

		/// <summary>Returns the current fields of the class.</summary>
		public FieldBuilder[] Fields
		{
			get { return (FieldBuilder[]) mFields.ToArray(typeof(FieldBuilder)); }
		}

		/// <summary>Returns the current number of fields.</summary>
		public int FieldCount
		{
			get
			{
				return mFields.Count;
			}
		}


		/// <summary>Return the field at the specified index.</summary>
		/// <param name="index">The index of the field.</param>
		/// <returns>The field at the specified index.</returns>
		public FieldBuilder FieldByIndex(int index)
		{
			return (FieldBuilder) mFields[index];
		}

		#endregion
    	
		#region ClassName
		
    	private string mClassName;
		/// <summary>The name of the Class.</summary>
		public string ClassName
		{
			get { return mClassName; }
			set { mClassName = value; }
		}
		
    	#endregion

   	
    	//----------------------------
    	//->  ATTRIBUTE MAPPING
    	
		#region IgnoreFirstLines
    	
		private int mIgnoreFirstLines = 0;

		/// <summary>Indicates the number of FIRST LINES to be ignored by the engines.</summary>
		public int IgnoreFirstLines
		{
			get { return mIgnoreFirstLines; }
			set { mIgnoreFirstLines = value; }
		}
		
    	#endregion

		#region IgnoreLastLines
    	
		private int mIgnoreLastLines = 0;

		/// <summary>Indicates the number of LAST LINES to be ignored by the engines.</summary>
		public int IgnoreLastLines
		{
			get { return mIgnoreLastLines; }
			set { mIgnoreLastLines = value; }
		}

		#endregion
    	
		#region IgnoreEmptyLines

		private bool mIgnoreEmptyLines = false;

		/// <summary>Indicates that the engines must ignore the empty lines in the files.</summary>
		public bool IgnoreEmptyLines
		{
			get { return mIgnoreEmptyLines; }
			set { mIgnoreEmptyLines = value; }
		}

    	#endregion

		private bool mGenerateProperties = false;

		/// <summary>Indicates if this ClassBuilder generates also the property accessors (Perfect for DataBinding)</summary>
		public bool GenerateProperties
		{
			get { return mGenerateProperties; }
			set { mGenerateProperties = value; }
		}

		
		/// <summary>
		/// Returns the ENCRIPTED code for the current class in the specified language.
		/// </summary>
		/// <param name="lang">The language for the return code.</param>
		/// <returns>The ENCRIPTED code for the class that are currently building.</returns>
		public string GetClassBinaryCode(NetLanguage lang)
		{
			return Encrypt(GetClassSourceCode(lang), "withthefilehelpers1.0.0youcancodewithoutproblems1.5.0");
		}

		/// <summary>
		/// Returns the source code for the current class in the specified language.
		/// </summary>
		/// <param name="lang">The language for the return code.</param>
		/// <returns>The Source Code for the class that are currently building.</returns>
		public string GetClassSourceCode(NetLanguage lang)
		{
			StringBuilder sb = new StringBuilder(100);
			
			BeginNamespace(lang, sb);
			
			AttributesBuilder attbs = new AttributesBuilder(lang);
			
			AddAttributesInternal(attbs, lang);
			AddAttributesCode(attbs, lang);
			
			sb.Append(attbs.GetAttributesCode());
			
			switch (lang)
			{
				case NetLanguage.VbNet:
					sb.Append(GetVisibility(lang, mVisibility) + GetSealed(lang) + "Class " + mClassName);
					sb.Append(StringHelper.NewLine);
					break;
				case NetLanguage.CSharp:
					sb.Append(GetVisibility(lang, mVisibility) + GetSealed(lang) + "class " + mClassName);
					sb.Append(StringHelper.NewLine);
					sb.Append("{");
					break;
			}

			sb.Append(StringHelper.NewLine);
			sb.Append(StringHelper.NewLine);

			foreach (FieldBuilder field in mFields)
			{
				sb.Append(field.GetFieldCode(lang));
				sb.Append(StringHelper.NewLine);
			}
			
			
			sb.Append(StringHelper.NewLine);
			
			switch (lang)
			{
				case NetLanguage.VbNet:
					sb.Append("End Class");
					break;
				case NetLanguage.CSharp:
					sb.Append("}");
					break;
			}

			EndNamespace(lang, sb);
			
			return sb.ToString();
			
		
		}
    	
		internal abstract void AddAttributesCode(AttributesBuilder attbs, NetLanguage lang);

		private void AddAttributesInternal(AttributesBuilder attbs, NetLanguage lang)
		{

			if (mIgnoreFirstLines != 0)
				attbs.AddAttribute("IgnoreFirst("+ mIgnoreFirstLines.ToString() +")");

			if (mIgnoreLastLines != 0)
				attbs.AddAttribute("IgnoreLast("+ mIgnoreLastLines.ToString() +")");

			if (mIgnoreEmptyLines == true)
				attbs.AddAttribute("IgnoreEmptyLines()");

		
		}
    	
		
		#region "  EncDec  "	
		
		private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV) 
		{ 
			MemoryStream ms = new MemoryStream(); 
			Rijndael alg = Rijndael.Create(); 
			alg.Key = Key; 
			alg.IV = IV; 
			CryptoStream cs = new CryptoStream(ms, 
				alg.CreateEncryptor(), CryptoStreamMode.Write); 
			cs.Write(clearData, 0, clearData.Length); 
			cs.Close(); 
			byte[] encryptedData = ms.ToArray();
			return encryptedData; 
		} 

		private static string Encrypt(string clearText, string Password) 
		{ 
			byte[] clearBytes = Encoding.Unicode.GetBytes(clearText); 

			PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
				new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 
							   0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76}); 
			byte[] encryptedData = Encrypt(clearBytes, 
				pdb.GetBytes(32), pdb.GetBytes(16)); 
			return Convert.ToBase64String(encryptedData); 
		}
    

		// Decrypt a byte array into a byte array using a key and an IV 
		private static byte[] Decrypt(byte[] cipherData, 
			byte[] Key, byte[] IV) 
		{ 
			MemoryStream ms = new MemoryStream(); 
			Rijndael alg = Rijndael.Create(); 
			alg.Key = Key; 
			alg.IV = IV; 

			CryptoStream cs = new CryptoStream(ms, 
				alg.CreateDecryptor(), CryptoStreamMode.Write); 

			cs.Write(cipherData, 0, cipherData.Length); 
			cs.Close(); 

			byte[] decryptedData = ms.ToArray(); 

			return decryptedData; 
		}

		private static string Decrypt(string cipherText, string Password) 
		{ 
			byte[] cipherBytes = Convert.FromBase64String(cipherText); 
			PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, 
				new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 
							   0x64, 0x76, 0x65, 0x64, 0x65, 0x76}); 
			byte[] decryptedData = Decrypt(cipherBytes, 
				pdb.GetBytes(32), pdb.GetBytes(16)); 
			return Encoding.Unicode.GetString(decryptedData); 
		}

    	
		
		#endregion
    	

		
		private NetVisibility mVisibility = NetVisibility.Public;

		/// <summary>The Visibility for the class.</summary>
		public NetVisibility Visibility
		{
			get { return mVisibility; }
			set { mVisibility = value; }
		}

		private bool mSealedClass = true;

		/// <summary>Indicates if the generated class must be sealed.</summary>
		public bool SealedClass
		{
			get { return mSealedClass; }
			set { mSealedClass = value; }
		}

		private string mNamespace = string.Empty;

		/// <summary>The namespace used when creating the class.</summary>
		public string Namespace
		{
			get { return mNamespace; }
			set { mNamespace = value; }
		}


		internal static string GetVisibility(NetLanguage lang, NetVisibility visibility)
		{
			switch(lang)
			{
				case NetLanguage.CSharp:
					switch(visibility)
					{
						case NetVisibility.Public:
							return "public ";
						case NetVisibility.Private:
							return "private ";
						case NetVisibility.Internal:
							return "internal ";
						case NetVisibility.Protected:
							return "protected ";
					}
					break;

				case NetLanguage.VbNet:
					switch(visibility)
					{
						case NetVisibility.Public:
							return "Public ";
						case NetVisibility.Private:
							return "Private ";
						case NetVisibility.Internal:
							return "Friend ";
						case NetVisibility.Protected:
							return "Protected ";
					}
					break;
			}
			
			return string.Empty;
		}

		private string GetSealed(NetLanguage lang)
		{
			if (mSealedClass == false)
				return string.Empty;
			
			switch(lang)
			{
				case NetLanguage.CSharp:
					return "sealed ";

				case NetLanguage.VbNet:
					return "NotInheritable ";
			}
			
			return string.Empty;
		}

		private void BeginNamespace(NetLanguage lang, StringBuilder sb)
		{
			if (mNamespace == string.Empty)
				return;
			
			switch(lang)
			{
				case NetLanguage.CSharp:
					sb.Append("namespace ");
					sb.Append(mNamespace);
					sb.Append(StringHelper.NewLine);
					sb.Append("{");
					break;

				case NetLanguage.VbNet:
					sb.Append("Namespace ");
					sb.Append(mNamespace);
					sb.Append(StringHelper.NewLine);
					break;
			}		

			sb.Append(StringHelper.NewLine);
		}

		private void EndNamespace(NetLanguage lang, StringBuilder sb)
		{
			if (mNamespace == string.Empty)
				return;
			
			sb.Append(StringHelper.NewLine);

			switch(lang)
			{
				case NetLanguage.CSharp:
					sb.Append("}");
					break;

				case NetLanguage.VbNet:
					sb.Append("End Namespace");
					break;
			}		
		}

		

		/// <summary>
		/// Loads and return a ClassBuilder inheritor that has been saved with the SaveToXml method
		/// </summary>
		/// <param name="filename">A file name with the ClassBuilder difinition.</param>
		/// <returns>A new instance of a ClassBuilder inheritor.</returns>
		public static ClassBuilder LoadFromXml(string filename)
		{
			ClassBuilder res = null;
			
			XmlDocument document = new XmlDocument();
			document.Load(filename);
			
			string classtype = document.ChildNodes[0].LocalName;
			
			if (classtype == "DelimitedClass")
				res = DelimitedClassBuilder.LoadXmlInternal(document);
			else
				res = FixedLengthClassBuilder.LoadXmlInternal(document);
			
			XmlNode node = document.ChildNodes.Item(0)["IgnoreLastLines"];
			if (node != null) res.IgnoreLastLines = int.Parse(node.InnerText);
			
			node = document.ChildNodes.Item(0)["IgnoreFirstLines"];
			if (node != null) res.IgnoreFirstLines = int.Parse(node.InnerText);

			node = document.ChildNodes.Item(0)["IgnoreEmptyLines"];
			if (node != null) res.IgnoreEmptyLines = true;

			node = document.ChildNodes.Item(0)["SealedClass"];
			if (node != null) res.SealedClass = true;

			node = document.ChildNodes.Item(0)["Namespace"];
			if (node != null) res.Namespace = node.InnerText;

			node = document.ChildNodes.Item(0)["Visibility"];
			if (node != null) res.Visibility = (NetVisibility) Enum.Parse(typeof(NetVisibility), node.InnerText);;

			res.ReadClassElements(document);
			
			node = document.ChildNodes.Item(0)["Fields"];
			XmlNodeList nodes ;
				
			if (classtype == "DelimitedClass")
				nodes = node.SelectNodes("/DelimitedClass/Fields/Field");
			else
				nodes = node.SelectNodes("/FixedLengthClass/Fields/Field");

			foreach (XmlNode n in nodes)
			{
				res.ReadField(n);
			}
		
			return res;

		}

		
		/// <summary>
		/// Creates a custom serialization of the current ClassBuilder
		/// </summary>
		/// <param name="filename">A file name to write to.</param>
		public void SaveToXml(string filename)
		{
			XmlHelper writer = new XmlHelper();
			
			writer.BeginWriteFile(filename);
			
			WriteHeaderElement(writer);
			
			writer.WriteElement("ClassName", ClassName);
			writer.WriteElement("Namespace", this.Namespace, string.Empty);
			writer.WriteElement("SealedClass", this.SealedClass);
			writer.WriteElement("Visibility", this.Visibility.ToString(), "Public");

			writer.WriteElement("IgnoreEmptyLines", this.IgnoreEmptyLines);
			writer.WriteElement("IgnoreFirstLines", this.IgnoreFirstLines.ToString(), "0");
			writer.WriteElement("IgnoreLastLines", this.IgnoreLastLines.ToString(), "0");

			WriteExtraElements(writer);

			writer.mWriter.WriteStartElement("Fields");
			
			for(int i = 0; i < mFields.Count; i++)
				((FieldBuilder) mFields[i]).SaveToXml(writer);
			
			writer.mWriter.WriteEndElement();
			
			writer.mWriter.WriteEndElement();
			writer.EndWrite();
		}
		
		internal abstract void WriteHeaderElement(XmlHelper writer);
		internal abstract void WriteExtraElements(XmlHelper writer);

		internal abstract void ReadClassElements(XmlDocument document);
		internal abstract void ReadField(XmlNode node);

		internal static bool ValidIdentifier(string id)
		{
			return ValidIdentifier(id, false);
		}

		internal static bool ValidIdentifier(string id, bool isType)
		{
			if (id == null || id.Length == 0)
				return false;
			
			if (Char.IsLetter(id[0]) == false && id[0] != '_')
				return false;

			for(int i=1; i < id.Length;i++)
			{
				if (isType && id[i] == '.')
					continue;
				
				if (id[i] != '_' && Char.IsLetterOrDigit(id[i]) == false)
					return false;
			}
			
			return true;
		}
		
		internal const string sInvalidIdentifier = "The string '{0}' NOT IS a valid .NET identifier.";
	}
}
