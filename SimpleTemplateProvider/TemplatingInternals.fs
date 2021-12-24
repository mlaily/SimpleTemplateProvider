﻿// $begin{copyright}
//
// This file is part of Bolero
//
// Copyright (c) 2018 IntelliFactory and contributors
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}

/// [omit]
module Bolero.TemplatingInternals

open System

/// This indirection resolves two problems:
/// 1. TPs can't generate delegate constructor calls;
/// 2. Generative TPs have problems with `_ -> unit`, see https://github.com/fsprojects/FSharp.TypeProviders.SDK/issues/279
type Events =

    static member NoOp<'T>() =
        Action<'T>(ignore)

    static member OnChange(f: Action<string>) =
        Action<EventArgs>(fun e ->
            f.Invoke(unbox<string> "")
        )

    static member OnChangeInt(f: Action<int>) =
        Events.OnChange(fun s ->
            match Int32.TryParse(s) with
            | true, x -> f.Invoke(x)
            | false, _ -> ()
        )

    static member OnChangeFloat(f: Action<float>) =
        Events.OnChange(fun s ->
            match Double.TryParse(s) with
            | true, x -> f.Invoke(x)
            | false, _ -> ()
        )

    static member OnChangeBool(f: Action<bool>) =
        Action<EventArgs>(fun e ->
            f.Invoke(unbox<bool> true)
        )

type TemplateNode() =
    /// For internal use only.
    member val Holes : obj[] = null with get, set

[<AllowNullLiteral>]
type IClient =
    /// subtemplate is null to request the full file template.
    abstract RequestTemplate : filename: string * subtemplate: string -> option<Map<string, obj> -> Node>
    abstract SetOnChange : (unit -> unit) -> unit
    abstract FileChanged : filename: string * content: string -> unit

module TemplateCache =
    let mutable client =
        { new IClient with
            member this.RequestTemplate(_, _) = None
            member this.SetOnChange(_) = ()
            member this.FileChanged(_, _) = ()
        }

//#if !IS_DESIGNTIME
//[<assembly:FSharp.Core.CompilerServices.TypeProviderAssembly "Bolero.Templating.Provider">]
//do ()
//#endif
