namespace RedditSteamBot
module SteamBot = 
    open System
    open WatiN.Core
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

    let inline postCuration brow group gameTitle tagline link = async {
        do! openPage brow <| newCurationLink group

        let! appField = findTextField brow <| Find.ById appFieldId
        do! setText brow appField (escapeChars gameTitle) 
        let! suggestDiv = findDiv brow <| Find.ById "game_select_suggestions"
        do appField.Click()
        do  suggestDiv.WaitUntil(fun (d:Div) -> d.Divs.Count > 0)
        let suggestions : Div seq = seq suggestDiv.Divs
        let editDist = (String.editDist gameTitle) << divText
        let bestSuggest = Seq.minBy editDist suggestions
        let bestTitle = bestSuggest.Text
        do bestSuggest.Click()

        let! linkField = findTextField brow <| Find.ById linkFieldId
        do! setText brow linkField link


        let! recommendField = findTextField brow <| Find.ById recommendFieldId
        do! setText brow recommendField tagline

        let! div = findDiv brow <| Find.ByClass acceptPostButtonClass
        do!  click brow div
        }

    let inline deleteCuration brow group gameId = async {
        do! openPage brow <| appCurationLink group gameId

        let! controls = findDiv brow <| Find.ByClass "panel owner"
        let delLink = controls.Link(Find.ByText "Delete this recommendation")
        let! delSpan = findSpan brow <| Find.ByText "Delete recommendation"
        do! click brow delLink
        do! click brow delSpan
        }

    let inline editCuration brow group gameId tagline link = async {
        do! openPage brow <| editCurationLink group gameId

        let! tagLineField = findTextField brow <| Find.ById recommendFieldId
        do! setText brow tagLineField tagline

        let! linkField = findTextField brow <| Find.ById linkFieldId
        do! setText brow linkField link

        let! div = findDiv brow <| Find.ByClass acceptEditButtonClass
        do! click brow div

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

    let inline readPage brow group = async {
        let! div = findDiv brow <| Find.ById "RecommendedAppsRows"
        let divL = List.ofSeq <| div.ChildrenOfType<Div>()
        let cDiv (d:Div) = d.Div(Find.ByClass("curation_app_block_body"))
        let divBodyL = List.map cDiv divL
        return List.choose (readBlock group) divBodyL
        }

    let inline isLast brow = async {
        let! span = findSpan brow <| Find.ById "RecommendedApps_btn_next"
        let! activePage = findSpan brow <| Find.ByClass("RecommendedApps_paging_pagelink active")

        return not activePage.Exists || not span.Exists || span.ClassName = "pagebtn disabled"
        }
   
    let inline nextPage brow = async {
        let! span = findSpan brow <| Find.ById "RecommendedApps_btn_next"
        let! activePage = findSpan brow <| Find.ByClass("RecommendedApps_paging_pagelink active")
        let oldText = activePage.Text
        do! click brow span
        do activePage.WaitUntil(fun (s:Span) -> s.Text <> oldText)
        }


    let inline readAllPages brow group  = 
        let rec read group = async {
            let! isLast = isLast brow 
            let! page = readPage brow group
            if isLast then
                return page
            else
                do! nextPage brow
                let! rest = read group
                return page @ rest
        }
        read group

     
    let inline readAllSteamRecommends brow group = async {
        do! openPage brow <| curationLink group
        do! refreshPage brow
        let! recommends = readAllPages brow group
        return recommends
        }
       