module KdbDriver

open System
open System.Net.Http
open FSharp.Data
open AngleSharp.Html.Parser
open MyUtil.DynamicJson

let getTerm() =
    let today = DateTime.Today
    let m = today.Month
    let y = today.Year
    if m < 3 then y - 1
    else y

let http_post url data =
    async {
        let client = new HttpClient()
        return! Http.AsyncRequestString(url, body = FormValues data)
    }
    |> Async.RunSynchronously

let make_map_of_id_and_facility_about_classes class_ids =
    let count_of_classes = List.length class_ids
    let url = "https://kdb.tsukuba.ac.jp"
    let query = String.concat " " class_ids

    let req_params =
        [ "pageId", "SB0070"
          "action", "search"
          "txtFy", getTerm() |> string
          "cmbTerm", ""
          "cmbDay", ""
          "cmbPeriod", ""
          "hdnOrg", ""
          "hdnReq", ""
          "hdnFac", ""
          "hdnDepth", ""
          "chkSyllabi", "false"
          "chkAuditor", "false"
          "txtSyllabus", query
          "reschedule", "true"
          "page", "0"
          "total", "-1" ]

    let json = http_post url req_params |> asJson

    let doc =
        let elem = json?lists |> describe
        let parser = HtmlParser()
        parser.ParseDocument(elem)
    Seq.zip (doc.QuerySelectorAll("tbody > tr > td:nth-child(1)"))
        (doc.QuerySelectorAll("tbody > tr > td:nth-child(8)"))
    |> Seq.map
           (fun (e1, e2) ->
           e1.TextContent.Replace("<\/p><\/td>\\n", ""),
           e2.TextContent.Replace("<\/p><\/td>\\n", ""))
    |> Map.ofSeq
