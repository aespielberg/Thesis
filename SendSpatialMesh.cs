using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using Parabox;
using WebSocketSharp;

public class SendSpatialMesh : MonoBehaviour

{
    private MLInput.Controller _controller;
    public GameObject objMeshToExport;

    // Start is called before the first frame update
    void Start()
    {
        MLInput.Start();
        _controller = MLInput.GetController(MLInput.Hand.Left);
        objMeshToExport = GameObject.Find("MeshingNodes");
    }

    // Update is called once per frame
    void Update()
    {
        CheckTrigger();
    }

    void OnDestroy()
    {
        MLInput.Stop();
    }

    void CheckTrigger()
    {
        if (_controller.TriggerValue > 0.2f)
        {
            using (var ws = new WebSocket("ws://127.0.0.1:4649/Laputa"))
            {
                Debug.Log("Trigger Pressed");
                string path = Path.Combine(Application.persistentDataPath, "data");
                path = Path.Combine(path, "roomModel" + ".stl");

                Mesh mesh = objMeshToExport.GetComponent<MeshFilter>().mesh;
                MeshFilter[] meshFilters = objMeshToExport.GetComponentsInChildren<MeshFilter>();


                CombineInstance[] combine = new CombineInstance[meshFilters.Length];

                int i = 0;
                while (i < meshFilters.Length)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                    meshFilters[i].gameObject.SetActive(false);

                    i++;
                }
                transform.GetComponent<MeshFilter>().mesh = new Mesh();
                //Mesh finalMesh = GetComponent<MeshFilter>().mesh;
                transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
                transform.gameObject.SetActive(true);

                //Create Directory if it does not exist
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                Parabox.Stl.Exporter.WriteFile(path, combine, Parabox.Stl.FileType.Binary);

                byte[] bytes = System.IO.File.ReadAllBytes(path);
                //Console.WriteLine("bytes", bytes);
                ws.Send(bytes);
            }

        }

        if (_controller.IsBumperDown)
        {
            Debug.Log("Bumper Pressed");
        }
    }
}
