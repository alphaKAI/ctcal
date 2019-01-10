module PrintUtil

open MyUtil

let ( /!= ) a b = a := !a / b
let color_convert_from_hex_to_rgb (hex_col_str : string) =
  let hex_col_str = "0x" + if hex_col_str.[0] = '#' then String.slice hex_col_str 1 0
                           else hex_col_str
  let color_of_int = ref (int hex_col_str)
  let b = !color_of_int % 256 in color_of_int /!= 256
  let g = !color_of_int % 256 in color_of_int /!= 256
  let r = !color_of_int
  [r; g; b]

type Color (col_str : string) =
  let col = color_convert_from_hex_to_rgb col_str
  member this.Col = col
  member this.convert_to_hex () = sprintf "#%02X%02X%02X" (List.item 0 col) (List.item 1 col) (List.item 2 col)

let style_RESET         = 1 <<< 0
let style_BOLD          = 1 <<< 1
let style_WEAKEN        = 1 <<< 2
let style_ITALIC        = 1 <<< 3
let style_UNDERSCORE    = 1 <<< 4
let style_SLOW_BLINK    = 1 <<< 5
let style_FAST_BLINK    = 1 <<< 6
let style_INVERT        = 1 <<< 7
let style_INVISIBLE     = 1 <<< 8
let style_STRIKETHROUGH = 1 <<< 9

let get_string_width = Unicode.east_asian_width

let str_repeat =
  let rec str_repeat_impl ret i str t =
    if i < t then str_repeat_impl (ret + str) (i + 1) str t
    else ret
  str_repeat_impl "" 0

(*
width文字分を半角スペースで埋めて右寄せにして返す
str: 対象の文字列
width: 半角基準の幅
*)
let rjust str width = (str_repeat " " (width - (get_string_width str))) + str

(*
width文字分を半角スペースで埋めて左寄せにして返す
str: 対象の文字列
width: 半角基準の幅
*)
let ljust str width = str + (str_repeat " " (width - (get_string_width str)))

let center_impl ljust str width =
  let tmp = width - get_string_width str
  let (ll, lr) = (str_repeat " " (tmp / 2), str_repeat " " (tmp - tmp / 2))
  if ljust then
    ll + str + lr
  else
    lr + str + ll

let center = center_impl true

let center_rjust = center_impl false

let change_style_impl style_opts conky =
  if has_opt style_opts style_RESET then
    if conky then
      printf "${color}${font}"
    else
      printf "\x1b[0m"

  let cmd = ref "\x1b["
  [
    style_RESET;
    style_BOLD;
    style_WEAKEN;
    style_ITALIC;
    style_UNDERSCORE;
    style_SLOW_BLINK;
    style_FAST_BLINK;
    style_INVERT;
    style_INVISIBLE;
    style_STRIKETHROUGH;
  ] |> List.iteri (fun i opt ->
      if has_opt style_opts opt then
        cmd := !cmd + (sprintf "%d;" i))

  let cmd = String.slice !cmd 0 (-1)

  printf "%sm" cmd

let change_style style_opts = change_style_impl style_opts false

let change_style_conky style_opts = change_style_impl style_opts true

let change_color_impl weaken conky (col : Color) =
  let color = col.Col
  let k = if weaken then 0.6 else 1.0
  let (r, g, b) = (List.item 0 color |> float, List.item 1 color |> float, List.item 2 color |> float)
  let (r, g, b) = (r * k |> int, g * k |> int, b * k |> int)
  let cmd =
    if conky then
      Printf.sprintf "${color %s}" (col.convert_to_hex ())
    else
      Printf.sprintf "\x1b[38;2;%d;%d;%dm" r g b
  in
  printf "%s" cmd

let change_color: Color -> unit = change_color_impl false false
let change_color_weaken: Color -> unit = change_color_impl true false
let change_color_conky: Color -> unit = change_color_impl false true
let change_color_conky_weaken: Color -> unit = change_color_impl true true
