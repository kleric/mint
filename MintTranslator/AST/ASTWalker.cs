using System;
using System.Collections.Generic;
using System.Text;

namespace MintTranslator.AST
{
    public class ASTWalker
    {
        private void Visit(Node node)
        {
            if (node is StatementList) VisitStatementList(node as StatementList);
            else if (node is FunctionDeclaration) VisitFunctionDeclaration(node as FunctionDeclaration);
            else if (node is VariableDeclaration) VisitVariableDeclaration(node as VariableDeclaration);
            else if (node is IfStatement) VisitIfStatement(node as IfStatement);
            else if (node is SwitchStatement) VisitSwitchStatement(node as SwitchStatement);
            else if (node is WhileStatement) VisitWhile(node as WhileStatement);
            else if (node is DoWhileStatement) VisitDoWhile(node as DoWhileStatement);
            else if (node is BreakStatement) VisitBreak();
            else if (node is ContinueStatement) VisitContinue();
            else if (node is ReturnStatement) VisitReturn(node as ReturnStatement);
            else if (node is ForStatement) VisitFor(node as ForStatement);
            else if (node is ExpressionStatement) VisitExpressionStatement(node as ExpressionStatement);
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
        }

        private void VisitBlock(Block block)
        {
            VisitStatementList(block.Statements);
        }
        private void VisitVariableExpression(VariableExpression e)
        {
        }
        private void VisitEvaluatedStrExpression(EvaluatedStrExpression e)
        {
            Visit(e.Expression);
        }
        private void VisitLocalizationExpression(LocalizationExpression l)
        {
        }
        private void VisitLiteralExpression(LiteralExpression l)
        {
        }
        private void VisitParenExpression(ParenExpression p)
        {
            Visit(p.Expression);
        }
        private void VisitIncExpression(IncExpression i)
        {
            Visit(i.Expression);
        }
        private void VisitDecExpression(DecExpression i)
        {
            Visit(i.Expression);
        }
        private void VisitUnaryExpression(UnaryExpression u)
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
            Visit(u.Expression);
        }
        private void VisitBinaryExpression(BinaryExpression b)
        {
            Visit(b.Exp1);
            string o = "";
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
            Visit(b.Exp2);
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
            Visit(a.Val);
        }

        private void VisitTypeCastExp(TypeCastExpression t)
        {
            Visit(t.Expression);
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
        }
        private void VisitExpressionStatement(ExpressionStatement e)
        {
            Visit(e.Expression);
        }
        private void VisitWhile(WhileStatement w)
        {
            Visit(w.Condition);
            Visit(w.Loop);
        }
        private void VisitFor(ForStatement f)
        {
            if (f.InitExp != null) Visit(f.InitExp);
            if (f.Condition != null) Visit(f.Condition);
            if (f.LoopExp != null) Visit(f.LoopExp);
            Visit(f.Loop);
        }

        private void VisitBreak()
        {
        }

        private void VisitContinue()
        {
        }
        private void VisitReturn(ReturnStatement r)
        {
            if (r.ReturnVal != null)
            {
                Visit(r.ReturnVal);
            }
        }
        private void VisitDoWhile(DoWhileStatement d)
        {
            Visit(d.Loop);
            Visit(d.Condition);
        }
        private void VisitSwitchStatement(SwitchStatement s)
        {
            Visit(s.Condition);
            foreach (var c in s.CaseStatements)
            {
                VisitCaseStatement(c);
                VisitBreak();
            }
        }

        private void VisitCaseStatement(CaseStatement c)
        {
            if (c.CaseLabel is DefaultCase)
            {
            }
            else
            {
                Visit(((Case)c.CaseLabel).Expression);
            }
            foreach (var s in c.Statements)
            {
                Visit(s);
            }
        }
        private void VisitIfStatement(IfStatement i)
        {
            Visit(i.Condition);
            Visit(i.Statement);
            if (i.Else != null)
            {
                Visit(i.Else);
            }
        }

        private void VisitFunctionDeclaration(FunctionDeclaration f)
        {
            VisitBlock(f.Block);
        }

        private void VisitVariableDeclaration(VariableDeclaration v)
        {
            bool first = true;
            foreach (var decl in v.Declarators)
            {
                VisitVariableDeclarator(decl, v.Type);
            }
        }

        private void VisitVariableDeclarator(VariableDeclarator v, Type t)
        {
            if (v.InitialValue != null)
            {
                Visit(v.InitialValue);
            }
        }
        private void VisitStatementList(StatementList s)
        {
            foreach (var st in s.Statements)
            {
                Visit(st);
            }
        }
    }
}
