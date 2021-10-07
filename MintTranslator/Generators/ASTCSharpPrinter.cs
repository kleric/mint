using System;
using System.Collections.Generic;
using System.Text;

namespace MintTranslator.AST
{
    public class ASTCSharpPrinter
    {
        public HashSet<string> ExternalScriptRefs { get; set; }
        protected StringBuilder _sb;
        private int _depth = 0;
        public ASTCSharpPrinter()
        {
            _sb = new StringBuilder();
            ExternalScriptRefs = new HashSet<string>();
        }
        public string PrintAST(Node node, int initDepth = 0)
        {
            _depth = initDepth;
            _sb = new StringBuilder();
            ExternalScriptRefs.Clear();
            Print(node);
            return _sb.ToString();
        }

        protected void Print(Node node)
        {
            if (node is StatementList) PrintStatementList(node as StatementList);
            else if (node is FunctionDeclaration) PrintFunctionDeclaration(node as FunctionDeclaration);
            else if (node is VariableDeclaration) PrintVariableDeclaration(node as VariableDeclaration);
            else if (node is IfStatement) PrintIfStatement(node as IfStatement);
            else if (node is SwitchStatement) PrintSwitchStatement(node as SwitchStatement);
            else if (node is WhileStatement) PrintWhile(node as WhileStatement);
            else if (node is DoWhileStatement) PrintDoWhile(node as DoWhileStatement);
            else if (node is BreakStatement) PrintBreak();
            else if (node is ContinueStatement) PrintContinue();
            else if (node is ReturnStatement) PrintReturn(node as ReturnStatement);
            else if (node is ForStatement) PrintFor(node as ForStatement);
            else if (node is ExpressionStatement) PrintExpressionStatement(node as ExpressionStatement);
            else if (node is MethodInvocation) PrintMethodInvocation(node as MethodInvocation);
            else if (node is MemberAccess) PrintMemberAccess(node as MemberAccess);
            else if (node is TypeCastExpression) PrintTypeCastExp(node as TypeCastExpression);
            else if (node is AssignmentExpression) PrintAssignmentExp(node as AssignmentExpression);
            else if (node is BinaryExpression) PrintBinaryExpression(node as BinaryExpression);
            else if (node is UnaryExpression) PrintUnaryExpression(node as UnaryExpression);
            else if (node is IncExpression) PrintIncExpression(node as IncExpression);
            else if (node is DecExpression) PrintDecExpression(node as DecExpression);
            else if (node is ParenExpression) PrintParenExpression(node as ParenExpression);
            else if (node is LiteralExpression) PrintLiteralExpression(node as LiteralExpression);
            else if (node is LocalizationExpression) PrintLocalizationExpression(node as LocalizationExpression);
            else if (node is EvaluatedStrExpression) PrintEvaluatedStrExpression(node as EvaluatedStrExpression);
            else if (node is VariableExpression) PrintVariableExpression(node as VariableExpression);
            else if (node is Block) PrintBlock(node as Block);
            else if (node is MethodArg) PrintMethodArg(node as MethodArg);
        }

        private void PrintBlock(Block block)
        {
            _sb.AppendLine();
            PrintLevel();
            _sb.Append("{\n");
            _depth++;
            PrintStatementList(block.Statements);
            _depth--;
            PrintLevel();
            _sb.Append("}");
        }
        private void PrintVariableExpression(VariableExpression e)
        {
            _sb.Append(e.Identifier);
        }
        private void PrintEvaluatedStrExpression(EvaluatedStrExpression e)
        {
            Print(e.Expression);
        }
        private void PrintLocalizationExpression(LocalizationExpression l)
        {
            _sb.Append("GetLocalizedString(\"");
            _sb.Append(l.StringReference);
            _sb.Append("\")");
        }
        private void PrintLiteralExpression(LiteralExpression l)
        {
            if (l.Literal is StringLiteral)
            {
                if (((StringLiteral)l.Literal).Val.EndsWith(".mint"))
                {
                    ExternalScriptRefs.Add(((StringLiteral)l.Literal).Val);
                }
            }

            _sb.Append(l.Literal.GetVal());
        }
        private void PrintParenExpression(ParenExpression p)
        {
            _sb.Append("(");
            Print(p.Expression);
            _sb.Append(")");
        }
        private void PrintIncExpression(IncExpression i)
        {
            Print(i.Expression);
            _sb.Append("++");
        }
        private void PrintDecExpression(DecExpression i)
        {
            Print(i.Expression);
            _sb.Append("--");
        }
        private void PrintUnaryExpression(UnaryExpression u)
        {
            string o = "";
            switch (u.Op)
            {
                case UnaryOp.Inc:
                    o = "++";
                    break;
                case UnaryOp.Dec:
                    o = "--";
                    break;
                case UnaryOp.Minus:
                    o = "-";
                    break;
                case UnaryOp.Plus:
                    o = "+";
                    break;
                case UnaryOp.Bang:
                    o = "!";
                    break;
            }
            _sb.Append(o);
            Print(u.Expression);
        }
        private void PrintBinaryExpression(BinaryExpression b)
        {
            Print(b.Exp1);
            string o = "";
            _sb.Append(" ");
            switch (b.Op)
            {
                case BinaryOp.Add:
                    o = "+";
                    break;
                case BinaryOp.Subtract:
                    o = "-";
                    break;
                case BinaryOp.Multiply:
                    o = "*";
                    break;
                case BinaryOp.Divide:
                    o = "/";
                    break;
                case BinaryOp.Modulo:
                    o = "%";
                    break;
                case BinaryOp.LT:
                    o = "<";
                    break;
                case BinaryOp.GT:
                    o = ">";
                    break;
                case BinaryOp.LTE:
                    o = "<=";
                    break;
                case BinaryOp.GTE:
                    o = ">=";
                    break;
                case BinaryOp.Equals:
                    o = "==";
                    break;
                case BinaryOp.NotEquals:
                    o = "!=";
                    break;
                case BinaryOp.AND:
                    o = "&&";
                    break;
                case BinaryOp.OR:
                    o = "||";
                    break;

            }
            _sb.Append(o);
            _sb.Append(" ");
            Print(b.Exp2);
        }
        private void PrintAssignmentExp(AssignmentExpression a)
        {
            Print(a.Assignee);
            _sb.Append(" ");
            string o = "";
            switch (a.Op)
            {
                case AssignOp.Assign:
                    o = "=";
                    break;
                case AssignOp.AddAssign:
                    o = "+=";
                    break;
                case AssignOp.SubAssign:
                    o = "-=";
                    break;
                case AssignOp.DivAssign:
                    o = "/=";
                    break;
                case AssignOp.MultAssign:
                    o = "*=";
                    break;
                case AssignOp.ModAssign:
                    o = "%=";
                    break;
                case AssignOp.ANDAssign:
                    o = "&=";
                    break;
                case AssignOp.ORAssign:
                    o = "|=";
                    break;
            }
            _sb.Append(o);
            _sb.Append(" ");
            Print(a.Val);
        }

        private void PrintTypeCastExp(TypeCastExpression t)
        {
            _sb.Append("(");
            _sb.Append(t.Type.Name);
            _sb.Append(")");
            Print(t.Expression);
        }
        private void PrintMemberAccess(MemberAccess m)
        {
            Print(m.Object);
            _sb.Append(".");
            _sb.Append(m.Member);
        } 
        private void PrintMethodInvocation(MethodInvocation m) 
        {
            if (m.Extern != null)
            {
                _sb.Append("CallExtern(\"");
                _sb.Append(m.Extern);
                _sb.Append("\", \"");
                Print(m.Method);
                _sb.Append('"');
                foreach (var e in m.Args)
                {
                    _sb.Append(", ");
                    Print(e);
                }
                _sb.Append(")");
            } else
            {
                if (OverrideMethodInvocation(m)) return;

                Print(m.Method);
                _sb.Append("(");
                bool first = true;
                foreach (var e in m.Args)
                {
                    if (first) first = false;
                    else _sb.Append(", ");
                    Print(e);
                }
                _sb.Append(")");
            }
        }
        private void PrintMethodArg(MethodArg e)
        {
            if (e.IsRef)
            {
                var strLit = ((e.Expression as LiteralExpression)?.Literal as StringLiteral);
                if (strLit != null) strLit.IsMethodRef = true;
                else
                {
                    if (e.Expression is VariableExpression)
                    {
                        _sb.Append(" ref ");
                    }
                }
            }
            Print(e.Expression);
        }
        private void PrintExpressionStatement(ExpressionStatement e)
        {
            Print(e.Expression);
            _sb.Append(";");
        }
        private void PrintWhile(WhileStatement w)
        {
            _sb.Append("while(");
            Print(w.Condition);
            _sb.Append(")");
            Print(w.Loop);
            _sb.AppendLine();
        }
        private void PrintFor(ForStatement f)
        {
            _sb.Append("for(");
            if (f.InitExp != null) Print(f.InitExp);
            _sb.Append("; ");
            if (f.Condition != null) Print(f.Condition);
            _sb.Append("; ");
            if (f.LoopExp != null) Print(f.LoopExp);
            _sb.Append(")");
            Print(f.Loop);
        }

        private void PrintBreak()
        {
            _sb.Append("break;");
        }

        private void PrintContinue()
        {
            _sb.Append("continue;");
        }
        private void PrintReturn(ReturnStatement r)
        {
            _sb.Append("return ");
            if (r.ReturnVal != null)
            {
                Print(r.ReturnVal);
            }
            _sb.Append(";");
        }
        private void PrintDoWhile(DoWhileStatement d)
        {
            _sb.Append("do");
            Print(d.Loop);
            _sb.AppendLine();
            _sb.Append("while(");
            Print(d.Condition);
            _sb.Append(");");
        }
        private void PrintSwitchStatement(SwitchStatement s)
        {
            _sb.Append("switch (");
            Print(s.Condition);
            _sb.Append(")\n");
            PrintLevel();
            _sb.Append("{\n");
            _depth++;
            foreach(var c in s.CaseStatements)
            {
                PrintLevel();
                PrintCaseStatement(c);
                _depth++;
                PrintLevel();
                PrintBreak();
                _depth--;
                _sb.AppendLine();
            }
            _depth--;
            PrintLevel();
            _sb.Append("}");
        }

        private void PrintLevel()
        {
            for (int i = 0; i < _depth; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    _sb.Append(" ");
                }
            }
        }

        private void PrintCaseStatement(CaseStatement c)
        {
            if (c.CaseLabel is DefaultCase)
            {
                _sb.Append("default:");
            } else {
                _sb.Append("case ");
                Print(((Case)c.CaseLabel).Expression);
                _sb.Append(":");
            }
            _depth++;
            foreach (var s in c.Statements)
            {
                PrintLevel();
                if (!(s is Block))
                {
                    _sb.AppendLine();
                    PrintLevel();
                }
                Print(s);
                _sb.AppendLine();
            }
            _depth--;
        }
        private void PrintIfStatement(IfStatement i)
        {
            _sb.Append("if(");
            Print(i.Condition);
            _sb.Append(")");
            Print(i.Statement);
            if (i.Else != null)
            {
                _sb.AppendLine();
                PrintLevel();
                _sb.Append("else ");
                Print(i.Else);
            }
        }

        private void PrintFunctionDeclaration(FunctionDeclaration f)
        {
            _sb.Append("public ");
            if (f.ReturnType.IsRef)
            {
                _sb.Append("ref ");
            }
            _sb.Append(f.ReturnType.Name);
            _sb.Append(" ");
            _sb.Append(f.Identifier);
            _sb.Append("(");
            bool first = true;
            foreach (var param in f.Parameters)
            {
                if (!first) _sb.Append(", ");
                else first = false;

                if (param.Type.IsRef) _sb.Append("ref ");
                _sb.Append(param.Type.Name);
                _sb.Append(" ");
                _sb.Append(param.Identifier);
            }
            _sb.Append(")");
            PrintBlock(f.Block);
        }

        private void PrintVariableDeclaration(VariableDeclaration v)
        {
            _sb.Append(v.Type.Name);
            _sb.Append(" ");
            bool first = true;
            foreach (var decl in v.Declarators)
            {
                if (!first) _sb.Append(", ");
                else first = false;
                PrintVariableDeclarator(decl, v.Type);
            }
            _sb.Append(";");
        }

        private void PrintVariableDeclarator(VariableDeclarator v, Type t)
        {
            _sb.Append(v.Identifier);
            if (v.InitialValue != null)
            {
                _sb.Append(" = ");
                Print(v.InitialValue);
            } else
            {
                // Mint does not need to define a variable before using it
                // we have to adjust that to work with C#
                _sb.Append(" = ");
                _sb.Append(MintTypeMap.GetTypeDefaultValue(t));
            }
        }
        private void PrintStatementList(StatementList s)
        {
            foreach (var st in s.Statements)
            {
                PrintLevel();
                Print(st);
                _sb.AppendLine();
            }
        }

        protected virtual bool OverrideMethodInvocation(MethodInvocation m)
        {
            return false;
        }
    }
}
