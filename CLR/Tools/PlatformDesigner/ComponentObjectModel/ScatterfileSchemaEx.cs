using System;
using System.Collections.Generic;

namespace XsdScatterfileSchemaObject
{
    public partial class ScatterFile : object
    {
        public ScatterFile()
        {
            this.ifDefinedField = new List<IfDefined>();
            this.ifNotDefinedField = new List<IfNotDefined>();
            this.loadRegionField = new List<LoadRegion>();
            this.setField = new List<Set>();
        }
    }
    public partial class IfDefined : object
    {
        public IfDefined()
        {
            this.execRegionField = new List<ExecRegion>();
            this.fileMappingField = new List<FileMapping>();
            this.ifDefined1Field = new List<IfDefined>();
            this.ifField = new List<If>();
            this.ifNotDefinedField = new List<IfNotDefined>();
            this.loadRegionField = new List<LoadRegion>();
        }
    }
    public partial class IfNotDefined : object
    {
        public IfNotDefined()
        {
            this.fileMappingField = new System.Collections.Generic.List<FileMapping>();
            this.execRegionField = new List<ExecRegion>();
            this.ifDefinedField = new List<IfDefined>();
            this.ifField = new List<If>();
            this.ifNotDefined1Field = new List<IfNotDefined>();
            this.loadRegionField = new List<LoadRegion>();
        }
    }
    public partial class If : Object
    {
        public If()
        {
            this.errorField = new Error();
            this.execRegionField = new List<ExecRegion>();
            this.fileMappingField = new List<FileMapping>();
            this.if1Field = new List<If>();
            this.ifDefinedField = new List<IfDefined>();
            this.ifNotDefinedField = new List<IfNotDefined>();
            this.loadRegionField = new List<LoadRegion>();
            this.setField = new List<Set>();
        }
    }
    public partial class LoadRegion : object
    {
        public LoadRegion()
        {
            this.execRegionField = new List<ExecRegion>();
            this.ifDefinedField = new List<IfDefined>();
            this.ifField = new List<If>();
            this.ifNotDefinedField = new List<IfNotDefined>();
        }
    }
    public partial class ExecRegion : Object
    {
        public ExecRegion()
        {
            this.fileMappingField = new List<FileMapping>();
            this.ifDefinedField = new List<IfDefined>();
            this.ifField = new List<If>();
            this.ifNotDefinedField = new List<IfNotDefined>();
        }
    }
}
