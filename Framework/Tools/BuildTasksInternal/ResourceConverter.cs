#region Using directives

using System;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Threading;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using System.IO;

#endregion

namespace Microsoft.SPOT.Tasks.Internal
{
    public class ResourceConverter : Microsoft.Build.Utilities.Task
    {
        private string m_assemblyName;
        private string m_outputFile;
        private string[] m_resourceDirs;

        private ResXResourceWriter m_writer;
        private ushort m_idNext;
        private bool m_fAnyResources;

        [Microsoft.Build.Framework.Required]
        public string AssemblyName
        {
            get { return m_assemblyName; }
            set { m_assemblyName = value; }
        }

        [Microsoft.Build.Framework.Required]
        public string OutputFile
        {
            get { return m_outputFile; }
            set { m_outputFile = value; }
        }

        public string[] ResourceDirectories
        {
            get { return m_resourceDirs; }
            set { m_resourceDirs = value; }
        }

        public override bool Execute()
        {
            Assembly assembly = Assembly.LoadFrom( m_assemblyName );
            if(assembly == null)
                throw new ArgumentException( "could not load assembly", "AssemblyName" );

            using(FileStream fs = File.Open( m_outputFile, FileMode.OpenOrCreate, FileAccess.Write ))
            {
                GenerateResx( fs, assembly );
            }

            return true;
        }

        private string GetStringFieldValue( Attribute attribute, string name )
        {
            return (string)attribute.GetType().GetField( name ).GetValue( attribute );
        }

        private void AddStringResource( Attribute attribute, string name )
        {
            string value = GetStringFieldValue( attribute, "m_value" );
            m_writer.AddResource( name, value );
            m_fAnyResources = true;
        }

        private void AddFileResource( Attribute attribute, string name, string value, Type type)
        {
            string fileName = null;

            string currentDir = Environment.CurrentDirectory;

            if(File.Exists( value ))
            {
                fileName = value;
            }
            else if(m_resourceDirs != null)
            {
                for(int iDir = 0; iDir < m_resourceDirs.Length; iDir++)
                {
                    string dir = (string)m_resourceDirs[iDir];

                    fileName = Path.Combine( dir, value );

                    if(File.Exists( fileName ))
                    {
                        break;
                    }

                    fileName = null;
                }
            }

            if(fileName == null)
            {
                throw new ApplicationException( string.Format( "Could not file file {0}", value ) );
            }

            m_writer.AddResource( new ResXDataNode( name, new ResXFileRef( fileName, type.AssemblyQualifiedName ) ) );
            m_fAnyResources = true;
        }

        private void AddFontResource( Attribute attribute, string name )
        {
            string value = GetStringFieldValue( attribute, "m_id" );

            value = Path.ChangeExtension( value, "tinyfnt" );

            AddFileResource( attribute, name, value, typeof(byte[]) );
        }

        private int GetId( FieldInfo field )
        {
            int id;
            object obj = field.GetValue( null );
            //id = (int)obj;
            id = Convert.ToInt32( obj );
            return id;
        }

        private void AddResource( FieldInfo field, bool fGenerateId )
        {
            Type type = field.DeclaringType;
            Attribute attribute = null;

            object[] attributes = (object[])field.GetCustomAttributes( false );

            ushort id = m_idNext;

            for(int iAttribute = 0; iAttribute < attributes.Length; iAttribute++)
            {
                Attribute attributeT = (Attribute)attributes[iAttribute];

                switch(attributeT.GetType().FullName)
                {
                    case "Microsoft.SPOT.StringResourceAttribute":
                    case "Microsoft.SPOT.BitmapResourceAttribute":
                    case "Microsoft.SPOT.BinaryResourceAttribute":
                    case "Microsoft.SPOT.FontResourceAttribute":
                        attribute = attributeT;
                        break;
                    default:
                        continue;
                }
            }

            if(attribute == null)
            {
                return;
            }

            if(fGenerateId)
            {
                m_idNext++;
            }
            else
            {
                id = (ushort)GetId( field );
            }

            string name = string.Format( "{0}.{1};0x{2}", type.FullName, field.Name, id.ToString( "X4" ) );
            switch(attribute.GetType().FullName)
            {
                case "Microsoft.SPOT.StringResourceAttribute":
                    AddStringResource( attribute, name );
                    break;
                case "Microsoft.SPOT.BitmapResourceAttribute":
                    AddFileResource( attribute, name, GetStringFieldValue( attribute, "m_id" ), typeof( System.Drawing.Bitmap ) );
                    break;
                case "Microsoft.SPOT.BinaryResourceAttribute":
                    AddFileResource( attribute, name, GetStringFieldValue( attribute, "m_id" ), typeof( string ) );
                    break;
                case "Microsoft.SPOT.FontResourceAttribute":
                    AddFontResource( attribute, name );
                    break;
            }
        }

        private void AddResources( Type type )
        {
            if(type.IsEnum)
            {
                FieldInfo[] fields = type.GetFields( );
                bool fAutoId = true;

                for(int iField = 1; iField < fields.Length; iField++)
                {
                    FieldInfo field = fields[iField];

                    if(iField == 1)
                    {
                        fAutoId = GetId( field ) == 0;
                    }

                    AddResource( field, fAutoId );
                }
            }
        }

        private void GenerateResx( FileStream fs, Assembly assembly )
        {
            using(ResXResourceWriter writer = new ResXResourceWriter( fs ))
            {
                m_writer = writer;

                Type[] types = assembly.GetTypes();

                for(int iType = 0; iType < types.Length; iType++)
                {
                    Type type = types[iType];

                    AddResources( type );
                }

                if(m_fAnyResources)
                {
                    writer.Generate();
                }
            }

            m_writer = null;
        }
    }
}
