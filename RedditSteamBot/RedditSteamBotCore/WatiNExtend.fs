namespace RedditSteamBotCore
open System
open WatiN.Core
open WatiN.Core.Constraints
open FSharpx.State

module WatiNExtend =


    let evalState m s = m s |> fst
    let execState m s = m s |> snd

    let runScript (script:State<'a,Browser>) =
      evalState script (new IE() :> Browser)

    let openPage (url:string) = state {
        let! (browser : Browser) = getState
        return browser.GoTo(url) 
        }
    let findTextField (finder:Constraint) = state {
        let! (brower : Browser) = getState
        return brower.TextField(finder)
        }
    
    let findDiv (finder:Constraint) = state {
        let! (brower : Browser) = getState
        return brower.Div(finder)
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

    let closePage = state {
        let! (browser : Browser) = getState
        return browser.Dispose() 
        }
    let click (elem:Element) = state {
        return elem.Click()
        }