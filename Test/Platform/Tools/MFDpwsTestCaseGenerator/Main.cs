using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
//using Microsoft.Build.BuildEngine;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.Build.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;

namespace MFDpwsTestCaseGenerator
{
    public class TestCaseGenerator : Task 
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Usage();
            }

            var gen = new TestCaseGenerator();
            var list = new List<string>();

            foreach (string arg in args)
            {
                string lower = arg.ToLower();

                if (lower.EndsWith(".wsdl"))
                {
                    gen.WsdlFile = arg;
                }
                else if (lower.EndsWith(".cs"))
                {
                    list.Add(arg);
                }
                else
                {
                    try
                    {
                        gen.TestIterations = int.Parse(arg);
                    }
                    catch
                    {
                        Usage();
                    }
                }
            }

            gen.BuildTreeRoot = Environment.GetEnvironmentVariable("BUILD_ROOT") + "\\Release";
            gen.BuildFlavor = "Release";
            gen.ComponentGuid = Guid.NewGuid().ToString();
            gen.CSharpFile = list.ToArray();
            
            gen.Execute();
        }

        static void Usage()
        {
            Console.WriteLine("This program has only one required argument.  " + 
                "A valid MF Web Service Definition Language (.wsdl) file.\r\n" +
                "The following are optional:\r\n" + 
                "An integer specifying how many times to execute a test case.\r\n" +
                "Any number of .cs files to add to the generated project.");
            Environment.Exit(1);
        }

        public override bool Execute()
        {
            string currentDirectory = Environment.CurrentDirectory;

            try
            {
                WsdlFileX wsdl = new WsdlFileX(m_wsdlFile);

                Environment.CurrentDirectory = Path.GetDirectoryName(wsdl.FileInfo.FullName);

                string service = Path.GetFileNameWithoutExtension(wsdl.FileInfo.Name);

                MFSvcUtil svcutil = new MFSvcUtil(m_buildTreeRoot);
                svcutil.WSDL = wsdl;
                if (!svcutil.GenerateCode())
                {
                    return false;
                }

                string assemblyName = service + "TestFixture";

                ServiceProject sp = new ServiceProject(
                    svcutil.Interface,
                    svcutil.ClientProxy,
                    svcutil.HostedService,
                    wsdl)
                {
                    AssemblyName = assemblyName,
                    RootNamespace = assemblyName
                };

                if (sp.IsSDKProject)
                {
                    sp.SetProperty("OutputPath", "bin\\DebugTemp");
                }

                sp.Save(assemblyName + ".csproj");

                MSBuildProcess msbuild = new MSBuildProcess();
                msbuild.Project = sp.FullPath;
                msbuild.Properties.Add("Configuration", m_buildFlavor);
                msbuild.Properties.Add("FLAVOR", m_buildFlavor);

                int result = msbuild.Build();

                if (result > 0)
                {
                    return false;
                }

                IEnumerable<ProjectItem> references = sp.GetItems("Reference");

                ILocator locator = LocatorFactory.GetLocator(m_buildTreeRoot);

                List<Assembly> referenceAssemblies = new List<Assembly>();

                foreach (ProjectItem reference in references)
                {
                    string mfAssemblyFile = locator.GetAssemblyFileInfo(reference.EvaluatedInclude).FullName;
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(mfAssemblyFile);
                    referenceAssemblies.Add(asm);
                }

                // Load the assembly just built.
                string assemblyFile = locator.TestAssemblies + "\\" + sp.AssemblyName + ".dll";
                Assembly assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);

                CodeCompileUnit ccu = new CodeCompileUnit();

                FixtureNamespace fns = new FixtureNamespace(
                    assembly, 
                    referenceAssemblies.ToArray())
                    {
                        TestIterations = m_testIterations
                    };

                ccu.Namespaces.Add(fns.Namespace);

                CSharpCodeProvider provider = new CSharpCodeProvider();

                string codeFile = assemblyName + ".cs";

                IndentedTextWriter itw = new IndentedTextWriter(
                    new StreamWriter(codeFile), "    ");

                CodeGeneratorOptions options = new CodeGeneratorOptions();
                options.BracingStyle = "C";

                provider.GenerateCodeFromCompileUnit(
                    ccu,
                    itw,
                    options);

                itw.Flush();
                itw.Close();

                // Update the project file before the final build
                if (sp.IsSDKProject)
                {
                    sp.SetProperty("OutputPath", "bin\\Debug");
                }
                
                // Add the generated file to the project
                sp.AddItem("Compile", codeFile);
                
                // Add user supplied c# files if any
                if (m_csharpFiles != null && m_csharpFiles.Length > 0)
                {
                    foreach (string csharpfile in m_csharpFiles)
                    {
                        sp.AddItem("Compile", csharpfile);
                    }
                }
                
                sp.SetProperty("OutputType", "Exe");
                sp.Save(assemblyName + ".csproj");

                msbuild.Target = null;
                result = msbuild.Build();

                if (result > 0)
                {
                    return false;
                }

                if (sp.IsDevProject)
                {
                    new SlnProj(assemblyName + ".csproj", new Guid(m_componentGuid)).Save();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\r\n\r\n" + e.StackTrace);
                return false;
            }
            finally
            {
                Environment.CurrentDirectory = currentDirectory;
            }
        }

        string m_wsdlFile;
        string m_componentGuid;
        string []m_csharpFiles;
        int m_testIterations;
        string m_buildTreeRoot;
        string m_buildFlavor;

        [Required]
        public string BuildFlavor
        {
            set { m_buildFlavor = value; }
            get { return m_buildFlavor; }
        }

        [Required]
        public string BuildTreeRoot
        {
            set { m_buildTreeRoot = value; }
            get { return m_buildTreeRoot; }
        }

        [Required]
        public string WsdlFile
        {
            set { m_wsdlFile = value; }
            get { return m_wsdlFile; }
        }

        [Required]
        public string ComponentGuid
        {
            set { m_componentGuid = value; }
            get { return m_componentGuid; }
        }

        public string[] CSharpFile
        {
            set { m_csharpFiles = value; }
            get { return m_csharpFiles; }
        }

        public int TestIterations
        {
            set { m_testIterations = value; }
            get { return m_testIterations; }
        }
    }
}
