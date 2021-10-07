// Dummy example script
server void OnInitDungeon(dungeonbuilder builder) {
	builder.SetManual();
	puzzlebuilder puzzleBuilder = builder.AddFixedPuzzle(0, `coolawesomescript.mint`);
}