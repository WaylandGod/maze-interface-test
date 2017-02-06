package main

import (
	"bufio"
	"fmt"
	"math/rand"
	"net/http"
	"os"
	"time"
)

func main() {
	// Print the version:
	fmt.Println(`maze-interface-test v1.0.0`)

	// Set the seed for the random function:
	rand.Seed(time.Now().Unix())

	// Set up the HTTPS server:
	serverMUX := http.NewServeMux()

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

		// Create a reader and connect it to stdin:
		stdinReader := bufio.NewReader(os.Stdin)

		for {
			// Read the next line i.e. action of the agent:
			line, _ := stdinReader.ReadString('\n')

			// Forward the data to the HTTP channel:
			fmt.Fprintf(w, "%s\n", line)

			// Flush the data in order to use the HTTP channel as never ending stream:
			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			} else {
				fmt.Println("Damn, no flush")
			}

			//
			// TODO: Use the data to calculate some reaction of the maze...
			//

			//
			// Simulate the reaction:
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
			// TODO
		}
	})

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
