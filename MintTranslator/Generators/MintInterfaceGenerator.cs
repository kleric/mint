using Antlr4.Runtime;
using MintTranslator.AST;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Type = MintTranslator.AST.Type;

namespace MintTranslator.Generators
{
    public class MintInterfaceGenerator
    {
        private Dictionary<string, List<Call>> _invocations;

        private Dictionary<string, Type> _vars;

        private ASTGenerator _astGenerator;

        public MintInterfaceGenerator()
        {
            _astGenerator = new ASTGenerator();
            _vars = new Dictionary<string, Type>();
            _invocations = new Dictionary<string, List<Call>>();
        }

        public void OutputInterface()
        {
            var pathBase = "mint_interfaces/";
            var interfaces = GetInterfaces();
            foreach (var type in interfaces.Keys)
            {
                var outputSb = new StringBuilder();
                Dictionary<string, HashSet<InterfaceMethod>> methods = interfaces[type];
                foreach (var methodName in methods.Keys)
                {
                    foreach (var variation in methods[methodName])
                    {
                        outputSb.Append(variation.ToString());
                        outputSb.AppendLine();
                    }
                }
                File.WriteAllText(pathBase + type + ".mint", outputSb.ToString());
            }
        }
        public void AnalyzeInterfaces(string inputDir)
        {
            ProcessDirectory(inputDir);
            OutputInterface();
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

                ParseMethodInvocations(node);
            }

        }

        public Dictionary<string, Dictionary<string, HashSet<InterfaceMethod>>> GetInterfaces()
        {
            var interfaces = new Dictionary<string, Dictionary<string, HashSet<InterfaceMethod>>>();
            foreach (var type in _invocations.Keys)
            {
                Dictionary<string, HashSet<InterfaceMethod>> methods = new Dictionary<string, HashSet<InterfaceMethod>>();
                foreach (var invoke in _invocations[type])
                {
                    var m = new InterfaceMethod();
                    m.Name = invoke.MethodName;
                    m.Args = invoke.Invocation.Args.Select(x => x.InferredType).ToList();
                    m.ReturnType = invoke.Invocation.InferredType;

                    if (!methods.ContainsKey(m.Name)) methods[m.Name] = new HashSet<InterfaceMethod>();

                    bool combined = false;
                    foreach (var me in methods[m.Name])
                    {
                        combined = me.Combine(m);
                        if (combined) break;
                    }
                    if (!combined) methods[m.Name].Add(m);
                }
                interfaces[type] = methods;
            }
            return interfaces;
        }
        public class InterfaceMethod
        {
            public Type ReturnType { get; set; }
            public string Name { get; set; }
            public List<Type> Args { get; set; }

            public InterfaceMethod() { }
            public InterfaceMethod(FunctionDeclaration funcDecl)
            {
                Name = funcDecl.Identifier;
                ReturnType = funcDecl.ReturnType;
                Args = funcDecl.Parameters.Select(p => { if (p.Type.Name == "unk_type") return null; return p.Type; }).ToList();
            }

            public bool Combine(InterfaceMethod m)
            {
                if (Args.Count != m.Args.Count) return false;
                var newArgs = new List<Type>();
                for (int i = 0; i < Args.Count; i++)
                {
                    var a1 = Args[i];
                    var a2 = m.Args[i];
                    if (a1 == null)
                    {
                        newArgs.Add(a2);
                        continue;
                    } 
                    if (a2 == null)
                    {
                        newArgs.Add(a1);
                        continue;
                    }
                    if (a1.Name.Equals(a2.Name))
                    {
                        newArgs.Add(a1);
                        continue;
                    }
                    else return false;
                }
                Args = newArgs;
                return true;
            }
            public override bool Equals(object obj)
            {
                if (Object.ReferenceEquals(obj, this)) return true;
                InterfaceMethod other = obj as InterfaceMethod;
                if (Object.ReferenceEquals(null, other)) return false;
                if (!String.Equals(Name, other.Name, StringComparison.Ordinal)) return false;
                return Enumerable.SequenceEqual(Args, other.Args);
            }
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("server ");

                if (ReturnType == null) sb.Append("void");
                else sb.Append(ReturnType.Name);

                sb.Append(Name);
                sb.Append('(');
                bool first = true;
                foreach (var arg in Args)
                {
                    if (first) first = false;
                    else sb.Append(", ");
                    if (arg == null) sb.Append("unk_type");
                    else sb.Append(arg.Name);
                    sb.Append(" _");
                }
                sb.Append(") { }");
                sb.AppendLine();
                return sb.ToString();
            }
            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }
        }
        public class Call
        {
            public string MethodName { get; set; }
            public MethodInvocation Invocation { get; set; }
        }
        public void ParseMethodInvocations(Node root)
        {
            Dictionary<string, Type> temp = null;
            if (root is VariableDeclarator)
            {
                var decl = (VariableDeclarator)root;
                var varDecl = decl.Parent as VariableDeclaration;
                _vars[decl.Identifier] = varDecl.Type;
            } 
            else if (root is MethodInvocation)
            {
                var invoke = root as MethodInvocation;
                if (invoke.Method is MemberAccess)
                {
                    var memberAccess = invoke.Method as MemberAccess;
                    var obj = memberAccess.Object as VariableExpression;
                    if (obj != null && _vars.ContainsKey(obj.Identifier)) 
                    { 
                        var variableType = _vars[obj.Identifier].Name;
                        if (!_invocations.ContainsKey(variableType))
                        {
                            _invocations[variableType] = new List<Call>();
                        }
                        Call call = new Call();
                        call.MethodName = memberAccess.Member;
                        call.Invocation = invoke;
                        _invocations[variableType].Add(call);
                    }
                }
            } else if (root is FunctionDeclaration)
            {
                _vars.Clear();
            } 
            else if (root is Block)
            {
                temp = _vars;
                _vars = new Dictionary<string, Type>(temp);
            }
            ApplyToChildren(root, ParseMethodInvocations);
            if (temp != null)
            {
                _vars = temp;
            }
        }
        private void ApplyToChildren(Node node, Action<Node> func)
        {
            foreach (var child in node.GetChildren())
            {
                if (child != null) func.Invoke(child);
            }
        }
    }
}
