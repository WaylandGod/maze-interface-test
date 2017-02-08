using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System;
using System.Linq;
using System.Threading;

// This component reads the maze walls and re-build the maze
public class MazeReader : MonoBehaviour {

	// The wall height
	public int wallHeight = 20;

	// The maze stream
	private Stream mazeStream;

	// The maze reader
	private StreamReader mazeStreamReader;

	// List of all current walls
	private List<Wall> currentWalls = new List<Wall> ();

	// Was there a change since the last frame?
	private bool wallChanged = false;

	// Stop the read-thread
	private bool stopThread = false;

	// The read-thread:
	private Thread readThread;

	// A rw-mutex in order to sync readers and writers:
	private ReaderWriterLockSlim mutex = new ReaderWriterLockSlim ();

	// Use this for initialization
	void Start () {

		while (true) {

			try {

				// Connect to the maze server:
				var tcp = new TcpClient ("127.0.0.1", 50000);
				this.mazeStream = tcp.GetStream ();
				break;

			} catch {
			}
		}

		// Create a reader in order to read lines from the stream:
		this.mazeStreamReader = new StreamReader (this.mazeStream);

		// HTTP GET Request:
		var writer = new StreamWriter(this.mazeStream);
		writer.Write ("GET /maze HTTP/1.1\r\n");
		writer.Write ("Host: 127.0.0.1\r\n");
		writer.Write ("\r\n");
		writer.Flush ();

		// Create the reader thread:
		this.readThread = new Thread (this.readThreadProcessor);
		this.readThread.Start ();
	}

	void OnApplicationQuit() {
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

			// Delete all old walls. It works by using TAGs. Each wall gets tagged with "MazeWall".
			// Then, all objects with the same TAG could be selected...
			var oldWalls = GameObject.FindGameObjectsWithTag("MazeWall");
			foreach (var oldWall in oldWalls) {
				GameObject.Destroy (oldWall);
			}

			// Create new walls:
			foreach (var wall in walls) {
				var wallObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
				wallObject.tag = "MazeWall"; // Tag the new wall

				// This is not currect, but it shows that the basic idea of Go <=> Unity by HTTP:
				wallObject.transform.position = new Vector3 (wall.x2, this.wallHeight, wall.y2);
				wallObject.transform.localScale = new Vector3 (wall.x1, this.wallHeight, wall.y1);
			}
		}
	}

	// Read all the time from the maze stream
	void readThreadProcessor() {

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

					// Read the next line and split the coordinates:
					var wallLine = this.mazeStreamReader.ReadLine ();
					var wallCoordinateText = wallLine.Split (';').ToArray ();

					// Ensure, that this line was valid -- we need 4 coordinates per line!
					if (wallCoordinateText.Length != 4) {

						// This line was garbage. Read the next line:
						n++;
						continue;
					}

					// The line is valid. Construct a wall object with this
					// coordinates. It is just a intermediate representation
					// which gets visualized at the next frame:
					var wall = new Wall ();
					wall.x1 = int.Parse (wallCoordinateText [0]);
					wall.y1 = int.Parse (wallCoordinateText [1]);
					wall.x2 = int.Parse (wallCoordinateText [2]);
					wall.y2 = int.Parse (wallCoordinateText [3]);
					walls.Add (wall);
				}

				// Commit the changed maze thread-safely:
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