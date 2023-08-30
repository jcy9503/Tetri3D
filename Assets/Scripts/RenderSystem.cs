/*
 * Render.cs
 * ---------
 * Made by Lucas Jeong
 * Contains rendering related class.
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

/// <summary>
/// line mesh rendering using LineRenderer
/// </summary>
public class LineMesh
{
	public GameObject   Obj      { get; private set; }
	public LineRenderer Renderer { get; set; }

	/// <summary>
	/// main constructor of LineMesh
	/// </summary>
	/// <param name="name">LineMesh GameObject name</param>
	/// <param name="parent">parent GameObject</param>
	/// <param name="pos1">start position</param>
	/// <param name="pos2">end position</param>
	/// <param name="width">line width</param>
	/// <param name="matPath">material path</param>
	public LineMesh(string name,  GameObject parent, Vector3 pos1, Vector3 pos2,
	                float  width, string     matPath)
	{
		Obj = new GameObject(name)
		{
			transform =
			{
				parent = parent.transform
			}
		};
		Renderer               = Obj.AddComponent<LineRenderer>();
		Renderer.positionCount = 2;
		Renderer.SetPosition(0, pos1);
		Renderer.SetPosition(1, pos2);
		Renderer.startWidth    = width;
		Renderer.endWidth      = width;
		Renderer.useWorldSpace = true;
		Renderer.startColor    = Color.white;
		Renderer.endColor      = Color.white;
		Renderer.material      = new Material(Resources.Load<Material>(matPath));
	}
}

/// <summary>
/// cube mesh rendering class using UnityEngine
/// </summary>
public class CubeMesh
{
	public  GameObject   Obj       { get; private set; }
	private MeshFilter   MFilter   { get; set; }
	public  MeshRenderer MRenderer { get; private set; }
	private Vector3[]    Vertices  { get; set; }
	private Vector2[]    UVs       { get; set; }
	private int[]        Triangles { get; set; }

	/// <summary>
	/// main constructor of CubeMesh
	/// </summary>
	/// <param name="name">cube mesh GameObject name</param>
	/// <param name="x">width</param>
	/// <param name="y">height</param>
	/// <param name="z">depth</param>
	/// <param name="unit">default unit of size</param>
	/// <param name="inverse">flipping faces option</param>
	/// <param name="matPath">material data path in Resources/</param>
	/// <param name="bTexture">using texture option</param>
	/// <param name="texture">Texture2D object</param>
	public CubeMesh(string name,    int  x,        int       y, int z, float unit, bool inverse,
	                string matPath, bool bTexture, Texture2D texture)
	{
		Obj       = new GameObject(name);
		MFilter   = Obj.AddComponent<MeshFilter>();
		MRenderer = Obj.AddComponent<MeshRenderer>();

		InitializeVertices(x, y, z, unit);
		InitializeTriangles(inverse);
		InitializeUVs();

		if (bTexture)
		{
			CreateMesh(matPath, texture);
		}
		else
		{
			CreateMesh(matPath);
		}
	}

	private void InitializeVertices(int x, int y, int z, float unit)
	{
		Vertices = new[]
		{
			new Vector3(-x / 2f, y / 2f, -z / 2f) * unit,
			new Vector3(x  / 2f, y / 2f, -z / 2f) * unit,
			new Vector3(x  / 2f, y / 2f, z  / 2f) * unit,
			new Vector3(-x / 2f, y / 2f, z  / 2f) * unit,

			new Vector3(-x / 2f, y  / 2f, z / 2f) * unit,
			new Vector3(x  / 2f, y  / 2f, z / 2f) * unit,
			new Vector3(x  / 2f, -y / 2f, z / 2f) * unit,
			new Vector3(-x / 2f, -y / 2f, z / 2f) * unit,

			new Vector3(-x / 2f, -y / 2f, z  / 2f) * unit,
			new Vector3(x  / 2f, -y / 2f, z  / 2f) * unit,
			new Vector3(x  / 2f, -y / 2f, -z / 2f) * unit,
			new Vector3(-x / 2f, -y / 2f, -z / 2f) * unit,

			new Vector3(-x / 2f, -y / 2f, -z / 2f) * unit,
			new Vector3(x  / 2f, -y / 2f, -z / 2f) * unit,
			new Vector3(x  / 2f, y  / 2f, -z / 2f) * unit,
			new Vector3(-x / 2f, y  / 2f, -z / 2f) * unit,

			new Vector3(x / 2f, y  / 2f, z  / 2f) * unit,
			new Vector3(x / 2f, y  / 2f, -z / 2f) * unit,
			new Vector3(x / 2f, -y / 2f, -z / 2f) * unit,
			new Vector3(x / 2f, -y / 2f, z  / 2f) * unit,

			new Vector3(-x / 2f, y  / 2f, -z / 2f) * unit,
			new Vector3(-x / 2f, y  / 2f, z  / 2f) * unit,
			new Vector3(-x / 2f, -y / 2f, z  / 2f) * unit,
			new Vector3(-x / 2f, -y / 2f, -z / 2f) * unit
		};
	}

	private void InitializeTriangles(bool inverse)
	{
		if (inverse)
		{
			Triangles = new[]
			{
				0, 1, 2,
				0, 2, 3,
				4, 5, 6,
				4, 6, 7,
				8, 9, 10,
				8, 10, 11,
				12, 13, 14,
				12, 14, 15,
				16, 17, 18,
				16, 18, 19,
				20, 21, 22,
				20, 22, 23
			};
		}

		else
		{
			Triangles = new[]
			{
				0, 2, 1,
				0, 3, 2,
				4, 6, 5,
				4, 7, 6,
				8, 10, 9,
				8, 11, 10,
				12, 14, 13,
				12, 15, 14,
				16, 18, 17,
				16, 19, 18,
				20, 22, 21,
				20, 23, 22
			};
		}
	}

	private void InitializeUVs()
	{
		UVs = new[]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),

			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),

			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),

			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),

			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f),

			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
	}

	private void CreateMesh(string materialPath, Texture2D texture)
	{
		MFilter.mesh = new Mesh()
		{
			vertices  = Vertices,
			triangles = Triangles,
			uv        = UVs
		};

		MRenderer.material = new Material(Resources.Load<Material>(materialPath))
		{
			mainTexture = texture
		};

		MFilter.mesh.RecalculateNormals();
	}

	private void CreateMesh(string materialPath)
	{
		MFilter.mesh = new Mesh()
		{
			vertices  = Vertices,
			triangles = Triangles,
			uv        = UVs
		};

		MRenderer.material = new Material(Resources.Load<Material>(materialPath));

		MFilter.sharedMesh.RecalculateNormals();
	}
}

/// <summary>
/// prefab mesh load class
/// </summary>
public class PrefabMesh
{
	public          GameObject   Obj { get; set; }
	public readonly Coord        pos;
	public readonly MeshRenderer renderer;

	public PrefabMesh(string meshPath, Vector3 pos, IReadOnlyList<string> matPath, Coord coord)
	{
		Obj      = Object.Instantiate(Resources.Load<GameObject>(meshPath), pos, Quaternion.identity);
		renderer = Obj.GetComponent<MeshRenderer>();

		Material[] matSet = renderer.materials;
		for (int i = 0; i < matPath.Count; ++i)
		{
			matSet[i] = Resources.Load<Material>(matPath[i]);
		}

		renderer.materials = matSet;

		this.pos = coord;
	}
}

/// <summary>
/// particle load class
/// </summary>
public class ParticleRender
{
	public GameObject   Obj      { get; set; }
	public VisualEffect Renderer { get; set; }

	public ParticleRender(string particlePath, Vector3 pos, GameObject parent, Quaternion rotation)
	{
		Obj                  = Object.Instantiate(Resources.Load<GameObject>(particlePath), pos, rotation);
		Obj.transform.parent = parent.transform;
		Renderer             = Obj.GetComponentInChildren<VisualEffect>();
	}
}

public sealed class RenderSystem : MonoSingleton<RenderSystem>
{
	private static          GameObject       blockObj;
	private static          GameObject       shadowObj;
	public static           GameObject       gridObj;
	private static          List<PrefabMesh> blockMeshList;
	private static          List<PrefabMesh> shadowMeshList;
	public static           List<PrefabMesh> gridMeshList;
	public static           List<LineMesh>   lineMeshList;
	public static           Vector3          startOffset;
	private static readonly int              gradientColor = Shader.PropertyToID("_GradientColor");

	public void Init()
	{
		blockMeshList  = new List<PrefabMesh>();
		shadowMeshList = new List<PrefabMesh>();
		gridMeshList   = new List<PrefabMesh>();
		lineMeshList   = new List<LineMesh>();

		gridObj   = GameObject.Find("Grid");
		blockObj  = GameObject.Find("Blocks");
		shadowObj = GameObject.Find("Shadow");
	}

	public static void RenderLine()
	{
		lineMeshList.Clear();
		const float width = 0.05f;

		LineMesh mesh01 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      new Vector3(GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh01);

		LineMesh mesh02 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      new Vector3(GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh02);

		LineMesh mesh03 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh03);

		LineMesh mesh04 = new("Line", gridObj,
		                      new Vector3(GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh04);

		LineMesh mesh05 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX  / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh05);

		LineMesh mesh06 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX  / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh06);

		LineMesh mesh07 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh07);

		LineMesh mesh08 = new("Line", gridObj,
		                      new Vector3(GameManager.grid.SizeX  / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX  / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh08);

		LineMesh mesh09 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(-GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh09);

		LineMesh mesh10 = new("Line", gridObj,
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(-GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ  / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh10);

		LineMesh mesh11 = new("Line", gridObj,
		                      new Vector3(GameManager.grid.SizeX  / 2f, -GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX / 2f, -GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh11);

		LineMesh mesh12 = new("Line", gridObj,
		                      new Vector3(GameManager.grid.SizeX  / 2f, GameManager.grid.SizeY / 2f,
		                                  -GameManager.grid.SizeZ / 2f),
		                      new Vector3(GameManager.grid.SizeX / 2f, GameManager.grid.SizeY / 2f,
		                                  GameManager.grid.SizeZ / 2f),
		                      width, "Materials/Line");
		lineMeshList.Add(mesh12);
	}

	public static void RenderCurrentBlock()
	{
		ClearCurrentBlock();

		foreach (Coord coord in GameManager.currentBlock.TilePositions())
		{
			string[] matPath =
			{
				Block.MAT_PATH[GameManager.currentBlock.GetId()],
				Block.MAT_PATH[GameManager.currentBlock.GetId()],
			};

			Vector3 offset = new(coord.X, -coord.Y, coord.Z);
			PrefabMesh mesh = new("Prefabs/Mesh_Block", startOffset + offset, matPath, coord);

			blockMeshList.Add(mesh);
			mesh.Obj.transform.parent = blockObj.transform;
		}
	}

	public static void RenderShadowBlock()
	{
		ClearShadowBlock();
		GameManager.shadowBlock = GameManager.currentBlock.CopyBlock();

		do
		{
			GameManager.shadowBlock.Move(Coord.Down);
		} while (GameManager.BlockFits(GameManager.shadowBlock));

		GameManager.shadowBlock.Move(Coord.Up);

		foreach (Coord coord in GameManager.shadowBlock.TilePositions())
		{
			string[] matPath =
			{
				Block.MAT_PATH[0],
				Block.MAT_PATH[0],
			};
			Vector3 offset = new(coord.X, -coord.Y, coord.Z);
			PrefabMesh mesh = new("Prefabs/Mesh_Block", startOffset + offset, matPath, coord);

			shadowMeshList.Add(mesh);
			mesh.Obj.transform.parent = shadowObj.transform;
		}
	}

	public static void RenderGrid()
	{
		ClearGrid();

		for (int i = 0; i < GameManager.grid.SizeY; ++i)
		{
			for (int j = 0; j < GameManager.grid.SizeX; ++j)
			{
				for (int k = 0; k < GameManager.grid.SizeZ; ++k)
				{
					if (GameManager.grid[j, i, k] == 0) continue;

					string[] matPath =
					{
						Block.MAT_PATH[^2],
						Block.MAT_PATH[^1],
					};
					Vector3    offset = new(j, -i, k);
					PrefabMesh mesh   = new("Prefabs/Mesh_Block", startOffset + offset, matPath, new Coord(j, i, k));
					mesh.renderer.materials[0].SetFloat(gradientColor,
					                                    (float)i / (GameManager.grid.SizeY - 1));

					gridMeshList.Add(mesh);
					mesh.Obj.transform.parent = gridObj.transform;
				}
			}
		}
	}

	public static void RefreshCurrentBlock()
	{
		RenderCurrentBlock();
		RenderShadowBlock();
	}

	public static void ClearCurrentBlock()
	{
		foreach (PrefabMesh mesh in blockMeshList)
		{
			Destroy(mesh.Obj);
		}

		blockMeshList.Clear();
	}

	public static void ClearShadowBlock()
	{
		foreach (PrefabMesh mesh in shadowMeshList)
		{
			Destroy(mesh.Obj);
		}

		shadowMeshList.Clear();
	}

	private static void ClearGrid()
	{
		foreach (PrefabMesh mesh in gridMeshList)
		{
			Destroy(mesh.Obj);
		}

		gridMeshList.Clear();
	}

	public void Reset()
	{
		RenderCurrentBlock();
		RenderShadowBlock();

		if (GameManager.testGrid) RenderGrid();

		RenderLine();
		EffectSystem.lineGlowPower = lineMeshList[0].Renderer.material.GetFloat(EffectSystem.power);
	}
}