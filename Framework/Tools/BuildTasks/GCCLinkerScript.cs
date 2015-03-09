using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Microsoft.SPOT.Tasks.ScatterFile
{
    public class LoadRegionGCC : LoadRegion
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string baseAddress = doc.Resolve(m_baseAddress);
            string options = doc.Resolve(m_options);
            string size = doc.Resolve(m_size);

            doc.AppendLineFormat("    {0} : ORIGIN = {1}, LENGTH = {2}", name, baseAddress, size);
        }
    }

    public class ExecRegionGCC : ExecRegion
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string baseAddress = doc.Resolve(m_baseAddress);
            string align = doc.Resolve(m_align);
            string options = doc.Resolve(m_options);
            string size = doc.Resolve(m_size);

            if (align.Length > 0)
            {
                doc.AppendLineFormat("    {0} {1} : ALIGN({2})", name, baseAddress, align);
            }
            else
            {
                doc.AppendLineFormat("    {0} {1} :", name, baseAddress);
            }

            doc.AppendLine("    {"); EvaluateStatementInner(doc);
            doc.AppendLineFormat("    }}{0}", options);
        }
    }

    public class FileMappingGCC : FileMapping
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string options = doc.Resolve(m_options);

            doc.AppendLineFormat("        {0} {1}", name, options);
        }
    }

    public class NamedGroupGCC : NamedGroup
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);

            doc.AppendLineFormat("{0}", name);
            doc.AppendLine("{"); EvaluateStatementInner(doc);
            doc.AppendLine("}");
        }
    }

    public class EntryPointGCC : EntryPoint
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);

            doc.AppendLineFormat("ENTRY({0})", name);
        }
    }

    public class GlobalVariableGCC : GlobalVariable
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);
            string value = doc.Resolve(m_value);

            doc.AppendLineFormat("{0} = {1};", name, value);
        }
    }

    public class ProvideGCC : Provide
    {
        public override void EvaluateStatement(Document doc)
        {
            string name = doc.Resolve(m_name);

            doc.AppendLineFormat("        PROVIDE({0} = .);", name);
        }
    }
}
