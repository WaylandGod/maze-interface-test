using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;

// This component simulates the player input
public class PlayerInput : MonoBehaviour {

	private Stream playerStream;
	private StreamWriter playerStreamWriter;

	// Use this for initialization
	void Start () {
		
		while (true) {

			try {

				// Connect to the maze server:
				var tcp = new TcpClient ("127.0.0.1", 50000);
				this.playerStream = tcp.GetStream ();
				break;

			} catch {
			}
		}

		// Create a writer in order to write lines to the stream:
		this.playerStreamWriter = new StreamWriter (this.playerStream);

		// HTTP GET Request:
		this.playerStreamWriter.Write ("POST /player HTTP/1.1\r\n");
		this.playerStreamWriter.Write ("Host: 127.0.0.1\r\n");
		this.playerStreamWriter.Write ("Content-Type: application/octet-stream\r\n");
		this.playerStreamWriter.Write ("Content-Length: " + int.MaxValue + "\r\n");
		this.playerStreamWriter.Write ("\r\n");
		this.playerStreamWriter.Flush ();
	}

	void OnApplicationQuit() {

		try {
			this.playerStream.Close();
		} catch {
		}

		try {
			this.playerStreamWriter.Close();
		} catch {
		}
	}
	
	// Update is called once per frame
	void Update () {

		//
		// The basic idea is: Do no execute the player's input! Just send it to the maze
		// simulator. Later, read the maze simulator's agent actions. This solution is
		// smart, because the Unity program just has to handle the agent stream. It does
		// not matter, if the agent stream was sourced by an AI agent or by the player.
		//


		// Key: Arrow Up
		if (Input.GetKey (KeyCode.UpArrow)) {

			// Get meaningful data ... here it is just a simulation:
			var degree = (int) (Random.value * 360.0);
			var speed = (int) (Random.value * 100.0);

			// Send the player's state to the maze simulator:
			this.playerStreamWriter.Write ("{0};{1}\n", degree, speed); // Format: degree;speed
			this.playerStreamWriter.Flush();
		}
	}
}
