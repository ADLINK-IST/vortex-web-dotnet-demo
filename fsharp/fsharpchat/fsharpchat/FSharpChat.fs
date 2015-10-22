open System

open Eto.Forms
open Eto.Drawing

open FSharpx.Functional
open FSharpx.Option

open vortex.web

type Post = {
    name: string
    msg:  string }

let getVortex (uri : string, usr : string) : Async<Vortex option>  = async {
    let vortex = new Vortex (0)
    let connected = true
    let! connected = Async.AwaitTask (vortex.Connect (uri, usr))
    if connected then return Some(vortex)
    else return None
}

type ChatApp (uri: string, user: string) =
    let app = new Application()
    let bfont = new Font (family = "Verdana", size = float32 11, style = FontStyle.Bold) 
    let rfont = new Font (family = "Verdana", size = float32 11, style = FontStyle.None)
    let ifont = new Font (family = "Verdana", size = float32 11, style = FontStyle.Italic)
    let sbfont = new Font (family = "Verdana", size = float32 9, style = FontStyle.Bold) 
    let srfont = new Font (family = "Verdana", size = float32 9, style = FontStyle.None)
    let sifont = new Font (family = "Verdana", size = float32 9, style = FontStyle.Italic)
    let black = Color.FromRgb (0x0)
    let gray = Color.FromRgb (0x333333)
    let blue = Color.FromRgb (0x006699)
    let topicName = "Post"
    let qos =
        let plist = new System.Collections.Generic.List<QosPolicy>()
        plist.Add (Reliability.Reliable)
        plist 
    let rqos = qos
    let wqos = qos
   
    let createRW (uri : string) (user : string) : Async<(DataReader<Post> * DataWriter<Post>) option> = async {
        let! vortex = getVortex (uri, user)
        match vortex  with 
            | Some (v) -> 
                let! topic = Async.AwaitTask (v.CreateTopic<Post> (topicName))
                let! dw    = Async.AwaitTask (v.CreateDataWriter<Post>(topic, wqos))
                let! dr    = Async.AwaitTask (v.CreateDataReader<Post>(topic, rqos))
                return Some((dr, dw))
            | None -> 
                printfn "Unable to establish connection to: %A" uri
                return None
    }  
    
    let srw = Async.RunSynchronously (createRW uri user)
    let getRW srw = 
        match srw with
            | Some ((r, w)) -> (r,w)
            | None -> failwith "The application has not been properly initialised"
        
    member self.Reader = fst (getRW srw)
    member self.Writer = snd (getRW srw)

    member self.BoldFont = bfont
    member self.RegularFont = rfont
    member self.ItalicFont = ifont
    member self.SBoldFont = sbfont
    member self.SRegularFont = srfont
    member self.SItalicFont = sifont
    member self.Application = app
    member self.User = user

    member self.Black = black
    member self.Gray = gray
    member self.Blue = blue



let postMessage (messageBoard: RichTextArea) (uFont: Font) (uColor: Color) (mFont: Font) (mColor: Color) (post: Post) =       
    let msg = post.msg + "\n"
    let pos = messageBoard.Text.Length - 2
    let buf = messageBoard.Buffer
    let range = new Range<int>()
    let user = post.name + " >> "
    let nlen = user.Length
    buf.Insert (pos, user)
    buf.Insert (pos + nlen , msg)
    let r1 = range.WithStart(pos).WithEnd(pos + nlen)
    messageBoard.Selection <- (r1)
    messageBoard.SelectionFont <- uFont
    messageBoard.SelectionForeground <- uColor
    let r2 = range.WithStart(pos + 8).WithEnd(pos + 8 + msg.Length)
    messageBoard.Selection <- r2
    messageBoard.SelectionFont <- mFont
    messageBoard.SelectionForeground <- mColor

let mainWindow (capp: ChatApp) =

    let dr = capp.Reader
    let dw = capp.Writer 
    let user = capp.User
    
    let messageBoard = new RichTextArea(Size = new Size(450, 420))
    let postOwnMessage = postMessage messageBoard capp.SBoldFont capp.Blue capp.SItalicFont capp.Gray
    let postOtherMessage = postMessage messageBoard capp.BoldFont capp.Blue capp.RegularFont capp.Gray

    messageBoard.ReadOnly <- true
    let chatText =  new TextArea(Size = new Size(450, 60), AcceptsReturn = false)
    let layout = new DynamicLayout ()  
    layout.BeginVertical() |> ignore
    layout.Add (messageBoard) |> ignore
    layout.Add (chatText) |> ignore
    layout.EndBeginVertical() |> ignore

    
    messageBoard.Font <- capp.RegularFont

    let keyUpHandler = fun (ke: KeyEventArgs) -> 
        if ke.Key.Equals(Keys.Enter) then 
            let post = {name = user; msg = chatText.Text}
            postOwnMessage {name = user; msg = chatText.Text}
            chatText.Text <- ""
            dw.Write (post) |> ignore
                                                    
    chatText.KeyUp.Add (keyUpHandler)
    
    dr.OnDataAvailable.Add (fun sample -> 
        let data = sample.Data
        if data.name <> capp.User then capp.Application.AsyncInvoke( fun () -> postOtherMessage sample.Data))
                    
    new Form(ClientSize = new Size(500, 480), Content = layout, Resizable = false)
    

let runApp uri user =
    printfn "Connecting to: %A as %A" uri user
    let capp = new ChatApp (uri, user)
    let f = mainWindow (capp)
    capp.Application.Run(f)
    
        
[<STAThread>]
[<EntryPoint>] 
let main argv =
    if argv.Length > 1 then runApp argv.[0] argv.[1]
    else 
        printfn "Usage: \n\tfsharpchat <uri> <user>"
        printfn "Example:\n\tfsharpchat ws:\\demo-lab.prismtech.com:9000 guest"
    0
