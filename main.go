package main

import (
	"fmt"
	"sync"
	"time"
)

func produce(c chan<- int) {
	defer func() { close(c) }()
	for i := 1; i <= 7; i++ {
		c <- i
		fmt.Printf("produced %d\n", i)
	}
}

func consume(c <-chan int, wg *sync.WaitGroup) {
	defer func() { wg.Done() }()
	for i := range c {
		time.Sleep(time.Duration(i*77) * time.Millisecond)
		fmt.Printf("consumed %d\n", i)
	}
}

func main() {
	c := make(chan int, 2)
	var wg sync.WaitGroup
	wg.Add(1)
	go produce(c)
	go consume(c, &wg)
	wg.Wait()
}
