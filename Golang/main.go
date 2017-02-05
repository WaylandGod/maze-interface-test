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
	serverMUX.HandleFunc("/maze", func(w http.ResponseWriter, r *http.Request) {
		for {
			numberWalls := rand.Intn(11) // 0..10 walls
			fmt.Fprintf(w, "WALLS: %d\n", numberWalls)
			for n := 0; n < numberWalls; n++ {
				fmt.Fprintf(w, "%d;%d;%d;%d\n", rand.Intn(100), rand.Intn(100), rand.Intn(100), rand.Intn(100)) // x1, y1, x2, y2
			}

			if f, ok := w.(http.Flusher); ok {
				f.Flush()
			} else {
				fmt.Println("Damn, no flush")
			}

			sleepTime := time.Duration(rand.Intn(30)) // Seconds
			time.Sleep(sleepTime * time.Second)
		}
	})

	serverMUX.HandleFunc("/agent", func(w http.ResponseWriter, r *http.Request) {
		fmt.Fprintf(w, "Hello, %s", "test")
	})

	server := &http.Server{}
	server.Addr = ":50000"
	server.Handler = serverMUX
	server.SetKeepAlivesEnabled(true)
	server.ReadTimeout = 52560 * time.Hour
	server.WriteTimeout = 52560 * time.Hour
	server.MaxHeaderBytes = 36 * 1024 * 1024 * 1024 * 1024 // ~ 36 TB

	// Start the server:
	errHTTPS := server.ListenAndServe()
	if errHTTPS != nil {
		fmt.Printf("[Error] Was not able to start the HTTP server: %s.\n", errHTTPS.Error())
		os.Exit(0)
	}
}
