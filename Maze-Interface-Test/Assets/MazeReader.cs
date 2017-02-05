using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System;
using System.Linq;
using System.Threading;

public class MazeReader : MonoBehaviour {

	public int wallHeight = 20;

	private Stream mazeStream;
	private StreamReader mazeStreamReader;
	private List<Wall> currentWalls = new List<Wall> ();
	private bool wallChanged = false;
	private bool stopThread = false;
	private Thread readThread;
	private ReaderWriterLockSlim mutex = new ReaderWriterLockSlim ();

	// Use this for initialization
	void Start () {
		
		// Connect to the maze server:
		var web = new WebClient ();
		this.mazeStream = web.OpenRead("http://127.0.0.1:50000/maze");

		// Create a reader in order to read lines from the stream:
		this.mazeStreamReader = new StreamReader (this.mazeStream);

		// Create the reader thread:
		this.readThread = new Thread (this.readThreadProcessor);
		this.readThread.Start ();
		Debug.Log ("Init done.");
	}

	void OnApplicationQuit() {
		Debug.Log("Stop!");

		try {
			this.stopThread = true;
		} catch {
		}

		try {
			this.mazeStreamReader.Close();
		} catch {
		}

		try {
			this.mazeStream.Close();
		} catch {
		}
	}
	
	// Update is called once per frame
	void Update () {

		// Thread-local variables:
		var changed = false;
		var walls = new List<Wall> ();

		// Read thread-safe the current state:
		this.mutex.EnterReadLock ();
		try {
			changed = this.wallChanged;
			walls.AddRange(this.currentWalls);
			this.wallChanged = false;
		}
		finally {
			this.mutex.ExitReadLock ();
		}

		// Do we got a new maze?
		if (changed) {

			Debug.Log ("Build new walls...");

			// Delete all old walls:
			var oldWalls = GameObject.FindGameObjectsWithTag("MazeWall");
			foreach (var oldWall in oldWalls) {
				GameObject.Destroy (oldWall);
			}

			// Create new walls:
			foreach (var wall in walls) {
				var wallObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
				wallObject.tag = "MazeWall";

				// This is not currect, but it shows that the basic idea of Go <=> Unity by HTTP works:
				wallObject.transform.position = new Vector3 (wall.x2, this.wallHeight, wall.y2);
				wallObject.transform.localScale = new Vector3 (wall.x1, this.wallHeight, wall.y1);
			}
		}
	}

	// Read all the time from the maze stream
	void readThreadProcessor() {

		Debug.Log ("Maze Read Thread started.");

		while (!this.stopThread) {

			// Read a line:
			var line = this.mazeStreamReader.ReadLine ();

			// Is this the indicator for starting wall transmission?
			if (line.ToLower ().StartsWith ("walls: ")) {

				// How many walls are follow?
				var countWalls = int.Parse (line.Replace ("WALLS: ", string.Empty));

				Debug.Log ("There are " + countWalls + " new walls.");

				// Storage for all walls:
				var walls = new List<Wall> (countWalls);

				// Read all walls:
				for (int n = 0; n < countWalls; n++) {
					var wallLine = this.mazeStreamReader.ReadLine ();
					var wallCoordinateText = wallLine.Split (';').ToArray ();
					var wall = new Wall ();
					wall.x1 = int.Parse (wallCoordinateText [0]);
					wall.y1 = int.Parse (wallCoordinateText [1]);
					wall.x2 = int.Parse (wallCoordinateText [2]);
					wall.y2 = int.Parse (wallCoordinateText [3]);
					walls.Add (wall);
				}

				// Commit the changed maze thread-safe:
				this.mutex.EnterWriteLock();
				try {
					this.currentWalls.Clear();
					this.currentWalls.AddRange(walls);
					this.wallChanged = true;
				}
				finally {
					this.mutex.ExitWriteLock ();
				}
			}
		}
	}
}


public struct Wall {
	public int x1;
	public int y1;
	public int x2;
	public int y2;
}