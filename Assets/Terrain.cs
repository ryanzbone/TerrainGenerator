using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Terrain : MonoBehaviour {

	public int size, arrayDimension, lastElement;
	public float[,] heightMap;
	public float random_limit;
	public float roughness;

	private Mesh mesh;
	public int[] triangles;
	public Vector3[] vertices;
	private Transform cam;
	
	void Start () {
		roughness = 1;
		random_limit = 1f;
		size = 7;
		arrayDimension = (int)Mathf.Pow(2, size) + 1;
		lastElement = arrayDimension - 1;
		heightMap = new float[arrayDimension, arrayDimension];

		heightMap[0,0] = 0;
		heightMap[0, lastElement] = 0;
		heightMap[lastElement, 0] = 0;
		heightMap[lastElement, lastElement] = 0;

		generatePoints(lastElement);
		RenderMesh ();
		cam = transform.GetChild(0);
		cam.position = new Vector3(80, 128, -80);
		cam.Rotate(Vector3.left, -20);
	}

	void generatePoints(int current_size) {
		int x, y, half = current_size / 2;
		while(current_size > 1) {
			float scale = roughness * current_size;
			half = current_size / 2;

			for (x = half; x < arrayDimension; x += current_size) {
				for (y = half; y < arrayDimension; y += current_size) {
					heightMap[x, y] = Square(x, y, half, Random.Range(0, random_limit) * scale);
				}
			}
			for (x = 0; x < arrayDimension; x += half) {
				for (y = (x + half) % current_size; y < arrayDimension; y += current_size) {
					heightMap[x, y] = Diamond(x, y, half, Random.Range(-random_limit, random_limit) * scale);
				}
			}
			current_size = current_size / 2;
		}
	}
	
	float Square(int x, int y, int size, float rand)
	{
		return Average(heightMap[x - size, y - size], 
			           heightMap[x - size, y + size], 
			           heightMap[x + size, y - size], 
			           heightMap[x + size, y + size]) + rand;
	}
	float Diamond(int x, int y, int size, float rand)
	{
		int left, right, top, bottom;

		if(x - size < 0)
			left = lastElement - size;
		else
			left = x - size;
		if(x + size > lastElement)
			right = 0 + size;
		else
			right = x + size;
		if (y - size < 0)
			top = lastElement - size;
		else
			top = y - size;
		if (y + size > lastElement)
			bottom = 0 + size;
		else
			bottom = y + size;

		float p1, p2, p3, p4;

		p1 = heightMap[x, top];
		p2 = heightMap[right, y];
		p3 = heightMap[x, bottom];
		p4 = heightMap[left, y] + rand;
		return Average (p1, p2, p3, p4);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.LeftArrow))
			cam.RotateAround(new Vector3(64, 0, 64), Vector3.up, 50 * Time.deltaTime);
		if(Input.GetKey(KeyCode.RightArrow))
			cam.RotateAround(new Vector3(64, 0, 64), Vector3.up, -50 * Time.deltaTime);

		
	}

	float Average(float p1, float p2, float p3, float p4)
	{
		return (p1 + p2 + p3 + p4) / 4;
	}



	// ------------------------------------
	// Rendering the terrain
	// ------------------------------------

	void RenderMesh ()
	{
		// Rendering
		mesh = new Mesh ();
		GetComponent<MeshFilter> ().mesh = mesh;
		vertices = GetVertices (heightMap);
		mesh.vertices = vertices;
		triangles = GetTriangles ();
		mesh.triangles = triangles;
		mesh.RecalculateNormals ();
		Vector2[] uvs = new Vector2[vertices.Length];
		int i = 0;
		while (i < uvs.Length) {
			uvs [i] = new Vector2 (vertices [i].x, vertices [i].z);
			i++;
		}
		mesh.uv = uvs;
	}
	
	Vector3[] GetVertices(float[,] heightMap)
	{
		Vector3[] result = new Vector3[arrayDimension * arrayDimension];
		for(int i = 0; i < arrayDimension; i++) {
			for(int j = 0; j < arrayDimension; j++) {
				result[i * arrayDimension + j] = new Vector3(i, heightMap[i, j], j);
			}
		}
		return result;
	}
	
	int[] GetTriangles() {
		List<int> result = new List<int>();
		
		for(int row = 0; row < arrayDimension - 1; row++) {
			for(int col = 0; col < arrayDimension - 1; col++) {
				// creates first clockwise triangle
				result.Add(Offset(row, col)); 
				result.Add(Offset(row, col + 1));
				result.Add(Offset(row + 1, col));
				
				// creates second clockwise triangle
				result.Add(Offset(row + 1, col));
				result.Add(Offset(row, col + 1));
				result.Add(Offset(row + 1, col + 1));
			}
		}
		return result.ToArray();
	}
	
	int Offset(int row, int col)
	{
		return row * arrayDimension + col;
	}
}
