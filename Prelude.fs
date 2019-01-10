[<AutoOpen>]
module internal Prelude

module internal List =
  let fold_map f init t =
    let acc = ref init
    let result =
      List.map (fun x ->
        let new_acc, y = f !acc x
        acc := new_acc
        y) t
    !acc, result

module OrderedCollectionCommon =
  let normalize length_fun t i = if i < 0 then i + length_fun t else i
  let slice length_fun sub_fun t start stop =
    let stop = if stop = 0 then length_fun t else stop in
    let pos = normalize length_fun t start in
    let len = normalize length_fun t stop - pos in
    sub_fun t pos len

module internal String =
  let count str f =
    let c = ref 0
    String.iter (fun ch -> if f ch then c := !c + 1) str
    !c

  let sub (src:string) pos len = src.[pos..pos+len-1]

  let slice t start stop =
    OrderedCollectionCommon.slice String.length sub t start stop
  let split separator (s:string) =
    let values = ResizeArray<_>()
    let rec gather start i =
      let add () = s.Substring(start,i-start) |> values.Add
      if i = s.Length then add()
      elif s.[i] = '"' then inQuotes start (i+1)
      elif s.[i] = separator then add(); gather (i+1) (i+1)
      else gather start (i+1)
    and inQuotes start i =
      if s.[i] = '"' then gather start (i+1)
      else inQuotes start (i+1)
    gather 0 0
    values.ToArray()

module internal Array =
  let sub (src: 'a []) pos len = src.[pos..pos+len-1]

  let slice t start stop =
    OrderedCollectionCommon.slice Array.length sub t start stop

