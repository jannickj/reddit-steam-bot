namespace RedditSteamBotCore
open System
open WatiN.Core
open FSharpx.State
open WatiNExtend
open JSLibraryFSharp
open FSharpx
open FSharpx.Option
open JSLibraryFSharp.Type


module SteamBot = 

    let group = "pcmrcccp"
    let steamSite = "http://steamcommunity.com/groups/" 
    let curationLink = steamSite + group + "/curation"
    let newCurationLink = curationLink + "/new"
    let editCurationLink id = steamSite + group + "/edit/" + sprintf "%i" id
    let recommendFieldId = "curationBlurbInput"
    let appFieldId = "curationAppInput"
    let acceptButtonClass = "btnv6_green_white_innerfade btn_medium"

    type SteamRecommend = { Title : string
                          ; GameId : int
                          }

    let postCuration gameTitle tagline link = state {
        do! openPage <| newCurationLink

        let! appField = findTextField <| Find.ById appFieldId
        do! setText appField gameTitle

        let! recommendField = findTextField <| Find.ById recommendFieldId
        do! setText recommendField tagline

        let! div = findDiv <| Find.ByClass acceptButtonClass
        do! click div

        do! closePage
    }



    let editCuration gameId tagline link = state {
        do! openPage <| editCurationLink gameId

        let! textField = findTextField <| Find.ById recommendFieldId
        do! setText textField tagline

        let! div = findDiv <| Find.ByClass acceptButtonClass
        do! click div

        do! closePage
    }



    let getIdFromLink link =
        let s = String.replace link (curationLink+"/app/") ""
        match Regex.tryMatch "([0-9]+)" s with
        | Some m -> Int32.parse m.MatchValue
        | None -> None

    let readBlock (div : Div) = maybe {
        let dataElem = div.Child(Find.ByClass("curation_app_block_content"))
        let! (dataDiv : Div) = tryCast dataElem
        let! link = Seq.tryHead dataDiv.Links
        let! id = getIdFromLink link.Url
        let appDiv = link.Div(Find.ByClass("curation_app_block_name"))
        let gameTitle = appDiv.InnerHtml.Trim()
        return { Title = gameTitle; GameId = id }
    }

    let readPage = state {
        let! div = findDiv <| Find.ById "RecommendedAppsRows"
        let divL = List.ofSeq <| div.ChildrenOfType<Div>()
        return List.choose readBlock divL
    }

    let isLast = state {
        let! (browser:Browser) = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        return span.ClassName = "pagebtn disabled"
    }
   
    let nextPage = state {
        let! browser = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        do! click span
    }

    let rec readAllPages = state {
        let! isLast = isLast
        let! page = readPage
        if not <| isLast then
            do! nextPage    
            let! rest = readAllPages
            return page @ rest
        else
            return page

    }

    let readAllSteamRecommends = state {
        do! openPage curationLink
        let! recommends = readAllPages
        do! closePage
        return recommends
        
    }