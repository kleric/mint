using System;
using System.Collections.Generic;
using System.Text;

namespace MintTranslator.AST
{
    public class ASTProcessor
    {
        private Dictionary<string, List<FunctionDeclaration>> _funcDecls;
        private ASTTypeInferencer _typeInferencer;
        public ASTProcessor()
        {
            _funcDecls = new Dictionary<string, List<FunctionDeclaration>>();
            _typeInferencer = new ASTTypeInferencer();
        }
        public void ProcessAst(Node root)
        {
            Init(root);

            RemoveMultiVariableDeclarators(root);

            FixRefMethodInvocations(root);
            MapMintTypesToCSharp(root);

            _typeInferencer.InferTypes(root);
        }

        #region Processing Initialization
        private void Init(Node root)
        {
            ReadFunctionDeclarations(root);
        }

        private void ReadFunctionDeclarations(Node root)
        {
            if (root is FunctionDeclaration)
            {
                var func = (FunctionDeclaration)root;
                if (!_funcDecls.ContainsKey(func.Identifier))
                {
                    _funcDecls[func.Identifier] = new List<FunctionDeclaration>();
                }
                _funcDecls[func.Identifier].Add(func);
            }
            foreach (var child in root.GetChildren())
            {
                ReadFunctionDeclarations(child);
            }
        }

        #endregion
        private void MapMintTypesToCSharp(Node node)
        {
            if (node is Type)
            {
                MintTypeMap.Apply((Type)node);
            }
            node.ApplyToChildren(MapMintTypesToCSharp);
        }

        /**
         * Mint doesn't require you to preface out parameters with
         * out. Update all method invocations with reference info
         * if we have it.
         */
        private void FixRefMethodInvocations(Node node)
        {
            if (node is MethodInvocation)
            {
                var methodInvoke = (MethodInvocation)node;
                var method = methodInvoke.Method as VariableExpression;
                if (method != null)
                {
                    if (_funcDecls.ContainsKey(method.Identifier))
                    {
                        var funcs = _funcDecls[method.Identifier];
                        foreach (var f in funcs)
                        {
                            if (f.Parameters.Count == methodInvoke.Args.Count)
                            {
                                for (int i = 0; i < f.Parameters.Count; i++)
                                {
                                    methodInvoke.Args[i].IsRef = f.Parameters[i].Type.IsRef;
                                }
                                break;
                            }
                        }
                    }
                }
            }
            node.ApplyToChildren(FixRefMethodInvocations);
        }
       
        /**
         * Splits off all comma separated declarators into their own lines
         * 
         * This isn't necessary but makes it easier to modify the type.
         */
        private void RemoveMultiVariableDeclarators(Node node)
        {
            if (node is VariableDeclaration)
            {
                var dec = (VariableDeclaration)node;
                if (dec.Declarators.Count > 1)
                {
                    if (node.Parent is StatementList)
                    {
                        StatementList parent = (StatementList)node.Parent;
                        int index = parent.Statements.IndexOf(dec);
                        parent.Statements.RemoveAt(index);
                        List<VariableDeclaration> newDecls = new List<VariableDeclaration>();
                        foreach (var declarator in dec.Declarators)
                        {
                            VariableDeclaration newDec = new VariableDeclaration();
                            newDec.Parent = parent;
                            newDec.Type = dec.Type;
                            var declList = new List<VariableDeclarator>();
                            declarator.Parent = newDec;
                            declList.Add(declarator);
                            newDec.Declarators = declList;
                            newDecls.Add(newDec);
                        }
                        parent.Statements.InsertRange(index, newDecls);
                    } 
                }
            }
            node.ApplyToChildren(RemoveMultiVariableDeclarators);
        }
    }
}
