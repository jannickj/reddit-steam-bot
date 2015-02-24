namespace RedditSteamBot

module WatiN =
    open System
    open WatiN.Core
    open WatiN.Core.Constraints
    open FSharpx.Stm.Core
    open FSharpx.Functional.Async

    type IOWatinBrowser = { bs : Browser } with
        member x.Dispose()                              = async { do x.bs.Dispose() }
        member x.ClosePage()                            = async { do x.bs.Close() }
        member x.RefreshPage()                          = async { do x.bs.Refresh() }
        member x.OpenPage (url:string)                  = async { do x.bs.GoTo(url) }
        member x.FindTextField (finder:Constraint)      = async { return x.bs.TextField( finder )}
        member x.FindDiv  (finder:Constraint)           = async { return x.bs.Div (finder)}
        member x.Divs                                   = async { return x.bs.Divs }
        member x.FindSpan (finder:Constraint)           = async { return x.bs.Span finder }
        member x.Click (elem : Element)                 = async { do elem.Click() }
        member x.ClickButton (finder:Constraint)        = async { do x.bs.Button(finder).Click() }
        member x.ClearText (textField : TextField)      = async { do textField.Clear() }
        member x.EnterText (textField : TextField,text) = async { do textField.TypeText text }
        member x.ContainsText (text :string)            = async { return x.bs.ContainsText text }

    let toIOBrowser browser = { bs = browser }

    //Browser type class
    let inline dispose browser  = ( ^browser : (member  Dispose: unit -> Async<unit>) (browser))

    let inline closePage browser = ( ^browser : (member ClosePage: unit -> Async<unit>) (browser))

    let inline refreshPage browser = ( ^browser : (member RefreshPage: unit -> Async<unit>) (browser))

    let inline openPage browser url = ( ^browser : (member OpenPage: string -> Async<unit>) (browser,url))

    let inline findTextField browser finder = ( ^browser : (member FindTextField: Constraint -> Async<TextField>) (browser,finder)) 

    let inline findDiv browser finder = ( ^browser : (member FindDiv: Constraint -> Async<Div>) (browser,finder)) 

    let inline divs browser = ( ^browser : (member Divs: Async<DivCollection>) (browser)) 

    let inline findSpan browser finder = ( ^browser : (member FindSpan: Constraint -> Async<Span>) (browser,finder)) 

    let inline click browser elem = ( ^browser : (member Click: Element -> Async<unit>) (browser,elem))

    let inline clickButton browser finder = ( ^browser : (member ClickButton: Constraint -> Async<Button>) (browser,finder)) 

    let inline clearText browser textField = ( ^browser : (member ClearText: TextField -> Async<unit>) (browser,textField)) 

    let inline enterText browser textField text = ( ^browser : (member EnterText: TextField -> string -> Async<unit>) browser,textField,text) 

    let inline containsText browser text = ( ^browser : (member ContainsText: string -> Async<bool>) (browser,text))

    let inline setText browser textBox (text : string) = async {
        do! clearText browser textBox
        do! enterText browser textBox text
        }


    let createFireFox  =
       async { return { bs = new FireFox()} }
    
    let createIE  =
       async { return { bs = new IE()} }

    
    

   