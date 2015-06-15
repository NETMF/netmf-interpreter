using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.SPOT.Tasks.ScatterFile
{
	public interface IParse
	{
		void Parse( Document doc, System.Xml.XmlNode node );
	}

	public interface IStatement
	{
		void EvaluateStatement( Document doc );
	}

	public interface ICondition
	{
		bool EvaluateCondition( Document doc );
	}

	public interface IEnvironment
	{
		string GetVariable(string name);
	}
	//--//

    public class InternalNode
    {
        protected ArrayList m_children = new ArrayList();

        protected void ParseInner( Document doc, System.Xml.XmlNode node, Type type )
        {
            foreach(System.Xml.XmlNode subnode in node.ChildNodes)
            {
                if(subnode is System.Xml.XmlElement)
                {
                    object o = doc.Parse( subnode );

                    if(type != null && type.IsInstanceOfType( o ) == false)
                    {
                        throw Document.ParseException( "{0} is not an instance of {1}", subnode.Name, type.FullName );
                    }

                    m_children.Add( o );
                }
            }
        }

        protected void EvaluateStatementInner( Document doc )
        {
            foreach(IStatement st in m_children)
            {
                st.EvaluateStatement( doc );
            }
        }
    }

    public class Group : InternalNode, IParse, IStatement
    {
        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            ParseInner( doc, node, typeof(IStatement) );
        }

        public void EvaluateStatement( Document doc )
        {
            EvaluateStatementInner( doc );
        }
    }

    public class Variable : IParse, IStatement
	{
		string m_name;
		string m_value;

		public void Parse( Document doc, System.Xml.XmlNode node )
		{
			m_name  = Document.ReadAttribute( node, "Name"  );
			m_value = Document.ReadAttribute( node, "Value" );
		}

		public void EvaluateStatement( Document doc )
		{
			doc.AddVariable( m_name, doc.Resolve( m_value ) );
		}
	}

    public abstract class LoadRegion : InternalNode, IParse, IStatement
    {
        protected string m_name;
        protected string m_baseAddress;
        protected string m_options;
        protected string m_size;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_name        = Document.ReadAttribute( node, "Name"    );
            m_baseAddress = Document.ReadAttribute( node, "Base"    );
            m_options     = Document.ReadAttribute( node, "Options" );
            m_size        = Document.ReadAttribute( node, "Size"    );

            ParseInner( doc, node, typeof(IStatement) );
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class ExecRegion : InternalNode, IParse, IStatement
    {
        protected string m_name;
        protected string m_baseAddress;
        protected string m_align;
        protected string m_options;
        protected string m_size;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_name        = Document.ReadAttribute( node, "Name"    );
            m_baseAddress = Document.ReadAttribute( node, "Base"    );
            m_align       = Document.ReadAttribute( node, "Align"   );
            m_options     = Document.ReadAttribute( node, "Options" );
            m_size        = Document.ReadAttribute( node, "Size"    );

            ParseInner( doc, node, typeof(IStatement) );
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class FileMapping : IParse, IStatement
    {
        protected string m_name;
        protected string m_options;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_name    = Document.ReadAttribute( node, "Name"    );
            m_options = Document.ReadAttribute( node, "Options" );
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class Provide : IParse, IStatement
    {
        protected string m_name;

        public void Parse(Document doc, System.Xml.XmlNode node)
        {
            m_name = Document.ReadAttribute(node, "Name");
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class NamedGroup : InternalNode, IParse, IStatement
    {
        protected string m_name;

        public void Parse(Document doc, System.Xml.XmlNode node)
        {
            m_name = Document.ReadAttribute(node, "Name");

            ParseInner(doc, node, typeof(IStatement));
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class EntryPoint : IParse, IStatement
    {
        protected string m_name;

        public void Parse(Document doc, System.Xml.XmlNode node)
        {
            m_name = Document.ReadAttribute(node, "Name");
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public abstract class GlobalVariable : IParse, IStatement
    {
        protected string m_name;
        protected string m_value;

        public void Parse(Document doc, System.Xml.XmlNode node)
        {
            m_name = Document.ReadAttribute(node, "Name");
            m_value = Document.ReadAttribute(node, "Value");
        }

        public abstract void EvaluateStatement(Document doc);
    }

    public class Include : IParse, IStatement
    {
        string m_file;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_file = Document.ReadAttribute( node, "File" );
        }

        public void EvaluateStatement( Document doc )
        {
            string   file = doc     .Resolve( m_file      );
            Document doc2 = Document.Load   (   file, doc );

            foreach(string res in doc2.Execute())
            {
                doc.AppendLine( res );
            }
        }
    }

    public class Error : IParse, IStatement
    {
        string m_message;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_message = Document.ReadAttribute( node, "Message" );
        }

        public void EvaluateStatement( Document doc )
        {
            Console.WriteLine( "Error: {0}", m_message );
            throw new Exception( m_message );
        }
    }

    public class Conditional : InternalNode, IParse, IStatement
	{
		//	<Match Name= Value=/>
		//	<Or>
		//	<And>
		//	<Not>
        //
		//	<Positive>
		//	<Negative>

		ICondition m_condition;
		IStatement m_positive;
		IStatement m_negative;

		public void Parse( Document doc, System.Xml.XmlNode node )
		{
			foreach(System.Xml.XmlNode subnode in node.ChildNodes)
			{
				if(subnode.NodeType == System.Xml.XmlNodeType.Element)
				{
					object st = doc.Parse( subnode );

                    if(st is IStatement)
                    {
                        if     (subnode.Name == "Positive" ) m_positive = (IStatement)st;
                        else if(subnode.Name == "Negative" ) m_negative = (IStatement)st;
                        else                                 throw Document.ParseException( "{0} should be either Positive or Negative", subnode.Name );
                    }
                    else if(st is ICondition)
                    {
                        m_condition = (ICondition)st;
                    }
                    else
                    {
                        throw Document.ParseException( "{0} is not allowed in a conditional statement", subnode.Name );
                    }
                }
			}
		}

		public void EvaluateStatement( Document doc )
		{
			if(m_condition.EvaluateCondition( doc ))
			{
				if(m_positive != null) m_positive.EvaluateStatement( doc );
			}
			else
			{
				if(m_negative != null) m_negative.EvaluateStatement( doc );
			}
		}
	}

    public class MatchCondition : IParse, ICondition
    {
        string m_name;
        string m_value;

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            m_name  = node.Attributes["Name" ].Value;
            m_value = node.Attributes["Value"].Value;
        }

        public bool EvaluateCondition( Document doc )
        {
            Regex re = new Regex( m_value, RegexOptions.IgnoreCase );

            return re.IsMatch( doc.GetVariable( m_name ) );
        }
    }

    public class OrCondition : InternalNode, IParse, ICondition
	{
		public void Parse( Document doc, System.Xml.XmlNode node )
		{
            ParseInner( doc, node, typeof(ICondition) );
		}

		public bool EvaluateCondition( Document doc )
		{
			foreach(ICondition c in m_children)
			{
				if(c.EvaluateCondition( doc )) return true;
			}

			return false;
		}
	}

	public class AndCondition : InternalNode, IParse, ICondition
	{
		public void Parse( Document doc, System.Xml.XmlNode node )
		{
            ParseInner( doc, node, typeof(ICondition) );
		}

		public bool EvaluateCondition( Document doc )
		{
			foreach(ICondition c in m_children)
			{
				if(c.EvaluateCondition( doc ) == false) return false;
			}

			return true;
		}
	}

	public class NotCondition : InternalNode, IParse, ICondition
	{
		public ICondition m_child;

		public void Parse( Document doc, System.Xml.XmlNode node )
		{
            ParseInner( doc, node, typeof(ICondition) );

            if(m_children.Count != 1)
            {
                throw Document.ParseException( "Not elements only accept one child condition" );
            }

            m_child = (ICondition)m_children[0];
        }

		public bool EvaluateCondition( Document doc )
		{
			return m_child.EvaluateCondition( doc ) == false;
		}
	}

    public class Filter : InternalNode, IParse, IStatement
    {
        string m_name;
        string m_value;
        string m_set;
        bool   m_fPass;
        bool   m_fOnlyDefined;

        public Filter( bool fOnlyDefined, bool fPass )
        {
            m_fOnlyDefined = fOnlyDefined;
            m_fPass        = fPass;
        }

        public void Parse( Document doc, System.Xml.XmlNode node )
        {
            ParseInner( doc, node, typeof(IStatement) );

            m_name = node.Attributes["Name" ].Value;

            if(m_fOnlyDefined == false)
            {
                System.Xml.XmlAttribute attr;

                attr = node.Attributes["Value"]; if(attr != null) m_value = attr.Value;
                attr = node.Attributes["In"   ]; if(attr != null) m_set   = attr.Value;
            }
        }

        public void EvaluateStatement( Document doc )
        {
            string name = doc.GetVariable( m_name );
            bool   fRes = (name != null);


            if(fRes && m_fOnlyDefined == false)
            {
                if(m_value != null)
                {
                    fRes = Regex.IsMatch( name, m_value, RegexOptions.IgnoreCase );
                }

                if(m_set != null)
                {
                    fRes = false;

                    foreach(string word in m_set.Split( ' ' ))
                    {
                        if(word != null && word.Length > 0)
                        {
                            if(String.Compare( name, word, true ) == 0)
                            {
                                fRes = true;
                                break;
                            }
                        }
                    }
                }
            }

            if(fRes == m_fPass)
            {
                EvaluateStatementInner( doc );
            }
        }
    }

    //--//

	public class Document : IEnvironment
	{
        IEnvironment     m_enclosing;
		Hashtable        m_variables = new Hashtable();
		Group            m_root;
        ArrayList        m_output;
        Regex            m_re_Substitution = new Regex( @"(.*)%([^%]*)%(.*)"       );
        Regex            m_re_OneOperand   = new Regex( @"^(\w+)\s+(\w+)$"         );
        Regex            m_re_TwoOperands  = new Regex( @"^(\w+)\s+([\w+-]*)\s+(\w+)$" );

		private Document( System.Xml.XmlNode node, IEnvironment enclosing )
		{
            m_enclosing = enclosing;

			m_root = (Group)this.Parse( node );
		}

        public void AddVariable( string name, string val )
        {
            m_variables[name] = val;
        }

        public string GetVariable( string name )
        {
            if(m_variables.ContainsKey( name ))
            {
                return (string)m_variables[name];
            }

            return (m_enclosing == null) ? null : m_enclosing.GetVariable( name );
        }
        
        public string Resolve( string val )
		{
            return Resolve( new Hashtable(), val );
		}

        string Resolve( Hashtable seen, string val )
		{
            Match           match;
            GroupCollection groups;

            while(val != null)
            {
                match  = m_re_Substitution.Match( val ); if(match.Success == false) break;
                groups = match.Groups;

                string pre  = groups[1].Value;
                string var  = groups[2].Value;
                string post = groups[3].Value;

                match = m_re_TwoOperands.Match( var );
                if(match.Success)
                {
                    groups = match.Groups;

                    var = ResolveTwoOperands( seen, groups[1].Value, groups[2].Value, groups[3].Value );
                }
                else
                {
                    if(seen.ContainsKey( var ))
                    {
                        throw Document.ParseException( "Infinite recursion resolving '{0}'", var );
                    }

                    seen[var] = true;

                    var varName = var;
                    var = Resolve( seen, GetVariable( var ) );

                    if(var == null)
                    {
                        throw Document.ParseException( "'{0}' is not defined", varName );
                    }

                    seen.Remove( var );
                }

                val = pre + var + post;
            }

            return val;
		}

        string ResolveTwoOperands( Hashtable seen, string val1, string op, string val2 )
        {
            uint i1 = ResolveOperand( seen, val1 );
            uint i2 = ResolveOperand( seen, val2 );

            if(op == "+") return String.Format( "0x{0:X}", i1 + i2 );
            if(op == "-") return String.Format( "0x{0:X}", i1 - i2 );

            throw Document.ParseException( "Unknown operation: '{0}'", op );
        }

        uint ResolveOperand( Hashtable seen, string val )
        {
            if(GetVariable( val ) != null)
            {
                val = Resolve( seen, "%" + val + "%" );
            }

            Match m = Regex.Match( val, @"0x(\w+)" );
            if(m.Success)
            {
                return UInt32.Parse( m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber );
            }

            return UInt32.Parse( val, System.Globalization.NumberStyles.Integer );
        }

        //--//

        public string[] Execute()
        {
            m_output = new ArrayList();

            m_root.EvaluateStatement( this );

            return (string[])m_output.ToArray( typeof(string) );
        }

        //--//

        internal void AppendLine( string line )
        {
            m_output.Add( line );
        }

        internal void AppendLineFormat( string fmt, params object[] args )
        {
            AppendLine( String.Format( fmt, args ) );
        }

		internal object Parse( System.Xml.XmlNode node )
		{
            string name = node.Name;
            IParse o    = null;

            if (GetVariable("COMPILER_TOOL").ToUpper() == "RVDS" || GetVariable("COMPILER_TOOL").ToUpper() == "MDK")
            {
                if (name == "ScatterFile")          o = new Group();
                else if (name == "Set")             o = new Variable();
                else if (name == "Conditional")     o = new Conditional();
                else if (name == "If")              o = new Filter(false, true);
                else if (name == "IfDefined")       o = new Filter(true, true);
                else if (name == "IfNot")           o = new Filter(false, false);
                else if (name == "IfNotDefined")    o = new Filter(true, false);
                else if (name == "Match")           o = new MatchCondition();
                else if (name == "Or")              o = new OrCondition();
                else if (name == "And")             o = new AndCondition();
                else if (name == "Not")             o = new NotCondition();
                else if (name == "Positive")        o = new Group();
                else if (name == "Negative")        o = new Group();
                else if (name == "NamedGroup")      o = new NamedGroupRVDS();
                else if (name == "LoadRegion")      o = new LoadRegionRVDS();
                else if (name == "ExecRegion")      o = new ExecRegionRVDS();
                else if (name == "FileMapping")     o = new FileMappingRVDS();
                else if (name == "EntryPoint")      o = new EntryPointRVDS();
                else if (name == "GlobalVariable")  o = new GlobalVariableRVDS();
                else if (name == "Provide")         o = new ProvideRVDS();
                else if (name == "Include")         o = new Include();
                else if (name == "Error")           o = new Error();
            }
            else if (GetVariable("COMPILER_TOOL").ToUpper() == "GCC")
            {
                if (name == "ScatterFile") o = new Group();
                else if (name == "Set") o = new Variable();
                else if (name == "Conditional") o = new Conditional();
                else if (name == "If") o = new Filter(false, true);
                else if (name == "IfDefined") o = new Filter(true, true);
                else if (name == "IfNot") o = new Filter(false, false);
                else if (name == "IfNotDefined") o = new Filter(true, false);
                else if (name == "Match") o = new MatchCondition();
                else if (name == "Or") o = new OrCondition();
                else if (name == "And") o = new AndCondition();
                else if (name == "Not") o = new NotCondition();
                else if (name == "Positive") o = new Group();
                else if (name == "Negative") o = new Group();
                else if (name == "NamedGroup") o = new NamedGroupGCC();
                else if (name == "LoadRegion") o = new LoadRegionGCC();
                else if (name == "ExecRegion") o = new ExecRegionGCC();
                else if (name == "FileMapping") o = new FileMappingGCC();
                else if (name == "EntryPoint") o = new EntryPointGCC();
                else if (name == "GlobalVariable") o = new GlobalVariableGCC();
                else if (name == "Provide") o = new ProvideGCC();
                else if (name == "Include") o = new Include();
                else if (name == "Error") o = new Error();
            }
            else
            {
                throw new Exception("Environment Variable COMPILER_TOOL not set to a proper value. Supported compilers are ARM and GCC");
            }

            if(o != null)
            {
                o.Parse( this, node );
            }

			return o;
		}

        //--//

        internal class Converter : IDisposable
        {
            const int c_Idle               = 0;
            const int c_LoadRegionPreamble = 1;
            const int c_LoadRegion         = 2;
            const int c_ExecRegionPreamble = 3;
            const int c_ExecRegion         = 4;

            StreamReader m_sr;
            StreamWriter m_sw;
            Regex        m_re_Comments    = new Regex(  "([^;]*);(.*)"                       , RegexOptions.IgnoreCase );
            Regex        m_re_LoadRegion  = new Regex( @"(\S+)\s+(\S+)(\s+(\S+)(\s+(\S+))?)?", RegexOptions.IgnoreCase );
            Regex        m_re_ExecRegion  = new Regex( @"(\S+)\s+(\S+)(\s+(\S+)(\s+(\S+))?)?", RegexOptions.IgnoreCase );
            Regex        m_re_FileMapping = new Regex( @"(\S+)\s+(\(.*\))"                   , RegexOptions.IgnoreCase );
            int          m_state          = c_Idle;

            internal Converter( string armInput, string xmlOutput )
            {
                m_sr = new StreamReader( armInput  );
                m_sw = new StreamWriter( xmlOutput );
            }

            public void Dispose()
            {
                if(m_sw != null) { m_sw.Close(); m_sw = null; }
                if(m_sr != null) { m_sr.Close(); m_sr = null; }
            }

            internal void Execute()
            {
                m_sw.WriteLine( "<?xml version=\"1.0\"?>" );
                m_sw.WriteLine( "<ScatterFile>" );

                while(ProcessLine());

                m_sw.WriteLine( "</ScatterFile>" );
            }

            internal bool ProcessLine()
            {
                string line     = m_sr.ReadLine(); if(line == null) return false;
                string comments = null;

                Match m = m_re_Comments.Match( line );

                if(m.Success)
                {
                    GroupCollection groups = m.Groups;

                    line     = groups[1].Value;
                    comments = groups[2].Value.Trim();

                    if(m_state == c_ExecRegion)
                    {
                        m = m_re_FileMapping.Match( comments );

                        if(m.Success)
                        {
                            groups = m.Groups;

                            comments = String.Format( "<FileMapping Name=\"{0}\" Options=\"{1}\" />", groups[1], groups[2] );
                        }
                    }

                    comments = "<!-- " + comments + " -->";
                }

                line = ProcessLine( line );

                if(comments != null)
                {
                    if(line.Length > 0)
                    {
                        line += " ";
                    }
                    else
                    {
                        switch(m_state)
                        {
                            case c_LoadRegion: line = "        "    ; break;
                            case c_ExecRegion: line = "            "; break;
                        }
                    }
                    
                    line += comments;
                }

                m_sw.WriteLine( "{0}", line );

                return true;
            }

            internal string ProcessLine( string line )
            {
                Match m;

                line = line.Trim(); if(line.Length == 0) return line;

                switch(m_state)
                {
                    case c_Idle:
                        m = m_re_LoadRegion.Match( line );
                        if(m.Success)
                        {
                            m_state = c_LoadRegionPreamble;

                            GroupCollection groups = m.Groups;

                            return String.Format( "    <LoadRegion Name=\"{0}\" Base=\"{1}\" Options=\"{2}\" Size=\"{3}\">", groups[1], groups[2], groups[4], groups[6] );
                        }
                        break;

                    case c_LoadRegionPreamble:
                        if(line == "{")
                        {
                            m_state = c_LoadRegion;
                            return "";
                        }
                        break;

                    case c_LoadRegion:
                        if(line == "}")
                        {
                            m_state = c_Idle;
                            return "    </LoadRegion>";
                        }

                        m = m_re_ExecRegion.Match( line );
                        if(m.Success)
                        {
                            m_state = c_ExecRegionPreamble;

                            GroupCollection groups = m.Groups;

                            return String.Format( "        <ExecRegion Name=\"{0}\" Base=\"{1}\" Options=\"{2}\" Size=\"{3}\">", groups[1], groups[2], groups[4], groups[6] );
                        }
                        break;

                    case c_ExecRegionPreamble:
                        if(line == "{")
                        {
                            m_state = c_ExecRegion;
                            return "";
                        }
                        break;

                    case c_ExecRegion:
                        if(line == "}")
                        {
                            m_state = c_LoadRegion;
                            return "        </ExecRegion>";
                        }

                        m = m_re_FileMapping.Match( line );
                        if(m.Success)
                        {
                            GroupCollection groups = m.Groups;

                            return String.Format( "            <FileMapping Name=\"{0}\" Options=\"{1}\" />", groups[1], groups[2] );
                        }
                        break;
                }

                throw Document.ParseException( "Invalid scatter file input: {0}", line );
            }
        }

        static public void Convert( string armInput, string xmlOutput )
        {
            using(Converter cvrt = new Converter( armInput, xmlOutput ))
            {
                cvrt.Execute();
            }
        }

        //--//

        static public Document Load( string file, Document doc )
        {
            Document docRet = null;

            Console.WriteLine("Processing file: " + file);

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            nsMgr.AddNamespace("ScatterNSOld", "http://tempuri.org/ScatterfileSchema.xsd");
            nsMgr.AddNamespace("ScatterNS",    "http://schemas.microsoft.com/netmf/ScatterfileSchema.xsd");

            xmlDoc.Load( file );

            try { docRet = new Document(xmlDoc.GetElementsByTagName("ScatterFile", "ScatterNS").Item(0), doc);    } catch{}
            try { docRet = new Document(xmlDoc.GetElementsByTagName("ScatterFile", "ScatterNSOld").Item(0), doc); } catch{}

            if (docRet == null)
            {
                docRet = new Document(xmlDoc.GetElementsByTagName("ScatterFile").Item(0), doc);
            }

            return docRet;
        }

        static public Document Load( string file, IEnvironment e )
        {
            Document doc = null;

            Console.WriteLine("Processing file: " + file);

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            
            XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            nsMgr.AddNamespace("ScatterNSOld", "http://tempuri.org/ScatterfileSchema.xsd");
            nsMgr.AddNamespace("ScatterNS",    "http://schemas.microsoft.com/netmf/ScatterfileSchema.xsd");

            xmlDoc.Load( file );

            try { doc = new Document(xmlDoc.SelectSingleNode("ScatterNS:ScatterFile", nsMgr), e);    } catch { }
            try { doc = new Document(xmlDoc.SelectSingleNode("ScatterNSOld:ScatterFile", nsMgr), e); } catch { }

            if (doc == null)
            {
                doc = new Document(xmlDoc.SelectSingleNode("ScatterFile"), e);
            }

            return doc;
        }

        static internal Exception ParseException( string fmt, params object[] args )
        {
            return new FormatException( String.Format( fmt, args ) );
        }

        static internal string ReadAttribute( System.Xml.XmlNode node, string name )
        {
            System.Xml.XmlAttribute attr = node.Attributes[name];

            return attr != null ? attr.Value : "";
        }
    }
}
