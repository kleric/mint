
using Aura.Channel.Scripting.Mint;
using Aura.Channel.World.Entities;
using Aura.Mabi.Mint;

namespace Aura.Mabi.Mint { 
    public class mint_example_dungeon : MintScript {
		public void OnInitDungeon(DungeonBuilder builder)
		{
			builder.SetManual();
			PuzzleBuilder puzzleBuilder = builder.AddFixedPuzzle(0, "coolawesomescript.mint");
		}
	}
}
