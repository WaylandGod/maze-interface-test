using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

// This component starts the external maze simulator
public class MazeProcessStarter : MonoBehaviour {

	public string pathToMazeServer = "maze-server";
	public bool startExternalProcess = true;

	private Process mazeProcess;

	// Use this for initialization
	void Start () {

		if (!this.startExternalProcess) {
			UnityEngine.Debug.Log ("Does not start the maze process.");
			return;
		}

		UnityEngine.Debug.Log ("Start the maze process.");

		// Start parameters for the maze proces:
		var startParams = new ProcessStartInfo ();
		startParams.FileName = this.pathToMazeServer;
		startParams.Arguments = "";
		startParams.UseShellExecute = false;

		// Redirect the stdin and stdout?
		startParams.RedirectStandardInput = false;
		startParams.RedirectStandardOutput = false;

		// Start the proces:
		this.mazeProcess = new Process ();
		this.mazeProcess.StartInfo = startParams;
		this.mazeProcess.Start ();
	}

	// At the end, just kill the proces:
	void OnApplicationQuit() {
		try {
			this.mazeProcess.Kill();
		} catch {
		}
	}
	
	// Update is called once per frame
	void Update () {
		// NOP
	}
}
