module Program
open FSharp.CommandLine
open MBrace.FsPickler
open ClassPrinter
open DataFetch
open System.IO
open System

let inline writeCredential file credential=
  let binarySerializer = FsPickler.CreateBinarySerializer()
  let pickle = binarySerializer.Pickle credential
  File.WriteAllBytes (file, pickle)

let inline readCredential file =
  if File.Exists file then
    let binarySerializer = FsPickler.CreateBinarySerializer()
    Some (File.ReadAllBytes file |> binarySerializer.UnPickle<TwinsCredential>)
  else
    None

let makeCredential () =
  printfn "s17xxxxxのIDとパスワードを入力してください"
  printf "ID: "
  let id = Console.ReadLine ()
  printfn ""
  printf "PW: "
  let pw = Console.ReadLine ()
  printfn ""
  {id = id; pw = pw}

type Cache = {
  classes: class_ list list
  fetch_date: DateTime
}

let writeCache file cache =
  let binarySerializer = FsPickler.CreateBinarySerializer()
  let pickle = binarySerializer.Pickle cache
  File.WriteAllBytes (file, pickle)

let inline readCache file =
  if File.Exists file then
    let binarySerializer = FsPickler.CreateBinarySerializer()
    Some (File.ReadAllBytes file |> binarySerializer.UnPickle<Cache>)
  else
    None

let widthOption =
  commandOption {
    names ["w"; "width"]
    description "各コマの要素の表示幅を指定(デフォルトはウィンドウの幅で自動的に調節する)"
    takes (format("%d").map (fun width -> width))
  }

let updateOption =
  commandFlag {
    names ["u"; "update"]
    description "キャッシュを強制的に更新する．(デフォルトは前回取得時から30日経過してる場合に更新する)"
  }

let default_width =
  let w = Console.WindowWidth
  (w - 6) / 5

let mainCommand () =
  command {
    name "ctcal"
    description "Class Table Calendar"

    opt width in widthOption |> CommandOption.zeroOrExactlyOne
                             |> CommandOption.whenMissingUse default_width
    opt update in updateOption |> CommandOption.zeroOrExactlyOne
                               |> CommandOption.whenMissingUse false

    do
      let file_path = "setting.bin"
      let credential =
        readCredential file_path
        |> function
        | Some credential -> credential
        | None ->
          let ret = makeCredential ()
          writeCredential file_path ret
          ret

      let cache_path = "timetable.cache"

      let update_cache () =
        printfn "[FETCH DATA] ID: %A" credential.id
        let classes = data_fetch credential
        writeCache cache_path {
          classes=classes
          fetch_date=DateTime.Now
        }
        classes

      let classes =
        if update then
          update_cache ()
        else
          readCache cache_path
          |> function
          | Some cache ->
            let now = DateTime.Now
            (* fetch日時が現時点より30日前ならキャッシュを更新する *)
            if cache.fetch_date.AddDays 30. <= now then
              update_cache ()
            else
              cache.classes
          | None -> update_cache ()

      print_classes classes width
    return 0
  }

[<EntryPoint>]
let main argv =
  mainCommand () |> Command.runAsEntryPoint argv