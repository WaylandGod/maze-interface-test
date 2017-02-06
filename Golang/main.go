package main

import (
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

	serverMUX.HandleFunc("/agent", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintf(w, "Hello, %s", "test")
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
