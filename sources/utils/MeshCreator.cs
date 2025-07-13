using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


internal class MeshCreator
{
    public static float PixelsToUnits(float pixels)
    {
        return pixels / 32; // Se usa 32f para asegurar que la división sea en punto flotante
    }
    public static ArrayMesh CreateSquareMesh(float widthPixels, float heightPixels)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        float halfWidth = 0.5f * PixelsToUnits(widthPixels);
        float halfHeight = 0.5f * PixelsToUnits(heightPixels);



        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),  // Bottom left
            new Vector3(halfWidth, -halfHeight, 0),   // Bottom right
            new Vector3(halfWidth, halfHeight, 0),    // Top right
            new Vector3(-halfWidth, halfHeight, 0)   // Top left
        };


        //float width = widthPixels / unitGodotPerPixel;
        //float height = heightPixels / unitGodotPerPixel;

        //Vector3[] vertices = new Vector3[]
        //{
        //new Vector3(0, 0, 0),              // Bottom left
        //new Vector3(width, 0, 0),          // Bottom right
        //new Vector3(width, height, 0),     // Top right
        //new Vector3(0, height, 0)          // Top left
        //};
        // Define the indices for two triangles that make up the square
        int[] indices = new int[]
        {
            0, 2, 1,  // First triangle
            0, 3, 2   // Second triangle
        };

        // Define the normals (optional but good practice)
        Vector3[] normals = new Vector3[]
        {
            new Vector3(0, 0, 1),  // Normal pointing out of the screen
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1)
        };

        // Define the UV coordinates (optional, used for texturing)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1),  // Bottom left
            new Vector2(1, 1),  // Bottom right
            new Vector2(1, 0),  // Top right
            new Vector2(0, 0)   // Top left
        };


        // Define an array to hold the data
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        // Assign the vertex, index, normal, and UV data
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV] = uvs;

        // Add a surface to the mesh
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh;
    }
    public static ArrayMesh CreateSquareMesh(float widthPixels, float heightPixels,  Vector2 unitGodotPerPixel , Vector3 offset)
    {
        ArrayMesh arrayMesh = new ArrayMesh();
        float halfWidth = 0.5f * PixelsToUnits(widthPixels );
        float halfHeight = 0.5f * PixelsToUnits(heightPixels );

     

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0) -offset,  // Bottom left
            new Vector3(halfWidth, -halfHeight, 0)-offset,   // Bottom right
            new Vector3(halfWidth, halfHeight, 0) -offset,    // Top right
            new Vector3(-halfWidth, halfHeight, 0) -offset   // Top left
        };


        //float width = widthPixels / unitGodotPerPixel;
        //float height = heightPixels / unitGodotPerPixel;

        //Vector3[] vertices = new Vector3[]
        //{
        //new Vector3(0, 0, 0),              // Bottom left
        //new Vector3(width, 0, 0),          // Bottom right
        //new Vector3(width, height, 0),     // Top right
        //new Vector3(0, height, 0)          // Top left
        //};
        // Define the indices for two triangles that make up the square
        int[] indices = new int[]
        {
            0, 2, 1,  // First triangle
            0, 3, 2   // Second triangle
        };

        // Define the normals (optional but good practice)
        Vector3[] normals = new Vector3[]
        {
            new Vector3(0, 0, 1),  // Normal pointing out of the screen
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1)
        };

        // Define the UV coordinates (optional, used for texturing)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1),  // Bottom left
            new Vector2(1, 1),  // Bottom right
            new Vector2(1, 0),  // Top right
            new Vector2(0, 0)   // Top left
        };


        // Define an array to hold the data
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        // Assign the vertex, index, normal, and UV data
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV] = uvs;

        // Add a surface to the mesh
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh;
    }

    public static Godot.Collections.Array CreateSquareArray(float widthPixels, float heightPixels, float unitGodotPerPixel = 32.0f)
    {

        float halfWidth = 0.5f * (widthPixels / unitGodotPerPixel);
        float halfHeight = 0.5f * (heightPixels / unitGodotPerPixel);

        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfWidth, -halfHeight, 0),  // Bottom left
            new Vector3(halfWidth, -halfHeight, 0),   // Bottom right
            new Vector3(halfWidth, halfHeight, 0),    // Top right
            new Vector3(-halfWidth, halfHeight, 0)    // Top left
        };

        //float width = widthPixels / unitGodotPerPixel;
        //float height = heightPixels / unitGodotPerPixel;

        //Vector3[] vertices = new Vector3[]
        //{
        //new Vector3(0, 0, 0),              // Bottom left
        //new Vector3(width, 0, 0),          // Bottom right
        //new Vector3(width, height, 0),     // Top right
        //new Vector3(0, height, 0)          // Top left
        //};

        // Define the indices for two triangles that make up the square
        int[] indices = new int[]
        {
            0, 2, 1,  // First triangle
            0, 3, 2   // Second triangle
        };

        // Define the normals (optional but good practice)
        Vector3[] normals = new Vector3[]
        {
            new Vector3(0, 0, 1),  // Normal pointing out of the screen
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 1)
        };

        // Define the UV coordinates (optional, used for texturing)
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 1),  // Bottom left
            new Vector2(1, 1),  // Bottom right
            new Vector2(1, 0),  // Top right
            new Vector2(0, 0)   // Top left
        };


        // Define an array to hold the data
        Godot.Collections.Array arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);


        // Assign the vertex, index, normal, and UV data
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = indices;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV] = uvs;




        return arrays;
    }
}

