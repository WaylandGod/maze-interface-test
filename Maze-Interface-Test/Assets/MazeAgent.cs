using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

// This component reads the agent's actions. This could be an AI agent
// or a player using this Unity project. The Unity component PlayerInput
// will send player's actions to the maze server. The maze server forwards
// all actions to this component regardless if the source is the AI or the
// player.
public class MazeAgent : MonoBehaviour {

	private Stream agentStream;
	private StreamReader agentStreamReader;
	private int currentSpeed = 0;
	private int currentDegree = 90;
	private bool stopThread = false;
	private Thread readThread;
	private ReaderWriterLockSlim mutex = new ReaderWriterLockSlim ();

	// Use this for initialization
	void Start () {

		while (true) {

			try {

				// Connect to the maze server:
				var tcp = new TcpClient ("127.0.0.1", 50000);
				this.agentStream = tcp.GetStream ();
				break;

			} catch {
			}
		}

		// Create a reader in order to read lines from the stream:
		this.agentStreamReader = new StreamReader (this.agentStream);

		// HTTP GET Request:
		var writer = new StreamWriter(this.agentStream);
		writer.Write ("GET /agent HTTP/1.1\r\n");
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
			this.agentStreamReader.Close();
		} catch {
		}

		try {
			this.agentStream.Close();
		} catch {
		}
	}
	
	// Update is called once per frame
	void Update () {

		var speed = 0;
		var degree = 0;

		// Read thread-safe the current state:
		this.mutex.EnterReadLock ();
		try {
			speed = this.currentSpeed;
			degree = this.currentDegree;
		}
		finally {
			this.mutex.ExitReadLock ();
		}

		// TODO: Use speed and degree any how...
	}

	// Read all the time from the agent stream
	void readThreadProcessor() {

		var started = false;
		while (!this.stopThread) {

			// Read a line:
			var line = this.agentStreamReader.ReadLine ();

			// Check the start signal. Without start, all data gets deleted:
			if (line.StartsWith ("START")) {
				started = true;
				continue;
			}

			// Ignore any data before start signal:
			if (!started) {
				continue;
			}

			// Ignore empty lines:
			if (line.Trim () == string.Empty) {
				continue;
			}

			// Split the data:
			var elements = line.Split(';');

			// Check the correct format:
			if (elements.Length != 2) {
				continue;
			}

			Debug.Log ("Agent: " + line);

			try {

				// Store the data:
				var degree = int.Parse(elements[0]);
				var speed = int.Parse(elements[1]);

				// Commit the changed data thread-safe:
				this.mutex.EnterWriteLock();
				try {
					this.currentDegree = degree;
					this.currentSpeed = speed;
				}
				finally {
					this.mutex.ExitWriteLock ();
				}

			} catch {
			}
		}
	}
}
