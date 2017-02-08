package main

import (
	"bufio"
	"fmt"
	"math/rand"
	"net/http"
	"os"
	"strings"
	"time"
)

func main() {

	//
	// Remark
	// In order to run this implementation, the provided HTTP handlers must get started.
	// To do so, please start http://127.0.0.1:5000/maze, http://127.0.0.1:5000/agent,
	// http://127.0.0.1:5000/reaction and http://127.0.0.1:5000/player Instead, just
	// start the visualizer which connects to all of these end-points.
	//
	// This behaviour is not intended for the final implementation, where the HTTP handler
	// are optional. This is just a test implementation in order to show the desired
	// interface.
	//

	// Set the seed for the random function:
	rand.Seed(time.Now().Unix())

	// Set up the HTTPS server:
	serverMUX := http.NewServeMux()

	// A Go channel in order to connect the agent processing with the reaction HTTP handler:
	reactionChannel := make(chan string)

	// A Go channel in order to connect the stdin and player reader towards the agent handler:
	actionChannel := make(chan string)

	//
	// A simulation for the maze channel. It should send the wall coordinates to the visualizer.
	//
	// Please notice:
	// This implementation will create a new maze generate for each HTTP request. Thus, each
	// client gets his own simulator. This behaviour is maybe not desired for the final implementation.
	//
	serverMUX.HandleFunc("/maze", func(w http.ResponseWriter, r *http.Request) {
		for {

			// Generate the a number as count of alls:
			numberWalls := rand.Intn(11) // 0..10 walls

			// Send the message WALLS: in order to indicate how many wall coordinates are follow:
			fmt.Fprintf(w, "WALLS: %d\n", numberWalls)

			// Generate the desired amount of walls:
			for n := 0; n < numberWalls; n++ {

				// Send a wall's coordinates to the channel:
				fmt.Fprintf(w, "%d;%d;%d;%d\n", rand.Intn(100), rand.Intn(100), rand.Intn(100), rand.Intn(100)) // x1, y1, x2, y2
			}

			// Flush the data in order to use the HTTP channel as never ending stream:
			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			} else {
				fmt.Println("Damn, no flush")
			}

			// Simulate random time interval until the maze changes:
			sleepTime := time.Duration(rand.Intn(30)) // Seconds
			time.Sleep(sleepTime * time.Second)
		}
	})

	//
	// A simulation for the agent i.e. AI interface. It reads from stdin and forwards
	// the data to the HTTP channel towards the visualization.
	//
	// Please notice:
	// This implementation will create a new simulation for each HTTP request. Thus, each
	// client gets his own simulator. This behaviour is maybe not desired for the final implementation.
	//
	serverMUX.HandleFunc("/agent", func(w http.ResponseWriter, r *http.Request) {

		// Send the START signal:
		fmt.Fprint(w, "START\n")

		for {

			// Read one action ... from stdin or player handler:
			line := <-actionChannel

			// Check the right format:
			if len(strings.Split(line, `;`)) != 2 {
				continue
			}

			// Forward the data to the HTTP channel:
			fmt.Fprintf(w, "%s", line) // Line includes \n!

			// Flush the data in order to use the HTTP channel as never ending stream:
			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			} else {
				fmt.Println("Damn, no flush")
			}

			//
			// TODO: Use the data to calculate some reaction of the maze by processing the input i.e. action (speed & degree)
			//

			//
			// Simulate the reaction
			//

			// 6 Range finder sensors i.e. distance to the next walls:
			range1 := rand.Intn(100)
			range2 := rand.Intn(100)
			range3 := rand.Intn(100)
			range4 := rand.Intn(100)
			range5 := rand.Intn(100)
			range6 := rand.Intn(100)

			// 4 Pie-Slice Sensors i.e. angel and distance to goal (if visible):
			pieSlice1Angel := rand.Intn(361)
			pieSlice1Dist := rand.Intn(100)

			pieSlice2Angel := rand.Intn(361)
			pieSlice2Dist := rand.Intn(100)

			pieSlice3Angel := rand.Intn(361)
			pieSlice3Dist := rand.Intn(100)

			pieSlice4Angel := rand.Intn(361)
			pieSlice4Dist := rand.Intn(100)

			// Create the reaction string:
			reaction := fmt.Sprintf("%d;%d;%d;%d;%d;%d;%d;%d;%d;%d;%d;%d;%d;%d\n", range1, range2, range3, range4, range5, range6, pieSlice1Angel, pieSlice1Dist, pieSlice2Angel, pieSlice2Dist, pieSlice3Angel, pieSlice3Dist, pieSlice4Angel, pieSlice4Dist)

			// Write it to stdout i.e. to the agent / A.I:
			fmt.Print(reaction)

			// Write it to the HTTP channel:
			reactionChannel <- reaction
		}
	})

	// This is the HTTP channel with the reactions for the visualizations:
	serverMUX.HandleFunc("/reaction", func(w http.ResponseWriter, r *http.Request) {

		// Send the START signal:
		fmt.Fprint(w, "START\n")

		for {

			// Read one reaction:
			reaction := <-reactionChannel

			// Check the right format:
			if len(strings.Split(reaction, `;`)) != 14 {
				continue
			}

			// Write the reaction to the HTTP stream:
			fmt.Fprintf(w, "%s", reaction) // The reaction string has already the \n!

			// Flush the data in order to use the HTTP channel as never ending stream:
			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			} else {
				fmt.Println("Damn, no flush")
			}
		}
	})

	// This is the HTTP channel to read player interactions from the visualization:
	serverMUX.HandleFunc("/player", func(w http.ResponseWriter, r *http.Request) {

		// In this case, we just read from the visualization:
		reader := bufio.NewReader(r.Body)

		for {

			// Read one line with player interaction i.e. angel and speed:
			line, errRead := reader.ReadString('\n') // Line includes \n!

			if errRead != nil {
				fmt.Println(errRead.Error())
				return
			}

			// Check if the line is empty:
			if strings.TrimSpace(line) == `` {
				continue
			}

			// Check the right format:
			if len(strings.Split(line, `;`)) != 2 {
				continue
			}

			// Write the data to the processor:
			actionChannel <- line
		}
	})

	// Read from stdin
	go func() {

		// Create a reader and connect it to stdin:
		stdinReader := bufio.NewReader(os.Stdin)

		for {

			// Read the next line i.e. action of the agent:
			line, _ := stdinReader.ReadString('\n') // Format: degree;speed ... line includes \n!

			// Write the data to the processor:
			actionChannel <- line
		}
	}()

	// Define the HTTP server:
	server := &http.Server{}
	server.Addr = ":50000"
	server.Handler = serverMUX
	server.SetKeepAlivesEnabled(true)                      // use keep alive
	server.ReadTimeout = 52560 * time.Hour                 // approx. 6 years
	server.WriteTimeout = 52560 * time.Hour                // approx. 6 years
	server.MaxHeaderBytes = 36 * 1024 * 1024 * 1024 * 1024 // ~ 36 TB

	// Start the server:
	errHTTPS := server.ListenAndServe()
	if errHTTPS != nil {
		fmt.Printf("[Error] Was not able to start the HTTP server: %s.\n", errHTTPS.Error())
		os.Exit(0)
	}
}
