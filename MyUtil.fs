module MyUtil

let rec range i j =
  if i > j then [] else
    i :: (range (i + 1) j)

let (>>) f g x = g (f x)

let (%%) = Printf.sprintf

let read_lines_into_string = System.IO.File.ReadAllLines

let has_opt opts opt = opts &&& opt <> 0