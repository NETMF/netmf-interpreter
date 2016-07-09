using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using XsdInventoryFormatObject;
using XsdScatterfileSchemaObject;

namespace ComponentObjectModel
{
    public class ScatterfileWrapper
    {
        MFProject m_project;

        const string ScatterfileNameStart = "scatterfile_definition_";
        const string ScatterfileExtension = ".xml";

        const string ScatterfileNamespace = "http://schemas.microsoft.com/netmf/ScatterfileSchema.xsd";

        const string IfInFormat = "If Name=\"{0}\" In=\"{1}\"";
        const string IfEqFormat = "If Name=\"{0}\" Value=\"{1}\"";
        const string IfDefFormat = "IfDefined Name=\"{0}\"";
        const string IfNotDefFormat = "IfNotDefined Name=\"{0}\"";

        const string RegExIfIn = "If\\s+Name\\s*=\\s*\"(\\S+)\"\\s+In\\s*=\\s*\"([\\S\\s]*)\"";
        const string RegExIfEq = "If\\s+Name\\s*=\\s*\"(\\S+)\"\\s+Value\\s*=\\s*\"([\\S\\s]*)\"";
        const string RegExIfDef = "IfDefined\\s+Name\\s*=\\s*\"(\\S+)\"";
        const string RegExIfNotDef = "IfNotDefined\\s+Name\\s*=\\s*\"(\\S+)\"";

        Regex m_expIfIn = new Regex(RegExIfIn, RegexOptions.IgnoreCase);
        Regex m_expIfEq = new Regex(RegExIfEq, RegexOptions.IgnoreCase);
        Regex m_expIfDef = new Regex(RegExIfDef, RegexOptions.IgnoreCase);
        Regex m_expIfNotDef = new Regex(RegExIfNotDef, RegexOptions.IgnoreCase);

        Hashtable m_execRegionOrderMap = new Hashtable();
        Hashtable m_execRegionsToNavNode = new Hashtable();

        //Hashtable m_symOrderMap  = new Hashtable();
        //Hashtable m_symToNavNode = new Hashtable();


        public ScatterfileWrapper(MFProject project)
        {
            m_project = project;
        }

        private object AddConditional(object owner, string conditional)
        {
            if (string.IsNullOrEmpty(conditional)) return owner;

            object ret = owner;

            if (m_expIfEq.IsMatch(conditional))
            {
                Match m = m_expIfEq.Match(conditional);
                If cond = new If();

                cond.Name = m.Groups[1].Value;
                cond.Value = m.Groups[2].Value;

                (owner.GetType().GetProperty(typeof(List<If>).Name).GetValue(owner, null) as IList).Add(cond);

                ret = cond;
            }
            else if (m_expIfIn.IsMatch(conditional))
            {
                Match m = m_expIfIn.Match(conditional);
                If cond = new If();

                cond.Name = m.Groups[1].Value;
                cond.In = m.Groups[2].Value;

                (owner.GetType().GetProperty(typeof(List<If>).Name).GetValue(owner, null) as IList).Add(cond);

                ret = cond;
            }
            else if (m_expIfDef.IsMatch(conditional))
            {
                Match m = m_expIfDef.Match(conditional);
                IfDefined cond = new IfDefined();

                cond.Name = m.Groups[1].Value;

                (owner.GetType().GetProperty(typeof(List<IfDefined>).Name).GetValue(owner, null) as IList).Add(cond);

                ret = cond;
            }
            else if (m_expIfNotDef.IsMatch(conditional))
            {
                Match m = m_expIfNotDef.Match(conditional);
                IfNotDefined cond = new IfNotDefined();

                cond.Name = m.Groups[1].Value;

                (owner.GetType().GetProperty(typeof(List<IfNotDefined>).Name).GetValue(owner, null) as IList).Add(cond);

                ret = cond;
            }

            return ret;
        }

        private ICollection CreateSortedList(IList list)
        {
            SortedList sort = new SortedList(list.Count);

            foreach (object o in list)
            {
                int order = 0;
                object val = o.GetType().GetProperty("Order").GetValue(o, null);
                if (val != null)
                {
                    order = (int)val;
                }
                while (sort.ContainsKey(order))
                {
                    order++;
                }
                sort.Add(order, o);
            }

            return sort.Values;
        }

        private object GetObjectFromProperty(object o, Type propType)
        {
            foreach (PropertyInfo pi in o.GetType().GetProperties())
            {
                if (pi.PropertyType == propType)
                {
                    return pi.GetValue(o, null);
                }
            }

            return null;
        }

        private IList GetCollectionFromObject(object o, Type collectionType)
        {
            foreach (PropertyInfo pi in o.GetType().GetProperties())
            {
                if (pi.PropertyType == collectionType)
                {
                    return pi.GetValue(o, null) as IList;
                }
            }

            return null;
        }

        private void AddEnvVarCollection(object owner, EnvVars varSet)
        {
            object setOwner = AddConditional(owner, varSet.Conditional);

            foreach (EnvVar var in varSet.EnvVarCollection)
            {
                object varOwner = AddConditional(setOwner, var.Conditional);

                List<Set> list = GetCollectionFromObject(varOwner, typeof(List<Set>)) as List<Set>;

                Set env = new Set();
                env.Name = var.Name;
                env.Value = var.Value;

                list.Add(env);
            }
            foreach (EnvVars set in varSet.EnvVarsCollection)
            {
                AddEnvVarCollection(setOwner, set);
            }
        }

        public void GenerateScatterFile(string path)
        {
            m_execRegionOrderMap.Clear();

            string fileName = path + "\\" + ScatterfileNameStart + m_project.Name + ScatterfileExtension;

            if (File.Exists(fileName)) File.Delete(fileName);

            ScatterFile scatterData = new ScatterFile();

            //TODO: FIX THIS
            MemoryMap map = m_project.MemoryMap;

            object baseOwner = AddConditional(scatterData, map.Conditional);

            AddEnvVarCollection(baseOwner, map.EnvironmentVariables);

            foreach (MemoryRegion region in map.Regions) //CreateSortedList(map.RegionsCollection))
            {
                object owner = AddConditional(baseOwner, region.Conditional);

                IList list = GetCollectionFromObject(owner, typeof(List<LoadRegion>));

                LoadRegion lr = new LoadRegion();
                lr.Name = region.Name;
                lr.Base = region.Address;
                lr.Options = region.Options;
                lr.Size = region.Size;

                list.Add(lr);

                SortedList erList = new SortedList();
                m_execRegionOrderMap[lr.Name] = erList;

                foreach (MemorySection sec in CreateSortedList(region.Sections))
                {
                    owner = AddConditional(lr, sec.Conditional);

                    list = GetCollectionFromObject(owner, typeof(List<ExecRegion>));

                    ExecRegion er = new ExecRegion();
                    er.Name = sec.Name;
                    er.Base = sec.Address;
                    er.Options = sec.Options;
                    er.Size = sec.Size;

                    erList.Add(sec.Order, er);

                    list.Add(er);

                    /*
                    SortedList symList = new SortedList();
                    m_symOrderMap[er.Name] = symList;
                    */

                    foreach (MemorySymbol sym in CreateSortedList(sec.Symbols))
                    {
                        owner = AddConditional(er, sym.Conditional);

                        list = GetCollectionFromObject(owner, typeof(List<FileMapping>));

                        FileMapping fm = new FileMapping();
                        fm.Name = sym.Name;
                        fm.Options = sym.Options;

                        //symList.Add(sym.Order, fm);

                        list.Add(fm);
                    }
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(ScatterFile));
                xmlSer.Serialize(stream, scatterData);

                stream.Seek(0, SeekOrigin.Begin);

                XPathDocument xp = new XPathDocument(stream);
                XPathNavigator nav = xp.CreateNavigator();
                XPathNodeIterator lrs = nav.SelectDescendants("LoadRegion", ScatterfileNamespace, false);

                while (lrs.MoveNext())
                {
                    string lrName = lrs.Current.GetAttribute("Name", "");

                    XPathNodeIterator ers = lrs.Current.SelectDescendants("ExecRegion", ScatterfileNamespace, false);

                    while (ers.MoveNext())
                    {
                        string erName = ers.Current.GetAttribute("Name", "");

                        XPathNavigator navParent = ers.Current.CreateNavigator();
                        XPathNavigator navTrail = navParent.CreateNavigator();

                        while (navParent.MoveToParent())
                        {
                            if (navParent.Name == "LoadRegion")
                            {
                                break;
                            }
                            navTrail.MoveToParent();
                        }

                        m_execRegionsToNavNode[lrName + ":" + erName] = navTrail.CreateNavigator();

                        /*
                        XPathNodeIterator files = ers.Current.SelectDescendants("FileMapping", ScatterfileNamespace, false);
                        while (files.MoveNext())
                        {
                            string fm = ers.Current.GetAttribute("Name", "");

                            navParent = files.Current.CreateNavigator();
                            navTrail = navParent.CreateNavigator();

                            while (navParent.MoveToParent())
                            {
                                if (navParent.Name == "ExecRegion")
                                {
                                    break;
                                }
                                navTrail.MoveToParent();
                            }

                            m_symToNavNode[erName + ":" + fm] = navTrail.CreateNavigator();
                        }
                        */
                    }
                }

                XmlWriterSettings xmlSet = new XmlWriterSettings();
                xmlSet.Indent = true;
                xmlSet.NewLineChars = "\r\n";


                using (XmlWriter writer = XmlWriter.Create(fileName, xmlSet))
                {
                    writer.WriteStartDocument();
                    nav.MoveToFirstChild();

                    WriteNextNode(writer, nav, true);

                    writer.WriteEndDocument();
                    //writer.WriteNode(nav, false);
                    writer.Flush();
                }
            }
        }

        private void WriteNextNode(XmlWriter writer, XPathNavigator nav, bool iterateSiblings)
        {
            if (nav.Name == "ScatterFile")
            {
                writer.WriteStartElement(nav.Name, nav.NamespaceURI);
            }
            else
            {
                writer.WriteStartElement(nav.Name);
            }

            XPathNavigator attrib = nav.CreateNavigator();
            if (attrib.MoveToFirstAttribute())
            {
                do
                {
                    writer.WriteAttributeString(attrib.Name, attrib.Value);
                }
                while (attrib.MoveToNextAttribute());
            }

            if (nav.Name == "LoadRegion")
            {
                string lrName = nav.GetAttribute("Name", "");

                foreach (ExecRegion er in (m_execRegionOrderMap[lrName] as SortedList).Values)
                {
                    WriteNextNode(writer, m_execRegionsToNavNode[lrName + ":" + er.Name] as XPathNavigator, false);
                }
            }
            /*
            else if (nav.Name == "ExecRegion")
            {
                string erName = nav.GetAttribute("Name", "");

                foreach (FileMapping fm in (m_symOrderMap[erName] as SortedList).Values)
                {
                    WriteNextNode(writer, m_symToNavNode[erName + ":" + fm.Name] as XPathNavigator, false);
                }
            }
            */
            else
            {
                XPathNodeIterator children = nav.SelectChildren(XPathNodeType.Element);
                while (children.MoveNext())
                {
                    WriteNextNode(writer, children.Current, iterateSiblings);
                }
            }
            writer.WriteEndElement();

            if (iterateSiblings && nav.MoveToNext())
            {
                WriteNextNode(writer, nav, iterateSiblings);
            }
        }

        enum MemberPrecidence : int
        {
            Unknown = -1,
            ErrorMember = 0,
            EnvMember = 1,
            SetMember = 2,
            LoadRegion = 3,
            ExecRegion = 3,
            FileMapping = 3,
        };

        private string GetConditionalName(object cond)
        {
            string ret = "";

            PropertyInfo pi = cond.GetType().GetProperty("Name");

            if (pi != null)
            {
                ret = (string)pi.GetValue(cond, null);
            }

            return ret;
        }

        private string GetConditionalValue(object cond)
        {
            string ret = "";

            If ifCond = cond as If;
            IfDefined ifDef = cond as IfDefined;
            IfNotDefined ifNotDef = cond as IfNotDefined;

            if (ifCond != null)
            {
                if (ifCond.In != null)
                {
                    ret = string.Format(IfInFormat, ifCond.Name, ifCond.In);
                }
                else if (ifCond.Value != null)
                {
                    ret = string.Format(IfEqFormat, ifCond.Name, ifCond.Value);
                }

            }
            else if (ifDef != null)
            {
                ret = string.Format(IfDefFormat, ifDef.Name);
            }
            else if (ifNotDef != null)
            {
                ret = string.Format(IfNotDefFormat, ifNotDef.Name);
            }

            return ret;
        }

        private MemberPrecidence LoadScatterConditional(object owner, object conditional, List<MemoryMap> maps, ArrayList conditionalStack)
        {
            MemberPrecidence ret = MemberPrecidence.Unknown;

            foreach (object cond in (conditional as IList))
            {
                //if (owner is MemoryMap && ret >= MemberPrecidence.LoadRegion)
                //{
                //    MemoryMap map   = new MemoryMap();
                //    map.Name        = GetConditionalName(cond);
                //    map.Conditional = GetConditionalValue(cond);
                //    owner = map;
                //    maps.Add(map);
                //}

                conditionalStack.Add(cond);

                ret = LoadScatterElements(owner, cond, maps, conditionalStack);

                conditionalStack.Remove(cond);
            }

            return ret;
        }

        private MemberPrecidence LoadScatterElements(object owner, object conditional, List<MemoryMap> maps, ArrayList conditionalStack)
        {
            MemberPrecidence ret = MemberPrecidence.Unknown;

            List<If> ifs = GetCollectionFromObject(conditional, typeof(List<If>)) as List<If>;
            if (ifs != null && ifs.Count > 0)
            {
                LoadScatterConditional(owner, ifs, maps, conditionalStack);
            }
            List<IfDefined> ifdefs = GetCollectionFromObject(conditional, typeof(List<IfDefined>)) as List<IfDefined>;
            if (ifdefs != null)
            {
                LoadScatterConditional(owner, ifdefs, maps, conditionalStack);
            }
            List<IfNotDefined> ifndefs = GetCollectionFromObject(conditional, typeof(List<IfNotDefined>)) as List<IfNotDefined>;
            if (ifndefs != null)
            {
                LoadScatterConditional(owner, ifndefs, maps, conditionalStack);
            }

            List<LoadRegion> lrCollection = GetCollectionFromObject(conditional, typeof(List<LoadRegion>)) as List<LoadRegion>;
            if (lrCollection != null && lrCollection.Count > 0)
            {
                List<MemoryRegion> regionSet = GetCollectionFromObject(owner, typeof(List<MemoryRegion>)) as List<MemoryRegion>;

                ret = MemberPrecidence.LoadRegion;

                foreach (LoadRegion lr in lrCollection)
                {
                    MemoryRegion region = new MemoryRegion();
                    region.Name = lr.Name;
                    region.Address = lr.Base;
                    region.Options = lr.Options;
                    region.Size = lr.Size;
                    //                    region.Order = regionSet.Count;
                    region.Conditional = GetConditionalValue(conditional);

                    regionSet.Add(region);

                    ret = LoadScatterElements(region, lr, maps, conditionalStack);
                }
            }

            List<ExecRegion> erCollection = GetCollectionFromObject(conditional, typeof(List<ExecRegion>)) as List<ExecRegion>;
            if (erCollection != null && erCollection.Count > 0)
            {
                List<MemorySection> sectionSet = GetCollectionFromObject(owner, typeof(List<MemorySection>)) as List<MemorySection>;

                ret = MemberPrecidence.ExecRegion;

                foreach (ExecRegion er in erCollection)
                {
                    MemorySection section = new MemorySection();
                    section.Name = er.Name;
                    section.Address = er.Base;
                    section.Options = er.Options;
                    section.Size = er.Size;
                    section.Order = (int)m_execRegionOrderMap[(owner as MemoryRegion).Name + ":" + er.Name];
                    section.Conditional = GetConditionalValue(conditional);

                    sectionSet.Add(section);

                    ret = LoadScatterElements(section, er, maps, conditionalStack);
                }
            }

            List<FileMapping> fileCollection = GetCollectionFromObject(conditional, typeof(List<FileMapping>)) as List<FileMapping>;
            if (fileCollection != null && fileCollection.Count > 0)
            {
                List<MemorySymbol> symSet = GetCollectionFromObject(owner, typeof(List<MemorySymbol>)) as List<MemorySymbol>;

                ret = MemberPrecidence.FileMapping;


                foreach (FileMapping fm in fileCollection)
                {
                    MemorySymbol sym = new MemorySymbol();
                    sym.Name = fm.Name;
                    sym.Options = fm.Options;
                    sym.Conditional = GetConditionalValue(conditional);

                    symSet.Add(sym);
                }
            }

            List<Set> setCollection = GetCollectionFromObject(conditional, typeof(List<Set>)) as List<Set>;
            if (setCollection != null && setCollection.Count > 0)
            {
                EnvVars envSets = GetObjectFromProperty(owner, typeof(EnvVars)) as EnvVars;

                if (ret < MemberPrecidence.SetMember)
                {
                    ret = MemberPrecidence.SetMember;
                }

                EnvVars envSet = null;

                // conditional belongs to the map if we have a load region in this conditional
                if (conditionalStack.Count == 0)
                {
                    envSet = FindMatchingSet(envSets, "Global");

                    if (envSet == null)
                    {
                        envSet = new EnvVars();
                        envSet.Name = "Global";
                        envSets.EnvVarsCollection.Add(envSet);
                    }
                }
                else
                {
                    for (int i = 0; i < conditionalStack.Count; i++)
                    {
                        object cond = conditionalStack[i];

                        if (i == (conditionalStack.Count - 1) && owner is MemoryMap && ret >= MemberPrecidence.LoadRegion)
                        {
                            ((MemoryMap)owner).Name = GetConditionalName(conditional);
                            ((MemoryMap)owner).Conditional = GetConditionalValue(conditional);
                        }
                        else
                        {
                            string name = GetConditionalName(cond);
                            string value = GetConditionalValue(cond);

                            name += "_" + value.Replace(" ", "_").Replace("\"", "");

                            envSet = FindMatchingSet(envSets, name);

                            if (envSet == null)
                            {
                                envSet = new EnvVars();
                                envSet.Name = name;
                                envSet.Conditional = value;

                                envSets.EnvVarsCollection.Add(envSet);
                            }

                            //envSets = envSet.EnvVarsCollection;
                        }
                    }
                }


                if ((int)ret < (int)MemberPrecidence.SetMember)
                {
                    ret = MemberPrecidence.SetMember;
                }

                foreach (Set set in GetCollectionFromObject(conditional, typeof(List<Set>)))
                {
                    EnvVar var = new EnvVar();
                    var.Name = set.Name;
                    var.Value = set.Value;

                    envSet.EnvVarCollection.Add(var);
                }
            }

            return ret;
        }

        private EnvVars FindMatchingSet(EnvVars sets, string name)
        {
            if (sets.Name == name)
            {
                return sets;
            }

            foreach (EnvVars set in sets.EnvVarsCollection)
            {
                if (set.Name == name)
                {
                    return set;
                }
            }
            return null;
        }

        public List<MemoryMap> LoadFromFile(string filePath)
        {

            List<MemoryMap> maps = new List<MemoryMap>();
            m_execRegionOrderMap.Clear();

            using (XmlReader reader = XmlReader.Create(filePath))
            {
                try
                {
                    XPathDocument doc = new XPathDocument(reader);
                    XPathNavigator nav = doc.CreateNavigator();

                    XPathNodeIterator lrs = nav.SelectDescendants("LoadRegion", ScatterfileNamespace, false);
                    while (lrs.MoveNext())
                    {
                        int order = 0;

                        string lrName = lrs.Current.GetAttribute("Name", "");

                        XPathNodeIterator ers = nav.SelectDescendants("ExecRegion", ScatterfileNamespace, false);

                        while (ers.MoveNext())
                        {
                            string erName = ers.Current.GetAttribute("Name", "");

                            m_execRegionOrderMap[lrName + ":" + erName] = order++;
                        }
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Invalid scatterfile format");
                }
            }

            using (XmlReader reader = XmlReader.Create(filePath))
            {
                try
                {
                    XmlSerializer xmlSer = new XmlSerializer(typeof(ScatterFile));
                    ScatterFile scatter = xmlSer.Deserialize(reader) as ScatterFile;

                    MemoryMap map = new MemoryMap();
                    map.Name = "MAP";
                    maps.Add(map);

                    LoadScatterElements(map, scatter, maps, new ArrayList());
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("Invalid scatterfile format");
                }
            }
            return maps;
        }
    }
}
