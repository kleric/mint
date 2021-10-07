using MintTranslator.AST;
using System;
using System.Text;

namespace MintTranslator.Generators
{
    public class AuraPrinter : ASTCSharpPrinter
    {
        protected override bool OverrideMethodInvocation(MethodInvocation m)
        {
            if (m.Method is MemberAccess)
            {
                var memberAccess = (MemberAccess)m.Method;
                switch (memberAccess.Member)
                {
                    case "GetData":
                        PrintGetData(m);
                        return true;
                    case "OverrideFunction":
                        PrintOverrideFunction(m, "=");
                        return true;
                    case "AddFunction":
                        PrintOverrideFunction(m, "+=");
                        return true;
                    case "CreateMonsterGroup":
                        m.Args[1].IsRef = true;
                        break;
                    case "AddPropWithAbsolutePosition":
                        if (m.Args.Count == 8)
                        {
                            m.Args[6].IsRef = true;
                        }
                        else if (m.Args.Count == 9)
                        {
                            m.Args[7].IsRef = true;
                        }
                        break;
                    case "AddPropWithAngle":
                        m.Args[6].IsRef = true;
                        break;
                    case "SetTimer":
                        m.Args[1].IsRef = true;
                        break;
                    case "CallFunction":
                        PrintCallFunction(m);
                        return true;

                }
            }
            return false;
        }

        private void PrintCallFunction(MethodInvocation m)
        {
            Print(((MemberAccess)m.Method).Object);
            _sb.Append(".CallFunction(");
            // ? Does CallFunction invoke the method on the object?
            //Print(((MemberAccess)m.Method).Object);
            //sb.Append(".");
            var temp = _sb;
            _sb = new StringBuilder();
            Print(m.Args[0]);
            temp.Append(_sb.ToString().Replace("\"", String.Empty));
            _sb = temp;
            _sb.Append(")");
        }
        private void PrintOverrideFunction(MethodInvocation m, string assignment)
        {
            Print(((MemberAccess)m.Method).Object);
            _sb.Append(".");
            var temp = _sb;
            _sb = new StringBuilder();
            Print(m.Args[0]);
            temp.Append(_sb.ToString().Replace("\"", String.Empty));
            temp.Append(" ");
            temp.Append(assignment);
            temp.Append(" ");

            _sb = new StringBuilder();
            Print(m.Args[1]);
            temp.Append(_sb.ToString().Replace("\"", String.Empty));
            _sb = temp;
        }

        private void PrintGetData(MethodInvocation m)
        {
            Print(m.Method);
            _sb.Append("(");
            bool first = true;
            foreach (var e in m.Args)
            {
                if (first) first = false;
                else _sb.Append(", ref ");
                Print(e);
            }
            _sb.Append(")");
        }
    }
}
