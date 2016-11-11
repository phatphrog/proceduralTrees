using UnityEngine;
using System.Collections.Generic;

namespace Marcel.TreeGenerator
{
    public class LeafGenerator : MonoBehaviour {

        public List<GameObject> leaves;
        private static string leafMaterialsPath;
        private static int leafMaterialIndex;
        private static Material leafMat;

        //create a leaf at a specific position on the tree
        public void  GrowLeaf(Vector3 position, TreeGenerator tree)
        {
            if (leaves == null)
            {
                leaves = new List<GameObject>();
            }

            //only spawn a new leaf if we haven't already spawned the maximum number of leaves available per tree
            if(leaves.Count < tree.numLeaves)
            { 
                GameObject leaf = new GameObject(string.Format("Leaf_{0:X4}", Random.Range(0, 65536)));
                leaf.transform.position = position;
                leaf.transform.rotation = new Quaternion(Random.value, Random.value, Random.value, Random.value);
  
                MeshFilter meshFilter = (MeshFilter)leaf.AddComponent(typeof(MeshFilter));
                meshFilter.mesh = CreateLeafMesh(1, 1);
                MeshRenderer renderer = leaf.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            
                SetMaterial(tree, leaf);
                leaves.Add(leaf);
            }
            else //otherwise we move existing leaves to the ends of the branches of the new tree being generated
            {
               
                for (int i = 0; i < leaves.Count; i++)
                {
                    if(leaves[i].activeSelf == false)
                    {
                        leaves[i].SetActive(true);
                        leaves[i].transform.position = position;
                        SetMaterial(tree, leaves[i]);
                        break;
                    }   
                }
            }
        }

        //deactivate all leaves
        public void ClearLeaves()
        {
           if (leaves != null)
            {
                for (int i = 0; i < leaves.Count; i++)
                {
                    leaves[i].SetActive(false);
                }
            }
        }

        //create leaf mesh plane
        public Mesh CreateLeafMesh(float width, float height)
        {
            Mesh m = new Mesh();
            m.name = "ScriptedMesh";
            m.vertices = new Vector3[] {
                new Vector3(-width, -height, 0.01f),
                new Vector3(width, -height, 0.01f),
                new Vector3(width, height, 0.01f),
                new Vector3(-width, height, 0.01f)
            };
            m.uv = new Vector2[] {
                new Vector2 (0, 0),
                new Vector2 (0, 1),
                new Vector2(1, 1),
                new Vector2 (1, 0)
            };
            m.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            m.RecalculateNormals();

            return m;
        }

        //set the material of the leaf
        private static void SetMaterial(TreeGenerator tree, GameObject leaf)
        {
            //first leaf of a new tree sets new random leaf material
            if(tree.currentLeafCount == 0)
            {
                leafMat = Resources.Load("Materials/LeafMaterials/Leaves" + Random.Range(1, 15), typeof(Material)) as Material;
                
            }
            leaf.GetComponent<Renderer>().material = leafMat;
        }
    }
}
