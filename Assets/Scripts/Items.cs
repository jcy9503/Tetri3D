public abstract class Item
{
	private int id;

	protected Item(int id)
	{
		this.id = id;
	}

	protected abstract void Execute();
}

public class ItemBomb : Item
{
	public ItemBomb() : base(id: 0)
	{
	}
	
	protected override void Execute()
	{
		
	}
}