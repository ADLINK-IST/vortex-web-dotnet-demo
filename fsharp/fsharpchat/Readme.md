## F# Chat Application

This application is a simple chat illustrating how the Vortex Web C#
API can be used from F#.


## Building the Application

### Windows (With MS Tools)

Assuming that you have the Microsoft .net tool chain installed (for
instance Visual Studio Express 2015), then you can build the exaple by
simply typing:

       > MSBuild fsharpchat.sln /t:Rebuild /p:Configuration=Release


### Mono Supported Platforms (Linux, MacOS, Windows)

xbuild is Mono's implementation of MSBuild, thus to build the example
with mono do:

     $ xbuild fsharpchat.sln /t:Rebuild /p:Configuration=Release

You may need to do a few tweaks as described [here](http://www.mono-project.com/archived/porting_msbuild_projects_to_xbuild/). In the future we will provide a [FAKE](http://fsharp.github.io/FAKE/) based build.

## Running the Demo

Once you've build the demo you can run it as follows:

     on-windows> fsharpchat.exe ws://demo-lab.prismtech.com:9000 your-name

     on-mono$ mono fsharpchat.exe ws://demo-lab.prismtech.com:9000 your-name

You can also use the HTML5 chat application available at [http://demo-lab.prismtech.com:8080](http://demo-lab.prismtech.com:8080)