﻿namespace GameOfLife.Tests

module Tests =
    open System
    open Xunit
    open FsUnit.Xunit
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit
    open Akka.TestKit.Xunit
    open GameOfLife.Domain

    type GameOfLifeTests () as kit =
        inherit TestKit()

        let system = kit.Sys
        let waitFor = TimeSpan.FromMilliseconds(250.0)
        let timeout = Nullable(waitFor)

        [<Fact>]
        member self.``Let's 'Spawn' cell and receive back 'Neighborhood' messages`` () =
            let cellRef = spawn system "Cell" <| cellActorCont

            cellRef <! Spawn(0, 0)

            self.ExpectMsgAllOf(waitFor,
                                Neighborhood((0, 0), Occupied), 
                                Neighborhood((+1, +1), Unknown),
                                Neighborhood((+1, +0), Unknown),
                                Neighborhood((+1, -1), Unknown),
                                Neighborhood((+0, -1), Unknown),
                                Neighborhood((-1, -1), Unknown),
                                Neighborhood((-1, +0), Unknown),
                                Neighborhood((-1, +1), Unknown),
                                Neighborhood((+0, +1), Unknown)) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating none 'Neighborhood' messages will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating single 'Neighborhood' message with 'Occupied' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating single 'Neighborhood' message with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages all with 'Unknown' status will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating two 'Neighborhood' messages one with 'Unknown' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages all with 'Unknown' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating three 'Neighborhood' messages one with 'Occupied' status will result in new cell``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Occupied)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            system.Shutdown()

        [<Fact>]
        member self.``Aggregating four 'Neighborhood' messages will not result in nothing``() =
            let aggregateRef = spawn system "Aggregate" <| aggregateActorCont

            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! Neighborhood((0, 0), Unknown)
            aggregateRef <! AggregationCompleted

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Coordinator spawns cells and they sends back 'Neighborhood' messages``() =
            let coordinatorRef = spawn system "Coordinator" <| coordinatorActorCont

            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(3, 3)
            coordinatorRef <! SpawnCompleted

            self.ExpectMsg(AggregationStarted (9 * 2) (*, timeout *)) |> ignore

            self.ExpectMsgAllOf(waitFor, 
                                Neighborhood((0 + 0, 0 + 0), Occupied),
                                Neighborhood((0 + 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 1, 0 + 0), Unknown), 
                                Neighborhood((0 + 1, 0 - 1), Unknown), 
                                Neighborhood((0 + 0, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 + 0), Unknown), 
                                Neighborhood((0 - 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 0, 0 + 1), Unknown), 

                                Neighborhood((3 + 0, 3 + 0), Occupied),
                                Neighborhood((3 + 1, 3 + 1), Unknown), 
                                Neighborhood((3 + 1, 3 + 0), Unknown), 
                                Neighborhood((3 + 1, 3 - 1), Unknown), 
                                Neighborhood((3 + 0, 3 - 1), Unknown), 
                                Neighborhood((3 - 1, 3 - 1), Unknown), 
                                Neighborhood((3 - 1, 3 + 0), Unknown), 
                                Neighborhood((3 - 1, 3 + 1), Unknown), 
                                Neighborhood((3 + 0, 3 + 1), Unknown)) |> ignore 

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Coordinator spawns only unique cells``() =
            let coordinatorRef = spawn system "Coordinator" <| coordinatorActorCont

            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! Spawn(0, 0)
            coordinatorRef <! SpawnCompleted

            self.ExpectMsg(AggregationStarted 9 (*, timeout *)) |> ignore

            self.ExpectMsgAllOf(waitFor,
                                Neighborhood((0 + 0, 0 + 0), Occupied),
                                Neighborhood((0 + 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 1, 0 + 0), Unknown), 
                                Neighborhood((0 + 1, 0 - 1), Unknown), 
                                Neighborhood((0 + 0, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 - 1), Unknown), 
                                Neighborhood((0 - 1, 0 + 0), Unknown), 
                                Neighborhood((0 - 1, 0 + 1), Unknown), 
                                Neighborhood((0 + 0, 0 + 1), Unknown)) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates no 'Neighborhood' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 0

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates single 'Neighborhood' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 1
            collectorRef <! Neighborhood((0, 0), Occupied)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates two times same 'Neighborhood' one with 'Occupied' and spawns back new cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 2
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates two times same 'Neighborhood' both 'Unknow' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 2
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates three times same 'Neighborhood' one with 'Occupied' and spawns back new cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 3
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates three times same 'Neighborhood' both 'Unknow' and spawns back new cell``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 3
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectMsg(Spawn(0, 0), timeout) |> ignore

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates four times same 'Neighborhood' one with 'Occupied' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 4
            collectorRef <! Neighborhood((0, 0), Occupied)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()

        [<Fact>]
        member self.``Collector aggregates four times same 'Neighborhood' both 'Unknow' and has nothing to do``() =
            let collectorRef = spawn system "Collector" <| collectorActorCont

            collectorRef <! AggregationStarted 4
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)
            collectorRef <! Neighborhood((0, 0), Unknown)

            self.ExpectNoMsg(waitFor)

            system.Shutdown()