using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

// This component reads the maze reaction for the last action
public class MazeReaction : MonoBehaviour {

	private Stream reactionStream;
	private StreamReader reactionStreamReader;
	private bool stopThread = false;
	private Thread readThread;
	private ReaderWriterLockSlim mutex = new ReaderWriterLockSlim ();

	private int currentRange1 = 0;
	private int currentRange2 = 0;
	private int currentRange3 = 0;
	private int currentRange4 = 0;
	private int currentRange5 = 0;
	private int currentRange6 = 0;

	private int currentPieSlice1Range = 0;
	private int currentPieSlice1Degree = 0;

	private int currentPieSlice2Range = 0;
	private int currentPieSlice2Degree = 0;

	private int currentPieSlice3Range = 0;
	private int currentPieSlice3Degree = 0;

	private int currentPieSlice4Range = 0;
	private int currentPieSlice4Degree = 0;

	// Use this for initialization
	void Start () {
		
		while (true) {

			try {

				// Connect to the maze server:
				var tcp = new TcpClient ("127.0.0.1", 50000);
				this.reactionStream = tcp.GetStream ();
				break;

			} catch {
			}
		}

		// Create a reader in order to read lines from the stream:
		this.reactionStreamReader = new StreamReader (this.reactionStream);

		// HTTP GET Request:
		var writer = new StreamWriter(this.reactionStream);
		writer.Write ("GET /reaction HTTP/1.1\r\n");
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
			this.reactionStreamReader.Close();
		} catch {
		}

		try {
			this.reactionStream.Close();
		} catch {
		}
	}
	
	// Update is called once per frame
	void Update () {

		var currentRange1 = 0;
		var currentRange2 = 0;
		var currentRange3 = 0;
		var currentRange4 = 0;
		var currentRange5 = 0;
		var currentRange6 = 0;

		var currentPieSlice1Range = 0;
		var currentPieSlice1Degree = 0;

		var currentPieSlice2Range = 0;
		var currentPieSlice2Degree = 0;

		var currentPieSlice3Range = 0;
		var currentPieSlice3Degree = 0;

		var currentPieSlice4Range = 0;
		var currentPieSlice4Degree = 0;


		// Read thread-safe the current state:
		this.mutex.EnterReadLock ();
		try {
			currentRange1 = this.currentRange1;
			currentRange2 = this.currentRange2;
			currentRange3 = this.currentRange3;
			currentRange4 = this.currentRange4;
			currentRange5 = this.currentRange5;
			currentRange6 = this.currentRange6;

			currentPieSlice1Range = this.currentPieSlice1Range;
			currentPieSlice1Degree = this.currentPieSlice1Degree;

			currentPieSlice2Range = this.currentPieSlice2Range;
			currentPieSlice2Degree = this.currentPieSlice2Degree;

			currentPieSlice3Range = this.currentPieSlice3Range;
			currentPieSlice3Degree = this.currentPieSlice3Degree;

			currentPieSlice4Range = this.currentPieSlice4Range;
			currentPieSlice4Degree = this.currentPieSlice4Degree;
		}
		finally {
			this.mutex.ExitReadLock ();
		}

		// TODO: Use these values any how...
	}

	// Read all the time from the agent stream
	void readThreadProcessor() {

		var started = false;
		while (!this.stopThread) {

			// Read a line:
			var line = this.reactionStreamReader.ReadLine ();

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
			if (elements.Length != 14) {
				continue;
			}

			Debug.Log ("Reaction: " + line);

			try {

				// Store the data:
				var range1 = elements[0];
				var range2 = elements[1];
				var range3 = elements[2];
				var range4 = elements[3];
				var range5 = elements[4];
				var range6 = elements[5];

				var pieSlice1Range = elements[6];
				var pieSlice1Degree = elements[7];
					
				var pieSlice2Range = elements[8];
				var pieSlice2Degree = elements[9];
					
				var pieSlice3Range = elements[10];
				var pieSlice3Degree = elements[11];
					
				var pieSlice4Range = elements[12];
				var pieSlice4Degree = elements[13];

				// Commit the changed data thread-safe:
				this.mutex.EnterWriteLock();
				try {
					this.currentRange1 = int.Parse(range1);
					this.currentRange2 = int.Parse(range2);
					this.currentRange3 = int.Parse(range3);
					this.currentRange4 = int.Parse(range4);
					this.currentRange5 = int.Parse(range5);
					this.currentRange6 = int.Parse(range6);
					this.currentPieSlice1Range  = int.Parse(pieSlice1Range);
					this.currentPieSlice1Degree = int.Parse(pieSlice1Degree);
					this.currentPieSlice2Range  = int.Parse(pieSlice2Range);
					this.currentPieSlice2Degree = int.Parse(pieSlice2Degree);
					this.currentPieSlice3Range  = int.Parse(pieSlice3Range);
					this.currentPieSlice3Degree = int.Parse(pieSlice3Degree);
					this.currentPieSlice4Range  = int.Parse(pieSlice4Range);
					this.currentPieSlice4Degree = int.Parse(pieSlice4Degree);
				}
				finally {
					this.mutex.ExitWriteLock ();
				}

			} catch {
			}
		}
	}
}
