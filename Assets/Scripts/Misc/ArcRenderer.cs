using System.Collections.Generic;
using UnityEngine;

namespace Misc {

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class ArcRenderer : MonoBehaviour {

        [SerializeField] private float radius;
        [SerializeField] private float angleStart;
        [SerializeField] private float angleEnd;
        [SerializeField] private float maxVertexDistance;
        private MeshFilter _filter;
        private Mesh _arcMesh;
        /// <summary>
        /// List of vertices forming the outer arc, excluding the first center vertex that all vertices connect to
        /// </summary>
        private List<Vector3> _verts;
        private List<int> _tris;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start() {
            _filter = GetComponent<MeshFilter>();
            if (_arcMesh == null)
                _arcMesh = new Mesh();
            _filter.mesh = _arcMesh;
        }

        // Update is called once per frame
        void Update() {
            UpdateMesh();
        }

        void UpdateMesh() {
            if (_arcMesh == null) {
                _arcMesh = new Mesh();
                _arcMesh.MarkDynamic();
            }
            if (_verts == null)
                _verts = new List<Vector3>();
            if (_tris == null)
                _tris = new List<int>();
            
            int vertCount = Mathf.CeilToInt((angleEnd - angleStart) / maxVertexDistance) + 2; // Add one extra for the centre vert
            int triCount = (vertCount - 2); //There are always two triangles fewer than there are vertices.
            triCount *= 3; // There are three vertex indices per triangle
            if (_verts.Count == 0)
                _verts.Add(Vector3.zero);
            else {
                _verts[0] = Vector3.zero;
            }
            Vector3 vert = new Vector3();
            int[] triplet = new int [3];
            for (int i = 1; i < vertCount; i++) {
                // Calculate this vertex's angle
                float angle = (i - 1) / (vertCount - 2f) * (angleEnd - angleStart) + angleStart;
                // Calculate position using trigonometry (thanks, high school math!)
                vert.x = Mathf.Sin(angle * Mathf.Deg2Rad);
                vert.y = Mathf.Cos(angle * Mathf.Deg2Rad);
                // Apply radius scaling
                vert *= radius;
                // Add/update vertex position
                if (i >= _verts.Count)
                    _verts.Add(vert);
                else
                    _verts[i] = vert;
                // Add vertex index triplet to triangle buffer(?)
                // MY BRAIN IS TOO SMOOTH AND MY CRANIUM TOO SMALL FOR THIS KINDA MATH AAAAA *explodes*
                if (i > 1) {
                    triplet[1] = i - 1;
                    triplet[2] = i;
                     if ((i - 1) * 3 > _tris.Count) {
                        _tris.AddRange(triplet);
                    } else {
                        _tris[(i - 1) * 3 - 2] = triplet[1];
                        _tris[(i - 1) * 3 - 1] = triplet[2];
                    }
                }
            }
            // Prune excess vertices
            if (_verts.Count > vertCount)
                _verts.RemoveRange(vertCount, _verts.Count - vertCount);
            if (_tris.Count > triCount)
                _tris.RemoveRange(triCount, _tris.Count - triCount);

            for (int i = 2; i < _verts.Count; i++)
                Debug.DrawLine(transform.position + (_verts[i] * 1.2f), transform.position + (_verts[i - 1] * 1.2f), Color.darkRed);

            for (int i = 2; i < _tris.Count; i++) {
                Debug.DrawLine(transform.position + (_verts[_tris[i]] * 1.1f),
                    transform.position + (_verts[_tris[i-1]] * 1.1f), Color.darkGreen);
                Debug.DrawLine(transform.position + (_verts[_tris[i-2]] * 1.1f),
                    transform.position + (_verts[_tris[i-2]] * 1.1f), Color.darkGreen);
            }
            
            Debug.DrawLine(transform.position + (_verts[_tris[^1]] * 1.05f),
                transform.position + (_verts[_tris[^2]] * 1.05f), Color.white);
            Debug.DrawLine(transform.position + (_verts[_tris[^2]] * 1.05f),
                transform.position + (_verts[_tris[^3]] * 1.05f), Color.black);
            Debug.DrawLine(transform.position + (_verts[_tris[^1]] * 1.05f),
                transform.position + (_verts[_tris[^3]] * 1.05f), Color.blue);
            
            Debug.DrawLine(transform.position + (_verts[_tris[2]] * 1.05f),
                transform.position + (_verts[_tris[1]] * 1.05f), Color.white);
            Debug.DrawLine(transform.position + (_verts[_tris[1]] * 1.05f),
                transform.position + (_verts[_tris[0]] * 1.05f), Color.black);
            Debug.DrawLine(transform.position + (_verts[_tris[2]] * 1.05f),
                transform.position + (_verts[_tris[0]] * 1.05f), Color.blue);

            _arcMesh.SetVertices(_verts);
            _arcMesh.SetTriangles(_tris, 0);
        }

    }

}