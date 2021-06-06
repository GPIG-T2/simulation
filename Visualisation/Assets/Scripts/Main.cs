using System.Collections;
using System.Collections.Generic;
using Interface.Client;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] GameObject m_nodePrefab;

    public static Main Instance { get; private set; }

    private List<Node> _nodes = new List<Node>();

    public IClient Client { get; private set; }
    public Models.SimulationSettings Settings { get; private set; }

    async void Awake()
    {
        Debug.Log("Started");

        if (Instance != null)
        {
            Debug.LogError("Secondary creation of main instance");
            return;
        }
        Instance = this;

        this.Client = new WebSocket("ws://127.0.0.1");
        this.Settings = await this.Client.GetSettings().ConfigureAwait(false);

        foreach (var node in this.Settings.Locations)
        {
            var n = Instantiate(this.m_nodePrefab).GetComponent<Node>();
            n.Definition = node;
            this._nodes.Add(n);
        }

        Debug.Log("Initial setup complete");
    }

    // Update is called once per frame
    void Update()
    {

    }

    async void OnDestroy()
    {
        await this.Client.DisposeAsync();
    }
}
