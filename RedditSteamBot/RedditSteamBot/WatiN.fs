namespace RedditSteamBot

module WatiN =
    open System
    open WatiN.Core
    open WatiN.Core.Constraints
    open FSharpx.Stm.Core
    open FSharpx.Functional.State


    let evalState m s = m s |> fst
    let execState m s = m s |> snd

    let inline dispose (x:^a) = ( ^a : (member Dispose: unit -> unit) (x))
    let inline closeBrowser() = state {
        let! (browser ) = getState
        return dispose browser 
        }

    let inline runScriptAndClose (browser : ^b) (script:State<'a,^b>) = 
        let closeScript = state {
          let! outp = script
          do! closeBrowser()
          return outp
          }
        closeScript (browser) |> fst
    
    let runScriptFox (script:State<'a,FireFox>) =
        runScriptAndClose (new FireFox()) script

    let runScriptIE (script:State<'a,Browser>) =
        runScriptAndClose (new IE() :> Browser) script

    let inline goTo (x:^a) url = ( ^a : (member GoTo: string -> unit) (x,url)) 
    let inline openPage (url:string) = state {
        let! browser = getState
        return goTo browser url 
        }

    let inline textField x finder = ( ^a : (member TextField: 'b -> 'c) (x,finder)) 
    let inline findTextField finder = state {
        let! browser = getState
        return textField browser finder
        }

    let inline div x finder = ( ^a : (member Div: Constraint -> Div) (x,finder)) 
    let inline findDiv finder = state {
        let! browser = getState
        return div browser finder
        }
     
    let findDivs (finder:Constraint) = state {
        let! (brower : Browser) = getState
        return List.ofSeq brower.Divs
        }

    let findSpan (finder:Constraint) = state {
        let! (brower : Browser) = getState
        return brower.Span(finder)
        }

    let clearText (textBox : TextField) = state {
        return textBox.Clear()
        }
   
    let setText (textBox : TextField) (text : string) = state {
        do textBox.Clear()
        return textBox.TypeText(text) 
        }

    let enterText (textBox : TextField) (text : string) = state {
        let! (browser : Browser) = getState
        return textBox.TypeText(text) 
        }

    let containsText (text : string) = state {
        let! (browser : Browser) = getState
        return browser.ContainsText(text) 
        }

    let clickButton (buttonText : string) = state {
        let! (browser : Browser) = getState
        return browser.Button(Find.ByName(buttonText)).Click() 
        }

    let click (elem:Element) = state {
        let! (browser : Browser) = getState
        do elem.Click()
        }