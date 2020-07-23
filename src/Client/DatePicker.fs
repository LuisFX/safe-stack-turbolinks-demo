module Client.Datepicker

open System
open Shared
open Fable.Core.JsInterop
open Browser

type private Datepicker = { date : {| start: Nullable<DateTime> |}
                            value : string -> unit
                            refresh : unit -> unit }
                            with
                                member this.SetCurrentDate(date: NotFutureDate) =
                                   this.value(date.ToString())
                                member this.OnSelect(handler) =
                                    this?on("select", handler)



type private IBulmaCalendar =
    abstract attach : string * obj -> Datepicker[]

let private bulmaCalendar : IBulmaCalendar = importDefault "bulma-calendar"

let private options =
    let tomorrow = DateTime.Today.AddDays(1.) //https://github.com/Wikiki/bulma-calendar/issues/211
    {| ``type`` = "date"
       displayMode = "default"
       dateFormat = "YYYY-MM-DD"
       showHeader = false
       showFooter = false
       weekStart = 1
       maxDate = tomorrow
       disabledDates = [| tomorrow |] |}

let mutable private datepicker : Datepicker option = None

let init currentDate =
    let datepickerId = "chickencheck-datepicker"
    let isInitialized = (document.getElementById datepickerId).className.Contains("is-hidden")
    match isInitialized, datepicker with
    | (false, _) ->
        let newDatepicker = (bulmaCalendar.attach("#" + datepickerId, !!options)).[0]
        datepicker <- Some newDatepicker
    | (true, Some d) ->
        d.refresh()
    | (true, None) ->
        eprintf "Invalid datepicker state, no datepicker stored"

    datepicker
    |> Option.iter (fun d ->
        d.SetCurrentDate(currentDate)
        d.OnSelect(fun _ ->
            let date =
                d.date.start
                |> Option.ofNullable
                |> Option.map NotFutureDate.create
                |> Option.defaultValue (NotFutureDate.today())
            if date <> currentDate then
                let dateQueryStr = sprintf "?date=%s" (date.ToString())
                Turbolinks.visit(window.location.pathname + dateQueryStr))
        )
