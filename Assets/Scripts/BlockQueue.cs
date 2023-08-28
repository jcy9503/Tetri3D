using Random = System.Random;

public class BlockQueue
{
	private const int   blockTypeNum = 7;
	private       Block nextBlock    = RandomBlock();
	private       Block saveBlock;

	private static Block RandomBlock()
	{
		if (GameManager.testBlock)
		{
			Block returnBlock = BlockFactory.BlockSpawn((int)GameManager.testBlockType);
			return returnBlock;
		}

		Random randValue = new();

		return BlockFactory.BlockSpawn(randValue.Next(0, (int)Block.BLOCK_TYPE.COUNT));
	}

	public void SaveBlockReset()
	{
		saveBlock = null;
	}

	public Block SaveAndUpdateBlock()
	{
		if (saveBlock == null)
		{
			saveBlock = GameManager.currentBlock;

			return GetAndUpdateBlock();
		}

		saveBlock.Reset();

		Block block = saveBlock;
		saveBlock = GameManager.currentBlock;

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

	public int GetNextBlockId()
	{
		return nextBlock.GetId();
	}

	public int GetSaveBlockId()
	{
		return saveBlock?.GetId() ?? 0;
	}
}