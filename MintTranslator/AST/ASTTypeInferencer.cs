using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using static MintTranslator.Generators.MintInterfaceGenerator;

namespace MintTranslator.AST
{
    public class ASTTypeInferencer
    {
        private Dictionary<string, FunctionDeclaration> _funcs;
        private Dictionary<string, Node> _vars;
        private Dictionary<string, HashSet<InterfaceMethod>> _interfaceMethods;

        private void ReadFunctions(Node node)
        {
            node.ApplyToChildren(ReadFunctions);
            if (node is FunctionDeclaration)
            {
                var funcDecl = (FunctionDeclaration)node;
                _funcs[funcDecl.Identifier] = funcDecl;
            }
        }

        private void ReadInterfaces()
        {
            var pathBase = "mint_interfaces/";
            ProcessInterfaceDirectory(pathBase);
        }

        private void ProcessInterfaceDirectory(string targetDirectory)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessInterfaceFile(fileName);

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessInterfaceDirectory(subdirectory);
        }

        private void ProcessInterfaceFile(string path)
        {
            if (path.EndsWith(".mint"))
            {
                ICharStream target = new AntlrInputStream(File.ReadAllText(path));
                ITokenSource lexer = new MintDLexer(target);
                ITokenStream tokens = new CommonTokenStream(lexer);

                MintDParser parser = new MintDParser(tokens);
                var stList = parser.statementList();
                ASTGenerator generator = new ASTGenerator();
                var node = generator.VisitStatementList(stList);

                var typeName = Path.GetFileNameWithoutExtension(path);
                ReadInterfaces(node, typeName);
            }

        }
        public void ReadInterfaces(Node node, string typeName)
        {
            node.ApplyToChildren(n => ReadInterfaces(n, typeName));
            if (node is FunctionDeclaration)
            {
                var funcDecl = (FunctionDeclaration)node;
                InterfaceMethod m = new InterfaceMethod(funcDecl);
                if (!(_interfaceMethods.ContainsKey(typeName)))
                {
                    _interfaceMethods[typeName] = new HashSet<InterfaceMethod>();
                }
                _interfaceMethods[typeName].Add(m);
            }
        }

        private void Init(Node node)
        {
            _funcs = new Dictionary<string, FunctionDeclaration>();
            _vars = new Dictionary<string, Node>();
            _interfaceMethods = new Dictionary<string, HashSet<InterfaceMethod>>();

            ReadFunctions(node);
            ReadInterfaces();
        }
        public void InferTypes(Node node)
        {
            Init(node);

            Visit(node);
        }
        private void Visit(Node node)
        {
            if (node is FunctionDeclaration) VisitFunctionDeclaration(node as FunctionDeclaration);
            else if (node is VariableDeclaration) VisitVariableDeclaration(node as VariableDeclaration);
            else if (node is MethodInvocation) VisitMethodInvocation(node as MethodInvocation);
            else if (node is MemberAccess) VisitMemberAccess(node as MemberAccess);
            else if (node is TypeCastExpression) VisitTypeCastExp(node as TypeCastExpression);
            else if (node is AssignmentExpression) VisitAssignmentExp(node as AssignmentExpression);
            else if (node is BinaryExpression) VisitBinaryExpression(node as BinaryExpression);
            else if (node is UnaryExpression) VisitUnaryExpression(node as UnaryExpression);
            else if (node is IncExpression) VisitIncExpression(node as IncExpression);
            else if (node is DecExpression) VisitDecExpression(node as DecExpression);
            else if (node is ParenExpression) VisitParenExpression(node as ParenExpression);
            else if (node is LiteralExpression) VisitLiteralExpression(node as LiteralExpression);
            else if (node is LocalizationExpression) VisitLocalizationExpression(node as LocalizationExpression);
            else if (node is EvaluatedStrExpression) VisitEvaluatedStrExpression(node as EvaluatedStrExpression);
            else if (node is VariableExpression) VisitVariableExpression(node as VariableExpression);
            else if (node is Block) VisitBlock(node as Block);
            else if (node is MethodArg) VisitMethodArg(node as MethodArg);
            else VisitNode(node);
        }
        private void VisitMethodArg(MethodArg arg)
        {
            Visit(arg.Expression);
            arg.InferredType = arg.Expression.InferredType;
        }
        private void VisitBlock(Block block)
        {
            var temp = _vars;
            _vars = new Dictionary<string, Node>(_vars);
            Visit(block.Statements);
            _vars = temp;
        }
        private void VisitVariableExpression(VariableExpression e)
        {
            if (_vars.ContainsKey(e.Identifier)) e.InferredType = _vars[e.Identifier].InferredType;
        }
        private void VisitEvaluatedStrExpression(EvaluatedStrExpression e)
        {
            Visit(e.Expression);
            e.InferredType = new Type() { Name = "string" };
        }
        private void VisitLocalizationExpression(LocalizationExpression l)
        {
            l.InferredType = new Type() { Name = "string" };
        }
        private void VisitLiteralExpression(LiteralExpression l)
        {
            l.InferredType = l.Literal.Type();
        }
        private void VisitParenExpression(ParenExpression p)
        {
            Visit(p.Expression);
            p.InferredType = p.Expression.InferredType;
        }
        private void VisitIncExpression(IncExpression i)
        {
            Visit(i.Expression);
            i.InferredType = i.Expression.InferredType;
        }
        private void VisitDecExpression(DecExpression i)
        {
            Visit(i.Expression);
            i.InferredType = i.Expression.InferredType;
        }
        private void VisitUnaryExpression(UnaryExpression u)
        {
            string o = "";
            switch (u.Op)
            {
                case UnaryOp.Inc:
                case UnaryOp.Dec:
                case UnaryOp.Minus:
                case UnaryOp.Plus:
                    o = "+";
                    break;
                case UnaryOp.Bang:
                    o = "!";
                    break;
            }
            Visit(u.Expression);
            u.InferredType = u.Expression.InferredType;
        }
        private void VisitBinaryExpression(BinaryExpression b)
        {
            Visit(b.Exp1);
            Visit(b.Exp2);
            bool numericType = false;
            switch (b.Op)
            {
                case BinaryOp.Add:
                case BinaryOp.Subtract:
                case BinaryOp.Multiply:
                case BinaryOp.Divide:
                case BinaryOp.Modulo:
                    numericType = true;
                    break;
            }
            if (!numericType)
            {
                b.InferredType = new Type() { Name = "bool" };
            } else
            {
                b.InferredType = Type.CombineType(b.Exp1.InferredType, b.Exp2.InferredType);
            }
        }
        private void VisitAssignmentExp(AssignmentExpression a)
        {
            Visit(a.Assignee);
            string o = "";
            switch (a.Op)
            {
                case AssignOp.Assign:
                    o = "=";
                    break;
                case AssignOp.AddAssign:
                case AssignOp.SubAssign:
                case AssignOp.DivAssign:
                case AssignOp.MultAssign:
                case AssignOp.ModAssign:
                    o = "%=";
                    break;
                case AssignOp.ANDAssign:
                case AssignOp.ORAssign:
                    o = "|=";
                    break;
            }
            Visit(a.Val);
            if (a.Assignee.InferredType != a.Val.InferredType)
            {
                Type resolvedType;
                try
                {
                    resolvedType = Type.CombineType(a.Assignee.InferredType, a.Val.InferredType);
                } 
                catch (Exception)
                {
                    if (a.Val is TypeCastExpression)
                    {
                        var castExp = (TypeCastExpression)a.Val;
                        castExp.InferredType = a.Assignee.InferredType;
                        castExp.Type = castExp.InferredType;
                    }
                    return;
                }
                if (a.Assignee is VariableExpression)
                {
                    var exp = (VariableExpression)a.Assignee;
                    if (!_vars.ContainsKey(exp.Identifier))
                    {
                        throw new System.Exception("Missing variable " + exp.Identifier);
                    }
                    var node = _vars[exp.Identifier];

                    if (node is VariableDeclaration)
                    {
                        ((VariableDeclaration)_vars[exp.Identifier]).Type = resolvedType;
                        a.Assignee.InferredType = resolvedType;
                    }
                    else if (node is Parameter)
                    {
                        a.Val.InferredType = Type.CombineType(((Parameter)_vars[exp.Identifier]).Type, a.Val.InferredType);
                    }
                } else
                {
                    a.Assignee.InferredType = resolvedType;
                    a.Val.InferredType = resolvedType;
                }
            } 
        }
        private void VisitNode(Node n)
        {
            foreach (var child in n.GetChildren())
            {
                Visit(child);
            }
        }

        private void VisitTypeCastExp(TypeCastExpression t)
        {
            Visit(t.Expression);
            t.InferredType = t.Type;
        }
        private void VisitMemberAccess(MemberAccess m)
        {
            Visit(m.Object);
        }
        private void VisitMethodInvocation(MethodInvocation m)
        {
            Visit(m.Method);
            foreach (var e in m.Args)
            {
                Visit(e);
            }
            var memberAccess = m.Method as MemberAccess;
            if (memberAccess == null) return;
            if (memberAccess.Object.InferredType == null) return;
            var type = memberAccess.Object.InferredType.Name;
            if (!_interfaceMethods.ContainsKey(type)) return;
            var interfaces = _interfaceMethods[type];
            foreach (var method in interfaces)
            {
                if (method.Name != memberAccess.Member) continue;
                if (method.Args.Count != m.Args.Count) continue;
                bool match = true;
                for (int i = 0; i < method.Args.Count; i++)
                {
                    if (method.Args[i] == null) continue;
                    if (m.Args[i].InferredType == null) continue;
                    if (method.Args[i].Name == m.Args[i].InferredType.Name) continue;
                    match = false;
                    break;
                }
                if (match)
                {
                    m.InferredType = method.ReturnType;
                    for (int i = 0; i < method.Args.Count; i++)
                    {
                        if (method.Args[i] == null) continue;
                        m.Args[i].InferredType = method.Args[i];
                    }
                    return;
                }
            }
        }
        private void VisitFunctionDeclaration(FunctionDeclaration f)
        {
            _funcs[f.Identifier] = f;
            var temp = _vars;
            _vars = new Dictionary<string, Node>(_vars);
            foreach (var param in f.Parameters)
            {
                param.InferredType = param.Type;
                _vars[param.Identifier] = param;
            }

            VisitBlock(f.Block);

            _vars = temp;
        }

        private void VisitVariableDeclaration(VariableDeclaration v)
        {
            foreach (var decl in v.Declarators)
            {
                v.InferredType = v.Type;
                _vars[decl.Identifier] = v;
                decl.InferredType = v.Type;
                VisitVariableDeclarator(decl, v.Type);
                if (!v.Type.Equals(decl.InferredType))
                {
                    Type resolvedType;
                    try
                    {
                        resolvedType = Type.CombineType(v.Type, decl.InferredType);
                    }
                    catch (Exception)
                    {
                        if (decl.InitialValue is TypeCastExpression)
                        {
                            var castExp = (TypeCastExpression)decl.InitialValue;
                            castExp.InferredType = v.Type;
                            castExp.Type = castExp.InferredType;
                        }
                        continue;
                    }
                    v.Type = resolvedType;
                    decl.InferredType = resolvedType;
                }
            }
        }

        private void VisitVariableDeclarator(VariableDeclarator v, Type t)
        {
            if (v.InitialValue != null)
            {
                Visit(v.InitialValue);
                if (v.InitialValue.InferredType == null)
                {
                    v.InitialValue.InferredType = t;
                } else if (!v.InitialValue.InferredType.Equals(t))
                {
                    v.InferredType = v.InitialValue.InferredType;
                }
            }
        }
    }
}
