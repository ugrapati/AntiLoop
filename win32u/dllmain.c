#include "..\AntiLoop.h"

extern i32 __stdcall NtUserMsgWaitForMultipleObjectsEx(u32 n,uxx*h,u32 ms,u32 wm,u32 fl){return 0;}
extern i32 __stdcall NtUserPeekMessage(uxx msg,uxx hw,u32 lo,u32 hi,u32 rm,u32 isA){return 0;}

extern u32 __stdcall DllMain(uxx hModule,u32 ul_reason_for_call,uxx lpReserved){return 1;}
