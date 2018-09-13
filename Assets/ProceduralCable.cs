using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ProceduralCable : MonoBehaviour {

    public Vector3 a;
    public Vector3 b;
    public int step = 20;
    public float curvature = 1;
    public float radius = 0.2f;
    public int radiusStep = 6;
    public Vector2 uvMultiply = Vector2.one;

    public bool drawEditorLines = false;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    void OnEnable() {

        meshFilter = GetComponent<MeshFilter>() == null ? gameObject.AddComponent<MeshFilter>() : GetComponent<MeshFilter>();
        UpdateObject();

    }

	void Update () {
		
	}

    public float CurveHeight(int i)
    {
        i = Mathf.Clamp(i,0,step);
        float normalizedStep = (float)i / step;
        return (Mathf.Pow((normalizedStep * 2) - 1, 2) - 1)*curvature;
    }

    public Vector3 PointPosition(int i)
    {
        Vector3 segment = (b - a) / step;
        return a + segment * i + new Vector3(0,CurveHeight(i),0);
    }

    public Vector3[] VerticesForPoint(int i)
    {
        Vector3 pointPosition = PointPosition(i);
        Vector3 orientation;

        if (i == 0)
            orientation = PointPosition(1) - PointPosition(0);
        else if(i == step)
            orientation = PointPosition(step) - PointPosition(step-1);
        else
            orientation = PointPosition(i + 1) - PointPosition(i - 1);

        Quaternion rotation = Quaternion.LookRotation(orientation, Vector3.Cross(Vector3.down, b - a)); 

        List<Vector3> vertices = new List<Vector3>();
        float angleStep = 360f / (radiusStep-1);

        for(int h = 0; h < radiusStep; h++)
        {
            float angle = angleStep * h * Mathf.Deg2Rad;
            vertices.Add(pointPosition + rotation * (new Vector3( Mathf.Cos(angle)*radius , Mathf.Sin(angle)*radius , 0 )));
        }

        return vertices.ToArray();
    }

    public void UpdateObject()
    {
        meshFilter.sharedMesh = GenerateMesh();
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Cable mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i <= step; i++)
        {
            Vector3[] verticesForPoint = VerticesForPoint(i);
            for (int h = 0; h < verticesForPoint.Length; h++)
            {
                vertices.Add(verticesForPoint[h]);
                normals.Add((verticesForPoint[h] - PointPosition(i)).normalized);



                uvs.Add(new Vector2(i * uvMultiply.x,(float)h / (verticesForPoint.Length-1) * uvMultiply.y));



                if (i < step)
                {
                    int index = h + (i * radiusStep);

                    triangles.Add(index);
                    triangles.Add(index + 1);
                    triangles.Add(index + radiusStep);

                    triangles.Add(index);
                    triangles.Add(index + radiusStep);
                    triangles.Add(index + radiusStep - 1);

                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateBounds();
        
        mesh.RecalculateTangents();
        
        return mesh;
    }

    public float SegmentLenght(int a,int b)
    {
        return (PointPosition(b) - PointPosition(a)).magnitude;
    }
}
