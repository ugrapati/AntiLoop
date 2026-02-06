I started this project in 2012 and only used it as a DLL without any interface for myself to be able to play Skyrim and other videogames on my two-core CPU.
I added a GUI to it in 2021 to try to make it usable for others. While I was doing it, I discovered that all .NET apps have the same error in them.

What is this about? - it prevents endless looping of Sleep(0) and PeekMessage() which waste huge amounts of CPU time and power.

Microsoft is responsible for the biggest programming error of all time teaching it to all software developers in Visual Studio Help:

    Microsoft still recommends to use PeekMessage() in an endless loop.

Proof at https://learn.microsoft.com/en-us/windows/win32/winmsg/using-messages-and-message-queues

    while (PeekMessage(&msg, hwnd,  0, 0, PM_REMOVE)) 
    { 
        switch(msg.message) 
        { 
            case WM_LBUTTONDOWN: 
            case WM_RBUTTONDOWN: 
            case WM_KEYDOWN: 
                // 
                // Perform any required cleanup. 
                // 
                fDone = TRUE; 
        } 
    } 
    
Sleep(0) worked on NT4 by releasing the CPU. With single-core Intel CPUs, this looping actually kept the thread asleep most of the time.
But starting with Windows XP and multi-core CPUs, it doesn't work anymore. It has the exact opposite effect. Both functions return immediately and keep the CPU occupied.

The correct message loop must consist of two threads:

    1. WaitMessage() looped - sleeping until the next Windows message
    2. NtDelayExecution(0,&ticks64) - sleeping until the next timing event

Nobody does it correctly. Among the worst offenders are: QT4/QT5 and .NET

They both do everything in a single thread. .NET only sleeps for some time between PeekMessage() calls if the dialog does not have any timing events in it.
If it does, it loops wasting huge amounts of CPU time and performing hundreds of thousands, even millions of calls to get the current time every second.



Current issues:

    1. Lack of explanation of what is what. I need to add some balloons to explain everything. People who don't know what it does, can't figure it out easily.
    2. Lack of proper signature. Some processes are started requiring signature and for some reason, my certificate is not accepted - the DLLs do not get loaded into those processes.
    3. 64-bit version of taskhostw.exe hangs on logon if PeekMessage is intercepted. I haven't tried to figure out why.

I have no money to pay for a proper certificate and I can't do it without knowing first that it will work.
It's better to require a signature for all the DLLs in AppInit_DLLs. RequireSignedAppInit_DLLs should be set to 1. It doesn't work yet.
