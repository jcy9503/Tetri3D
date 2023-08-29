using System.Collections.Generic;
using UnityEngine;

public static class ItemFactory
{
	private delegate Item CreateItem();
	private static readonly List<CreateItem> createFactory = new()
	{
		CreateItemBomb,
	};

	public static Item ItemRandomSpawn()
	{
		int id = Random.Range(0, createFactory.Count);
		
		CreateItem func = createFactory[id];

		return func();
	}

	private static Item CreateItemBomb()
	{
		Item item = new ItemBomb();

		return item;
	}
}