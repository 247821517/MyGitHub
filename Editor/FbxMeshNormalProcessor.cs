using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Formats.Fbx.Exporter;

public class FbxMeshNormalProcessor
{

    [MenuItem("Assets/Fbx模型法线平滑化")]
    static void ExportToFbx()
    {
        foreach (GameObject asset in Selection.objects)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            Debug.Log(assetPath);
            string outputPath = assetPath.Insert(assetPath.LastIndexOf('.'), "copy");
            Debug.Log(outputPath);
            PostProcessModel(asset);
            ModelExporter.ExportObject(outputPath, asset);
        }
        Debug.Log("Fbx模型法线平滑化完毕！");
    }

    static void PostProcessModel(GameObject obj)
    {
        Mesh oriMesh = null;
        if (obj.GetComponent<MeshFilter>())
        {
            MeshFilter component = obj.GetComponent<MeshFilter>();
            oriMesh = component.sharedMesh;
            Mesh newMesh = ProcessMesh(oriMesh);
            component.mesh = newMesh;
        }
        else if (obj.GetComponent<SkinnedMeshRenderer>())
        {
            SkinnedMeshRenderer component = obj.GetComponent<SkinnedMeshRenderer>();
            oriMesh = component.sharedMesh;
            Mesh newMesh = ProcessMesh(oriMesh);
            component.sharedMesh = newMesh;
        }
        foreach (Transform child in obj.transform)
        {
            PostProcessModel(child.gameObject);
        }
    }

    public static Mesh ProcessMesh(Mesh mesh)
    {
        Vector3[] m_vertices = mesh.vertices;
        Vector3[] m_normals = mesh.normals;
        Vector4[] m_tangents = mesh.tangents;
        Hashtable hashtable = new Hashtable();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 vertex = m_vertices[i];
            Vector3 normal = m_normals[i];
            if (hashtable.Contains(vertex))
            {
                List<Vector3> normals = hashtable[vertex] as List<Vector3>;
                normals.Add(normal);
            }
            else
            {
                List<Vector3> normals = new List<Vector3>(new Vector3[] { normal });
                hashtable.Add(vertex, normals);
            }
        }

        Color[] m_colors = new Color[mesh.vertexCount];
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            Vector3 vertex = m_vertices[i];
            List<Vector3> normals = hashtable[vertex] as List<Vector3>;
            Vector3 sum = Vector3.zero;
            foreach (Vector3 normal in normals)
            {
                sum += normal;
            }
            Vector3 m_binormal = Vector3.Cross(m_normals[i], m_tangents[i]) * m_tangents[i].w;
            m_binormal.Normalize();
            Vector3 m_normal = m_normals[i].normalized;
            Vector3 m_tangent = m_tangents[i];
            m_tangent.Normalize();
            float sum_x = sum.x * m_tangent.x + sum.y * m_tangent.y + sum.z * m_tangent.z;
            float sum_y = sum.x * m_binormal.x + sum.y * m_binormal.y + sum.z * m_binormal.z;
            float sum_z = sum.x * m_normal.x + sum.y * m_normal.y + sum.z * m_normal.z;
            sum = new Vector3(sum_x, sum_y, sum_z);
            sum.Normalize();
            m_colors[i] = new Color(sum.x * 0.5f + 0.5f, sum.y * 0.5f + 0.5f, sum.z * 0.5f + 0.5f);
        }

        Mesh newMesh = new Mesh
        {
            vertices = mesh.vertices,
            triangles = mesh.triangles,
            normals = mesh.normals,
            tangents = mesh.tangents,
            uv = mesh.uv,
            uv2 = mesh.uv2,
            uv3 = mesh.uv3,
            uv4 = mesh.uv4,
            uv5 = mesh.uv5,
            uv6 = mesh.uv6,
            uv7 = mesh.uv7,
            uv8 = mesh.uv8,
            colors = m_colors,
            bounds = mesh.bounds,
            indexFormat = mesh.indexFormat,
            bindposes = mesh.bindposes,
            boneWeights = mesh.boneWeights,
        };
        return newMesh;
    }
}
