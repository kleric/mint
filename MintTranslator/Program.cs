using Antlr4.Runtime;
using MintTranslator.AST;
using MintTranslator.Generators;
using System;
using System.IO;
using System.Text;

namespace MintTranslator
{
    class Program
    {
        private ASTGenerator _astGenerator;

        private string _inputPath;
        private string _outputPath;
        static void Main(string[] args)
        {
            NavigateToRoot();

            #region Generate C# Output
            Program p = new Program("scripts/csharp");
            p.Process("scripts/mint");
            #endregion

            #region Generate Interfaces
            //MintInterfaceGenerator gen = new MintInterfaceGenerator();
            //gen.AnalyzeInterfaces("scripts/mint");
            #endregion
        }

        public static void NavigateToRoot()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (Directory.Exists("mint_interfaces"))
                    return;

                Directory.SetCurrentDirectory("..");
            }
        }

        private Program(string outputPath)
        {
            _astGenerator = new ASTGenerator();

            _outputPath = outputPath;
        }

        public void Process(string inputDir)
        {
            _inputPath = inputDir;
            ProcessDirectory(inputDir);
        }
        private void ProcessDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        private void ProcessFile(string path)
        {
            if (path.EndsWith(".mint"))
            {
                path = path.Replace("\\", "/");
                Console.WriteLine(path);
                ICharStream target = new AntlrInputStream(File.ReadAllText(path));
                ITokenSource lexer = new MintDLexer(target);
                ITokenStream tokens = new CommonTokenStream(lexer);
                MintDParser parser = new MintDParser(tokens);
                var stList = parser.statementList();
                var node = _astGenerator.VisitStatementList(stList);
                ASTProcessor processor = new ASTProcessor();
                processor.ProcessAst(node);

                AuraPrinter p = new AuraPrinter();
                var outputBase = _outputPath;
                var rel = Path.GetDirectoryName(Path.GetRelativePath(_inputPath, path).Replace("\\", "/"));
                outputBase += "/" + rel;
                Directory.CreateDirectory(outputBase);

                var scriptName = Path.GetFileNameWithoutExtension(path);
                var output = outputBase + "/" + scriptName + ".cs";

                OutputCSharpFile(output, scriptName, p.PrintAST(node, 2));
                /*foreach (var script in p.ExternalScriptRefs)
                {
                   // TODO: process external script references 
                }*/
            }
           
        }

        private void OutputCSharpFile(string path, string scriptName, string ast)
        {
            var sb = new StringBuilder();
            sb.Append(
                @"
using Aura.Channel.Scripting.Mint;
using Aura.Channel.World.Entities;
using Aura.Mabi.Mint;

namespace Aura.Mabi.Mint { 
    public class mint_");
            sb.Append(scriptName);
            sb.AppendLine(" : MintScript {");
            sb.Append(ast);
            sb.AppendLine("    }");
            sb.AppendLine("}");
            File.WriteAllText(path, sb.ToString());
        }
    }
}
