namespace RedditSteamBot
module SteamBot = 
    open System
    open WatiN.Core
    open FSharpx.Functional.State
    open WatiN
    open JSLibraryFSharp
    open FSharpx.Text
    open FSharpx.Functional.Option
    open JSLibraryFSharp.Type


    //let group = "pcmrcccp"
    let steamSite = "http://steamcommunity.com/groups/" 
    let curationLink group = steamSite + group + "/curation"
    let newCurationLink group = curationLink group + "/new"
    let editCurationLink group id = steamSite + group + "/edit/" + sprintf "%i" id
    let recommendFieldId = "curationBlurbInput"
    let appFieldId = "curationAppInput"
    let acceptButtonClass = "btnv6_green_white_innerfade btn_medium"

    type SteamRecommend = { Title : string
                          ; GameId : int
                          }

    let divText (div:Div) = div.Text 

    let escapeChars s = 
        String.replace s ":" "\\:"

    let postCuration group gameTitle tagline link = state {
        do! openPage <| newCurationLink group

        let! appField = findTextField <| Find.ById appFieldId
        do! setText appField (escapeChars gameTitle)
        let! suggestDiv = findDiv <| Find.ById "game_select_suggestions"
        do appField.Click()
        do  suggestDiv.WaitUntil(fun (d:Div) -> d.Divs.Count > 0)
        let suggestions : Div seq = seq suggestDiv.Divs
        let editDist = (String.editDist gameTitle) << divText
        let bestSuggest = Seq.minBy editDist suggestions
        let bestTitle = bestSuggest.Text
        do bestSuggest.Click()


        let! recommendField = findTextField <| Find.ById recommendFieldId
        do! setText recommendField tagline

        let! div = findDiv <| Find.ByClass acceptButtonClass
        do! click div

    }



    let editCuration group gameId tagline link = state {
        do! openPage <| editCurationLink group gameId

        let! textField = findTextField <| Find.ById recommendFieldId
        do! setText textField tagline

        let! div = findDiv <| Find.ByClass acceptButtonClass
        do! click div

    }



    let getIdFromLink group link =
        let s = String.replace link (curationLink group + "/app/") ""
        maybe {
            let! m = Regex.tryMatch "([0-9]+)" s
            return! Integer.parse m.MatchValue
        }

    let readBlock group (div : Div) = maybe {
        let dataDiv = div.Div(Find.ByClass("curation_app_block_content"))
        let! link = Seq.tryHead dataDiv.Links
        let! id = getIdFromLink group link.Url
        let appDiv = link.Div(Find.ByClass("curation_app_block_name"))
        let gameTitle = appDiv.InnerHtml.Trim()
        return { Title = gameTitle; GameId = id }
    }

    let readPage group = state {
        let! div = findDiv <| Find.ById "RecommendedAppsRows"
        let divL = List.ofSeq <| div.ChildrenOfType<Div>()
        let cDiv (d:Div) = d.Div(Find.ByClass("curation_app_block_body"))
        let divBodyL = List.map cDiv divL
        return List.choose (readBlock group) divBodyL
    }

    let isLast = state {
        let! (browser:Browser) = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        return span.ClassName = "pagebtn disabled"
    }
   
    let nextPage = state {
        let! browser = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        let activePage = browser.Span(Find.ByClass("RecommendedApps_paging_pagelink active"))
        let oldText = activePage.Text
        do! click span
        do activePage.WaitUntil(fun (s:Span) -> s.Text <> oldText)
    }

    let rec readAllPages group = state {
        let! isLast = isLast
        let! page = readPage group
        if not <| isLast then
            do! nextPage    
            let! rest = readAllPages group
            return page @ rest
        else
            return page

    }

    let readAllSteamRecommends group = state {
        do! openPage <| curationLink group
        let! recommends = readAllPages group
        return recommends
        
    }