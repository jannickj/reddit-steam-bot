namespace RedditSteamBot

module WatiN =
    open System
    open WatiN.Core
    open WatiN.Core.Constraints
    open FSharpx.Stm.Core
    open FSharpx.Functional.IO

    type IOWatinBrowser = { bs : Browser } with
        member x.Dispose()                              = io { do x.bs.Dispose() }
        member x.ClosePage()                            = io { do x.bs.Close() }
        member x.RefreshPage()                          = io { do x.bs.Refresh() }
        member x.OpenPage (url:string)                  = io { do x.bs.GoTo(url) }
        member x.FindTextField (finder:Constraint)      = io { return x.bs.TextField( finder )}
        member x.FindDiv  (finder:Constraint)           = io { return x.bs.Div (finder)}
        member x.Divs                                   = io { return x.bs.Divs }
        member x.FindSpan (finder:Constraint)           = io { return x.bs.Span finder }
        member x.Click (elem : Element)                 = io { do elem.Click() }
        member x.ClickButton (finder:Constraint)        = io { do x.bs.Button(finder).Click() }
        member x.ClearText (textField : TextField)      = io { do textField.Clear() }
        member x.EnterText (textField : TextField,text) = io { do textField.TypeText text }
        member x.ContainsText (text :string)            = io { return x.bs.ContainsText text }

    let toIOBrowser browser = { bs = browser }

    //Browser type class
    let inline dispose browser  = ( ^browser : (member  Dispose: unit -> IO<unit>) (browser))

    let inline closePage browser = ( ^browser : (member ClosePage: unit -> IO<unit>) (browser))

    let inline refreshPage browser = ( ^browser : (member RefreshPage: unit -> IO<unit>) (browser))

    let inline openPage browser url = ( ^browser : (member OpenPage: string -> IO<unit>) (browser,url))

    let inline findTextField browser finder = ( ^browser : (member FindTextField: Constraint -> IO<TextField>) (browser,finder)) 

    let inline findDiv browser finder = ( ^browser : (member FindDiv: Constraint -> IO<Div>) (browser,finder)) 

    let inline divs browser = ( ^browser : (member Divs: IO<DivCollection>) (browser)) 

    let inline findSpan browser finder = ( ^browser : (member FindSpan: Constraint -> IO<Span>) (browser,finder)) 

    let inline click browser elem = ( ^browser : (member Click: Element -> IO<unit>) (browser,elem))

    let inline clickButton browser finder = ( ^browser : (member ClickButton: Constraint -> IO<Button>) (browser,finder)) 

    let inline clearText browser textField = ( ^browser : (member ClearText: TextField -> IO<unit>) (browser,textField)) 

    let inline enterText browser textField text = ( ^browser : (member EnterText: TextField -> string -> IO<unit>) browser,textField,text) 

    let inline containsText browser text = ( ^browser : (member ContainsText: string -> IO<bool>) (browser,text))

    let inline setText browser textBox (text : string) = io {
        do! clearText browser textBox
        do! enterText browser textBox text
        }


    let createFireFox  =
       io { return { bs = new FireFox()} }
    
    let createIE  =
       io { return { bs = new IE()} }

    
    

   