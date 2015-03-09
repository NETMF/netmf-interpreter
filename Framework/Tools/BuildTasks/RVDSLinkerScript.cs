using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.SPOT.Tasks.ScatterFile
{
    public class LoadRegionRVDS : LoadRegion
    {
        public override void EvaluateStatement(Document doc)
        {
            string name        = doc.Resolve( m_name        );
            string baseAddress = doc.Resolve( m_baseAddress );
            string options     = doc.Resolve( m_options     );
            string size        = doc.Resolve( m_size        );

            doc.AppendLineFormat( "{0} {1} {2} {3}", name, baseAddress, options, size );
            doc.AppendLine      ( "{"                                               ); EvaluateStatementInner( doc );
            doc.AppendLine      ( "}"                                               );
        }
    }

    public class ExecRegionRVDS : ExecRegion
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string baseAddress = doc.Resolve(m_baseAddress);
            string options = doc.Resolve(m_options);
            string size = doc.Resolve(m_size);

            doc.AppendLineFormat("    {0} {1} {2} {3}", name, baseAddress, options, size);
            doc.AppendLine("    {"); EvaluateStatementInner(doc);
            doc.AppendLine("    }");
        }
    }

    public class FileMappingRVDS : FileMapping
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string options = doc.Resolve(m_options);

            doc.AppendLineFormat("        {0} {1}", name, options);
        } 
    }

    public class NamedGroupRVDS : NamedGroup
    {
        public override void EvaluateStatement(Document doc)
        {
            // Nothing to do here for RVDS
        }
    }

    public class EntryPointRVDS : EntryPoint
    {
        public override void EvaluateStatement(Document doc)
        {
            // Nothing to do here for RVDS
        }
    }

    public class GlobalVariableRVDS : GlobalVariable
    {
        public override void EvaluateStatement(Document doc)
        {
            // Nothing to do here for RVDS
        }
    }

    public class ProvideRVDS : Provide
    {
        public override void EvaluateStatement(Document doc)
        {
            // Nothing to do here for RVDS
        }
    }
}
