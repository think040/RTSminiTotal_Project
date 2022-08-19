using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public static class RenderUtil
{
    static readonly GraphicsDeviceType gdt;
    //static readonly int igdt;
    static RenderUtil()
    {
        RenderUtil.gdt = SystemInfo.graphicsDeviceType;
        //if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        //{
        //    igdt = 0;
        //}
        //else if (gdt == GraphicsDeviceType.OpenGLCore)
        //{
        //    igdt = 1;
        //}
        //else if (gdt == GraphicsDeviceType.Vulkan)
        //{
        //    igdt = 2;
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetW(Transform tr)
    {
        float4x4 W = float4x4.identity;

        float3 t = tr.position;
        float3x3 R = new float3x3(tr.rotation);
        float3 s = tr.localScale;

        W.c0 = new float4(s.x * R.c0, 0.0f);
        W.c1 = new float4(s.y * R.c1, 0.0f);
        W.c2 = new float4(s.z * R.c2, 0.0f);
        W.c3 = new float4(t, 1.0f);


        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCV(Camera cam)
    {
        return math.mul(RenderUtil.GetC(cam), RenderUtil.GetV(cam));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetV(Camera cam)
    {
        float4x4 V = float4x4.identity;

        quaternion rot = cam.transform.rotation;
        float3x3 rotM = new float3x3(rot);
        float3 pos = cam.transform.position;

        V.c0.x = rotM.c0.x; V.c1.x = rotM.c0.y; V.c2.x = rotM.c0.z;
        V.c0.y = rotM.c1.x; V.c1.y = rotM.c1.y; V.c2.y = rotM.c1.z;
        V.c0.z = rotM.c2.x; V.c1.z = rotM.c2.y; V.c2.z = rotM.c2.z;

        V.c3.x = math.dot(-pos, rotM.c0);
        V.c3.y = math.dot(-pos, rotM.c1);
        V.c3.z = math.dot(-pos, rotM.c2);
        V.c3.w = 1.0f;

        if (gdt == GraphicsDeviceType.OpenGLCore)
        {
            V.c2.z *= +1.0f;
        }

        return V;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetC(Camera cam)
    {
        float4x4 C = float4x4.identity;

        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
        C.c0.x = (1.0f / aspect) * cotFov;
        C.c1.y = cotFov;

        float4x4 T = float4x4.identity;

        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            C.c2.z = +far / (far - near);
            C.c3.z = -(far * near / (far - near));
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;

            if (cam.cameraType == CameraType.Game)
            {
                T.c2.z = -1.0f; T.c3.z = 1.0f;
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                T.c1.y = -1.0f;
                T.c2.z = -1.0f; T.c3.z = 1.0f;
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore)
        {
            C.c2.z = (far + near) / (far - near);
            C.c3.z = -(2 * far * near) / (far - near);
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;

            if (cam.cameraType == CameraType.Game)
            {

            }
            else if (cam.cameraType == CameraType.SceneView)
            {

            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            C.c2.z = (far + near) / (far - near);
            C.c3.z = -(2 * far * near) / (far - near);
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;

            if (cam.cameraType == CameraType.Game)
            {
                T.c2.z = -1.0f; T.c3.z = 1.0f;
            }
            else if (cam.cameraType == CameraType.SceneView)
            {

                T.c1.y = -1.0f;
                T.c2.z = -1.0f; T.c3.z = 1.0f;
            }
        }

        C = math.mul(T, C);

        return C;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetC_noSRP(Camera cam)
    {
        float4x4 C = float4x4.identity;

        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
        C.c0.x = (1.0f / aspect) * cotFov;
        C.c1.y = cotFov;


        float4x4 T = float4x4.identity;

        if (gdt == GraphicsDeviceType.Direct3D11)
        {
            C.c2.z = +far / (far - near);
            C.c3.z = -(far * near / (far - near));
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;

            T.c1.y = -1.0f;
            T.c2.z = -1.0f; T.c3.z = 1.0f;
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore)
        {
            C.c2.z = (far + near) / (far - near);
            C.c3.z = -(2 * far * near) / (far - near);
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            C.c2.z = (far + near) / (far - near);
            C.c3.z = -(2 * far * near) / (far - near);
            C.c2.w = 1.0f;
            C.c3.w = 0.0f;

            T.c1.y = -1.0f;
            T.c2.z = -1.0f; T.c3.z = 1.0f;
        }

        C = math.mul(T, C);
        return C;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetW_IT(Transform tr)
    {
        return math.transpose(math.inverse(RenderUtil.GetW(tr)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetV_IT(Camera cam)
    {
        return math.transpose(math.inverse(RenderUtil.GetV(cam)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetW_Light(Light light)
    {
        float4x4 W = float4x4.identity;
        Transform tr = light.gameObject.transform;
        float3x3 R = new float3x3(tr.rotation);
        float3 t = tr.position;

        W.c0 = new float4(R.c0, 0.0f);
        W.c1 = new float4(R.c1, 0.0f);
        W.c2 = new float4(R.c2, 0.0f);
        W.c3 = new float4(t, 1.0f);

        float4x4 W_Light = W;
        return W_Light;
    }

    public static Mesh CreateTriangleMesh(float normalAngle, Mesh inMesh = null)
    {
        Mesh outMesh;
        if (inMesh != null)
        {
            outMesh = inMesh;
        }
        else
        {
            outMesh = new Mesh();
        }

        Vector3[] vertices = new Vector3[3];
        int[] indices = new int[3];
        Vector3[] normals = new Vector3[3];

        //Set vertices
        float size = 1.0f;
        vertices[0] = new Vector3(
            0.0f,
            size * Mathf.Sin(60.0f * Mathf.Deg2Rad) * (2.0f / 3.0f),
            0.0f);
        vertices[1] = Quaternion.AngleAxis(120.0f, new Vector3(0.0f, 0.0f, -1.0f)) * vertices[0];
        vertices[2] = Quaternion.AngleAxis(-120.0f, new Vector3(0.0f, 0.0f, -1.0f)) * vertices[0];

        //Set indices
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        //Set normals         
        normals[0] = Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.back, vertices[0])) * Vector3.back;
        normals[1] = Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.back, vertices[1])) * Vector3.back;
        normals[2] = Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.back, vertices[2])) * Vector3.back;

        //Applaying vertices indices normals
        List<Vector3> ListVertices = new List<Vector3>();
        foreach (Vector3 temp in vertices)
        {
            ListVertices.Add(temp);
        }

        List<Vector3> ListNormals = new List<Vector3>();
        foreach (Vector3 temp in normals)
        {
            ListNormals.Add(temp);
        }

        outMesh.SetVertices(ListVertices);
        outMesh.SetIndices(indices, MeshTopology.Triangles, 0);
        outMesh.SetNormals(ListNormals);

        return outMesh;
    }

    public static Mesh CreateSimpleTriangle(bool cw)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();

        if (cw)
        {
            vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
            vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
        }
        else
        {
            vertices.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertices.Add(new Vector3(1.0f, 0.0f, 0.0f));
            vertices.Add(new Vector3(0.0f, 1.0f, 0.0f));
        }
        mesh.SetVertices(vertices);

        int[] indices = new int[3];
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        /*
        NativeArray<Vector3> ver = new NativeArray<Vector3>(3, Allocator.Temp);
        ver[0] = vertices[0];
        ver[1] = vertices[1];
        ver[2] = vertices[2];
        mesh.SetVertices<Vector3>(ver);

        NativeArray<int> indi = new NativeArray<int>(3, Allocator.Temp);
        indi[0] = indices[0];
        indi[1] = indices[1];
        indi[2] = indices[2];
        mesh.SetIndices<int>(indi, MeshTopology.Triangles, 0);
        */
        return mesh;
    }

    public static Mesh CreateQuadMesh(float normalAngle, bool bTri = true)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        vertices.Add(new Vector3(-0.5f, +0.0f, -0.5f));
        vertices.Add(new Vector3(+0.5f, +0.0f, -0.5f));
        vertices.Add(new Vector3(-0.5f, +0.0f, +0.5f));
        vertices.Add(new Vector3(+0.5f, +0.0f, +0.5f));

        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[0])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[1])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[2])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[3])) * Vector3.up);

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        //mesh.SetTangents(tangents);
        //mesh.SetUVs(0, uvs);

        if (bTri)
        {
            indices.Add(0);
            indices.Add(3);
            indices.Add(1);

            indices.Add(3);
            indices.Add(0);
            indices.Add(2);

            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
        else
        {
            //indices.Add(0);
            //indices.Add(1);
            //indices.Add(3);
            //indices.Add(2);

            indices.Add(0);
            indices.Add(2);
            indices.Add(3);
            indices.Add(1);

            //indices.Add(0);
            //indices.Add(1);
            //indices.Add(2);
            //indices.Add(3);

            mesh.SetIndices(indices, MeshTopology.Quads, 0);
        }

        return mesh;
    }

    public static void ChangeQuadMesh(float normalAngle, bool bTri, Mesh mesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        //List<Vector4> tangents = new List<Vector4>();
        //List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        mesh.GetVertices(vertices);

        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[0])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[1])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[2])) * Vector3.up);
        normals.Add(Quaternion.AngleAxis(normalAngle, Vector3.Cross(Vector3.up, vertices[3])) * Vector3.up);

        mesh.SetNormals(normals);
        //mesh.RecalculateNormals();
        //mesh.SetTangents(tangents);
        //mesh.SetUVs(0, uvs);         

        if (bTri)
        {
            indices.Add(0);
            indices.Add(3);
            indices.Add(1);

            indices.Add(3);
            indices.Add(0);
            indices.Add(2);

            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        }
        else
        {
            indices.Add(0);
            indices.Add(2);
            indices.Add(3);
            indices.Add(1);

            mesh.SetIndices(indices, MeshTopology.Quads, 0);
        }
    }

    public static Mesh CreateTerrainMesh(float3 size)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        vertices.Add(new Vector3(+0.0f, +0.0f, +0.0f));
        vertices.Add(new Vector3(size.x, +0.0f, +0.0f));
        vertices.Add(new Vector3(+0.0f, +0.0f, size.z));
        vertices.Add(new Vector3(size.x, +0.0f, size.z));

        mesh.SetVertices(vertices);
        //mesh.SetNormals(normals);
        //mesh.SetTangents(tangents);
        //mesh.SetUVs(0, uvs);

        indices.Add(0);
        indices.Add(2);
        indices.Add(3);
        indices.Add(1);
        mesh.SetIndices(indices, MeshTopology.Quads, 0);

        return mesh;
    }

    public static Mesh CreateTerrainMeshGrid(float3 size, int3 count)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        int cx = count.x;
        int cz = count.z;

        float dx = size.x / count.x;
        float dz = size.z / count.z;

        for (int i = 0; i < cz + 1; i++)
        {
            for (int j = 0; j < cx + 1; j++)
            {
                vertices.Add(new Vector3(dx * j, 0.0f, dz * i));
            }
        }

        for (int i = 0; i < cz; i++)
        {
            for (int j = 0; j < cx; j++)
            {
                int i0 = (i + 0) * (cx + 1);
                int i1 = (i + 1) * (cx + 1);
                int j0 = (j + 0);
                int j1 = (j + 1);

                indices.Add(i0 + j0);
                indices.Add(i1 + j0);
                indices.Add(i1 + j1);
                indices.Add(i0 + j1);
            }
        }

        mesh.SetVertices(vertices);
        //mesh.SetNormals(normals);
        //mesh.SetTangents(tangents);
        //mesh.SetUVs(0, uvs);        
        mesh.SetIndices(indices, MeshTopology.Quads, 0);

        return mesh;
    }


    public static Mesh CreateTorusMesh0(float radiusOut = 1.0f, float radiusIn = 2.0f, int sliceCone = 24, int sliceCircle = 24)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radiusOut;
        float r1 = radiusIn;
        int s0 = sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(360.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        for (int i = 0; i < s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                (+r0 * math.sin(theta) + r1) * math.cos(phi),
                (+r0 * math.cos(theta)),
                (+r0 * math.sin(theta) + r1) * math.sin(phi));

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.cos(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.sin(phi));

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta) + r1) * (-math.sin(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta) + r1) * (+math.cos(phi)));

                float3 vertex = r;
                float3 normal = -math.normalize(math.cross(vTheta, vPhi));
                float4 tangent = -new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);
                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
            }
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = ((i + 1) % s0) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v10);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v01);
            }
        }

        //mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);

        //mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);        
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents();        
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }

    public static Mesh CreateTorusMesh(float radiusOut = 1.0f, float radiusIn = 2.0f, int sliceCone = 24, int sliceCircle = 24)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radiusOut;
        float r1 = radiusIn;
        int s0 = sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(360.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        for (int i = 0; i < s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                     (+r0 * math.sin(theta) + r1) * math.sin(phi),
                     (+r0 * math.cos(theta)),
                     (+r0 * math.sin(theta) + r1) * math.cos(phi)
                 );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta) + r1) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta) + r1) * (-math.sin(phi))
                        );

                float3 vertex = r;
                float3 normal = math.normalize(math.cross(vTheta, vPhi));
                float4 tangent = new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);
                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
            }
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = ((i + 1) % s0) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);

                //indices.Add(v00);
                //indices.Add(v11);
                //indices.Add(v10);
                //
                //indices.Add(v11);
                //indices.Add(v00);
                //indices.Add(v01);
            }
        }

        //mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);

        //mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);        
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        //mesh.RecalculateTangents();        
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }


    public static Mesh CreateSphereMesh0(float radius, int sliceCone, int sliceCircle)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                (+r0 * math.sin(theta)) * math.cos(phi),
                (+r0 * math.cos(theta)),
                (+r0 * math.sin(theta)) * math.sin(phi));

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.cos(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.sin(phi));

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (-math.sin(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (+math.cos(phi)));

                float3 vertex = r;
                float3 normal = float3.zero;
                float4 tangent = -new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                }
                else
                {
                    normal = -math.normalize(math.cross(vTheta, vPhi));
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
            }
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v10);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v01);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;

    }

    public static Mesh CreateSphereMesh(float radius, int sliceCone, int sliceCircle)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.sin(phi),
                    (+r0 * math.cos(theta)),
                    (+r0 * math.sin(theta)) * math.cos(phi)
                );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (-math.sin(phi))
                        );

                float3 vertex = r;
                float3 normal = float3.zero;
                float3 tangent = float3.zero;
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else
                {
                    normal = math.normalize(math.cross(vTheta, vPhi));
                    tangent = math.normalize(vPhi);
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(new float4(tangent, 0.0f));
                uvs.Add(uv);
            }
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;

    }


    public static Mesh CreateSphereMesh_ForArrow0(float radius, int sliceCone, int sliceCircle, out List<float> bonePos)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector4> boneI = new List<Vector4>();
        List<int> indices = new List<int>();
        bonePos = new List<float>();

        float r0 = radius;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        quaternion rot = quaternion.LookRotation(new float3(1.0f, 0.0f, 0.0f), new float3(0.0f, 0.0f, 1.0f));

        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            float3 vertex = float3.zero;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                (+r0 * math.sin(theta)) * math.cos(phi),
                (+r0 * math.cos(theta)),
                (+r0 * math.sin(theta)) * math.sin(phi));

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.cos(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.sin(phi));

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (-math.sin(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (+math.cos(phi)));

                vertex = r;
                float3 normal = float3.zero;
                float4 tangent = -new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                }
                else
                {
                    normal = -math.normalize(math.cross(vTheta, vPhi));
                }

                vertex = math.rotate(rot, vertex);
                normal = math.rotate(rot, normal);
                tangent.xyz = math.rotate(rot, tangent.xyz);

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
                boneI.Add(new Vector4((float)i, 0.0f, 0.0f, 0.0f));
            }
            bonePos.Add(vertex.z);
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v10);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v01);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(4, boneI);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;

    }

    public static Mesh CreateSphereMesh_ForArrow(float radius, int sliceCone, int sliceCircle, out List<float> bonePos)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector4> boneI = new List<Vector4>();
        List<int> indices = new List<int>();
        bonePos = new List<float>();

        float r0 = radius;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        quaternion rot = quaternion.LookRotation(new float3(1.0f, 0.0f, 0.0f), new float3(0.0f, 0.0f, 1.0f));

        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            float3 vertex = float3.zero;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.sin(phi),
                    (+r0 * math.cos(theta)),
                    (+r0 * math.sin(theta)) * math.cos(phi)
                );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (-math.sin(phi))
                        );

                vertex = r;
                float3 normal = float3.zero;
                float3 tangent = float3.zero;
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else
                {
                    normal = math.normalize(math.cross(vTheta, vPhi));
                    tangent = math.normalize(vPhi);
                }

                vertex = math.rotate(rot, vertex);
                normal = math.rotate(rot, normal);
                tangent = math.rotate(rot, tangent);

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(new float4(tangent, 0.0f));
                uvs.Add(uv);
                boneI.Add(new Vector4((float)i, 0.0f, 0.0f, 0.0f));
            }
            bonePos.Add(vertex.z);
        }

        for (int i = 0; i < s0; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);

                //indices.Add(v00);
                //indices.Add(v11);
                //indices.Add(v10);
                //
                //indices.Add(v11);
                //indices.Add(v00);
                //indices.Add(v01);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetUVs(4, boneI);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;

    }


    public static Mesh CreateCapsuleMesh0(float radius, float height, int sliceCone, int sliceCircle)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        float hh = 0.5f * height - radius;
        if (hh < 0.0f)
        {
            hh = 0.0f;
        }

        float sign = 1.0f;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        int k = 0;
        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                (+r0 * math.sin(theta)) * math.cos(phi),
                (+r0 * math.cos(theta)) + sign * hh,
                (+r0 * math.sin(theta)) * math.sin(phi));

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.cos(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.sin(phi));

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (-math.sin(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (+math.cos(phi)));

                float3 vertex = r;
                float3 normal = float3.zero;
                float4 tangent = -new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                }
                else
                {
                    normal = -math.normalize(math.cross(vTheta, vPhi));
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
            }

            if (k <= sliceCone)
            {
                k = k + 1;
                if (k == sliceCone)
                {
                    i = i - 1;
                    sign = -1.0f;
                    k = k + 1;
                }
            }
        }

        for (int i = 0; i < s0 + 1; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v10);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v01);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);


        return mesh;
    }

    public static Mesh CreateCapsuleMesh(float radius, float height, int sliceCone, int sliceCircle)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        float hh = 0.5f * height - radius;
        if (hh < 0.0f)
        {
            hh = 0.0f;
        }

        float sign = 1.0f;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        int k = 0;
        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.sin(phi),
                    (+r0 * math.cos(theta)) + sign * hh,
                    (+r0 * math.sin(theta)) * math.cos(phi)
                );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (-math.sin(phi))
                        );

                float3 vertex = r;
                float3 normal = float3.zero;
                float3 tangent = float3.zero;
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else
                {
                    normal = math.normalize(math.cross(vTheta, vPhi));
                    tangent = math.normalize(vPhi);
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(new float4(tangent, 0.0f));
                uvs.Add(uv);
            }

            if (k <= sliceCone)
            {
                k = k + 1;
                if (k == sliceCone)
                {
                    i = i - 1;
                    sign = -1.0f;
                    k = k + 1;
                }
            }
        }

        for (int i = 0; i < s0 + 1; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                //indices.Add(v00);
                //indices.Add(v11);
                //indices.Add(v10);
                //
                //indices.Add(v11);
                //indices.Add(v00);
                //indices.Add(v01);

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);


        return mesh;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mesh UpdateCapsuleMesh(float radius, float height, int sliceCone, int sliceCircle, Mesh mesh)
    {
        //Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        float hh = 0.5f * height - radius;
        if (hh < 0.0f)
        {
            hh = 0.0f;
        }

        float sign = 1.0f;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        int k = 0;
        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.sin(phi),
                    (+r0 * math.cos(theta)) + sign * hh,
                    (+r0 * math.sin(theta)) * math.cos(phi)
                );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (-math.sin(phi))
                        );

                float3 vertex = r;
                float3 normal = float3.zero;
                float3 tangent = float3.zero;
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else
                {
                    normal = math.normalize(math.cross(vTheta, vPhi));
                    tangent = math.normalize(vPhi);
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(new float4(tangent, 0.0f));
                uvs.Add(uv);
            }

            if (k <= sliceCone)
            {
                k = k + 1;
                if (k == sliceCone)
                {
                    i = i - 1;
                    sign = -1.0f;
                    k = k + 1;
                }
            }
        }

        for (int i = 0; i < s0 + 1; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);


        return mesh;
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetWMesh_fromCapsuleCollider(CapsuleCollider capCol, Transform tr, Mesh mesh, out float4x4 W)
    {
        float r = capCol.radius;
        float h = capCol.height;
        float3 c = capCol.center;
        int d = capCol.direction;

        float3 pos = tr.position;
        quaternion rot = tr.rotation;
        float3 sca = tr.localScale;

        float3x3 R = float3x3.identity;
        float3x3 R0 = float3x3.identity;

        float s;
        R = new float3x3(rot);
        if (d == 0)
        {
            s = math.max(sca.y, sca.z);
            r = r * s;
            h = h * sca.x;

            R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(0.0f, 0.0f, 1.0f), math.radians(-90.0f))));
        }
        else if (d == 1)
        {
            s = math.max(sca.x, sca.z);
            r = r * s;
            h = h * sca.y;

            R0 = R;
        }
        else if (d == 2)
        {
            s = math.max(sca.x, sca.y);
            r = r * s;
            h = h * sca.z;

            R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), math.radians(+90.0f))));
        }

        W.c0 = new float4(R0.c0, 0.0f);
        W.c1 = new float4(R0.c1, 0.0f);
        W.c2 = new float4(R0.c2, 0.0f);

        W.c3 = new float4(
                   pos +
                   math.mul(R, sca * c),
                   1.0f);
       
        {
            UpdateCapsuleMesh(r, h, 12, 24, mesh);
        }       
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mesh CreateCapsuleMesh(CapsuleCollider capCol, Transform tr,  int sliceCone, int sliceCircle)
    {
        Mesh mesh = new Mesh();
        float radius;
        float height;
       
        {
            float r = capCol.radius;
            float h = capCol.height;
            float3 c = capCol.center;
            int d = capCol.direction;

            //float3 pos = tr.position;
            //quaternion rot = tr.rotation;
            float3 sca = tr.localScale;

            //float3x3 R = float3x3.identity;
            //float3x3 R0 = float3x3.identity;

            float s;
            //R = new float3x3(rot);
            if (d == 0)
            {
                s = math.max(sca.y, sca.z);
                r = r * s;
                h = h * sca.x;

                //R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(0.0f, 0.0f, 1.0f), math.radians(-90.0f))));
            }
            else if (d == 1)
            {
                s = math.max(sca.x, sca.z);
                r = r * s;
                h = h * sca.y;

                //R0 = R;
            }
            else if (d == 2)
            {
                s = math.max(sca.x, sca.y);
                r = r * s;
                h = h * sca.z;

                //R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), math.radians(+90.0f))));
            }

            radius = r;
            height = h;
        }        

        

        //Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        float hh = 0.5f * height - radius;
        if (hh < 0.0f)
        {
            hh = 0.0f;
        }

        float sign = 1.0f;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        int k = 0;
        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.sin(phi),
                    (+r0 * math.cos(theta)) + sign * hh,
                    (+r0 * math.sin(theta)) * math.cos(phi)
                );

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.sin(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.cos(phi)
                        );

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (+math.cos(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (-math.sin(phi))
                        );

                float3 vertex = r;
                float3 normal = float3.zero;
                float3 tangent = float3.zero;
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                    vPhi = new float3(+math.cos(phi), 0.0f, -math.sin(phi));
                    tangent = vPhi;
                }
                else
                {
                    normal = math.normalize(math.cross(vTheta, vPhi));
                    tangent = math.normalize(vPhi);
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(new float4(tangent, 0.0f));
                uvs.Add(uv);
            }

            if (k <= sliceCone)
            {
                k = k + 1;
                if (k == sliceCone)
                {
                    i = i - 1;
                    sign = -1.0f;
                    k = k + 1;
                }
            }
        }

        for (int i = 0; i < s0 + 1; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v01);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v10);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);


        return mesh;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetW_fromCapsuleCollider(CapsuleCollider capCol, Transform tr, out float4x4 W)
    {
        //float r = capCol.radius;
        //float h = capCol.height;
        float3 c = capCol.center;
        int d = capCol.direction;

        float3 pos = tr.position;
        quaternion rot = tr.rotation;
        float3 sca = tr.localScale;

        float3x3 R = float3x3.identity;
        float3x3 R0 = float3x3.identity;

        float s;
        R = new float3x3(rot);
        if (d == 0)
        {
            //s = math.max(sca.y, sca.z);
            //r = r * s;
            //h = h * sca.x;

            R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(0.0f, 0.0f, 1.0f), math.radians(-90.0f))));
        }
        else if (d == 1)
        {
            //s = math.max(sca.x, sca.z);
            //r = r * s;
            //h = h * sca.y;

            R0 = R;
        }
        else if (d == 2)
        {
            //s = math.max(sca.x, sca.y);
            //r = r * s;
            //h = h * sca.z;

            R0 = new float3x3(math.mul(rot, quaternion.AxisAngle(new float3(1.0f, 0.0f, 0.0f), math.radians(+90.0f))));
        }

        W.c0 = new float4(R0.c0, 0.0f);
        W.c1 = new float4(R0.c1, 0.0f);
        W.c2 = new float4(R0.c2, 0.0f);

        W.c3 = new float4(
                   pos +
                   math.mul(R, sca * c),
                   1.0f);

        {
            //UpdateCapsuleMesh(r, h, 12, 24, mesh);
        }
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetW_fromSphereCollider(SphereCollider spCol, Transform tr, out float4x4 W)
    {
        W = float4x4.identity;

        float3 pos = tr.position;
        quaternion rot = tr.rotation;
        float3 sca = tr.localScale;

        float3x3 R = new float3x3(rot);

        float r = spCol.radius;
        float3 c = spCol.center;
        float s = math.max(sca.x, math.max(sca.y, sca.z));

        W.c3 = new float4(pos + math.mul(R, sca * c), 1.0f);
        R = R * s * r;
        W.c0 = new float4(R.c0, .0f);
        W.c1 = new float4(R.c1, .0f);
        W.c2 = new float4(R.c2, .0f);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 Cube_To_XZPlane(Transform tr)
    {
        float4x4 plane = float4x4.identity;

        float3 pos = tr.position;
        quaternion rot = tr.rotation;
        float3 sca = tr.localScale;

        float3x3 R = new float3x3(rot);

        float3 xn = +R.c0;
        float3 zn = +R.c2;

        float3 x0 = xn * sca.x * 0.5f;
        float3 z0 = zn * sca.z * 0.5f;

        plane.c0.xyz = +xn; //+x
        plane.c1.xyz = -xn; //-x
        plane.c2.xyz = +zn; //+z
        plane.c3.xyz = -zn; //-x

        plane.c0.w = math.dot(-xn, pos + x0); //+x
        plane.c1.w = math.dot(+xn, pos - x0); //-x
        plane.c2.w = math.dot(-zn, pos + z0); //+z
        plane.c3.w = math.dot(+zn, pos - z0); //-x


        return plane;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 Cube_To_XZPlane(Transform tr, out float4 center)
    {
        float4x4 plane = float4x4.identity;

        float3 pos = tr.position;
        quaternion rot = tr.rotation;
        float3 sca = tr.localScale;

        float3x3 R = new float3x3(rot);

        float3 xn = +R.c0;
        float3 zn = +R.c2;

        float3 x0 = xn * sca.x * 0.5f;
        float3 z0 = zn * sca.z * 0.5f;

        plane.c0.xyz = +xn; //+x
        plane.c1.xyz = -xn; //-x
        plane.c2.xyz = +zn; //+z
        plane.c3.xyz = -zn; //-x

        plane.c0.w = math.dot(-xn, pos + x0); //+x
        plane.c1.w = math.dot(+xn, pos - x0); //-x
        plane.c2.w = math.dot(-zn, pos + z0); //+z
        plane.c3.w = math.dot(+zn, pos - z0); //-x

        center = new float4(pos, 1.0f);

        return plane;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InCubeXZPlane(float4x4 plane, float3 pos)
    {
        unsafe
        {
            float4* p = (float4*)&plane;

            for (int i = 0; i < 4; i++)
            {
                if (math.dot(p[i].xyz, pos) + p[i].w > 0.0f)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static Mesh CreateIcosaHedron()
    {
        Mesh mesh = new Mesh();

        float size0 = 1.0f;
        float rad60 = math.radians(60.0f);
        float rad72 = math.radians(72.0f);
        float rad54 = math.radians(54.0f);
        float rad108 = math.radians(108.0f);

        float a = (1.0f / 3.0f) * size0 * math.sin(rad60);
        float b = (2.0f / 3.0f) * size0 * math.sin(rad72);

        float radX = 0.5f * math.acos((a * a + a * a - b * b) / (2 * a * a));

        float ec = a / math.cos(radX);
        float fc = a * math.tan(radX);
        float vc = math.sqrt(math.pow(0.5f * size0, 2) + math.pow(ec, 2));

        //float angle = math.acos((vc * vc + vc * vc - size0 * size0) / (2 * vc * vc));
        float angle = 2.0f * math.atan(0.5f * size0 / ec);


        return mesh;
    }

    //    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromL_noScale(Transform localTr)
    {
        float4x4 W = float4x4.identity;
        Transform tr = localTr;

        float3 t = tr.localPosition;
        quaternion r = tr.localRotation;
        while (tr.parent != null)
        {
            float3 pt = tr.parent.localPosition;
            quaternion pr = tr.parent.localRotation;
            quaternion q = math.mul(math.mul(pr, new quaternion(t.x, t.y, t.z, 0.0f)), math.conjugate(pr));
            t = pt + new float3(q.value.x, q.value.y, q.value.z);

            r = math.mul(pr, r);

            tr = tr.parent;
        }

        float3x3 R = new float3x3(r);
        W.c0 = new float4(R.c0, 0.0f);
        W.c1 = new float4(R.c1, 0.0f);
        W.c2 = new float4(R.c2, 0.0f);
        W.c3 = new float4(t, 1.0f);

        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetPfromC(Transform tr)
    {
        float4x4 P = float4x4.identity;
        float3 pos = tr.localPosition;
        float3x3 R = new float3x3(tr.localRotation);
        float3 sca = tr.localScale;

        P.c0 = new float4(sca.x * R.c0, 0.0f);
        P.c1 = new float4(sca.y * R.c1, 0.0f);
        P.c2 = new float4(sca.z * R.c2, 0.0f);
        P.c3 = new float4(pos, 1.0f);

        return P;
    }


    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromL(Transform localTr)
    {
        float4x4 W = float4x4.identity;
        W = GetPfromC(localTr);
        Transform tr = localTr;
        while (tr.parent != null)
        {
            W = math.mul(GetPfromC(tr.parent), W);
            tr = tr.parent;
        }

        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetVfromW(Camera cam)
    {
        float4x4 V = float4x4.identity;

        quaternion rot = cam.transform.rotation;
        float3x3 rotM = new float3x3(rot);
        float3 pos = cam.transform.position;

        V.c0.x = rotM.c0.x; V.c1.x = rotM.c0.y; V.c2.x = rotM.c0.z;
        V.c0.y = rotM.c1.x; V.c1.y = rotM.c1.y; V.c2.y = rotM.c1.z;
        V.c0.z = rotM.c2.x; V.c1.z = rotM.c2.y; V.c2.z = rotM.c2.z;

        V.c3.x = math.dot(-pos, rotM.c0);
        V.c3.y = math.dot(-pos, rotM.c1);
        V.c3.z = math.dot(-pos, rotM.c2);
        V.c3.w = 1.0f;

        if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            V.c2.z *= +1.0f;
        }

        return V;
        //return cam.worldToCameraMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV(Camera cam, bool correct = true)
    {
        float4x4 C = float4x4.identity;

        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float hv = cam.orthographicSize;
        //float right = hv;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!cam.orthographic)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }
            else
            {
                //s = new float3(2.0f / (right - left), 2.0f / (top - bottom), 2.0f / (far - near));
                //t = new float3(-(right + left) * 0.5f, -(top + bottom) * 0.5f, -(far + near) * 0.5f);
                //s = 2.0f / (new float3(right, top, far) - new float3(left, bottom, near));
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * (-0.5f);

                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;               //C.c0.x = (1.0f / aspect) * s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                    //s = new float3(+1.0f, -1.0f, -1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            if (!cam.orthographic)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = (far + near) / (far - near);
                C.c3.z = -(2 * far * near) / (far - near);
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;

                //C.c2.z = +far / (far - near);
                //C.c3.z = -(far * near / (far - near));
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;
            }
            else
            {
                s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                //s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            if (!cam.orthographic)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                //C.c2.z = (far + near) / (far - near);
                //C.c3.z = -(2 * far * near) / (far - near);
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }
            else
            {
                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, C);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetSfromN(Camera cam, bool correct = true)
    {
        float4x4 S = float4x4.identity;
        Rect pRect = cam.pixelRect;
        float x = pRect.x;
        float y = pRect.y;
        float w = pRect.width;
        float h = pRect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;

        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!cam.orthographic)
            {
                //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.0f);
                //s = new float3(w / 2.0f, h / 2.0f, 1.0f);
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }
            else
            {
                //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.5f);
                //s = new float3(w / 2.0f, h / 2.0f, 0.5f);
                //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
                //s = new float3(new float2(w, h) * 0.5f, 0.5f);
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            s = new float3(new float2(w, h) * 0.5f, 0.5f);
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            //s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            //s = new float3(new float2(w, h) * 0.5f, 0.5f);
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            S = math.mul(S, M);
        }

        return S;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetNfromS(Camera cam, bool correct = true)
    {
        float4x4 S = float4x4.identity;
        Rect pRect = cam.pixelRect;
        float x = pRect.x;
        float y = pRect.y;
        float w = pRect.width;
        float h = pRect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;


        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!cam.orthographic)
            {
                //s = new float3(2.0f / w, 2.0f / h, 1.0f);
                //t = new float3(-(x + w / 2.0f), -(y + h / 2.0f), 0.0f);
                s = new float3(2.0f / new float2(w, h), 1.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            }
            else
            {
                //s = new float3(2.0f / w, 2.0f / h, 2.0f);
                //t = new float3(-(x + w / 2.0f), -(y + h / 2.0f), -0.5f);
                //s = new float3(2.0f / new float2(w, h), 2.0f);
                //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);
                s = new float3(2.0f / new float2(w, h), 1.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            }

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            s = new float3(2.0f / new float2(w, h), 2.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);

            //s = new float3(2.0f / new float2(w, h), 1.0f);
            //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //s = new float3(2.0f / new float2(w, h), 2.0f);
            //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);

            s = new float3(2.0f / new float2(w, h), 1.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            S = math.mul(M, S);
        }

        return S;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetVfromC(Camera cam, bool correct = true)
    {
        float4x4 V = float4x4.identity;

        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        float hv = cam.orthographicSize;
        //float right = n;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 s = float3.zero;
        float3 t = float3.zero;

        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!cam.orthographic)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (far * near);
                V.c3.w = (far) / (far * near);
            }
            else
            {
                //t = new float3((right + left) * 0.5f, (top + bottom) * 0.5f, (far + near) * 0.5f);
                //s = new float3((right - left) / 2.0f, (top - bottom) / 2.0f, (far - near) / 2.0f);
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * 0.5f;
                //s = (new float3(right, top, far) - new float3(left, bottom, near)) * 0.5f;

                //t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                t = new float3(0.0f, 0.0f, near);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;           //V.c0.x = aspect * s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            if (!cam.orthographic)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (2.0f * far * near);
                V.c3.w = (far + near) / (2.0f * far * near);

                //V.c2.z = 0.0f;
                //V.c3.z = 1.0f;
                //V.c2.w = -(far - near) / (far * near);
                //V.c3.w = (far) / (far * near);
            }
            else
            {
                t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                //t = new float3(0.0f, 0.0f, near);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            if (!cam.orthographic)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                //V.c2.z = 0.0f;
                //V.c3.z = 1.0f;
                //V.c2.w = -(far - near) / (2.0f * far * near);
                //V.c3.w = (far + near) / (2.0f * far * near);

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (far * near);
                V.c3.w = (far) / (far * near);
            }
            else
            {
                //t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                t = new float3(0.0f, 0.0f, near);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (cam.cameraType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            V = math.mul(V, M);
        }

        return V;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromV(Camera cam)
    {
        float4x4 W = float4x4.identity;
        Transform tr = cam.transform;
        float3 t = tr.position;
        float3x3 R = new float3x3(tr.rotation);

        W.c0 = new float4(R.c0, 0.0f);
        W.c1 = new float4(R.c1, 0.0f);
        W.c2 = new float4(R.c2, 0.0f);
        W.c3 = new float4(t, 1.0f);


        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetLfromW(Transform localTr)
    {
        float3 pos = localTr.position;
        quaternion rot = localTr.rotation;
        float3 sca = localTr.localScale;

        float4x4 L = float4x4.identity;
        float3x3 R = new float3x3(rot);
        float3 rs = 1.0f / sca;

        R.c0 = rs.x * R.c0;
        R.c1 = rs.y * R.c1;
        R.c2 = rs.z * R.c2;

        float3 t = -new float3(math.dot(R.c0, pos), math.dot(R.c1, pos), math.dot(R.c2, pos));
        R = math.transpose(R);
        L.c0 = new float4(R.c0, t.x);
        L.c1 = new float4(R.c1, t.y);
        L.c2 = new float4(R.c2, t.z);
        L.c3 = new float4(0.0f, 0.0f, 0.0f, 1.0f);

        return L;
    }


    //UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromW_UI(Camera cam, bool correct = true)
    {
        float4x4 C = float4x4.identity;
        float4x4 S = float4x4.identity;
        Rect pRect = cam.pixelRect;
        float x = pRect.x;
        float y = pRect.y;
        float w = pRect.width;
        float h = pRect.height;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            s = new float3(2.0f / new float2(w, h), 1.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                    //s = new float3(+1.0f, -1.0f, -1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            s = new float3(2.0f / new float2(w, h), 2.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            s = new float3(2.0f / new float2(w, h), 1.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);

            if (cam.targetTexture == null)
            {
                if (cam.cameraType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                }
                else if (cam.cameraType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, S);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromL_UI(RectTransform tr)
    {
        float4x4 W = float4x4.identity;

        Rect rt = tr.rect;
        float w = rt.width;
        float h = rt.height;
        float xp = tr.pivot.x;
        float yp = tr.pivot.y;

        W.c0 = new float4(0.5f * w, 0.0f, 0.0f, 0.0f);
        W.c1 = new float4(0.0f, 0.5f * h, 0.0f, 0.0f);
        W.c3 = new float4(-(xp - 0.5f) * w, -(yp - 0.5f) * h, 0.0f, 1.0f);
        W = math.mul(tr.localToWorldMatrix, W);
        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromL_UI(Camera cam, RectTransform tr, bool correct = true)
    {
        float4x4 W = GetWfromL_UI(tr);
        float4x4 C = GetCfromW_UI(cam);

        C = math.mul(C, W);

        return C;
    }

    public static Mesh CreateRectMesh_UI()
    {
        Mesh mesh = new Mesh();

        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(-1.0f, -1.0f, 0.0f));
        vertices.Add(new Vector3(-1.0f, +1.0f, 0.0f));
        vertices.Add(new Vector3(+1.0f, +1.0f, 0.0f));
        vertices.Add(new Vector3(+1.0f, -1.0f, 0.0f));
        mesh.SetVertices(vertices);

        var indices = new List<int>();
        indices.Add(0);
        indices.Add(2);
        indices.Add(3);
        indices.Add(2);
        indices.Add(0);
        indices.Add(1);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        return mesh;
    }


    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetPosition_SfromW(float3 posW, Camera cam, bool zNormal = true)
    {
        float3 posS = float3.zero;

        float4x4 M = math.mul(RenderUtil.GetCfromV(cam), RenderUtil.GetVfromW(cam));
        float4 vec = math.mul(M, new float4(posW, 1.0f));
        vec = (1.0f / vec.w) * vec;
        posS = math.mul(RenderUtil.GetSfromN(cam), vec).xyz;

        if (!zNormal)
        {
            quaternion r = cam.transform.rotation;
            float3 t = cam.transform.position;
            float3 n = math.mul(math.mul(r, new quaternion(0.0f, 0.0f, 1.0f, 0.0f)), math.conjugate(r)).value.xyz;
            float d = math.dot((posW - t), n);
            posS.z = d;
        }

        return posS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ray GetRay_WfromS(float3 posS, Camera cam)
    {
        Ray ray = new Ray();

        float4 posV = math.mul(
                   math.mul(RenderUtil.GetVfromC(cam), RenderUtil.GetNfromS(cam)), new float4(posS.xy, 0.0f, 1.0f));
        posV = (1.0f / posV.w) * posV;
        float4x4 W = RenderUtil.GetWfromV(cam);
        float3 pos1 = math.mul(W, posV).xyz;
        float3 pos0 = math.mul(W, new float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

        ray.origin = pos1;
        if (!cam.orthographic)
        {
            ray.direction = math.normalize(pos1 - pos0);
        }
        else
        {
            quaternion r = cam.transform.rotation;
            ray.direction = math.mul(math.mul(r, new quaternion(0.0f, 0.0f, 1.0f, 0.0f)), math.conjugate(r)).value.xyz;
        }

        return ray;
    }



    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromL(float3 pos, quaternion rot, float3 sca)
    {
        float4x4 W = float4x4.identity;
        float3x3 R = new float3x3(rot);

        W.c0 = new float4(sca.x * R.c0, 0.0f);
        W.c1 = new float4(sca.y * R.c1, 0.0f);
        W.c2 = new float4(sca.z * R.c2, 0.0f);
        W.c3 = new float4(pos, 1.0f);

        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetVfromW(float3 pos, quaternion rot)
    {
        float4x4 V = float4x4.identity;

        float3x3 rotM = new float3x3(rot);

        V.c0.x = rotM.c0.x; V.c1.x = rotM.c0.y; V.c2.x = rotM.c0.z;
        V.c0.y = rotM.c1.x; V.c1.y = rotM.c1.y; V.c2.y = rotM.c1.z;
        V.c0.z = rotM.c2.x; V.c1.z = rotM.c2.y; V.c2.z = rotM.c2.z;

        V.c3.x = math.dot(-pos, rotM.c0);
        V.c3.y = math.dot(-pos, rotM.c1);
        V.c3.z = math.dot(-pos, rotM.c2);
        V.c3.w = 1.0f;

        return V;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV(
       float fov, float aspect, float near, float far, float hv,
       bool isOrtho = false, CameraType camType = CameraType.Game, bool noTargetTex = true, bool correct = true)
    {
        float4x4 C = float4x4.identity;

        //float fov = cam.fieldOfView;
        //float aspect = cam.aspect;
        //float near = cam.nearClipPlane;
        //float far = cam.farClipPlane;
        //
        //float hv = cam.orthographicSize;
        //float right = hv;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!isOrtho)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }
            else
            {
                //s = new float3(2.0f / (right - left), 2.0f / (top - bottom), 2.0f / (far - near));
                //t = new float3(-(right + left) * 0.5f, -(top + bottom) * 0.5f, -(far + near) * 0.5f);
                //s = 2.0f / (new float3(right, top, far) - new float3(left, bottom, near));
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * (-0.5f);

                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;               //C.c0.x = (1.0f / aspect) * s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (noTargetTex)
            {
                if (camType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                    //s = new float3(+1.0f, -1.0f, -1.0f);
                }
                else if (camType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            if (!isOrtho)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = (far + near) / (far - near);
                C.c3.z = -(2 * far * near) / (far - near);
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;

                //C.c2.z = +far / (far - near);
                //C.c3.z = -(far * near / (far - near));
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;
            }
            else
            {
                s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                //s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (noTargetTex)
            {
                if (camType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
                else if (camType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +0.0f);
                    s = new float3(+1.0f, +1.0f, +1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            if (!isOrtho)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                //C.c2.z = (far + near) / (far - near);
                //C.c3.z = -(2 * far * near) / (far - near);
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }
            else
            {
                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (noTargetTex)
            {
                if (camType == CameraType.Game)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, +1.0f, -1.0f);
                }
                else if (camType == CameraType.SceneView)
                {
                    t = new float3(+0.0f, +0.0f, +1.0f);
                    s = new float3(+1.0f, -1.0f, -1.0f);
                }
            }
            else
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, C);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetSfromN(
        float x, float y, float w, float h,
        CameraType camType = CameraType.Game, bool correct = true)
    {
        float4x4 S = float4x4.identity;
        //Rect pRect = cam.pixelRect;
        //float x = pRect.x;
        //float y = pRect.y;
        //float w = pRect.width;
        //float h = pRect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;

        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            //if (!isOrtho)
            //{
            //    //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.0f);
            //    //s = new float3(w / 2.0f, h / 2.0f, 1.0f);
            //    t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            //    s = new float3(new float2(w, h) * 0.5f, 1.0f);
            //}
            //else
            {
                //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.5f);
                //s = new float3(w / 2.0f, h / 2.0f, 0.5f);
                //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
                //s = new float3(new float2(w, h) * 0.5f, 0.5f);
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            s = new float3(new float2(w, h) * 0.5f, 0.5f);
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            //s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            //s = new float3(new float2(w, h) * 0.5f, 0.5f);
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            S = math.mul(S, M);
        }

        return S;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetNfromS(
        float x, float y, float w, float h,
        CameraType camType = CameraType.Game, bool correct = true)
    {
        float4x4 S = float4x4.identity;
        //Rect pRect = cam.pixelRect;
        //float x = pRect.x;
        //float y = pRect.y;
        //float w = pRect.width;
        //float h = pRect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;


        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            //if (!cam.orthographic)
            //{
            //    //s = new float3(2.0f / w, 2.0f / h, 1.0f);
            //    //t = new float3(-(x + w / 2.0f), -(y + h / 2.0f), 0.0f);
            //    s = new float3(2.0f / new float2(w, h), 1.0f);
            //    t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            //}
            //else
            {
                //s = new float3(2.0f / w, 2.0f / h, 2.0f);
                //t = new float3(-(x + w / 2.0f), -(y + h / 2.0f), -0.5f);
                //s = new float3(2.0f / new float2(w, h), 2.0f);
                //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);
                s = new float3(2.0f / new float2(w, h), 1.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            }

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (camType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            s = new float3(2.0f / new float2(w, h), 2.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);

            //s = new float3(2.0f / new float2(w, h), 1.0f);
            //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (camType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //s = new float3(2.0f / new float2(w, h), 2.0f);
            //t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);

            s = new float3(2.0f / new float2(w, h), 1.0f);
            t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);

            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
            S.c3 = new float4(s * t, 1.0f);
            if (camType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            S = math.mul(M, S);
        }

        return S;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetVfromC(
       float fov, float aspect, float near, float far, float hv,
       bool isOrtho = false, CameraType camType = CameraType.Game, bool correct = true)
    {
        float4x4 V = float4x4.identity;

        //float fov = cam.fieldOfView;
        //float aspect = cam.aspect;
        //float near = cam.nearClipPlane;
        //float far = cam.farClipPlane;
        //
        //float hv = cam.orthographicSize;
        //float right = n;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 s = float3.zero;
        float3 t = float3.zero;

        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!isOrtho)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (far * near);
                V.c3.w = (far) / (far * near);
            }
            else
            {
                //t = new float3((right + left) * 0.5f, (top + bottom) * 0.5f, (far + near) * 0.5f);
                //s = new float3((right - left) / 2.0f, (top - bottom) / 2.0f, (far - near) / 2.0f);
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * 0.5f;
                //s = (new float3(right, top, far) - new float3(left, bottom, near)) * 0.5f;

                //t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                t = new float3(0.0f, 0.0f, near);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;           //V.c0.x = aspect * s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            if (!isOrtho)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (2.0f * far * near);
                V.c3.w = (far + near) / (2.0f * far * near);

                //V.c2.z = 0.0f;
                //V.c3.z = 1.0f;
                //V.c2.w = -(far - near) / (far * near);
                //V.c3.w = (far) / (far * near);
            }
            else
            {
                t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                //t = new float3(0.0f, 0.0f, near);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, +1.0f, +1.0f);
                t = new float3(+0.0f, +0.0f, +0.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            if (!isOrtho)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                //V.c2.z = 0.0f;
                //V.c3.z = 1.0f;
                //V.c2.w = -(far - near) / (2.0f * far * near);
                //V.c3.w = (far + near) / (2.0f * far * near);

                V.c2.z = 0.0f;
                V.c3.z = 1.0f;
                V.c2.w = -(far - near) / (far * near);
                V.c3.w = (far) / (far * near);
            }
            else
            {
                //t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                //s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);

                t = new float3(0.0f, 0.0f, near);
                s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }

            if (camType == CameraType.Game)
            {
                s = new float3(+1.0f, +1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
            else if (camType == CameraType.SceneView)
            {
                s = new float3(+1.0f, -1.0f, -1.0f);
                t = new float3(+0.0f, +0.0f, -1.0f);
            }
        }

        if (correct)
        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            V = math.mul(V, M);
        }

        return V;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetWfromV(float3 pos, quaternion rot)
    {
        float4x4 W = float4x4.identity;
        float3 t = pos;
        float3x3 R = new float3x3(rot);

        W.c0 = new float4(R.c0, 0.0f);
        W.c1 = new float4(R.c1, 0.0f);
        W.c2 = new float4(R.c2, 0.0f);
        W.c3 = new float4(t, 1.0f);

        return W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetLfromW(float3 pos, quaternion rot, float3 sca)
    {
        float4x4 L = float4x4.identity;
        float3x3 R = new float3x3(rot);
        float3 rs = 1.0f / sca;

        R.c0 = rs.x * R.c0;
        R.c1 = rs.y * R.c1;
        R.c2 = rs.z * R.c2;

        float3 t = -new float3(math.dot(R.c0, pos), math.dot(R.c1, pos), math.dot(R.c2, pos));
        R = math.transpose(R);
        L.c0 = new float4(R.c0, 0.0f);
        L.c1 = new float4(R.c1, 0.0f);
        L.c2 = new float4(R.c2, 0.0f);
        L.c3 = new float4(t, 1.0f);

        return L;
    }



    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetPosition_SfromW(
        float3 posW,
        float3 pos, quaternion rot,
        float fov, float aspect, float near, float far, float hv,
        float x, float y, float w, float h,
        bool isOrtho = false, bool
        zNormal = true,
        CameraType camType = CameraType.Game, bool noTargetTex = true, bool correct = true)
    {
        float3 posS = float3.zero;

        float4x4 V = RenderUtil.GetVfromW(pos, rot);
        float4x4 C = RenderUtil.GetCfromV(fov, aspect, near, far, hv, isOrtho, camType, noTargetTex, correct);
        float4x4 S = RenderUtil.GetSfromN(x, y, w, h);

        float4x4 M = math.mul(C, V);
        float4 vec = math.mul(M, new float4(posW, 1.0f));
        vec = (1.0f / vec.w) * vec;
        posS = math.mul(S, vec).xyz;

        if (!zNormal)
        {
            quaternion r = rot;
            float3 t = pos;
            float3 n = math.mul(math.mul(r, new quaternion(0.0f, 0.0f, 1.0f, 0.0f)), math.conjugate(r)).value.xyz;
            float d = math.dot((posW - t), n);
            posS.z = d;
        }

        return posS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ray GetRay_WfromS(
        float3 posS,
        float x, float y, float w, float h,
        float fov, float aspect, float near, float far, float hv,
        float3 pos, quaternion rot,
        bool isOrtho = false, CameraType camType = CameraType.Game, bool correct = true)
    {
        Ray ray = new Ray();

        float4x4 N = RenderUtil.GetNfromS(x, y, w, h, camType, correct);
        float4x4 V = RenderUtil.GetVfromC(fov, aspect, near, far, hv, isOrtho, camType, correct);
        float4x4 W = RenderUtil.GetWfromV(pos, rot);

        float4 posV = math.mul(
                   math.mul(V, N), new float4(posS.xy, 0.0f, 1.0f));
        posV = (1.0f / posV.w) * posV;
        float3 pos1 = math.mul(W, posV).xyz;
        float3 pos0 = math.mul(W, new float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;

        ray.origin = pos1;
        if (!isOrtho)
        {
            ray.direction = math.normalize(pos1 - pos0);
        }
        else
        {
            ray.direction = math.rotate(rot, new float3(0.0f, 0.0f, 1.0f));
        }

        return ray;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetPosition_SfromW(
        float3 posW,
        float3 pos, quaternion rot,
        float fov, float aspect, float near, float far, float hv,
        float x, float y, float w, float h,
        bool isOrtho = false,
        bool zNormal = true)
    {
        float3 posS = float3.zero;

        float4x4 V = float4x4.identity;
        {
            float3x3 R = new float3x3(rot);

            float3 t = -new float3(math.dot(R.c0, pos), math.dot(R.c1, pos), math.dot(R.c2, pos));
            R = math.transpose(R);
            V.c0 = new float4(R.c0, 0.0f);
            V.c1 = new float4(R.c1, 0.0f);
            V.c2 = new float4(R.c2, 0.0f);
            V.c3 = new float4(t, 1.0f);
        }

        float4x4 C = float4x4.identity;
        {
            if (!isOrtho)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    C.c2.z = +far / (far - near);
                    C.c3.z = -(far * near / (far - near));
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    C.c2.z = (far + near) / (far - near);
                    C.c3.z = -(2 * far * near) / (far - near);
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    C.c2.z = +far / (far - near);
                    C.c3.z = -(far * near / (far - near));
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
            }
            else
            {
                float3 s = float3.zero;
                float3 t = float3.zero;

                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, -near);
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, -near);
                }

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }
        }

        float4x4 S = float4x4.identity;
        {
            float3 s = float3.zero;
            float3 t = float3.zero;
            if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }
            else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
                s = new float3(new float2(w, h) * 0.5f, 0.5f);
            }
            else if (gdt == GraphicsDeviceType.Vulkan)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
        }

        float4x4 M = math.mul(C, V);
        float4 vec = math.mul(M, new float4(posW, 1.0f));
        vec = (1.0f / vec.w) * vec;
        posS = math.mul(S, vec).xyz;

        if (!zNormal)
        {
            quaternion r = rot;
            float3 t = pos;
            float3 n = math.rotate(r, new float3(0.0f, 0.0f, 1.0f));
            float d = math.dot((posW - t), n);
            posS.z = d;
        }

        return posS;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Ray GetRay_WfromS(
        float3 posS,
        float x, float y, float w, float h,
        float fov, float aspect, float near, float far, float hv,
        float3 pos, quaternion rot,
        bool isOrtho = false)
    {
        Ray ray = new Ray();

        float4x4 N = float4x4.identity;
        {
            float3 t = float3.zero;
            float3 s = float3.zero;

            if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
            {
                s = new float3(2.0f / new float2(w, h), 1.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            }
            else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
            {
                s = new float3(2.0f / new float2(w, h), 2.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), -0.5f);
            }
            else if (gdt == GraphicsDeviceType.Vulkan)
            {
                s = new float3(2.0f / new float2(w, h), 1.0f);
                t = new float3(-(new float2(x, y) + new float2(w, h) * 0.5f), 0.0f);
            }

            N.c0.x = s.x;
            N.c1.y = s.y;
            N.c2.z = s.z;
            N.c3 = new float4(s * t, 1.0f);
        }

        float4x4 V = float4x4.identity;
        {
            float3 s = float3.zero;
            float3 t = float3.zero;

            if (!isOrtho)
            {
                float tanFov = math.tan(math.radians(fov / 2.0f));
                V.c0.x = aspect * tanFov;
                V.c1.y = tanFov;

                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    V.c2.z = 0.0f;
                    V.c3.z = 1.0f;
                    V.c2.w = -(far - near) / (far * near);
                    V.c3.w = (far) / (far * near);
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    V.c2.z = 0.0f;
                    V.c3.z = 1.0f;
                    V.c2.w = -(far - near) / (2.0f * far * near);
                    V.c3.w = (far + near) / (2.0f * far * near);
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    V.c2.z = 0.0f;
                    V.c3.z = 1.0f;
                    V.c2.w = -(far - near) / (far * near);
                    V.c3.w = (far) / (far * near);
                }
            }
            else
            {
                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    t = new float3(0.0f, 0.0f, near);
                    s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    t = new float3(0.0f, 0.0f, (far + near) * 0.5f);
                    s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 0.5f);
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    t = new float3(0.0f, 0.0f, near);
                    s = new float3(hv, hv, (far - near)) * new float3(aspect, 1.0f, 1.0f);
                }

                V.c3 = new float4(t, 1.0f);
                V.c0.x = s.x;
                V.c1.y = s.y;
                V.c2.z = s.z;
            }



            if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
            {
                if (!isOrtho)
                {

                }
                else
                {

                }

            }
            else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
            {
                if (!isOrtho)
                {

                }
                else
                {

                }

            }
            else if (gdt == GraphicsDeviceType.Vulkan)
            {
                if (!isOrtho)
                {

                }
                else
                {

                }
            }
        }

        float4x4 W = float4x4.identity;
        {
            float3 t = pos;
            float3x3 R = new float3x3(rot);

            W.c0 = new float4(R.c0, 0.0f);
            W.c1 = new float4(R.c1, 0.0f);
            W.c2 = new float4(R.c2, 0.0f);
            W.c3 = new float4(t, 1.0f);
        }

        float4 posV = math.mul(
                   math.mul(V, N), new float4(posS.xy, 0.0f, 1.0f));
        posV = (1.0f / posV.w) * posV;
        float3 pos1 = math.mul(W, posV).xyz;
        float3 pos0 = W.c3.xyz;

        ray.origin = pos1;
        if (!isOrtho)
        {
            ray.direction = math.normalize(pos1 - pos0);
        }
        else
        {
            ray.direction = math.rotate(rot, new float3(0.0f, 0.0f, 1.0f));
        }

        return ray;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetMat_SfromW(
        Transform trCam,
        Camera cam,
        out float4x4 S, out float4x4 CV)
    {
        float3 posS = float3.zero;

        float3 pos = trCam.position;
        quaternion rot = trCam.rotation;
        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float hv = cam.orthographicSize;

        Rect pRect = cam.pixelRect;
        float x = pRect.x;
        float y = pRect.y;
        float w = pRect.width;
        float h = pRect.height;
        bool isOrtho = cam.orthographic;

        float4x4 V = float4x4.identity;
        {
            float3x3 R = new float3x3(rot);

            float3 t = -new float3(math.dot(R.c0, pos), math.dot(R.c1, pos), math.dot(R.c2, pos));
            R = math.transpose(R);
            V.c0 = new float4(R.c0, 0.0f);
            V.c1 = new float4(R.c1, 0.0f);
            V.c2 = new float4(R.c2, 0.0f);
            V.c3 = new float4(t, 1.0f);
        }

        float4x4 C = float4x4.identity;
        {
            if (!isOrtho)
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    C.c2.z = +far / (far - near);
                    C.c3.z = -(far * near / (far - near));
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    C.c2.z = (far + near) / (far - near);
                    C.c3.z = -(2 * far * near) / (far - near);
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    C.c2.z = +far / (far - near);
                    C.c3.z = -(far * near / (far - near));
                    C.c2.w = 1.0f;
                    C.c3.w = 0.0f;
                }
            }
            else
            {
                float3 s = float3.zero;
                float3 t = float3.zero;

                if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
                {
                    s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, -near);
                }
                else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
                {
                    s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));
                }
                else if (gdt == GraphicsDeviceType.Vulkan)
                {
                    s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                    t = new float3(0.0f, 0.0f, -near);
                }

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }
        }

        S = float4x4.identity;
        {
            float3 s = float3.zero;
            float3 t = float3.zero;
            if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }
            else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
                s = new float3(new float2(w, h) * 0.5f, 0.5f);
            }
            else if (gdt == GraphicsDeviceType.Vulkan)
            {
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;
        }

        CV = math.mul(C, V);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 GetPosition_SfromW_Mat(
        float3 posW,
        float4x4 S, float4x4 CV)
    {
        float3 posS;

        float4 vec = math.mul(CV, new float4(posW, 1.0f));
        vec = (1.0f / vec.w) * vec;
        posS = math.mul(S, vec).xyz;

        return posS;
    }


    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetUNfromSN(float3 ud, float3x2 sd)
    {
        float4x4 UN = float4x4.identity;
        //float3 u = sd.c0 + sd.c1;
        float3 t = ud * (0.5f * sd.c0);
        float3 s = ud * (0.5f * sd.c0 + sd.c1);

        UN.c3 = new float4(t, 1.0f);
        UN.c0.x = s.x;
        UN.c1.y = s.y;
        UN.c2.z = s.z;

        return UN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetSNfromUN(float3x2 sd, float3 ud)
    {
        float4x4 SN = float4x4.identity;
        //float3 u = sd.c0 + sd.c1;
        float3 s = ud * (2.0f * sd.c0 + sd.c1);
        float3 t = ud * (-0.5f) * sd.c0;


        SN.c0.x = s.x;
        SN.c1.y = s.y;
        SN.c2.z = s.z;
        SN.c3 = new float4(s * t, 1.0f);

        return SN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetTfromN()
    {
        float4x4 T = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
            T.c1 = new float4(+0.0f, +0.5f, +0.0f, +0.0f);
            T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
            T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
            T.c1 = new float4(+0.0f, +0.5f, +0.0f, +0.0f);
            T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
            T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
            T.c1 = new float4(+0.0f, +0.5f, +0.0f, +0.0f);
            T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
            T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        }

        //if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        //{
        //    T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
        //    T.c1 = new float4(+0.0f, -0.5f, +0.0f, +0.0f);
        //    T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
        //    T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        //}
        //else if(gdt == GraphicsDeviceType.OpenGLCore)
        //{
        //    T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
        //    T.c1 = new float4(+0.0f, +0.5f, +0.0f, +0.0f);
        //    T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
        //    T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        //}
        //else if(gdt == GraphicsDeviceType.Vulkan)
        //{
        //    T.c0 = new float4(+0.5f, +0.0f, +0.0f, +0.0f);
        //    T.c1 = new float4(+0.0f, -0.5f, +0.0f, +0.0f);
        //    T.c2 = new float4(+0.0f, +0.0f, +1.0f, +0.0f);
        //    T.c3 = new float4(+0.5f, +0.5f, +0.0f, +1.0f);
        //}

        return T;
    }

    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetM_Correct(Camera cam)
    {
        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            if (cam.cameraType == CameraType.Game)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, +1.0f, -1.0f);
            }
            else if (cam.cameraType == CameraType.SceneView)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        M.c3 = new float4(t, 1.0f);
        M.c0.x = s.x;
        M.c1.y = s.y;
        M.c2.z = s.z;

        return M;

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetM_ToTex()
    {
        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }

            //{
            //    t = new float3(+0.0f, +0.0f, +1.0f);
            //    s = new float3(+1.0f, +1.0f, -1.0f);
            //}

            //{
            //    t = new float3(+0.0f, +0.0f, +0.0f);
            //    s = new float3(+1.0f, -1.0f, +1.0f);
            //}

            //{
            //    t = new float3(+0.0f, +0.0f, -1.0f);
            //    s = new float3(+1.0f, -1.0f, +1.0f);
            //}

        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        M.c3 = new float4(t, 1.0f);
        M.c0.x = s.x;
        M.c1.y = s.y;
        M.c2.z = s.z;

        return M;

    }

    //
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetVfromW_Cube(Camera cam, Matrix4x4[] V_cube)
    {
        float3 pos = cam.transform.position;
        float4x4 pX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(90.0f)));
        float4x4 nX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(270.0f)));
        float4x4 pY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(-90.0f)));
        float4x4 nY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(+90.0f)));
        float4x4 pZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(0.0f)));
        float4x4 nZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(180.0f)));

        V_cube[0] = pX;
        V_cube[1] = nX;
        V_cube[2] = pY;
        V_cube[3] = nY;
        V_cube[4] = pZ;
        V_cube[5] = nZ;

        //V_cube[0] = nX;
        //V_cube[1] = pX;
        //V_cube[2] = nY;
        //V_cube[3] = pY;
        //V_cube[4] = nZ;
        //V_cube[5] = pZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetVfromW_Cube(float3 pos, Matrix4x4[] V_cube)
    {
        float4x4 pX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(90.0f)));
        float4x4 nX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(270.0f)));
        float4x4 pY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(-90.0f)));
        float4x4 nY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(+90.0f)));
        float4x4 pZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(0.0f)));
        float4x4 nZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(180.0f)));

        V_cube[0] = pX;
        V_cube[1] = nX;
        V_cube[2] = pY;
        V_cube[3] = nY;
        V_cube[4] = pZ;
        V_cube[5] = nZ;

        //V_cube[0] = nX;
        //V_cube[1] = pX;
        //V_cube[2] = nY;
        //V_cube[3] = pY;
        //V_cube[4] = nZ;
        //V_cube[5] = pZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void GetVfromW_Cube(float3 pos, Matrix4x4* V_cube)
    {
        float4x4 pX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(90.0f)));
        float4x4 nX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(270.0f)));
        float4x4 pY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(-90.0f)));
        float4x4 nY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(+90.0f)));
        float4x4 pZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(0.0f)));
        float4x4 nZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(180.0f)));

        V_cube[0] = pX;
        V_cube[1] = nX;
        V_cube[2] = pY;
        V_cube[3] = nY;
        V_cube[4] = pZ;
        V_cube[5] = nZ;

        //V_cube[0] = nX;
        //V_cube[1] = pX;
        //V_cube[2] = nY;
        //V_cube[3] = pY;
        //V_cube[4] = nZ;
        //V_cube[5] = pZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4[] GetVfromW_Cube(Camera cam)
    {
        float4x4[] V_cube = new float4x4[6];
        float3 pos = cam.transform.position;
        float4x4 pX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(90.0f)));
        float4x4 nX = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(270.0f)));
        float4x4 pY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(-90.0f)));
        float4x4 nY = GetVfromW(pos, quaternion.AxisAngle(new float3(+1.0f, +0.0f, +0.0f), math.radians(+90.0f)));
        float4x4 pZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(0.0f)));
        float4x4 nZ = GetVfromW(pos, quaternion.AxisAngle(new float3(+0.0f, +1.0f, +0.0f), math.radians(180.0f)));

        V_cube[0] = pX;
        V_cube[1] = nX;
        V_cube[2] = pY;
        V_cube[3] = nY;
        V_cube[4] = pZ;
        V_cube[5] = nZ;

        return V_cube;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV_Ortho0(
        float size, float aspect, float near, float far,
        bool toTex = true
        )
    {
        float4x4 C = float4x4.identity;

        float hv = size;
        //float right = hv;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            {
                //s = new float3(2.0f / (right - left), 2.0f / (top - bottom), 2.0f / (far - near));
                //t = new float3(-(right + left) * 0.5f, -(top + bottom) * 0.5f, -(far + near) * 0.5f);
                //s = 2.0f / (new float3(right, top, far) - new float3(left, bottom, near));
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * (-0.5f);

                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;               //C.c0.x = (1.0f / aspect) * s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            {
                s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                //s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            {
                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (toTex)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, C);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV_Ortho(
        float size, float aspect, float near, float far,
        bool toTex = true, int igdt = 0
        )
    {
        float4x4 C = float4x4.identity;

        float hv = size;
        //float right = hv;
        //float left = -right;
        //float top = right;
        //float bottom = -top;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (igdt == 0)
        {
            {
                //s = new float3(2.0f / (right - left), 2.0f / (top - bottom), 2.0f / (far - near));
                //t = new float3(-(right + left) * 0.5f, -(top + bottom) * 0.5f, -(far + near) * 0.5f);
                //s = 2.0f / (new float3(right, top, far) - new float3(left, bottom, near));
                //t = (new float3(right, top, far) + new float3(left, bottom, near)) * (-0.5f);

                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;               //C.c0.x = (1.0f / aspect) * s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);

                //t = new float3(+0.0f, +0.0f, +1.0f);
                //s = new float3(+1.0f, -1.0f, -1.0f);

                //t = new float3(+0.0f, +0.0f, +0.0f);
                //s = new float3(+1.0f, -1.0f, +1.0f);
            }
        }
        else if (igdt == 1)
        {
            {
                s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                //s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (igdt == 2)
        {
            {
                //s = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                //t = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                s = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t = new float3(0.0f, 0.0f, -near);

                C.c0.x = s.x;
                C.c1.y = s.y;
                C.c2.z = s.z;
                C.c3 = new float4(s * t, 1.0f);
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (toTex)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, C);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetCfromV_Ortho_Optimized(
        float size, float aspect, float near, float far,
        out float4x4 C_toTex, out float4x4 C_depth, int igdt = 0
        )
    {
        C_toTex = float4x4.identity;
        C_depth = float4x4.identity;

        float hv = size;

        float4x4 M = float4x4.identity;
        float3 t0 = float3.zero;
        float3 s0 = float3.zero;
        float3 t1 = float3.zero;
        float3 s1 = float3.zero;
        if (igdt == 0)
        {
            {
                s0 = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t0 = new float3(0.0f, 0.0f, -near);
            }

            {
                t1 = new float3(+0.0f, +0.0f, +1.0f);
                s1 = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (igdt == 1)
        {
            {
                s0 = new float3(1.0f, 1.0f, 2.0f) / new float3(aspect * hv, hv, far - near);
                t0 = new float3(0.0f, 0.0f, (far + near) * (-0.5f));

                //s0 = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                //t0 = new float3(0.0f, 0.0f, -near);

            }

            {
                t1 = new float3(+0.0f, +0.0f, +0.0f);
                s1 = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (igdt == 2)
        {
            {
                s0 = new float3(1.0f, 1.0f, 1.0f) / new float3(aspect * hv, hv, far - near);
                t0 = new float3(0.0f, 0.0f, -near);
            }

            {
                t1 = new float3(+0.0f, +0.0f, +1.0f);
                s1 = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        {
            C_depth.c0.x = s0.x;               //C.c0.x = (1.0f / aspect) * s.x;
            C_depth.c1.y = s0.y;
            C_depth.c2.z = s0.z;
            C_depth.c3 = new float4(s0 * t0, 1.0f);

            M.c3 = new float4(t1, 1.0f);
            M.c0.x = s1.x;
            M.c1.y = s1.y;
            M.c2.z = s1.z;

            C_toTex = math.mul(M, C_depth);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV_Persp(
        float fov, float aspect, float near, float far, bool toTex = true)
    {
        float4x4 C = float4x4.identity;

        float4x4 M = float4x4.identity;
        float3 t = float3.zero;
        float3 s = float3.zero;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                C.c2.z = (far + near) / (far - near);
                C.c3.z = -(2 * far * near) / (far - near);
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;

                //C.c2.z = +far / (far - near);
                //C.c3.z = -(far * near / (far - near));
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +0.0f);
                s = new float3(+1.0f, +1.0f, +1.0f);
            }

        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            {
                float cotFov = 1.0f / math.tan(math.radians(fov / 2.0f));
                C.c0.x = (1.0f / aspect) * cotFov;
                C.c1.y = cotFov;

                //C.c2.z = (far + near) / (far - near);
                //C.c3.z = -(2 * far * near) / (far - near);
                //C.c2.w = 1.0f;
                //C.c3.w = 0.0f;

                C.c2.z = +far / (far - near);
                C.c3.z = -(far * near / (far - near));
                C.c2.w = 1.0f;
                C.c3.w = 0.0f;
            }

            if (toTex)
            {
                t = new float3(+0.0f, +0.0f, +1.0f);
                s = new float3(+1.0f, -1.0f, -1.0f);
            }
        }

        if (toTex)
        {
            M.c3 = new float4(t, 1.0f);
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;

            C = math.mul(M, C);
        }

        return C;
        //return cam.projectionMatrix;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetCfromV_toTex(Camera cam)
    {
        float4x4 C = float4x4.identity;

        if (!cam.orthographic)
        {
            C = RenderUtil.GetCfromV_Persp(cam.fieldOfView, cam.aspect, cam.nearClipPlane, cam.farClipPlane, true);
        }
        else
        {
            C = RenderUtil.GetCfromV_Ortho0(cam.orthographicSize, cam.aspect, cam.nearClipPlane, cam.farClipPlane, true);
        }

        return C;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetSfromN_toTex(Camera cam)
    {
        float4x4 S = float4x4.identity;
        Rect pRect = cam.pixelRect;
        float x = pRect.x;
        float y = pRect.y;
        float w = pRect.width;
        float h = pRect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;

        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            if (!cam.orthographic)
            {
                //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.0f);
                //s = new float3(w / 2.0f, h / 2.0f, 1.0f);
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }
            else
            {
                //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.5f);
                //s = new float3(w / 2.0f, h / 2.0f, 0.5f);
                //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
                //s = new float3(new float2(w, h) * 0.5f, 0.5f);
                t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
                s = new float3(new float2(w, h) * 0.5f, 1.0f);
            }

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, -1.0f, -1.0f);
            t = new float3(+0.0f, +0.0f, -1.0f);
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore)
        {
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            s = new float3(new float2(w, h) * 0.5f, 0.5f);
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            //s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, +1.0f, +1.0f);
            t = new float3(+0.0f, +0.0f, +0.0f);
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            //s = new float3(new float2(w, h) * 0.5f, 0.5f);
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, -1.0f, -1.0f);
            t = new float3(+0.0f, +0.0f, -1.0f);
        }

        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            S = math.mul(S, M);
        }

        return S;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4x4 GetSfromN_toTex(Rect rect)
    {
        float4x4 S = float4x4.identity;
        //Rect pRect = cam.pixelRect;
        float x = rect.x;
        float y = rect.y;
        float w = rect.width;
        float h = rect.height;

        float3 t = float3.zero;
        float3 s = float3.zero;

        float4x4 M = float4x4.identity;
        if (gdt == GraphicsDeviceType.Direct3D11 || gdt == GraphicsDeviceType.Direct3D12)
        {
            //t = new float3(x + w / 2.0f, y + h / 2.0f, 0.5f);
            //s = new float3(w / 2.0f, h / 2.0f, 0.5f);
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            //s = new float3(new float2(w, h) * 0.5f, 0.5f);
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            s = new float3(new float2(w, h) * 0.5f, 1.0f);


            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, -1.0f, -1.0f);
            t = new float3(+0.0f, +0.0f, -1.0f);
        }
        else if (gdt == GraphicsDeviceType.OpenGLCore || gdt == GraphicsDeviceType.OpenGLES3)
        {
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            s = new float3(new float2(w, h) * 0.5f, 0.5f);
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            //s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, +1.0f, +1.0f);
            t = new float3(+0.0f, +0.0f, +0.0f);
        }
        else if (gdt == GraphicsDeviceType.Vulkan)
        {
            //t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.5f);
            //s = new float3(new float2(w, h) * 0.5f, 0.5f);
            t = new float3(new float2(x, y) + new float2(w, h) * 0.5f, 0.0f);
            s = new float3(new float2(w, h) * 0.5f, 1.0f);

            S.c3 = new float4(t, 1.0f);
            S.c0.x = s.x;
            S.c1.y = s.y;
            S.c2.z = s.z;

            s = new float3(+1.0f, -1.0f, -1.0f);
            t = new float3(+0.0f, +0.0f, -1.0f);
        }

        {
            M.c0.x = s.x;
            M.c1.y = s.y;
            M.c2.z = s.z;
            M.c3 = new float4(s * t, 1.0f);

            S = math.mul(S, M);
        }

        return S;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 RotateVector(quaternion rot, float3 vIn)
    {
        return math.rotate(rot, vIn);
        //return math.mul(math.mul(rot, new quaternion(vIn.x, vIn.y, vIn.z, 0.0f)), math.conjugate(rot)).value.xyz;       
    }
}

class CapsuleMesh
{
    public Mesh mesh
    {
        get; private set;
    }

    public float radius
    {
        get; set;
    }

    public float height
    {
        get
        {
            return _height;
        }
        set
        {
            float temp = value;
            float diameter = 2.0f * radius;
            if (temp > diameter)
            {
                _height = temp;
                _offset = temp - diameter;
            }
            else
            {
                _height = diameter;
                _offset = 0.0f;
            }
        }
    }

    public int sliceCone
    {
        get; private set;
    }

    public int sliceCircle
    {
        get; private set;
    }

    protected float _offset;
    protected float _height;
    protected List<Vector3> vertices;

    public CapsuleMesh(float radius, float height, int sliceCone, int sliceCircle)
    {
        mesh = new Mesh();
        this.radius = radius;
        this.height = height;
        this.sliceCone = sliceCone;
        this.sliceCircle = sliceCircle;
        this.vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indices = new List<int>();

        float r0 = radius;
        float hoffset = 0.5f * _offset;
        float sign = 1.0f;
        int s0 = 2 * sliceCone;
        int s1 = sliceCircle;
        float dtheta = (float)math.radians(180.0f / s0);
        float dphi = (float)math.radians(360.0f / s1);
        float theta = 0.0f;
        float phi = 0.0f;

        int k = 0;
        for (int i = 0; i <= s0; i = i + 1)
        {
            theta = i * dtheta;
            for (int j = 0; j < s1; j = j + 1)
            {
                phi = j * dphi;

                float3 r = new float3(
                (+r0 * math.sin(theta)) * math.cos(phi),
                (+r0 * math.cos(theta)) + sign * hoffset,
                (+r0 * math.sin(theta)) * math.sin(phi));

                float3 vTheta = new float3(
                        (+r0 * math.cos(theta)) * math.cos(phi),
                        (-r0 * math.sin(theta)),
                        (+r0 * math.cos(theta)) * math.sin(phi));

                float3 vPhi = new float3(
                        (+r0 * math.sin(theta)) * (-math.sin(phi)),
                        (0.0f),
                        (+r0 * math.sin(theta)) * (+math.cos(phi)));

                float3 vertex = r;
                float3 normal = float3.zero;
                float4 tangent = -new float4(math.normalize(vPhi), 0.0f);
                float2 uv = new float2((float)i / (float)s0, (float)j / (float)s1);

                if (i == 0)
                {
                    normal = new float3(0.0f, +1.0f, 0.0f);
                }
                else if (i == s0)
                {
                    normal = new float3(0.0f, -1.0f, 0.0f);
                }
                else
                {
                    normal = -math.normalize(math.cross(vTheta, vPhi));
                }

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(tangent);
                uvs.Add(uv);
            }

            if (k <= sliceCone)
            {
                k = k + 1;
                if (k == sliceCone)
                {
                    i = i - 1;
                    sign = -1.0f;
                    k = k + 1;
                }
            }
        }

        for (int i = 0; i < s0 + 1; i = i + 1)
        {
            for (int j = 0; j < s1; j = j + 1)
            {
                int i0 = i * s1;
                int i1 = (i + 1) * s1;
                int j0 = j;
                int j1 = (j + 1) % s1;

                int v00 = i0 + j0;
                int v10 = i1 + j0;
                int v01 = i0 + j1;
                int v11 = i1 + j1;

                indices.Add(v00);
                indices.Add(v11);
                indices.Add(v10);

                indices.Add(v11);
                indices.Add(v00);
                indices.Add(v01);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
    }

    public void ChangeMesh(float radius, float height)
    {
        if (this.radius != radius || this.height != height)
        {
            this.radius = radius;
            this.height = height;

            float r0 = radius;
            float hoffset = 0.5f * _offset;
            float sign = 1.0f;
            int s0 = 2 * sliceCone;
            int s1 = sliceCircle;
            float dtheta = (float)math.radians(180.0f / s0);
            float dphi = (float)math.radians(360.0f / s1);
            float theta = 0.0f;
            float phi = 0.0f;

            int k = 0;
            int m = 0;
            for (int i = 0; i <= s0; i = i + 1)
            {
                theta = i * dtheta;
                for (int j = 0; j < s1; j = j + 1)
                {
                    phi = j * dphi;

                    float3 r = new float3(
                    (+r0 * math.sin(theta)) * math.cos(phi),
                    (+r0 * math.cos(theta)) + sign * hoffset,
                    (+r0 * math.sin(theta)) * math.sin(phi));

                    float3 vertex = r;
                    vertices[m] = vertex;
                    m = m + 1;
                }

                if (k <= sliceCone)
                {
                    k = k + 1;
                    if (k == sliceCone)
                    {
                        i = i - 1;
                        sign = -1.0f;
                        k = k + 1;
                    }
                }
            }

            mesh.SetVertices(vertices);
        }
    }

}
