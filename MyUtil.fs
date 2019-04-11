module MyUtil

let rec range i j =
  if i > j then [] else
    i :: (range (i + 1) j)

let (%%) = Printf.sprintf

let read_lines_into_string = System.IO.File.ReadAllLines

let has_opt opts opt = opts &&& opt <> 0

// Copyright 2015 Mårten Rånge
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
module DynamicJson =
    open System
    open Newtonsoft.Json.Linq

    exception JsonCastException of Type

    type Json =
        | JsonValue of JValue
        | JsonObject of JObject
        | JsonArray of JArray
        | JsonMissingProperty of JToken * string
        | JsonIndexOutOfRange of JToken * int
        | JsonUnrecognizedType of obj

        static member AsJson(o : obj) =
            match o with
            | :? JObject as jobj -> JsonObject jobj
            | :? JArray as jarr -> JsonArray jarr
            | :? JValue as jval -> JsonValue jval
            | _ -> JsonUnrecognizedType o

        member x.Path =
            match x with
            | JsonValue v -> v.Path
            | JsonObject o -> o.Path
            | JsonArray a -> a.Path
            | JsonMissingProperty(t, key) -> t.Path + "." + key
            | JsonIndexOutOfRange(t, idx) -> sprintf "%s.[%d]" t.Path idx
            | JsonUnrecognizedType v -> "<NULL>"

        override x.ToString() =
            match x with
            | JsonValue v -> sprintf "Value: %s" <| v.ToString()
            | JsonObject o -> sprintf "Object: %s" <| o.ToString()
            | JsonArray a -> sprintf "Array: %s" <| a.ToString()
            | JsonMissingProperty(t, key) ->
                sprintf "Missing property: %s.%s" t.Path key
            | JsonIndexOutOfRange(t, idx) ->
                sprintf "Out of range: %s.[%d]" t.Path idx
            | JsonUnrecognizedType v -> sprintf "Value: %s" <| v.ToString()

        static member (?) (json : Json, key : string) : Json =
            match json with
            | JsonValue jval -> JsonMissingProperty(jval, key)
            | JsonArray jarr -> JsonMissingProperty(jarr, key)
            | JsonObject jobj ->
                let p = jobj.Property key
                if p <> null then Json.AsJson p.Value
                else JsonMissingProperty(jobj, key)
            | _ -> json

        member x.Item(idx : int) : Json =
            match x with
            | JsonValue jval -> JsonIndexOutOfRange(jval, idx)
            | JsonObject jobj -> JsonIndexOutOfRange(jobj, idx)
            | JsonArray jarr ->
                if idx < 0 then JsonIndexOutOfRange(jarr, idx)
                elif idx >= jarr.Count then JsonIndexOutOfRange(jarr, idx)
                else Json.AsJson jarr.[idx]
            | _ -> x

        member x.HasValue =
            match x with
            | JsonValue jval -> true
            | _ -> false

        member x.CastTo<'T>() =
            match x with
            | JsonValue jval -> jval.ToObject<'T>()
            | JsonIndexOutOfRange(t, idx) ->
                raise (IndexOutOfRangeException(x.ToString()))
            | _ -> raise (InvalidCastException())

    let asJson (o : obj) = Json.AsJson o
    let hasValue (json : Json) = json.HasValue
    let castTo<'T> (json : Json) = json.CastTo<'T>()
    let describe (json : Json) = json.ToString()
