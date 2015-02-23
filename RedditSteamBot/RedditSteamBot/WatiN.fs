namespace RedditSteamBot

module WatiN =
    open System
    open WatiN.Core
    open WatiN.Core.Constraints
    open FSharpx.Stm.Core
    open FSharpx.Functional.State



    let inline dispose (x:^a) = ( ^a : (member Dispose: unit -> unit) (x))
    let inline closeBrowser() = state {
        let! (browser ) = getState
        do dispose browser 
        }

    let inline close (x:^a) = ( ^a : (member Close: unit -> unit) (x))
    let inline closePage() = state {
        let! (browser ) = getState
        do close browser 
        }
    
    let inline refresh (x:^a) = ( ^a : (member Refresh: unit -> unit) (x))
    let inline refreshPage () = state {
        let! (browser ) = getState
        do refresh browser 
        }


    let inline runScriptAndClose (browser : ^b) (script:State<'a,^b>) = 
        let closeScript = state {
          let! outp = script
          do! closeBrowser()
          return outp
          }
        closeScript (browser) |> fst
    
    let runScriptFox (script:State<'a,_>) =
        runScriptAndClose (new FireFox()) script

    let runScriptIE (script:State<'a,Browser>) =
        runScriptAndClose (new IE() :> Browser) script

    let inline goTo (x:^a) url = ( ^a : (member GoTo: string -> unit) (x,url)) 
    let inline openPage (url:string) = state {
        let! browser = getState
        return goTo browser url 
        }

    let inline textField x finder = ( ^a : (member TextField: Constraint -> TextField) (x,finder)) 
    let inline findTextField finder = state {
        let! browser = getState
        return textField browser finder
        }

    let inline div x finder = ( ^a : (member Div: Constraint -> Div) (x,finder)) 
    let inline findDiv finder = state {
        let! browser = getState
        return div browser finder
        }
    
    let inline divs x = ( ^a : (member Divs: DivCollection) (x)) 
    let inline downloadDivs()  = state {
        let! browser = getState
        return List.ofSeq <| divs browser
        }

    let inline span x finder = ( ^a : (member Span: Constraint -> Span) (x,finder)) 
    let inline findSpan (finder:Constraint) = state {
        let! browser = getState
        return span browser finder
        }

    let inline clear (x:^a) = ( ^a : (member Clear: unit -> unit) (x)) 
    let inline clearText textBox = state {
        do clear textBox
        }
    
    let inline typeText (x:^a) url = ( ^a : (member TypeText: string -> unit) (x,url)) 
    let inline enterText textBox (text : string) = state {
        do typeText textBox text 
        }

    let inline setText textBox (text : string) = state {
        do! clearText textBox
        do! enterText textBox text
        }

    
    

    let inline hasText (x:^a) text = ( ^a : (member ContainsText: string -> bool) (x,text))
    let inline containsText (text : string) = state {
        let! browser = getState
        return hasText browser text 
        }

    let inline clickIt (x:^a) = ( ^a : (member Click: unit -> unit) (x))
    let inline click elem = state {
        do clickIt elem
        }
     
    let inline button x finder = ( ^a : (member Button: Constraint -> Button) (x,finder)) 
    let inline clickButton finder = state {
        let! browser = getState
        let but =  button browser finder
        do! click but
        return but
        }

   