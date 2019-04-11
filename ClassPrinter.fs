module ClassPrinter

type class_entry =
    { id : string
      name : string
      teacher : string
      facility : string }

type class_entry_for_vis =
    { id : string
      names : string List
      teacher : string
      facility : string }

type class_ =
    | Unregistered
    | Registered of class_entry

let day_prefixes = [ "月"; "火"; "水"; "木"; "金" ]
let day_suffix = "曜日"
let count_of_class_per_day = 6

let unregisterd =
    { id = "未登録"
      name = ""
      teacher = ""
      facility = "" }

let unregisterd_vis =
    { id = "未登録"
      names = []
      teacher = ""
      facility = "" }

open PrintUtil
open System.Globalization

let color_UNK = new Color "#FF0A70"
let color_RED = new Color "#F06060"
let color_BLUE = new Color "#17ABEB"
let color_WHITE = new Color "#FFFFFF"
let color_YELLOW = new Color "#FFC235"

(*
  listの要素がlen未満のとき，padで要素を埋める．
  appendがtrue(default)のとき，末尾にうめ，falseのとき，先頭に埋める
*)
let padding_list_with_impl append list len pad =
    if List.length list < len then
        if append then
            List.append list
                ([ 0..(len - List.length list - 1) ] |> List.map (fun _ -> pad))
        else
            List.append
                ([ 0..(len - List.length list - 1) ] |> List.map (fun _ -> pad))
                list
    else list

let padding_list_with = padding_list_with_impl true
let padding_list_with_prepend : string list -> int -> string -> string list =
    padding_list_with_impl false

(*
  与えられたclass_entryのclass_entry.nameについて max_widthの長さで折り返し，max_name_lines行分だけ改行を入れたclass_entryを返す
*)
let tarnsform_crass_entry_to_vis max_width max_name_lines =
    function
    | Unregistered ->
        let uv = unregisterd_vis
        { id = uv.id
          names =
              padding_list_with uv.names max_name_lines
                  (PrintUtil.str_repeat " " max_width)
          teacher = PrintUtil.str_repeat " " max_width
          facility = PrintUtil.str_repeat " " max_width }
    | Registered class_ ->
        let splits =
            let rec splits_impl ret i j text max_width =
                if String.length text <= i then ret |> List.rev
                else
                    (let sub_str =
                         if i = j then ""
                         else String.slice text j i
                     if Unicode.east_asian_width sub_str >= max_width - 1
                        || text.[i] = ' ' then
                         if Unicode.east_asian_width sub_str
                            <> String.length sub_str then
                             splits_impl (i :: ret) (i + 2) i text max_width
                         else splits_impl (i :: ret) (i + 1) i text max_width
                     else splits_impl ret (i + 1) j text max_width)
            splits_impl [] 0 0

        let (_, names) =
            List.append (splits class_.name max_width) [ 0 ]
            |> List.fold_map (fun p c -> (c, String.slice class_.name p c)) 0
        let names = padding_list_with names max_name_lines ""
        { id = class_.id
          names = names
          teacher = class_.teacher
          facility = class_.facility }

let chmax rv v =
    if !rv < v then rv := v

let sep_line width = str_repeat "-" (width * 5 + 6)

let print_classes (classes : class_ list list) max_width =
    let sep_line = sep_line max_width
    printfn "%s" sep_line
    List.iteri (fun i day_prefix ->
        let day = day_prefix + day_suffix
        let day_str = PrintUtil.center day max_width
        if i = 0 then printf "|"
        printf "%s|" day_str) day_prefixes
    printfn ""
    printfn "%s" sep_line
    let max_name_lines =
        let max_name_lines = ref (-1)
        for i = 0 to 29 do
            let x = i % 6
            let y = i / 6

            let c =
                classes.[x].[y]
                |> function
                | Unregistered -> unregisterd
                | Registered class_ -> class_

            let name = c.name
            chmax max_name_lines
                (max (Unicode.east_asian_width name / max_width + 1)
                     (String.count name (fun c -> c = ' ')))
        !max_name_lines
    List.map
        (fun sub_List ->
        List.map
            (fun class_ ->
            tarnsform_crass_entry_to_vis max_width max_name_lines class_)
            sub_List) classes
    |> List.iter (fun weekly_classes ->
           let ids = List.map (fun class_ -> class_.id) weekly_classes
           let nameses = List.map (fun class_ -> class_.names) weekly_classes
           let teachers = List.map (fun class_ -> class_.teacher) weekly_classes
           let facilities =
               List.map (fun class_ -> class_.facility) weekly_classes
           List.iteri (fun i id ->
               if i = 0 then printf "|"
               PrintUtil.change_color color_BLUE
               printf "%s" (PrintUtil.center id max_width)
               PrintUtil.change_style style_RESET
               printf "|") ids
           printfn ""
           [ 0..(max_name_lines - 1) ]
           |> List.iter (fun i ->
                  let t_names =
                      List.map (fun names -> List.item i names) nameses
                  List.iteri (fun j name ->
                      if j = 0 then printf "|"
                      PrintUtil.change_color color_RED
                      printf "%s" (PrintUtil.center name max_width)
                      PrintUtil.change_style style_RESET
                      printf "|") t_names
                  printfn "")
           List.iteri (fun i teacher ->
               if i = 0 then printf "|"
               PrintUtil.change_color color_YELLOW
               printf "%s" (PrintUtil.center teacher max_width)
               PrintUtil.change_style style_RESET
               printf "|") teachers
           printfn ""
           List.iteri (fun i facility ->
               if i = 0 then printf "|"
               PrintUtil.change_color color_WHITE
               printf "%s" (PrintUtil.center facility max_width)
               PrintUtil.change_style style_RESET
               printf "|") facilities
           printfn "\n%s" sep_line)
