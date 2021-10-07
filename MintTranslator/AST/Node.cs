using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MintTranslator.AST
{
    public abstract class Node
    {
        public Type InferredType { get; set; }
        public Node Parent { get; set; }
        public abstract IEnumerable<Node> GetChildren();

        public void ApplyToChildren(Action<Node> func)
        {
            foreach (var child in GetChildren())
            {
                if (child != null) func.Invoke(child);
            }
        }
    }
    public class StatementList : Node
    {
        public List<Statement> Statements { get; set; }

        public override IEnumerable<Node> GetChildren()
        {
            return new List<Node>(Statements);
        }
    }
    public abstract class Statement : Node {}
    public abstract class EmbeddedStatement : Statement { }
    public class NullStatement : EmbeddedStatement
    {
        public override IEnumerable<Node> GetChildren()
        {
            return new List<Node>();
        }
    }
    public class ExpressionStatement : EmbeddedStatement
    {
        public Expression Expression { get; set; }

        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class IfStatement : EmbeddedStatement
    {
        public Expression Condition { get; set; }
        public EmbeddedStatement Statement { get; set; }
        public EmbeddedStatement Else { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Condition);
            list.Add(Statement);
            if (Else != null) list.Add(Else);
            return list;
        }
    }
    public class SwitchStatement : EmbeddedStatement
    {
        public Expression Condition { get; set; }
        public List<CaseStatement> CaseStatements { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Condition);
            foreach (var stmt in CaseStatements)
            {
                if (stmt != null) list.Add(stmt);
            }
            return list;
        }

    }
    public class DoWhileStatement : EmbeddedStatement
    {
        public EmbeddedStatement Loop { get; set; }
        public Expression Condition { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Loop);
            list.Add(Condition);
            return list;
        }
    }
    public class WhileStatement : EmbeddedStatement
    {
        public Expression Condition { get; set; }
        public EmbeddedStatement Loop { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Condition);
            list.Add(Loop);
            return list;
        }
    }
    public class ForStatement : EmbeddedStatement
    {
        public Node InitExp { get; set; }
        public Expression Condition { get; set; }
        public Expression LoopExp { get; set; }
        public EmbeddedStatement Loop { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            if (InitExp != null) list.Add(InitExp);
            if (Condition != null) list.Add(Condition);
            if (LoopExp != null) list.Add(LoopExp);
            list.Add(Loop);
            return list;
        }
    }
    public class BreakStatement : EmbeddedStatement {
        public override IEnumerable<Node> GetChildren()
        {
            return new List<Node>();
        }
    }
    public class ContinueStatement : EmbeddedStatement {
        public override IEnumerable<Node> GetChildren()
        {
            return new List<Node>();
        }
    }
    public class ReturnStatement : EmbeddedStatement
    {
        public Expression ReturnVal { get; set; }

        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            if (ReturnVal != null)
            {
                list.Add(ReturnVal);
            }
            return list;
        }
    }
    public class CaseStatement : Node
    {
        public CaseLabel CaseLabel { get; set; }
        public List<Statement> Statements { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(CaseLabel);
            list.AddRange(Statements);
            return list;
        }
    }
    public abstract class CaseLabel : Node { }
    public class Case : CaseLabel
    {
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class DefaultCase : CaseLabel {
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
    }
    public abstract class Expression : Node { }

    public class Block : EmbeddedStatement
    {
        public StatementList Statements { get; set; }

        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Statements);
            return list;
        }
    }
    public class FunctionDeclaration : Statement
    {
        public bool Server { get; set; }
        public Type ReturnType { get; set; }
        public string Identifier { get; set; }
        public List<Parameter> Parameters { get; set; }
        public Block Block { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(ReturnType);
            list.AddRange(Parameters);
            list.Add(Block);
            return list;
        }
    }
    public class Parameter : Node
    {
        public Type Type { get; set; }
        public string Identifier { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Type);
            return list;
        }
    }
    public class MethodInvocation : Expression
    {
        public Expression Method { get; set; }
        public List<MethodArg> Args { get; set; }
        public string Extern { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Method);
            list.AddRange(Args);
            return list;
        }
    }
    public class MethodArg : Node
    {
        public bool IsRef { get; set; }
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class MemberAccess : Expression
    { 
        public Expression Object { get; set; }
        public string Member { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Object);
            return list;
        }
    }
    public class StringInjector : Expression
    {
        public Expression BaseString { get; set; }
        public List<Expression> StringsToInject { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(BaseString);
            list.AddRange(StringsToInject);
            return list;
        }
    }
    public class TypeCastExpression : Expression
    {
        public Type Type {get; set; }
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Type);
            list.Add(Expression);
            return list;
        }
    }
    public class VariableDeclaration : Statement
    {
        public Type Type { get; set; }
        public List<VariableDeclarator> Declarators { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Type);
            list.AddRange(Declarators);
            return list;
        }

    }
    public class VariableDeclarator : Node
    {
        public string Identifier { get; set; }
        public Expression InitialValue { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            if (InitialValue != null) list.Add(InitialValue);
            return list;
        }
    }
    public enum AssignOp
    {
        Assign,
        AddAssign,
        SubAssign,
        DivAssign,
        MultAssign,
        ModAssign,
        ANDAssign,
        ORAssign
    }
    public class AssignmentExpression : Expression
    {
        public Expression Assignee { get; set; }
        public AssignOp Op { get; set; }
        public Expression Val { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Assignee);
            list.Add(Val);
            return list;
        }
    }
    public enum BinaryOp
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        LT,
        GT,
        LTE,
        GTE,
        Equals,
        NotEquals,
        AND,
        OR
    }
    public class BinaryExpression : Expression
    {
        public Expression Exp1 { get; set; }
        public BinaryOp Op { get; set; }
        public Expression Exp2 { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Exp1);
            list.Add(Exp2);
            return list;
        }
    }

    public enum UnaryOp { 
        Inc,
        Dec,
        Minus,
        Plus,
        Bang
    };
    public class UnaryExpression : Expression
    {
        public UnaryOp Op { get; set; }
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class IncExpression : Expression
    {
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class DecExpression : Expression
    {
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class ParenExpression : Expression
    {
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }
    public class LiteralExpression : Expression
    {
        public Literal Literal { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Literal);
            return list;
        }
    }
    public class LocalizationExpression : Expression
    {
        public string StringReference { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
    }
    public class EvaluatedStrExpression : Expression
    {
        public Expression Expression { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            list.Add(Expression);
            return list;
        }
    }

    public class VariableExpression : Expression
    {
        public string Identifier { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
    }
    public class StringLiteral : Literal { public string Val { get; set; }
        public bool IsMethodRef { get; set; }
        public override string GetVal()
        {
            if (IsMethodRef)
            {
                if (String.IsNullOrEmpty(Val)) return "null";
                if (!Val.Contains(".mint"))
                {
                    return Val;
                }
                // Script ref, do parsing in c# side to identify the ref
            }
            return "\"" + Val + "\"";
        }

        public void SetMethodRef()
        {
            IsMethodRef = true;
        }

        public override Type Type()
        {
            return new Type() { Name = "string" };
        }
    }
    public class IntegerLiteral : Literal { public int Val { get; set; }
        public override string GetVal()
        {
            return ""+Val;
        }

        public override Type Type()
        {
            return new Type() { Name = "int" };
        }
    }
    public class LongLiteral : Literal { public long Val { get; set; }
        public override string GetVal()
        {
            return "" + Val;
        }

        public override Type Type()
        {
            return new Type() { Name = "long" };
        }
    }
    public class FloatLiteral : Literal { public float Val { get; set; }
        public override string GetVal()
        {
            return Val + "f";
        }

        public override Type Type()
        {
            return new Type() { Name = "float" };
        }
    }
    public class BooleanLiteral : Literal { public bool Val { get; set; }
        public override string GetVal()
        {
            return Val ? "true" : "false";
        }

        public override Type Type()
        {
            return new Type() { Name = "bool" };
        }
    }

    public abstract class Literal : Node {
        public abstract string GetVal();
        public abstract Type Type();
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
    }

    public class Type : Node 
    {
        public bool IsRef { get; set; }
        public string Name { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj, this))
                return true;

            Type other = obj as Type;

            if (Object.ReferenceEquals(null, other))
                return false;

            if (!String.Equals(Name, other.Name, StringComparison.Ordinal)) return false;
            if (IsRef != other.IsRef) return false;
            return true;
        }

        public bool IsNumericType()
        {
            switch (Name)
            {
                case "byte":
                case "short":
                case "int":
                case "float":
                case "long":
                    return true;
            }
            return false;
        }

        public static Type CombineType(Type left, Type right)
        {
            if (left == null) return right;
            if (right == null) return left;
            if (left.Name == "void") return right;
            if (right.Name == "void") return left;

            bool leftNum = left.IsNumericType();
            bool rightNum = right.IsNumericType();
            if (leftNum || rightNum)
            {
                if (leftNum && rightNum) return LargerNumberType(left, right);
                throw new Exception("Trying to assign " + left.Name + " with " + right.Name);
            }
            return left;
        }
        private static Type LargerNumberType(Type one, Type two)
        {
            if (one == null) return two;
            if (two == null) return one;
            return ConvertNumberOrdinalToType(Math.Max(ConvertTypeToNumberOrdinal(one), ConvertTypeToNumberOrdinal(two)));
        }
        private static Type ConvertNumberOrdinalToType(int ord)
        {
            switch (ord)
            {
                case 0:
                    return new Type() { Name = "byte" };
                case 1:
                    return new Type() { Name = "short" };
                case 2:
                    return new Type() { Name = "int" };
                case 3:
                    return new Type() { Name = "long" };
                case 4:
                    return new Type() { Name = "float" };
            }
            return null;
        }
        private static int ConvertTypeToNumberOrdinal(Type type)
        {
            if (type == null) return -1;
            var name = type.Name;
            switch (name)
            {
                case "byte":
                    return 0;
                case "short":
                    return 1;
                case "int":
                case "dword":
                    return 2;
                case "qword":
                case "long":
                    return 3;
                case "float":
                    return 4;
            }
            return -1;
        }
    }
    public class Identifier : Node
    {
        public string Name { get; set; }
        public override IEnumerable<Node> GetChildren()
        {
            var list = new List<Node>();
            return list;
        }
    }
}
