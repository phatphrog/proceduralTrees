using UnityEngine;
using System.Collections.Generic;

namespace Marcel.TreeGenerator
{
    public class TreeGenerator : MonoBehaviour
    {
        //tree parameters
        public int seed = 0; //random seed on which the procedural tree generation is based
        public int maxNumVertices = 65000; //maximum vertices making up the tree mesh
        public int numSides = 16; //number of sides for each branch in the tree
        public float trunkRadius = 2f; //the radius of the tree trunk (in meters)
        public float radiusStep = 0.9f; //how quickly radius decreases until it reachs branchTipRadius
        public float branchTipRadius = 0.02f; //minimum radius for the tips of the tree's smallest branches
        public float branchRoundness = 0.8f; //roundness of branches
        public float segLength = 0.5f; //length of branch segments
        public float twist = 20f; //controls how much branches will twist and curve
        public float branchProb = 0.1f; //probability of spawning a new branch
        public int numLeaves = 2000; //maximum number of leaves that will spawn on the tree

        //mesh renderer and filter
        public MeshRenderer meshRenderer;
        MeshFilter filter;

        //vertex, uv and triangle list for tree mesh
        List<Vector3> vertices; 
        List<Vector2> uvs;
        List<int> triangles;

        //shape of each individual tree ring
        float[] ringShape;
        //checksum for rebuilding tree only when parameters change
        float checksum;
        
        //generate leaves for the tree!
        LeafGenerator leaves;
        public int currentLeafCount = 0;

        //make sure MeshRenderer and MeshFilter components exist on initialise
        void OnEnable()
        {
            if (filter != null && meshRenderer != null) return;

            //set tree to static
            gameObject.isStatic = true;

            filter = gameObject.GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        //updates tree mesh with provided parameters -> only if parameters have changes since last tree was generated
        public void UpdateTree(int s, int verts, int sides, float trunk, float step, float tip, float roundness, float seg, float tw, float prob, int leaves)
        {
            var newChecksum = (s & 0xFFFF) + sides + seg + trunk + verts +
                step + tip + tw + prob + roundness + leaves;

            //return if tree params have not changed
            if (checksum == newChecksum && filter.sharedMesh != null) return;

            //otherwise we set the new parameters
            checksum = newChecksum;
            seed = s;
            maxNumVertices = verts;
            numSides = sides;
            trunkRadius = trunk;
            radiusStep = step;
            branchTipRadius = tip;
            branchRoundness = roundness;
            segLength = seg;
            twist = tw;
            branchProb = prob;
            numLeaves = leaves;

            GenerateTree(); //and update the tree mesh
        }

        //generate the tree
        public void GenerateTree()
        {
            //set the tree to be non static so we can rotate it during generation
            gameObject.isStatic = false;

            //create lists for new tree vertices, uvs and triangles
            if (vertices == null)
            {
                vertices = new List<Vector3>();
                uvs = new List<Vector2>();
                triangles = new List<int>();
                leaves = new LeafGenerator();
            }
            else //clear lists if we are updating
            {
                vertices.Clear();
                uvs.Clear();
                triangles.Clear();
                leaves.ClearLeaves();
                currentLeafCount = 0;
            }

            var originalRotation = transform.localRotation;
            var originalSeed = Random.seed;

            //set the the tree ring shape array with the current number of sides
            SetTreeRingShape(); 
            Random.seed = seed;
            
            //this is the main recursive grow function -> creates each ring of vertices in the tree starting at the base of the trunk
            Grow(Vector3.zero, new Quaternion(), -1, trunkRadius, 0f);
            
            Random.seed = originalSeed;
            transform.localRotation = originalRotation;

            //update/create the tree mesh
            SetTreeMesh();
        }

        //recursive Grow function to procedurally generate the tree one vertex ring at a time
        void Grow(Vector3 position, Quaternion quaternion, int prevVertRingIndex, float radius, float texCoordV)
        {
            var textureStepU = 1f / numSides;
            var texCoord = new Vector2(0f, texCoordV);
            var angInc = 2f * Mathf.PI * textureStepU;
            var ang = 0f;
            var offset = Vector3.zero;

            //create the new tree ring vertices
            for (var n = 0; n <= numSides; n++, ang += angInc) 
            {
                var r = ringShape[n] * radius;
                //get offsets and add new vertex for a natural looking shape
                offset.x = r * Mathf.Cos(ang);
                offset.z = r * Mathf.Sin(ang);
                vertices.Add(position + quaternion * offset);
                uvs.Add(texCoord);
                texCoord.x += textureStepU;
            }

            if (prevVertRingIndex >= 0) //after first tree ring has been added
            {
                //add the new branch segment quads between the previous 2 rings of vertices
                for (var currVertRingIndex = vertices.Count - numSides - 1; currVertRingIndex < vertices.Count - 1; currVertRingIndex++, prevVertRingIndex++) 
                {
                    //segment triangle 1
                    triangles.Add(prevVertRingIndex + 1); 
                    triangles.Add(prevVertRingIndex);
                    triangles.Add(currVertRingIndex);
                    //segment triangle 2
                    triangles.Add(currVertRingIndex); 
                    triangles.Add(currVertRingIndex + 1);
                    triangles.Add(prevVertRingIndex + 1);
                }
            }

            radius *= radiusStep;
            if (radius < branchTipRadius || vertices.Count + numSides >= maxNumVertices) //end the branch if we run out of vertices or reach branchTipRadius
            {
                //create branch cap at its end
                vertices.Add(position); 
                uvs.Add(texCoord + Vector2.one);
                for (var n = vertices.Count - numSides - 2; n < vertices.Count - 2; n++) 
                {
                    triangles.Add(n);
                    triangles.Add(vertices.Count - 1);
                    triangles.Add(n + 1);
                }

                //grow leaves at the end of tree branches
                if (currentLeafCount < numLeaves)
                {
                    leaves.GrowLeaf(position, this);
                    currentLeafCount++;
                }

                return; 
            }

            //continue growing current branch 
            texCoordV += 0.0625f * (segLength + segLength / radius);
            position += quaternion * new Vector3(0f, segLength, 0f);
            transform.rotation = quaternion; 
            var x = (Random.value - 0.5f) * twist;
            var z = (Random.value - 0.5f) * twist;
            //random angle at which branch rotates -> depending on twist
            transform.Rotate(x, 0f, z);
            prevVertRingIndex = vertices.Count - numSides - 1;
            Grow(position, transform.rotation, prevVertRingIndex, radius, texCoordV); //grow the next branch segment

            //check for creating a new branch
            if (vertices.Count + numSides >= maxNumVertices || Random.value > branchProb) return;

            //add new branch
            transform.rotation = quaternion;
            x = Random.value * 70f - 35f;
            x += x > 0 ? 10f : -10f;
            z = Random.value * 70f - 35f;
            z += z > 0 ? 10f : -10f;
            transform.Rotate(x, 0f, z);
            Grow(position, transform.rotation, prevVertRingIndex, radius, texCoordV); //start the next branch
        }

        //sets the shape of the tree ring
        private void SetTreeRingShape()
        {
            ringShape = new float[numSides + 1];
            var k = (1f - branchRoundness) * 0.5f;
            // Randomize the vertex offsets, according to BranchRoundness
            Random.seed = seed;
            for (var n = 0; n < numSides; n++) ringShape[n] = 1f - (Random.value - 0.5f) * k;
            ringShape[numSides] = ringShape[0];
        }

        //create/updates the MeshFilter's mesh from the generated vertices, uvs and triangles
        private void SetTreeMesh()
        {
            var treeMesh = filter.sharedMesh;
            //create new mesh if none exists
            if (treeMesh == null)
                treeMesh = filter.sharedMesh = new Mesh();
            else
                treeMesh.Clear();

            //assign the vertices, uvs and triangles
            treeMesh.vertices = vertices.ToArray();
            treeMesh.uv = uvs.ToArray();
            treeMesh.triangles = triangles.ToArray();

            //update the normals and bounds of the mesh
            treeMesh.RecalculateNormals();
            treeMesh.RecalculateBounds();
        }

    }
}