// AntiLoop.c: Sleep(0) error correction DLL (c) 2012 by Sean O'Neil

#include "AntiLoop.h"

// TODO: PeekNamedPipe and PeekConsoleInput; other stupid peeks?

typedef struct _CONFIG
{ i64 i64MinSL0;          // choose minimum wait: -1..-100000 (in negative 100ns) works best, must be negative
  volatile u32 u32MinPMS; // choose minimum wait: 1..10 (in ms) works best, must be positive
  volatile u32 u32MinMWM; // choose minimum wait: 1..10 (in ms) works best, must be positive
  i64 i64MinGTC;          // choose minimum wait: -1..-100000 (in negative 100ns) works best, must be negative
  i64 i64MinQPC;          // choose minimum wait: -1..-100000 (in negative 100ns) works best, must be negative
  volatile u8 byteFixSL0; // fix Sleep(0), SleepEx(0) and SwitchToThread() bool
  volatile u8 byteFixPMS; // fix PeekMessage() bool
  volatile u8 byteFixMWM; // fix the rare MsgWaitForMultipleObjectsEx(*,*,0,*,*) bool
  volatile u8 byteFixGTC; // fix GetTickCount()/GetTickCount64() bool
  volatile u8 byteFixQPC; // fix QueryPerformanceCounter() bool
} CONFIG;

static u64 u64CntSL0=0; // Sleep(0), SleepEx(0) and SwitchToThread() calls
static u64 u64CntPMS=0; // PeekMessage() calls
static u64 u64CntMWM=0; // MsgWaitForMultipleObjectsEx(*,*,0,*,*) calls
static u64 u64CntGTC=0; // GetTickCount() and GetTickCount64() calls
static u64 u64CntQPC=0; // QueryPerformanceCounter() calls
static CONFIG $;

extern u32 ntDelayExecution=0,ntYieldExecution=0,ntUserPeekMessage=0,ntUserMsgWaitForMultipleObjectsEx=0;
// ntWaitForMultipleObjects=0,
// ntWaitForSingleObject=0,

#ifdef __amd64__
  extern i32 __stdcall _ntDelayExecution(const u32 al,const i64*const ns);
  extern i32 __stdcall _ntYieldExecution(void);
  extern i32 __stdcall _ntUserPeekMessage(u8*const msg,const uxx hw,const u32 mlo,const u32 mhi,const u32 rm,const u32 v3);
  extern i32 __stdcall _ntUserMsgWaitForMultipleObjectsEx(const u32 n,uxx*const h,const u32 ms,const u32 wm,const u32 fl);
  // extern i32 __stdcall _ntWaitForMultipleObjects(const uxx n,const uxx*const h,const u8 wa,const u32 al,const u64*const ns);
  // extern i32 __stdcall _ntWaitForSingleObject(const u8*const h,const u32 al,const u64*const ns);
#else
  extern u32 wowCall=0;
  static __declspec(naked) i32 __stdcall _ntDelayExecution(const u32 al,const i64*const ns){__asm{mov eax,[ntDelayExecution]}__asm{call dword ptr[wowCall]}__asm{ret 8}}
  static __declspec(naked) i32 __stdcall _ntYieldExecution(void){__asm{mov eax,[ntYieldExecution]}__asm{call dword ptr[wowCall]}__asm{ret}}
  static __declspec(naked) i32 __stdcall _ntUserPeekMessage(u8*const msg,const uxx hw,const u32 mlo,const u32 mhi,const u32 rm,const u32 v3){__asm{mov eax,[ntUserPeekMessage]}__asm{call dword ptr[wowCall]}__asm{ret 24}}
  static __declspec(naked) i32 __stdcall _ntUserMsgWaitForMultipleObjectsEx(const u32 n,uxx*const h,const u32 ms,const u32 wm,const u32 fl){__asm{mov eax,[ntUserMsgWaitForMultipleObjectsEx]}__asm{call dword ptr[wowCall]}__asm{ret 20}}
  // static __declspec(naked) i32 __stdcall _ntWaitForMultipleObjects(const uxx n,const uxx*const h,const u8 wa,const u32 al,const u64*const ns){__asm{mov eax,[ntWaitForMultipleObjects]}__asm{call dword ptr[wowCall]}__asm{ret 20}}
  // static __declspec(naked) i32 __stdcall _ntWaitForSingleObject(const u8*const h,const u32 al,const u64*const ns){__asm{mov eax,[ntWaitForSingleObject]}__asm{call dword ptr[wowCall]}__asm{ret 12}}
#endif

static i32 __stdcall _NtDelayExecution(const u32 al,i64*ns)
{ if(ns&&!*ns){u64CntSL0++;if($.byteFixSL0)ns=&$.i64MinSL0;}return _ntDelayExecution(al,ns);
} // ns = null ptr means "wait forever"
static __forceinline void sleep(const i64 ns) {_ntDelayExecution(0,&ns);}
static i32 __stdcall _NtYieldExecution(void)
{ u64CntSL0++;return $.byteFixSL0?_ntDelayExecution(1,&$.i64MinSL0):_ntYieldExecution();
} // the bullshit SwitchToThread(); use alertable wait here

// TODO: add the option to ignore PM_NOYIELD flag
static i32 __stdcall _NtUserPeekMessage(u8*const msg,const uxx hw,const u32 lo,const u32 hi,const u32 rm,const u32 isA)
{ u64CntPMS++;i32 r=_ntUserPeekMessage(msg,hw,lo,hi,rm,isA);
  if($.byteFixPMS&&!r&&!(rm&PM_NOYIELD))NtUserMsgWaitForMultipleObjectsEx(1,&hw,$.u32MinPMS,0x5FF,6); // wait for any message up to 1 ms instead of just sleep(1)
  return r;
} // alertable msg wait here; this is the worst offender - should sleep here as long as possible; they should have used WaitMessage

static i32 __stdcall _NtUserMsgWaitForMultipleObjectsEx(const u32 n,uxx*const h,const u32 ms,const u32 wm,const u32 fl)
{ return _ntUserMsgWaitForMultipleObjectsEx(n,h,!ms&&(u64CntMWM++,$.byteFixMWM)?$.u32MinMWM:ms,wm,fl);
} // too bad can't wait here like everywhere else - 1 millisecond is too much for too many badly programmed apps; TODO
// static i32 __stdcall _NtWaitForMultipleObjects(uxx n,const uxx*h,u8 wa,u32 al,const i64*ns){return _ntWaitForMultipleObjects(n,h,wa,al,(ns&&!*ns)?&minWaitObj:ns);} // ns = null ptr means "wait forever"; TODO
// static i32 __stdcall _NtWaitForSingleObject(const u8*h,u32 al,const i64*ns){return _ntWaitForSingleObject(h,al,(ns&&!*ns)?&minWaitObj:ns);} // ns = null ptr means "wait forever"; had to disable this one only because of the badly programmed windows 10 logoncontroller.dll abusing it too much - it stalls CTRL-ALT-DEL for a few seconds before the window shows up; TODO

static u64 __stdcall _GetTickCount64(void)
{ u64CntGTC++;if($.byteFixGTC)_ntDelayExecution(1,&$.i64MinGTC); // min sleep here as well
  u64 a=U64V(0x7FFE0320),b=U32V(0x7FFE0328),c=a>>32; // there was never any need to pause there: the change only happens while incrementing the counter
  if(c!=b)a=(((c<b)?b:c)<<32); // when the lower part goes from -1 to 0: simply return max of the two in the upper half, 0 in the lower half
  u32 n=U32V(0x7FFE0004);if(!(n&-(1<<24)))a<<=8;return (a*n)>>24;
} // for now: patch C3 to F3 C3 by hand (pause before return - may free the CPU a little)
#define _GetTickCount _GetTickCount64

static i32 __stdcall _QueryPerformanceCounter(u64*const counter)
{ u64CntQPC++;if($.byteFixQPC)_ntDelayExecution(1,&$.i64MinGTC); // min sleep here as well
  return RtlQueryPerformanceCounter(counter);
} // for now: patch the last jump (FF 25) to push ptr (FF 35) followed by F3 C3 by hand (pause before return - may free the CPU a little)

#define hookLen (UXXs+4) // x32: 8, x64: 12

u8*f[]= // check their order in _DllMainCRTStartup
{ PU8(_NtDelayExecution),
  PU8(_NtYieldExecution),
  PU8(_NtUserPeekMessage),
  PU8(_NtUserMsgWaitForMultipleObjectsEx),
  PU8(_GetTickCount),
  PU8(_GetTickCount64),
  PU8(_QueryPerformanceCounter)
// PU8(_NtWaitForMultipleObjects),
// PU8(_NtWaitForSingleObject),
};
static u64 x0[]={0,0,0,0,0,0,0}; // make sure there are as many x0's as there are f's
#ifdef __amd64__
  static u32 x1[]={0,0,0,0,0,0,0}; // make sure there are as many x1's as there are f's
#endif

// API hook; i=0 must be in NTDLL to set wowCall
static __forceinline void __stdcall hook(const u32 i,u8*const s,u32*const c)
{ u32 p=0;u64 v=U64V(s);x0[i]=v;u8*a=s;uxx hl=hookLen; // NtProtectVirtualMemory destroys it
#ifdef __amd64__
  if(c)*c=U32(v>>32);x1[i]=U32V(s+8);
#else
  if(c)*c=U32(v>>8);if(!i)wowCall=U32V(PU32O(PU8O(s,6),2));
#endif
  NtProtectVirtualMemory(-1,&a,&hl,64,&p);
#ifdef __amd64__
  U16V(s)=0xB848;UXXV(s+2)=UXX(f[i]);U16V(s+2+UXXs)=0xE0FF;
#else
  *s=0xE9;UXXV(s+1)=PU8(f[i])-s-5;
#endif
  a=s;hl=hookLen;NtProtectVirtualMemory(-1,&a,&hl,p,&p);
}

// API unhook
static __forceinline void __stdcall unhk(const u32 i,u8*const s)
{ u32 p=0;u8*a=s;uxx hl=hookLen;
  NtProtectVirtualMemory(-1,&a,&hl,64,&p);U64V(s)=x0[i];
#ifdef __amd64__
  U32V(s+8)=x1[i];
#endif
  a=s;hl=hookLen;NtProtectVirtualMemory(-1,&a,&hl,p,&p);
}

OAT o={.l=OATs,.r=0,.n=&o.v,.a=0x40,.d=0,.q=0,.v={.l=52,.m=52,.s=L"\\??\\Pipe\\AntiLoop-00000000"}};
static u8*peb,*teb;static u32 pid,tid;static u16 s[288];NTIO io; // single-threaded use
uxx tnf[9]={sizeof(tnf),0x10003,16,UXX(&io),0,0x10004,8,UXX(&io),0}; // fixed thread info struct needed to create threads

extern u32 __stdcall pipeLoop(u8*const ctx)
{ uxx h=0;u8 d;i64 timeOut=1LL<<63; // 29247 years
  for(i32 i=0;i<8;i++)d=(pid>>(i*4))&15,o.v.s[25-i]=d+(d<10?0x30:0x37); // set PID.ToString("X8") in the pipe name
  i32 nt=NtCreateNamedPipeFile(&h,NT_SYNC_ACC|NT_STDR_REQ|NT_CR_PIPE|NT_WR_DATA|NT_RD_DATA,&o,&io,SHA_RW,NTCF_CRE,NT_SYNC|NT_THRU,NP_NOR|NP_MSG,NP_MSG,0,1,256,256,&timeOut); // 0x1F0007 = R/W, read msg, write msg, no remote
  u64 fp=0;u8 b[DATA_SIZE];if(!nt&&hisGOOD(h)) // terminated threads have their TEB wiped; main thread dead => time to go
  { while(TID(teb))
    if(!(nt=NtReadFile(h,0,0,0,&io,b,9,&fp,0)))switch(b[0]) // 0..10 = set value; 255 = send back all values
    { case SL0_CNT:u64CntSL0=U64O(b,1);break;
      case PMS_CNT:u64CntPMS=U64O(b,1);break;
      case MWM_CNT:u64CntMWM=U64O(b,1);break;
      case GTC_CNT:u64CntGTC=U64O(b,1);break;
      case QPC_CNT:u64CntQPC=U64O(b,1);break;

      case SL0_REQ:$.i64MinSL0=I64O(b,1);break;
      case PMS_REQ:$.u32MinPMS=U32O(b,1);break;
      case MWM_REQ:$.u32MinMWM=U32O(b,1);break;
      case GTC_REQ:$.i64MinGTC=I64O(b,1);break;
      case QPC_REQ:$.i64MinQPC=I64O(b,1);break;

      case SL0_FIX:$.byteFixSL0=b[1];break;
      case PMS_FIX:$.byteFixPMS=b[1];break;
      case MWM_FIX:$.byteFixMWM=b[1];break;
      case GTC_FIX:$.byteFixGTC=b[1];break;
      case QPC_FIX:$.byteFixQPC=b[1];break;

      case SEND_DATA:
        U64O(b, 0)=u64CntSL0; I64O(b,40)=$.i64MinSL0; b[72]=$.byteFixSL0;
        U64O(b, 8)=u64CntPMS; U32O(b,48)=$.u32MinPMS; b[73]=$.byteFixPMS;
        U64O(b,16)=u64CntMWM; U32O(b,52)=$.u32MinMWM; b[74]=$.byteFixMWM;
        U64O(b,24)=u64CntGTC; I64O(b,56)=$.i64MinGTC; b[75]=$.byteFixGTC;
        U64O(b,32)=u64CntQPC; I64O(b,64)=$.i64MinQPC; b[76]=$.byteFixQPC;
        b[77]=BITSIZE;nt=NtWriteFile(h,0,0,0,&io,b,DATA_SIZE,&fp,0);break;
    } else // any error => disconnect and wait for another connection
    { if(nt=NtFsControlFile(h,0,0,0,&io,IOCC_PIPE_DISC,0,0,0,0))sleep(-1000000);
      if(nt=NtFsControlFile(h,0,0,0,&io,IOCC_PIPE_CONN,0,0,0,0))sleep(-1000000);
      else NtWaitForSingleObject(h,0,0); // wait forever
    } NtClose(h);
  } return 0;
}

extern i32 __stdcall _DllMainCRTStartup(const uxx h,const u32 r,const uxx z)
{ switch(r){case DLL_PROCESS_ATT:{pid=PID(teb=TEB); // get TEB of the main thread and process id
  NTU*mod=NT_MODULE_NAME(NT_PROCESS_STARTUP_DATA(NT_PROCESS_DATA_BLOCK(peb=PEB))); // main module path; no need for any API calls
  u32 n;u16 *c=s+27,*e=mod->s+mod->l/U16s,*x=e;u64 p=0;uxx h=0;
  memcpy(s,L"\\??\\C:\\ProgramData\\AntiLoop",27*U16s); // NOTE: can't add .dat to a file name longer than 252 chars
  while((x>mod->s)&&(*x!=L'\\'))x--;if(*x!=L'\\')*c++='\\'; // file name [255 chars max allowed] + path [27 chars] + ".dat" [4] + trailing \0 [1] = 287, can't exceed the reserved 288
#ifdef __amd64__
  u8 svchost=(U64V(x)==0x6B007300610074&&U64V(x+4)==0x740073006F0068&&U64V(x+8)==0x780065002E0077&&U32V(x+12)==0x65); // is it "taskhostw.exe"?
#endif
  movsw(c,x,e-x);movsw(c+=e-x,L".dat",5);n=U32(PU8(c+4)-PU8(s)); // no security risk here
  OAT oat={.l=OATs,.r=0,.n=&oat.v,.a=OBJ_INS,.d=0,.q=0,.v={U16(n),U16(n),s}}; // C:\ProgramData\AntiLoop\game.exe.dat
  n=!NtOpenFile(&h,NT_RD_ATT|NT_RD_DATA|NT_SYNC_ACC,&oat,&io,SHA_RW,NT_SYNC|NT_SEQ_ONLY|NT_AS_FILE)&&hisGOOD(h);
  if(n)n=!NtReadFile(h,0,0,0,&io,PU8(&$),sizeof($),&p,0)&&!io.nt&&io.re==sizeof($),NtClose(h);
  // if(n=GetEnvironmentVariableW(L"APPDATA",c=s+4,32768)) // C:\Users\User\AppData\Roaming
  // { U64V(s)=0x5C003F003F005C;movsw(c+=n,L"\\AntiLoop",10);c+=9;while((x>mod->s)&&(*x!=L'\\'))x--;
  //   if(*x!=L'\\')*c++='\\';movsw(c,x,e-x);movsw(c+=e-x,L".dat",5);n=U32(PU8(c+4)-PU8(s));
  //   OAT oat={.l=OATs,.r=0,.n=&oat.v,.a=OBJ_INS,.d=0,.q=0,.v={U16(n),U16(n),s}}; // C:\Users\User\AppData\Roaming\AntiLoop\game.exe.dat
  //   n=!NtOpenFile(&h,NT_RD_ATT|NT_RD_DATA|NT_SYNC_ACC,&oat,&io,SHA_RW,NT_SYNC|NT_SEQ_ONLY|NT_AS_FILE)&&hisGOOD(h);
  //   if(n)n=!NtReadFile(h,0,0,0,&io,PU8(&$),sizeof($),&p,0)&&!io.nt&&io.re==sizeof($),NtClose(h);
  // }
  if(!n)
  { $.i64MinSL0=-3000; // choose minimum wait: -1..-100000 (in negative 100ns) works best
    $.u32MinPMS=1; // choose minimum wait: 1..10 (in ms) works best, must be positive
    $.u32MinMWM=1; // choose minimum wait: 1 ms works best, must be positive
    $.i64MinGTC=-1; // choose minimum wait: -1 (in negative 100ns) works best
    $.i64MinQPC=-1; // choose minimum wait: -1 (in negative 100ns) works best
    $.byteFixSL0=1; // fix Sleep(0), SleepEx(0) and SwitchToThread() bool - ENABLED by default
    $.byteFixPMS=!svchost; // fix PeekMessage() bool; it seems to break taskhostw.exe on logon for some reason, at least on x64 (haven't tested it on some old 32-bit Windows); it shouldn't glitch, needs to be looked into - otherwise ENABLED by default
    $.byteFixMWM=0; // fix the rare MsgWaitForMultipleObjectsEx(*,*,0,*,*) bool - DISABLED by default, for now (should be enabled with some exceptions)
    $.byteFixGTC=0; // fix GetTickCount()/GetTickCount64() bool - DISABLED by default, for now (should be enabled with some exceptions)
    $.byteFixQPC=0; // fix QueryPerformanceCounter() bool - DISABLED by default, for now (should be enabled with some exceptions)
  }
  hook(0,PU8(NtDelayExecution),&ntDelayExecution); // 338
  hook(1,PU8(NtYieldExecution),&ntYieldExecution); // 700
  hook(2,PU8(NtUserPeekMessage),&ntUserPeekMessage); // 1101
  hook(3,PU8(NtUserMsgWaitForMultipleObjectsEx),&ntUserMsgWaitForMultipleObjectsEx); // 1088
  hook(4,PU8(GetTickCount),0); // KERNEL32
  hook(5,PU8(GetTickCount64),0); // KERNEL32
  hook(6,PU8(QueryPerformanceCounter),0); // RtlQueryPerformanceCounter instead maybe?
  // ntHook(n+7,PU8(NtWaitForMultipleObjects),&ntWaitForMultipleObjects); // 671
  // ntHook(n+8,PU8(NtWaitForSingleObject),&ntWaitForSingleObject); // 672
  NtCreateThreadEx(&tid,NT_ALL_ACC,0,UXX(-1),pipeLoop,h,NT_NOT_SUSP,0,0,0,tnf);
  break;}case DLL_THREAD_DET:{if(TEB==teb)NtTerminateThread(tid,0);break; // detaching main thread => game over
  }case DLL_PROCESS_DET:{
  // unhk(n+8,PU8(NtWaitForSingleObject));
  // unhk(n+7,PU8(NtWaitForMultipleObjects));
  unhk(6,PU8(QueryPerformanceCounter));
  unhk(5,PU8(GetTickCount64));
  unhk(4,PU8(GetTickCount));
  unhk(3,PU8(NtUserMsgWaitForMultipleObjectsEx));
  unhk(2,PU8(NtUserPeekMessage));
  unhk(1,PU8(NtYieldExecution));
  unhk(0,PU8(NtDelayExecution));
  break;}}return 1;
} // 1 = success, 0 = failure

uxx __security_cookie=UXX(0x7BED1AFD2BAD4AE5LL);
__declspec(noinline) void __cdecl __security_check_cookie(void) {}
__declspec(noinline) void __cdecl __report_securityfailure(const unsigned long failure_code){__fastfail(failure_code);}
__declspec(noreturn) void __cdecl __report_rangecheckfailure(){__report_securityfailure(8);} // FAST_FAIL_RANGE_CHECK_FAILURE
