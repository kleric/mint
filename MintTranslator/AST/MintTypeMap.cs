using System;
using System.Collections.Generic;
using System.Text;

namespace MintTranslator.AST
{
    public static class MintTypeMap
    {
        public static void Apply(Type type)
        {
            type.Name = MapMintToCSharpType(type.Name);
        }
        private static string MapMintToCSharpType(string name)
        {
            switch (name)
            {
                case "dungeonbuilder": return "DungeonBuilder";
                case "dungeon2": return "Dungeon2";
                case "puzzlebuilder": return "PuzzleBuilder";
                case "dword": return "int";
                case "qword": return "long";
                case "prop": return "Prop";
                case "dungeon2puzzle": return "Dungeon2Puzzle";
                case "dungeon2monster": return "Dungeon2Monster";
                case "dungeon2chest": return "Dungeon2Chest";
                case "object_list": return "ObjectList";
                case "character": return "Creature";
                case "meta_array": return "MetaArray";
                case "item": return "Item";
            }
            return name;
        }
        public static string GetTypeDefaultValue(Type type)
        {
            switch (type.Name)
            {
                case "int":
                case "float":
                    return "0";
                case "string":
                    return "null";
                case "bool":
                    return "false";
            }
            return "null";
        }
    }
}
