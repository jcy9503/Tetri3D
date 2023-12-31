using System.Collections.Generic;

public static class BlockFactory
{
	private delegate Block CreateBlock();
	private static readonly List<CreateBlock> createFactory = new()
	{
		CreateBlockI,
		CreateBlockL,
		CreateBlockT,
		CreateBlockO,
		CreateBlockJ,
		CreateBlockZ,
		CreateBlockS
	};

	public static Block BlockSpawn(int id)
	{
		CreateBlock func = createFactory[id];

		return func();
	}

	private static Block CreateBlockI()
	{
		Block block = new BlockI();

		return block;
	}

	private static Block CreateBlockL()
	{
		Block block = new BlockL();

		return block;
	}

	private static Block CreateBlockT()
	{
		Block block = new BlockT();

		return block;
	}

	private static Block CreateBlockO()
	{
		Block block = new BlockO();

		return block;
	}

	private static Block CreateBlockJ()
	{
		Block block = new BlockJ();

		return block;
	}

	private static Block CreateBlockZ()
	{
		Block block = new BlockZ();

		return block;
	}

	private static Block CreateBlockS()
	{
		Block block = new BlockS();

		return block;
	}
}