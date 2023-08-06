using Random = System.Random;

public class BlockQueue
{
	public BlockQueue()
	{
		nextBlock = RandomBlock();
		saveBlock = null;
	}

	private static readonly BlockFactory blockCreateFunc = new();
	private const           int          blockTypeNum    = 7;
	private                 Block        nextBlock;
	private                 Block        saveBlock;

	private static Block RandomBlock()
	{
		if (GameManager.testBlock)
		{
			return blockCreateFunc.BlockSpawn((int)GameManager.testBlockType);
		}
		
		Random randValue = new();

		return blockCreateFunc.BlockSpawn(randValue.Next(0, blockTypeNum));
	}

	public void SaveBlockReset()
	{
		saveBlock = null;
	}

	public Block SaveAndUpdateBlock(Block save)
	{
		if (saveBlock == null)
		{
			saveBlock = save;

			return GetAndUpdateBlock();
		}

		saveBlock.Reset();

		Block block = saveBlock;
		saveBlock = save;

		return block;
	}

	public Block GetAndUpdateBlock()
	{
		Block block = nextBlock;

		if (GameManager.testBlock)
		{
			nextBlock = RandomBlock();
			block.Reset();

			return block;
		}

		do
		{
			nextBlock = RandomBlock();
		} while (block.GetId() == nextBlock.GetId());
		block.Reset();

		return block;
	}
}