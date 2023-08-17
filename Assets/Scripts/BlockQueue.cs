using Random = System.Random;

public class BlockQueue
{
	public BlockQueue(Block.BLOCK_TYPE type)
	{
		blockCreateFunc = new BlockFactory();
		nextBlock       = RandomBlock(type);
		saveBlock       = null;
	}

	private static BlockFactory blockCreateFunc;
	private const  int          blockTypeNum    = 7;
	private        Block        nextBlock;
	private        Block        saveBlock;

	private static Block RandomBlock(Block.BLOCK_TYPE type)
	{
		if (GameManager.testBlock)
		{
			Block returnBlock = blockCreateFunc.BlockSpawn((int)type);
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

			return GetAndUpdateBlock(Block.BLOCK_TYPE.DEFAULT);
		}

		saveBlock.Reset();

		Block block = saveBlock;
		saveBlock = save;

		return block;
	}

	public Block GetAndUpdateBlock(Block.BLOCK_TYPE type)
	{
		Block block = nextBlock;

		if (GameManager.testBlock)
		{
			nextBlock = RandomBlock(type);
			block.Reset();

			return block;
		}

		do
		{
			nextBlock = RandomBlock(Block.BLOCK_TYPE.DEFAULT);
		} while (block.GetId() == nextBlock.GetId());

		block.Reset();

		return block;
	}
}