using Random = System.Random;

public class BlockQueue
{
	public BlockQueue()
	{
		blockCreateFunc = new BlockFactory();
		nextBlock       = RandomBlock();
		saveBlock       = null;
	}

	private static BlockFactory blockCreateFunc;
	private const  int          blockTypeNum    = 7;
	private        Block        nextBlock;
	private        Block        saveBlock;

	private static Block RandomBlock()
	{
		if (GameManager.testBlock)
		{
			Block returnBlock = blockCreateFunc.BlockSpawn((int)GameManager.Instance.testBlockType);
			return returnBlock;
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