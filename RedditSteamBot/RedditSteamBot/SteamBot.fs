namespace RedditSteamBot
module SteamBot = 
    open System
    open WatiN.Core
    open FSharpx.Functional.State
    open WatiN
    open JSLibraryFSharp
    open FSharpx.Functional.Option
    open JSLibraryFSharp.Type


    //let group = "pcmrcccp"
    let steamSite = "http://steamcommunity.com/groups/" 
    let curationLink group = steamSite + group + "/curation"
    let newCurationLink group = curationLink group + "/new"
    let appCurationLink group id = curationLink group + "/app/" + sprintf "%i" id + "/" 
    let editCurationLink group id = curationLink group + "/edit/" + sprintf "%i" id
    let recommendFieldId = "curationBlurbInput"
    let linkFieldId = "curationURLInput"
    let appFieldId = "curationAppInput"
    let acceptPostButtonClass = "btnv6_green_white_innerfade btn_medium"
    let acceptEditButtonClass = "btn_green_white_innerfade btn_medium"

    type SteamRecommend = { Title : string
                          ; GameId : int
                          ; Link : string
                          ; PostId : string
                          ; TagLine : string
                          }

    let divText (div:Div) = div.Text 

    let escapeChars = 
        String.replace ":" "\\:"

    let inline postCuration group gameTitle tagline link = state {
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

        let! linkField = findTextField <| Find.ById linkFieldId
        do! setText linkField link


        let! recommendField = findTextField <| Find.ById recommendFieldId
        do! setText recommendField tagline

        let! div = findDiv <| Find.ByClass acceptPostButtonClass
        do!  click div
        }

    let inline deleteCuration group gameId = state {
        do! openPage <| appCurationLink group gameId

        let! controls = findDiv <| Find.ByClass "panel owner"
        let delLink = controls.Link(Find.ByText "Delete this recommendation")
        let! delSpan = findSpan <| Find.ByText "Delete recommendation"
        do! click delLink
        do! click delSpan
        }

    let inline editCuration group gameId tagline link = state {
        do! openPage <| editCurationLink group gameId

        let! tagLineField = findTextField <| Find.ById recommendFieldId
        do! setText tagLineField tagline

        let! linkField = findTextField <| Find.ById linkFieldId
        do! setText linkField link

        let! div = findDiv <| Find.ByClass acceptEditButtonClass
        do! click div

        }



    let getIdFromLink group link =
        let s = String.replace (curationLink group + "/app/") "" link
        maybe {
            let! (_,m) = Regex.tryMatch "([0-9]+)" s
            return! Integer.parse m
            }

    let postId link = maybe {
        let! (_,r) = Regex.tryMatch "/comments/.*?/" link
        return String.replace "/comments/" "" r
               |> String.replace "/" ""
        }

    let readBlock group (div : Div) = maybe {
        let dataDiv = div.Div(Find.ByClass("curation_app_block_content"))
        let! link = Seq.tryHead dataDiv.Links
        let! id = getIdFromLink group link.Url
        let appDiv = link.Div(Find.ByClass("curation_app_block_name"))
        let gameTitle = appDiv.InnerHtml.Trim()
        let reviewDiv = dataDiv.Div (Find.ByClass("highlighted_recommendation_link"))
        let! reviewLink = Seq.tryHead reviewDiv.Links
        let reviewLinkUrl = reviewLink.Url
        let! postId = postId reviewLinkUrl
        let taglineDiv = dataDiv.Div (Find.ByClass("curation_app_block_blurb"))
        let tagline = String.skip (taglineDiv.InnerHtml.Trim()) 1
        let tagline' = String.substring tagline 0 (tagline.Length-1)

        return { Title = gameTitle; GameId = id; Link = reviewLinkUrl; PostId = postId; TagLine = tagline' }
        }

    let inline readPage group = state {
        let! div = findDiv <| Find.ById "RecommendedAppsRows"
        let divL = List.ofSeq <| div.ChildrenOfType<Div>()
        let cDiv (d:Div) = d.Div(Find.ByClass("curation_app_block_body"))
        let divBodyL = List.map cDiv divL
        return List.choose (readBlock group) divBodyL
        }

    let inline isLast () = state {
        let! browser = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        let! activePage = findSpan <| Find.ByClass("RecommendedApps_paging_pagelink active")

        return not activePage.Exists || not span.Exists || span.ClassName = "pagebtn disabled"
        }
   
    let inline nextPage () = state {
        let! browser = getState
        let! span = findSpan <| Find.ById "RecommendedApps_btn_next"
        let! activePage = findSpan <| Find.ByClass("RecommendedApps_paging_pagelink active")
        let oldText = activePage.Text
        do! click span
        do activePage.WaitUntil(fun (s:Span) -> s.Text <> oldText)
        }


    let inline readAllPages group  = 
        let rec read group = state {
            let! browser = getState
            let! isLast = isLast () 
            let! page = readPage group
            if isLast then
                return page
            else
                do! nextPage()
                let! rest = read group
                return page @ rest
        }
        read group

    let inline readAllSteamRecommends group = state {
        do! openPage <| curationLink group
        do! refreshPage ()
        let! recommends = readAllPages group
        return recommends
        }
       