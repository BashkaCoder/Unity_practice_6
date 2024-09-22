using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Water
{
    public class MeshContainer
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public Vector3[] normals;


        public MeshContainer(Mesh m)
        {
            mesh = m;
            m.GetVertices(vertices);
            normals = m.normals;
        }


        public void Update()
        {
            mesh.SetVertices(vertices);
            mesh.normals = normals;
        }
    }
}