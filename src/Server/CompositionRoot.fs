﻿module Server.CompositionRoot

open Shared
open Server
open FsToolkit.ErrorHandling
open System

module Result =
    let bindAsync fAsync argResult =
        match argResult with
        | Ok arg -> fAsync arg |> Async.map Ok
        | Error err -> Error err |> Async.retn

let getStatus() =
    let now = DateTime.Now
    now.ToString("yyyyMMdd HH:mm:ss") |> sprintf "Ok at %s"

// services
let chickenStore = Database.ChickenStore (Database.ConnectionString.create "Data Source=database.db")

// workflows

let getAllChickens date = Workflows.getAllChickens chickenStore date

let addEgg (chicken, date) = Workflows.addEgg chickenStore chicken date

let removeEgg (chicken, date) = Workflows.removeEgg chickenStore chicken date

let api : IChickensApi =
    { AddEgg = addEgg
      RemoveEgg = removeEgg }
