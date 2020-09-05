package main

import (
	"fmt"
	"sync"
	"time"
)

func work(duration time.Duration) {
	start := time.Now()
	for time.Since(start) < duration {
	}
}

func produce(pipe chan<- int) {
	defer func() { close(pipe) }()
	for i := 1; i <= 7; i++ {
		pipe <- i
		fmt.Printf("produced %d\n", i)
		work(750 * time.Millisecond)
	}
	fmt.Print("stop producing\n")
}

func consume(pipe <-chan int, wg *sync.WaitGroup) {
	defer func() { wg.Done() }()
	for i := range pipe {
		fmt.Printf("consumed %d\n", i)
		work(1250 * time.Millisecond)
	}
	fmt.Print("stop consuming\n")
}

func main() {
	pipe := make(chan int, 2)
	go produce(pipe)
	var wg sync.WaitGroup
	wg.Add(1)
	go consume(pipe, &wg)
	wg.Wait()
	fmt.Print("stop waiting\n")
}
