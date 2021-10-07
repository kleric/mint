
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MintTranslator.AST
{
    public class ASTGenerator : MintDParserBaseVisitor<Node>
    {
        public override Node VisitStatementList([NotNull] MintDParser.StatementListContext context)
        {
            var statements = context.statement();
            var statementArr = new List<Statement>();
            for (int i = 0; i < statements.Length; i++)
            {
                statementArr.Add((Statement)Visit(statements[i]));
            }
            var list = new StatementList() { Statements = statementArr };
            for (int i = 0; i < statements.Length; i++)
            {
                statementArr[i].Parent = list;
            }
            return list;
        }

        public override Node VisitDeclarationStatement([NotNull] MintDParser.DeclarationStatementContext context)
        {
            var funcDecl = context.functionDeclaration();
            if (funcDecl != null)
            {
                return Visit(funcDecl);
            } else
            {
                return Visit(context.local_variable_declaration());
            }
        }

        public override Node VisitParameter([NotNull] MintDParser.ParameterContext context)
        {
            Parameter p = new Parameter();
            p.Type = (Type) VisitType(context.type());
            p.Identifier = context.Identifier().Symbol.Text;
            return p;
        }

        public override Node VisitLocalizeExp([NotNull] MintDParser.LocalizeExpContext context)
        {
            LocalizationExpression e = new LocalizationExpression();
            e.StringReference = context.DOUBLE_STRING().Symbol.Text.Replace("\"", string.Empty);
            return e;
        }

        public override Node VisitParenthExp([NotNull] MintDParser.ParenthExpContext context)
        {
            var exp = VisitExpression(context.expression()) as Expression;
            var paren = new ParenExpression() { Expression = exp };
            exp.Parent = paren;
            return paren;
        }

        public override Node VisitLocal_variable_declaration([NotNull] MintDParser.Local_variable_declarationContext context)
        {
            VariableDeclaration decl = new VariableDeclaration();
            decl.Type = Visit(context.type()) as Type;
            decl.Type.Parent = decl;
            var decls = context.variable_declarator();
            var arr = new List<VariableDeclarator>();
            for(int i = 0; i < decls.Length; i++)
            {
                arr.Add((VariableDeclarator)Visit(decls[i]));
                arr[i].Parent = decl;
            }
            decl.Declarators = arr;

            return decl;
        }
        public override Node VisitAssignmentExp([NotNull] MintDParser.AssignmentExpContext context)
        {
            AssignmentExpression e = new AssignmentExpression();
            e.Assignee = (Expression)VisitUnaryExp(context.unaryExp());
            e.Assignee.Parent = e;
            e.Val = VisitExpression(context.expression()) as Expression;
            e.Val.Parent = e;
            var op = context.assignment_operator();
            if (op.ASSIGNMENT() != null) e.Op = AssignOp.Assign;
            if (op.ADD_ASSIGN() != null) e.Op = AssignOp.AddAssign;
            if (op.SUB_ASSIGN() != null) e.Op = AssignOp.SubAssign;
            if (op.MULT_ASSIGN() != null) e.Op = AssignOp.MultAssign;
            if (op.DIV_ASSIGN() != null) e.Op = AssignOp.DivAssign;
            if (op.MOD_ASSIGN() != null) e.Op = AssignOp.ModAssign;
            if (op.AND_ASSIGN() != null) e.Op = AssignOp.ANDAssign;
            if (op.OR_ASSIGN() != null) e.Op = AssignOp.ORAssign;

            return e;
        }

        public override Node VisitConditionalOrExp([NotNull] MintDParser.ConditionalOrExpContext context)
        {
            var exps = context.conditionalAndExp();
            Expression prev = VisitConditionalAndExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitConditionalAndExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                b.Op = BinaryOp.OR;
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }
        public override Node VisitConditionalAndExp([NotNull] MintDParser.ConditionalAndExpContext context)
        {
            var exps = context.equalityExp();
            Expression prev = VisitEqualityExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitEqualityExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                b.Op = BinaryOp.AND;
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }

        public override Node VisitEqualityExp([NotNull] MintDParser.EqualityExpContext context)
        {
            var exps = context.relationalExp();
            Expression prev = VisitRelationalExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitRelationalExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                var op = context.GetChild((i * 2) - 1) as TerminalNodeImpl;
                switch (op.Symbol.Type)
                {
                    case MintDLexer.OP_EQ:
                        b.Op = BinaryOp.Equals;
                        break;
                    case MintDLexer.OP_NE:
                        b.Op = BinaryOp.NotEquals;
                        break;
                }
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }

        public override Node VisitRelationalExp([NotNull] MintDParser.RelationalExpContext context)
        {
            var exps = context.additiveExp();
            Expression prev = VisitAdditiveExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitAdditiveExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                var op = context.GetChild((i * 2) - 1) as TerminalNodeImpl;
                switch (op.Symbol.Type)
                {
                    case MintDLexer.LT:
                        b.Op = BinaryOp.LT;
                        break;
                    case MintDLexer.GT:
                        b.Op = BinaryOp.GT;
                        break;
                    case MintDLexer.LTE:
                        b.Op = BinaryOp.LTE;
                        break;
                    case MintDLexer.GTE:
                        b.Op = BinaryOp.GTE;
                        break;
                }
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }

        public override Node VisitAdditiveExp([NotNull] MintDParser.AdditiveExpContext context)
        {
            var exps = context.multiplicativeExp();
            Expression prev = VisitMultiplicativeExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitMultiplicativeExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                var op = context.GetChild((i * 2) - 1) as TerminalNodeImpl;
                switch (op.Symbol.Type)
                {
                    case MintDLexer.PLUS:
                        b.Op = BinaryOp.Add;
                        break;
                    case MintDLexer.MINUS:
                        b.Op = BinaryOp.Subtract;
                        break;
                }
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }

        public override Node VisitExpression([NotNull] MintDParser.ExpressionContext context)
        {
            var ass = context.assignmentExp();
            if (ass != null) return VisitAssignmentExp(ass);
            return VisitNonAssignmentExp(context.nonAssignmentExp());
        }

        public override Node VisitNonAssignmentExp([NotNull] MintDParser.NonAssignmentExpContext context)
        {
            return VisitConditionalOrExp(context.conditionalOrExp());
        }
        public override Node VisitMultiplicativeExp([NotNull] MintDParser.MultiplicativeExpContext context)
        {
            var exps = context.unaryExp();
            Expression prev = VisitUnaryExp(exps[0]) as Expression;
            if (exps.Length == 1) return prev;
            for (int i = 1; i < exps.Length; i++)
            {
                var node = VisitUnaryExp(exps[i]) as Expression;
                var b = new BinaryExpression();
                b.Exp1 = prev;
                var op = context.GetChild((i * 2) - 1) as TerminalNodeImpl;
                switch (op.Symbol.Type)
                {
                    case MintDLexer.TIMES:
                        b.Op = BinaryOp.Multiply;
                        break;
                    case MintDLexer.DIVIDE:
                        b.Op = BinaryOp.Divide;
                        break;
                    case MintDLexer.MODULO:
                        b.Op = BinaryOp.Modulo;
                        break;
                }
                b.Exp2 = node;
                prev.Parent = b;
                node.Parent = b;
                prev = b;
            }
            return prev;
        }

        public override Node VisitUnaryExp([NotNull] MintDParser.UnaryExpContext context)
        {
            var postFix = context.postfixExp();
            if (postFix != null) return VisitPostfixExp(postFix);
            var preIncDec = context.preIncDecExp();
            if (preIncDec != null) return VisitPreIncDecExp(preIncDec);
            var op = context.unaryOperator();
            if (op != null)
            {
                UnaryExpression exp = new UnaryExpression();
                if (op.MINUS() != null) exp.Op = UnaryOp.Minus;
                if (op.PLUS() != null) exp.Op = UnaryOp.Plus;
                if (op.BANG() != null) exp.Op = UnaryOp.Bang;
                exp.Expression = (Expression)VisitUnaryExp(context.unaryExp());
                exp.Expression.Parent = exp;
                return exp;
            }
            var cast = context.typeCast();
            TypeCastExpression t = new TypeCastExpression();
            t.Type = VisitType(cast.type()) as Type;
            t.Type.Parent = t;
            t.Expression = (Expression)VisitUnaryExp(context.unaryExp());
            t.Expression.Parent = t;
            return t;
        }

        public override Node VisitPreIncDecExp([NotNull] MintDParser.PreIncDecExpContext context)
        {
            bool inc = context.INC() != null;
            var exp = VisitUnaryExp(context.unaryExp()) as Expression;
            UnaryExpression unarExp;
            if (inc) unarExp = new UnaryExpression() { Expression = exp, Op = UnaryOp.Inc };
            else unarExp = new UnaryExpression() { Expression = exp, Op = UnaryOp.Dec };

            unarExp.Expression.Parent = unarExp;
            return unarExp;
        }

        public override Node VisitPrimaryExp([NotNull] MintDParser.PrimaryExpContext context)
        {
            var ident = context.Identifier();
            if (ident != null)
            {
                return new VariableExpression() { Identifier = ident.Symbol.Text };
            }
            var exp = context.expression();
            if (exp != null)
            {
                var strExp = new EvaluatedStrExpression() { Expression = VisitExpression(exp) as Expression };
                strExp.Expression.Parent = strExp;
                return strExp;
            }
            var literal = context.literal();
            if (literal != null)
            {
                var litExp = new LiteralExpression() { Literal = VisitLiteral(literal) as Literal };
                litExp.Literal.Parent = litExp;
                return litExp;
            }
            return VisitChildren(context);
        }

        public override Node VisitPostfixExp([NotNull] MintDParser.PostfixExpContext context)
        {
            var p = context.primaryExp();
            if (p != null) return VisitPrimaryExp(p);
            var post = context.postfix();
            var exp = VisitPostfixExp(context.postfixExp()) as Expression;
            var method = post.methodInvocation();
            if (method != null)
            {
                MethodInvocation m = new MethodInvocation();
                m.Method = exp;
                exp.Parent = m;
                var argList = method.argList();
                List<MethodArg> args = new List<MethodArg>();
                if (argList != null)
                {
                    var exps = argList.expression();
                    args = new List<MethodArg>();
                    for (int i = 0; i < exps.Length; i++)
                    {
                        var ex = (Expression)VisitExpression(exps[i]);
                        MethodArg a = new MethodArg();
                        a.Expression = ex;
                        ex.Parent = a;
                        a.Parent = m;
                        args.Add(a);
                    }
                }
                m.Args = args;
                var isExtern = method.methodExtern();
                if (isExtern != null)
                {
                    m.Extern = isExtern.BACK_STRING().Symbol.Text.Replace("`", "");
                }
                return m;
            }
            var member = post.memberAccess();
            if (member != null)
            {
                MemberAccess a = new MemberAccess();
                a.Object = exp;
                a.Object.Parent = a;
                a.Member = member.Identifier().Symbol.Text;
                return a;
            }
            if (post.INC() != null)
            {
                var incExp = new IncExpression() { Expression = exp };
                incExp.Expression.Parent = incExp;
                return incExp;
            }
            if (post.DEC() != null)
            {
                var decExp = new DecExpression() { Expression = exp };
                decExp.Expression.Parent = decExp;
                return decExp;
            }
            var l = post.localizeParams();
            StringInjector s = new StringInjector();
            s.BaseString = exp;
            s.BaseString.Parent = s;
            var ls = l.localizeParam();
            var ps = new List<Expression>();
            for (int i = 0; i < ls.Length; i++)
            {
                ps.Add((Expression)VisitExpression(ls[i].expression()));
                ps[i].Parent = s;
            }
            s.StringsToInject = ps;
            return s;
        }
        public override Node VisitVariable_declarator([NotNull] MintDParser.Variable_declaratorContext context)
        {
            VariableDeclarator d = new VariableDeclarator();
            d.Identifier = context.Identifier().Symbol.Text;
            var exp = context.expression();
            if (exp != null)
            {
                d.InitialValue = VisitExpression(exp) as Expression;
                d.InitialValue.Parent = d;
            }
            return d;
        }

        public override Node VisitFunctionDeclaration([NotNull] MintDParser.FunctionDeclarationContext funcDecl)
        {
            FunctionDeclaration dec = new FunctionDeclaration();
            dec.Server = funcDecl.functionSpecifier().SERVER() != null;
            dec.ReturnType = VisitType(funcDecl.type()) as Type;
            dec.ReturnType.Parent = dec;
            dec.Identifier = ((Identifier)Visit(funcDecl.Identifier())).Name;
            var p = funcDecl.parameters();
            if (p == null)
            {
                dec.Parameters = new List<Parameter>();
            }
            else
            {
                var parameters = p.parameter();
                var arr = new List<Parameter>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    arr.Add((Parameter)Visit(parameters[i]));
                    arr[i].Parent = dec;
                }
                dec.Parameters = arr;
            }
            dec.Block = (Block) VisitBlock(funcDecl.block());
            dec.Block.Parent = dec;
            return dec;
        }
        public override Node VisitIfStatement([NotNull] MintDParser.IfStatementContext context)
        {
            IfStatement f = new IfStatement();
            f.Condition = Visit(context.expression()) as Expression;
            f.Statement = (EmbeddedStatement) Visit(context.embeddedStatement());
            f.Condition.Parent = f;
            f.Statement.Parent = f;
            var elseStatement = context.elseStatement();
            if (elseStatement != null)
            {
                f.Else = (EmbeddedStatement) Visit(elseStatement);
                f.Else.Parent = f;
            }
            return f;
        }

        public override Node VisitElseStatement([NotNull] MintDParser.ElseStatementContext context)
        {
            var embed = context.embeddedStatement();
            return Visit(embed);
        }
        public override Node VisitIterationStatement([NotNull] MintDParser.IterationStatementContext context)
        {
            if (context.DO() != null)
            {
                var d = new DoWhileStatement();
                d.Condition = VisitExpression(context.expression()) as Expression;
                d.Condition.Parent = d;
                d.Loop = VisitEmbeddedStatement(context.embeddedStatement()) as EmbeddedStatement;
                d.Loop.Parent = d;
                return d;
            }
            return VisitChildren(context);
        }
        public override Node VisitForStatement([NotNull] MintDParser.ForStatementContext context)
        {
            var f = new ForStatement();
            var initExp = context.forInitExp();
            if (initExp != null)
            {
                f.InitExp = Visit(initExp);
                f.InitExp.Parent = f;
            }
            var condExp = context.forCondExp();
            if (condExp != null)
            {
                f.Condition = Visit(condExp) as Expression;
                f.Condition.Parent = f;
            }
            var loopExp = context.forLoopExp();
            if (loopExp != null)
            {
                f.LoopExp = Visit(loopExp) as Expression;
                f.LoopExp.Parent = f;
            }
            f.Loop = VisitEmbeddedStatement(context.embeddedStatement()) as EmbeddedStatement;
            f.Loop.Parent = f;
            return f;
        }
        public override Node VisitWhileStatement([NotNull] MintDParser.WhileStatementContext context)
        {
            var wh = new WhileStatement();
            wh.Condition = (Expression)Visit(context.expression());
            wh.Loop = (EmbeddedStatement)Visit(context.embeddedStatement());
            return wh;
        }
        public override Node VisitSwitchStatement([NotNull] MintDParser.SwitchStatementContext context)
        {
            var s = new SwitchStatement();
            s.Condition = Visit(context.expression()) as Expression;
            s.Condition.Parent = s;
            var cases = context.switchCases().caseStatement();
            var arr = new List<CaseStatement>();
            for (int i = 0; i < cases.Length; i++)
            {
                var stmt = Visit(cases[i]) as CaseStatement;
                stmt.Parent = s;
                arr.Add(stmt);
            }
            s.CaseStatements = arr;
            return s;
        }
        public override Node VisitBlock([NotNull] MintDParser.BlockContext context)
        {
            Block block = new Block();
            var s = context.statementList();
            if (s == null)
            {
                StatementList l = new StatementList();
                l.Statements = new List<Statement>();
                block.Statements = l;
            } else
            {
                block.Statements = Visit(s) as StatementList;
            }
            return block;
        }

        public override Node VisitCaseStatement([NotNull] MintDParser.CaseStatementContext context)
        {
            var label = VisitCaseLabel(context.caseLabel()) as CaseLabel;
            var s = context.statement();
            List<Statement> sts = new List<Statement>();
            for (int i = 0; i < s.Length; i++)
            {
                sts.Add((Statement)VisitStatement(s[i]));
            }
            var caseStatement = new CaseStatement() { CaseLabel = label, Statements = sts };
            label.Parent = caseStatement;

            foreach(var stmt in sts)
            {
                stmt.Parent = caseStatement;
            }
            return caseStatement;
        }
        public override Node VisitCaseLabel([NotNull] MintDParser.CaseLabelContext context)
        {
            if (context.DEFAULT() != null) return new DefaultCase();
            var caseExp = new Case() { Expression = VisitExpression(context.expression()) as Expression };
            caseExp.Expression.Parent = caseExp;
            return caseExp;
        }
        public override Node VisitChildren(IRuleNode node)
        {
            return Visit(node.GetChild(0));
        }
        public override Node VisitNullStatement([NotNull] MintDParser.NullStatementContext context)
        {
            return new NullStatement();
        }
        public override Node VisitExpressionStatement([NotNull] MintDParser.ExpressionStatementContext context)
        {
            var exp = new ExpressionStatement() { Expression = VisitExpression(context.expression()) as Expression };
            exp.Expression.Parent = exp;
            return exp;
        }
        public override Node VisitBreakStatement([NotNull] MintDParser.BreakStatementContext context)
        {
            return new BreakStatement();
        }
        public override Node VisitContinueStatement([NotNull] MintDParser.ContinueStatementContext context)
        {
            return new ContinueStatement();
        }
        public override Node VisitReturnStatement([NotNull] MintDParser.ReturnStatementContext context)
        {
            var exp = context.expression();
            ReturnStatement s = new ReturnStatement();
            if (exp != null)
            {
                s.ReturnVal = VisitExpression(exp) as Expression;
                s.ReturnVal.Parent = s;
            }
            return s;
        }
        public override Node VisitStatement([NotNull] MintDParser.StatementContext context)
        {
            var declStatement = context.declarationStatement();
            if (declStatement != null)
            {
                return VisitDeclarationStatement(declStatement);
            }
            return VisitEmbeddedStatement(context.embeddedStatement());
        }
        public override Node VisitEmbeddedStatement([NotNull] MintDParser.EmbeddedStatementContext context)
        {
            return Visit(context.children[0]);
        }
        public override Node Visit(IParseTree tree)
        {
            var payload = tree.Payload;
            if (!(payload is RuleContext))
            {
                return base.Visit(tree);
            }
            var payloadType = payload.GetType();
            var methods = typeof(ASTGenerator).GetMethods().ToArray();

            var method = System.Type.DefaultBinder.SelectMethod(BindingFlags.Default, methods, new[] { payloadType }, null);
            return (Node)method.Invoke(this, new[] { payload });
        }
        public override Node VisitTerminal(ITerminalNode node)
        {
            switch (node.Symbol.Type)
            {
                case MintDLexer.BACK_STRING:
                    return new StringLiteral()
                    {
                        Val = node.Symbol.Text.Replace("`", string.Empty)
                    };
                case MintDLexer.DOUBLE_STRING:
                    return new StringLiteral()
                    {
                        Val = node.Symbol.Text.Replace("\"", string.Empty)
                    };
                case MintDLexer.INTEGER_LITERAL:
                    int intVal;
                    try { intVal = int.Parse(node.Symbol.Text); } catch (OverflowException e)
                    {
                        return new LongLiteral() { Val = long.Parse(node.Symbol.Text) };
                    }
                    return new IntegerLiteral()
                    {
                        Val = intVal
                    };
                case MintDLexer.FLOAT_LITERAL:
                    return new FloatLiteral()
                    {
                        Val = float.Parse(node.Symbol.Text)
                    };
                case MintDLexer.TRUE:
                    return new BooleanLiteral() { Val = true };
                case MintDLexer.FALSE:
                    return new BooleanLiteral() { Val = false };
                case MintDLexer.HEX_INTEGER_LITERAL:
                    return new IntegerLiteral()
                    {
                        Val = (int)Convert.ToUInt32(node.Symbol.Text, 16)
                    };
                case MintDLexer.VOID:
                    return new Type() { Name = "void" };
                case MintDLexer.STRING:
                    return new Type() { Name = "string" };
                case MintDLexer.BOOL:
                    return new Type() { Name = "bool" };
                case MintDLexer.INT:
                    return new Type() { Name = "int" };
                case MintDLexer.FLOAT:
                    return new Type() { Name = "float" };
                case MintDLexer.Identifier:
                    return new Identifier() { Name = node.Symbol.Text };
            }
            return base.VisitTerminal(node);
        }

        public override Node VisitPrimitiveType([NotNull] MintDParser.PrimitiveTypeContext context)
        {
            return Visit(context.GetChild(0));
        }

        public override Node VisitType([NotNull] MintDParser.TypeContext context)
        {
            var primitiveType = Visit(context.primitiveType());

            Type type;
            if (primitiveType is Identifier)
            {

                type = new Type() { Name = (primitiveType as Identifier).Name};
            } else
            {
                type = primitiveType as Type;
            }
            type.IsRef = context.REF() != null;
            return type;
            
        }
        public override Node VisitLiteral(MintDParser.LiteralContext context)
        {
            Node prev = null;
            foreach (var c in context.children)
            {
                Node n = Visit(c);
                if (n is IntegerLiteral) return n;
                if (n is FloatLiteral) return n;
                if (n is BooleanLiteral) return n;
                if (prev == null)
                {
                    prev = n;
                } else
                {
                    (prev as StringLiteral).Val += (n as StringLiteral).Val;
                }
            }
            return prev;
        }
    }


}
