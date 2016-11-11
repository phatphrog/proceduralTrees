using UnityEngine;

namespace Marcel.TreeGenerator.UI
{
    public class TreeGeneratorUI : UIBase
    {
        public GameObject platform; //tree platform
        public RectTransform leftPanel; //panel for holdings buttons and sliders

        //the tree paramaters
        public int seed;
        [Range(minVertices, maxVertices)]
        public int maxNumVertices; 
        [Range(minSides, maxSides)]
        public int numSides; 
        [Range(minBaseRadius, maxBaseRadius)]
        public float trunkRadius; 
        [Range(minRadiusStep, maxRadiusStep)]
        public float radiusStep; 
        [Range(minRadius, maxRadius)]
        public float branchTipRadius; 
        [Range(minBranchRoundness, maxBranchRoundness)]
        public float branchRoundness; 
        [Range(minSegLength, maxSegLength)]
        public float segLength; 
        [Range(minTwist, maxTwist)]
        public float twist; 
        [Range(minBranchProb, maxBranchProb)]
        public float branchProb;
        [Range(minLeaves, maxLeaves)]
        public int numLeaves;

        //min and max range variables for each tree parameter
        private const int minVertices = 1024;
        private const int maxVertices = 65000;
        private const int minSides = 3;
        private const int maxSides = 32;
        private const int minLeaves = 0;
        private const int maxLeaves = 2000;
        private const float minBaseRadius = 0.25f;
        private const float maxBaseRadius = 4f;
        private const float minRadiusStep = 0.82f;
        private const float maxRadiusStep = 0.95f;
        private const float minRadius = 0.01f;
        private const float maxRadius = 0.1f;
        private const float minBranchRoundness = 0f;
        private const float maxBranchRoundness = 1f;
        private const float minSegLength = 0.35f;
        private const float maxSegLength = 0.75f;
        private const float minTwist = 0f;
        private const float maxTwist = 40f;
        private const float minBranchProb = 0.065f;
        private const float maxBranchProb = 0.25f;

        //material paths
        private static string barkMaterialsPath;
        private static string grassMaterialsPath;

        //create initial tree and initialise the UI buttons and sliders
        private void Awake()
        {
            RenderSettings.skybox = new Material(RenderSettings.skybox);
            //generate initial random tree
            RandomTree();

            //instantiate each slider update existing tree mesh if values are changed
            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Max Vertices", minVertices, maxVertices, maxNumVertices, value =>
                {
                    maxNumVertices = value;
                    Generate(); 
                });
            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Number of Sides", minSides, maxSides, numSides, value =>
                {
                    numSides = value;
                    Generate();
                });

            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Trunk Radius", minBaseRadius, maxBaseRadius, trunkRadius, value =>
                {
                    trunkRadius = value;
                    Generate(); 
                });
            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Radius Step", minRadiusStep, maxRadiusStep, radiusStep, value =>
                {
                    radiusStep = value;
                    Generate(); 
                });
            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Branch Tip Radius", minRadius, maxRadius, branchTipRadius, value =>
                {
                    branchTipRadius = value;
                    Generate(); 
                });

            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Branch Roundness", minBranchRoundness, maxBranchRoundness, branchRoundness, value =>
                {
                    branchRoundness = value;
                    Generate();
                });

            InstantiateControl<SliderControl>(leftPanel)
                .Initialize("Branch Segment Length", minSegLength, maxSegLength, segLength, value =>
                {
                    segLength = value;
                    Generate(); 
                });

            InstantiateControl<SliderControl>(leftPanel)
               .Initialize("Branch Twist", minTwist, maxTwist, twist, value =>
               {
                   twist = value;
                   Generate(); 
               });

            InstantiateControl<SliderControl>(leftPanel)
               .Initialize("Branch Probability", minBranchProb, maxBranchProb, branchProb, value =>
               {
                   branchProb = value;
                   Generate(); 
               });

            InstantiateControl<SliderControl>(leftPanel)
               .Initialize("Max Leaves", minLeaves, maxLeaves, numLeaves, value =>
               {
                   numLeaves = value;
                   Generate();
               });


            //instantiate buttons for generating random tree or random seed
            InstantiateControl<ButtonControl>(leftPanel).Initialize("Random Tree", RandomTree); 
            InstantiateControl<ButtonControl>(leftPanel).Initialize("Random Seed", RandomSeed);

            //quit button to exit application
            InstantiateControl<ButtonControl>(leftPanel).Initialize("Quit", Quit);
        }

        //generate new tree or update existing tree procedurally via the TreeGenerator class
        public void Generate(bool randomTree = false)
        {

            //check if a tree has already been generated
            TreeGenerator tree = GameObject.FindObjectOfType<TreeGenerator>();
            if(tree)
            {
                tree.UpdateTree(seed, maxNumVertices, numSides, trunkRadius, radiusStep, branchTipRadius, branchRoundness, segLength, twist, branchProb, numLeaves);
                
                if (tree.meshRenderer.sharedMaterial != null) 
                {
                    // randomize bark and platform material only when updating existing mesh with an entirely new randomized tree
                    if (randomTree)
                        SetMaterials(tree, platform);
                }
                else SetMaterials(tree, platform);
              
            }
            else
            {
                //otherwise create a new tree from a random seed
                var proceduralTree = new GameObject(string.Format("Tree_{0:X4}", Random.Range(0, 65536))).AddComponent<TreeGenerator>();
                seed = Random.Range(0, 65536);
                proceduralTree.UpdateTree(seed, maxNumVertices, numSides, trunkRadius, radiusStep, branchTipRadius, branchRoundness, segLength, twist, branchProb, numLeaves);
                SetMaterials(proceduralTree, platform);
                SetUIVariables(proceduralTree);
            }

        }

        /*------------helper functions----------*/

        //create an entirely random tree for all tree variables
        private void RandomTree()
        {
            seed = Random.Range(0, 65536);
            maxNumVertices = Random.Range(minVertices, maxVertices);
            numSides = Random.Range(minSides, maxSides);
            trunkRadius = Random.Range(minBaseRadius, maxBaseRadius);
            radiusStep = Random.Range(minRadiusStep, maxRadiusStep);
            branchTipRadius = Random.Range(minRadius, maxRadius);
            branchRoundness = Random.Range(minBranchRoundness, maxBranchRoundness);
            segLength = Random.Range(minSegLength, maxSegLength);
            twist = Random.Range(minTwist, maxTwist);
            branchProb = Random.Range(minBranchProb, maxBranchProb);
            numLeaves = Random.Range(minLeaves, maxLeaves);

            //update the UI sliders to reflect the new randomly generated values
            UpdateSliderValues();
            Generate(true);
        }

        //create a tree from random seed without changing other tree variables
        private void RandomSeed()
        {
            seed = Random.Range(0, 65536);
            Generate();
        }

        //update the slider values in UI
        private void UpdateSliderValues()
        {
            SliderControl[] a = FindObjectsOfType<SliderControl>();
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].headerText.text == "Max Vertices")
                {
                    a[i].slider.value = maxNumVertices;
                }
                if (a[i].headerText.text == "Number of Sides")
                {
                    a[i].slider.value = numSides;
                }
                if (a[i].headerText.text == "Trunk Radius")
                {
                    a[i].slider.value = trunkRadius;
                }
                if (a[i].headerText.text == "Radius Step")
                {
                    a[i].slider.value = radiusStep;
                }
                if (a[i].headerText.text == "Branch Tip Radius")
                {
                    a[i].slider.value = branchTipRadius;
                }
                if (a[i].headerText.text == "Branch Roundness")
                {
                    a[i].slider.value = branchRoundness;
                }
                if (a[i].headerText.text == "Branch Segment Length")
                {
                    a[i].slider.value = segLength;
                }
                if (a[i].headerText.text == "Branch Twist")
                {
                    a[i].slider.value = twist;
                }
                if (a[i].headerText.text == "Branch Probability")
                {
                    a[i].slider.value = branchProb;
                }
                if (a[i].headerText.text == "Max Leaves")
                {
                    a[i].slider.value = numLeaves;
                }
            }

        }

        //set the UI variables to match tree when a new tree is generated
        private void SetUIVariables(TreeGenerator tree)
        {
            seed = tree.seed;
            maxNumVertices = tree.maxNumVertices;
            numSides = tree.numSides;
            trunkRadius = tree.trunkRadius;
            radiusStep = tree.radiusStep;
            branchTipRadius = tree.branchTipRadius;
            branchRoundness = tree.branchRoundness;
            segLength = tree.segLength;
            twist = tree.twist;
            branchProb = tree.branchProb;
            numLeaves = tree.numLeaves;
            UpdateSliderValues();
        }

        //set the materials of both the tree bark and the platform grass
        private static void SetMaterials(TreeGenerator tree, GameObject platform)
        {
            //get materials at random
            Material barkMat = Resources.Load("Materials/BarkMaterials/bark" + Random.Range(1, 19), typeof(Material)) as Material;
            Material grassMat = Resources.Load("Materials/GrassMaterials/grass" + Random.Range(1, 12), typeof(Material)) as Material;

            tree.meshRenderer.sharedMaterial = barkMat;
            platform.GetComponent<Renderer>().material = grassMat;

        }

        private void Quit()
        {
            Application.Quit();
        }

    }

}
