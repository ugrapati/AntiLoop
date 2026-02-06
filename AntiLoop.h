// AntiLoop.h: Sleep(0) error correction DLL (c) 2012 by Sean O'Neil

#ifndef _ANTILOOP_DLL_H_
#define _ANTILOOP_DLL_H_

#pragma once

#if defined(_WIN64)||defined(AMD64)||defined(_M_AMD64)
  #define __amd64__
#endif

#if BUILDING_DLL
  #define DLLIMPORT __declspec(dllexport)
#else
  #define DLLIMPORT __declspec(dllimport)
#endif

typedef   signed __int8               i8;
typedef   signed __int16             i16;
typedef   signed __int32             i32;
typedef   signed __int64             i64;
typedef unsigned __int8               u8;
typedef unsigned __int16             u16;
typedef unsigned __int32             u32;
typedef unsigned __int64             u64;
#define I8s                    sizeof(i8)
#define I16s                  sizeof(i16)
#define I32s                  sizeof(i32)
#define I64s                  sizeof(i64)
#define IXXs                  sizeof(ixx)
#define U8s                    sizeof(u8)
#define U16s                  sizeof(u16)
#define U32s                  sizeof(u32)
#define U64s                  sizeof(u64)
#define UXXs                  sizeof(uxx)
#define U8(_x)                 ((u8)(_x))
#define U16(_x)               ((u16)(_x))
#define U32(_x)               ((u32)(_x))
#define U64(_x)               ((u64)(_x))
#define UXX(_x)               ((uxx)(_x))
#define I64(_x)               ((i64)(_x))
#define PU8(_x)               ((u8*)(_x))
#define PU16(_x)             ((u16*)(_x))
#define PU32(_x)             ((u32*)(_x))
#define PU64(_x)             ((u64*)(_x))
#define PUXX(_x)             ((uxx*)(_x))
#define PI64(_x)             ((i64*)(_x))
#define PI8P(_x)             ((i8**)(_x))
#define PI16P(_x)           ((i16**)(_x))
#define PI32P(_x)           ((i32**)(_x))
#define PIXXP(_x)           ((ixx**)(_x))
#define PI64P(_x)           ((i64**)(_x))
#define PU8P(_x)             ((u8**)(_x))
#define PU16P(_x)           ((u16**)(_x))
#define PU32P(_x)           ((u32**)(_x))
#define PU64P(_x)           ((u64**)(_x))
#define PUXXP(_x)           ((uxx**)(_x))
#define PI64P(_x)           ((i64**)(_x))
#define PTO(_x,_n)         (PU8(_x)+(_n))
#define U8V(_x)                (*PU8(_x))
#define U16V(_x)              (*PU16(_x))
#define U32V(_x)              (*PU32(_x))
#define U64V(_x)              (*PU64(_x))
#define I64V(_x)              (*PI64(_x))
#define UXXV(_x)              (*PUXX(_x))
#define PI8V(_x)              (*PI8P(_x))
#define PI16V(_x)            (*PI16P(_x))
#define PI32V(_x)            (*PI32P(_x))
#define PIXXV(_x)            (*PIXXP(_x))
#define PI64V(_x)            (*PI64P(_x))
#define PU8V(_x)              (*PU8P(_x))
#define PU32V(_x)            (*PU32P(_x))
#define PU64V(_x)            (*PU64P(_x))
#define PUXXV(_x)            (*PUXXP(_x))
#define U8O(_x,_n)        U8V(PTO(_x,_n))
#define U16O(_x,_n)      U16V(PTO(_x,_n))
#define U32O(_x,_n)      U32V(PTO(_x,_n))
#define U64O(_x,_n)      U64V(PTO(_x,_n))
#define UXXO(_x,_n)      UXXV(PTO(_x,_n))
#define I64O(_x,_n)      I64V(PTO(_x,_n))
#define PU8O(_x,_n)      PU8V(PTO(_x,_n))
#define PU32O(_x,_n)    PU32V(PTO(_x,_n))
#define PU64O(_x,_n)    PU64V(PTO(_x,_n))
#define PUXXO(_x,_n)    PUXXV(PTO(_x,_n))
#define PI64O(_x,_n)    PI64V(PTO(_x,_n))
#define NULL                       UXX(0)
#define null                       UXX(0)
#define NTIOs                sizeof(NTIO)
#define NTUs                  sizeof(NTU)
#define OATs    (sizeof(OAT)-sizeof(NTU))
#define PNTCID(_x)         ((NTCID*)(_x)) // CLIENT_ID
#define PNTIO(_x)           ((NTIO*)(_x)) // IO_STATUS_BLOCK
#define PNTU(_x)             ((NTU*)(_x)) // UNICODE_STRING
#define POAT(_x)             ((OAT*)(_x)) // OBJECT_ATTRIBUTES + NAME
#define BAD                        NXX(1) // our "INVALID_XX_BIT_VALUE"
#define BAD32                      N32(1) // our "INVALID_32_BIT_VALUE"
#define BAD64                      N64(1) // our "INVALID_64_BIT_VALUE"
#define BADHV             PU8(UXX(BAD32)) // our "INVALID_HANDLE_VALUE"; not exactly -1; hisGOOD/hisBAD check for 0, -1 and this garbage
#ifdef __amd64__
  #define BITSIZE                       64
  typedef   signed __int64              ixx,*pixx; // my signed size_t = i64 on x64,  IntPtr in .NET; size_t? WTF??? what kind of incompetent sadistic pervert came up with that name? was it the same idiot who called x32 x86?
  typedef unsigned __int64              uxx,*puxx; // my        size_t = u64 on x64, UIntPtr in .NET; size_t? WTF??? what kind of incompetent sadistic pervert came up with that name? was it the same idiot who called x32 x86?
  #define PEB                           PU8(__readgsqword(96))
  #define TEB                           PU8(__readgsqword(48))
  #define PID(teb)                      U32O(teb,64)
  #define TID(teb)                      U32O(teb,72)
  #define NT_PROCESS_DATA_BLOCK(peb)    (PU8(peb))
  #define NT_PROCESS_STARTUP_DATA(pdb)  (PU8O(pdb,32))
  #define NT_PROCESS_HEAP(pdb)          (PU8O(pdb,48))
  #define NT_MODULE_NAME(psd)           (PNTU(PTO(psd,96)))
  #define NT_COMMAND_LINE(psd)          (PNTU(PTO(psd,112)))
#else
  #define BITSIZE                       32
  typedef   signed __int32              ixx,*pixx;
  typedef unsigned __int32              uxx,*puxx;
  #define PEB                           PU8(__readfsdword(48))
  #define TEB                           PU8(__readfsdword(24))
  #define PID(teb)                      U32O(teb,32)
  #define TID(teb)                      U32O(teb,36)
  #define NT_PROCESS_DATA_BLOCK(peb)    (PU8(peb))
  #define NT_PROCESS_STARTUP_DATA(pdb)  (PU8O(pdb,16))
  #define NT_PROCESS_HEAP(pdb)          (PU8O(pdb,24))
  #define NT_MODULE_NAME(psd)           (PNTU(PTO(psd,56)))
  #define NT_COMMAND_LINE(psd)          (PNTU(PTO(psd,64)))
#endif
// my NT UNICODE_STRING
typedef struct _NTU{u16 l,m,*s;}NTU; // 8/16 bytes; ms UNICODE_STRING
// my NT CLIENT_ID
typedef struct _NTCID{uxx p,t;}NTCID; // 8-16; process ID & thread ID
// my NT OBJECT_ATTRIBUTES
typedef struct _OAT
{ uxx l;        // Length = sizeof(OAT)-sizeof(NTU)
  const u8* r;  // RootDirectory
  const NTU*n;  // ObjectName
  uxx a;        // Attributes
  const u8* d;  // SecurityDescriptor
  const u8* q;  // SecurityQualityOfService
  NTU v;        // ObjectName storage included - must not be counted in the length: use sizeof(OAT)-sizeof(NTU)
} OAT;
typedef struct _NTIO{uxx nt;uxx re,zz;}NTIO; // IO_STATUS_BLOCK
typedef u32(__stdcall*NT_THREAD_PROC)(u8*ctx);
typedef void(__stdcall*NT_APC)(u8*ctx,NTIO*io,uxx res); // PIO_APC_ROUTINE

#define OBJ_INS             0x00000040 // OBJECT_ATTRIBUTES - Attributes OBJ_CASE_INSENSITIVE
#define NT_FOREVER          U32(-1)
#define NT_UM_APC           0x000000C0 // STATUS_USER_APC
#define NT_COMPLETE         0x000000FF // STATUS_ALREADY_COMPLETE
#define NT_KM_APC           0x00000100 // STATUS_USER_APC
#define NT_ALERTED          0x00000101 // STATUS_ALERTED
#define NT_TIMEOUT          0x00000102 // STATUS_TIMEOUT
#define NT_PENDING          0x00000103 // STATUS_PENDING
#define NP_DISC             0xC00000B0 // STATUS_PIPE_DISCONNECTED
#define NP_CLOS             0xC00000B1 // STATUS_PIPE_CLOSING
#define NP_CONN             0xC00000B2 // STATUS_PIPE_CONNECTED
#define NP_LIST             0xC00000B3 // STATUS_PIPE_LISTENING
#define NP_BROK             0xC000014B // STATUS_PIPE_BROKEN
#define NT_PTERM                0x0001
#define NT_PCRTH                0x0002
#define NT_PSSID                0x0004
#define NT_PVMOP                0x0008
#define NT_PVMRD                0x0010
#define NT_PVMWR                0x0020
#define NT_PDUPH                0x0040
#define NT_PCRPR                0x0080
#define NT_PSQUO                0x0100
#define NT_PSINF                0x0200
#define NT_PQINF                0x0400
#define NT_PSURE                0x0800
#define NT_PQLIN                0x1000
#define NT_PSLIN                0x2000
#define NT_RD_DATA            0x000001 // Desired Access - file & pipe = FILE_READ_DATA
#define NT_WR_DATA            0x000002 // Desired Access - file & pipe = FILE_WRITE_DATA
#define NT_CR_PIPE            0x000004 // Desired Access - named pipe = FILE_CREATE_PIPE_INSTANCE
#define NT_RD_ATT             0x000080 // Desired Access - all = FILE_READ_ATTRIBUTES
#define NT_WR_ATT             0x000100 // Desired Access - all = FILE_WRITE_ATTRIBUTES
#define NT_STDR               0x020000 // Desired Access = STANDARD_RIGHTS_READ = STANDARD_RIGHTS_WRITE = STANDARD_RIGHTS_EXECUTE = READ_CONTROL
#define NT_STDR_REQ           0x0F0000 // Desired Access = STANDARD_RIGHTS_REQUIRED
#define NT_SYNC_ACC           0x100000 // Desired Access = SYNCHRONIZE
#define NT_STDR_ALL           0x1F0000 // Desired Access = STANDARD_RIGHTS_ALL
#define NT_ALL_ACC            0x1F01FF // Desired Access = FILE_ALL_ACCESS
#define NT_FULL_ACC           0x1FFFFF // Desired Access = FILE_FULL_ACCESS
#define NT_PALL_ACC           0x1F01DD // STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFDD
#define NT_PFUL_ACC           0x1FDDDD // Desired Access = FILE_FULL_ACCESS
#define NT_GEN_ALL          0x10000000 // Desired Access = GENERIC_ALL
#define NT_GEN_EX           0x20000000 // Desired Access = GENERIC_EXECUTE
#define NT_GEN_WR           0x40000000 // Desired Access = GENERIC_WRITE
#define NT_GEN_RD           0x80000000 // Desired Access = GENERIC_READ
#define SHA_NO              0 // no sharing
#define SHA_RD              1 // Sharing: read
#define SHA_WR              2 // Sharing: write
#define SHA_RW              3 // Sharing: read/write
#define SHA_DE              4 // Sharing: delete
#define NTCF_SUP            0 // CreateNamedPipe Disposition: supersede? supersede??? FILE_SUPERSEDE (always fail?)
#define NTCF_ALW            1 // CreateNamedPipe Disposition: open       fail         FILE_OPEN (only if it exists)
#define NTCF_CRE            2 // CreateNamedPipe Disposition: fail       create       FILE_CREATE (only if it doesn't exist)
#define NTCF_OEX            3 // CreateNamedPipe Disposition: open       create       FILE_OPEN_IF (always open)
#define NTCF_OVW            4 // CreateNamedPipe Disposition: overwrite  fail         FILE_OVERWRITE (only if it exists)
#define NTCF_TRU            5 // CreateNamedPipe Disposition: overwrite  create       FILE_OVERWRITE_IF (always create)
#define NT_ASYNC            0x00U // CreateNamedPipe Options = default
#define NT_THRU             0x02U // CreateNamedPipe Options = FILE_WRITE_THROUGH
#define NT_SEQ_ONLY         0x04U // NtOpenFile Options = FILE_SEQUENTIAL_ONLY
#define NT_SYNC_ALERT       0x10U // CreateNamedPipe Options = FILE_SYNCHRONOUS_IO_ALERT
#define NT_SYNC             0x20U // CreateNamedPipe Options = FILE_SYNCHRONOUS_IO_NONALERT
#define NT_AS_FILE          0x40U // NtOpenFile Options = FILE_NON_DIRECTORY_FILE
#define NP_MSG              1 // CreateNamedPipe pipeType: message, readMode: message
#define NP_NOR              2 // CreateNamedPipe pipeType: reject remote clients
#define NT_NOT_SUSP         0
#define NT_CREATE_SUSP      1
#define DLL_PROCESS_DET     0
#define DLL_PROCESS_ATT     1
#define DLL_THREAD_ATT      2
#define DLL_THREAD_DET      3
#define PM_REMOVE           1 // PeekMessage RemoveMsg option
#define PM_NOYIELD          2 // PeekMessage RemoveMsg option

#define IOCC_PIPE_CONN      0x110008 // ConnectNamedPipe
#define IOCC_PIPE_DISC      0x110004 // DisconnectNamedPipe

// TODO: NtCreateDebugObject
// TODO: NtDebugActiveProcess

__declspec(dllimport) i32 __stdcall LdrGetProcedureAddressForCaller(const uxx dllHandle,const NTU*name,const u32 fl,u8*const retAddr);
__declspec(dllimport) i32 __stdcall LdrGetDllHandleByName(const NTU*name,const uxx a0,uxx*const dllHandle);
__declspec(dllimport) i32 __stdcall NtDelayExecution(const u32 alertable,i64*const timeOutInNegNS);
__declspec(dllimport) i32 __stdcall NtOpenProcess(uxx*const procHandle,const u32 accessMask,const OAT*const obj,NTCID*const cid);
__declspec(dllimport) i32 __stdcall NtProtectVirtualMemory(const uxx procHandle,u8**const address,const uxx*const bytes,const u32 protection,u32*const oldProtection);
__declspec(dllimport) i32 __stdcall NtUserMsgWaitForMultipleObjectsEx(const u32 count,const uxx*const handles,const u32 timeOutInMS,const u32 wakeMask,const u32 flags);
__declspec(dllimport) i32 __stdcall NtUserPeekMessage(u8*const msg,const uxx winHandle,const u32 lo,const u32 hi,const u32 removeMsg,const u32 isA); // isA = {PeekMessageW:0,PeekMessageA:1}
__declspec(dllimport) i32 __stdcall NtWaitForSingleObject(const uxx objHandle,const u32 alertable,const i64*const timeOutInNegNS);
__declspec(dllimport) i32 __stdcall NtWaitForMultipleObjects(const uxx nObj,const uxx*const handles,const u8 dontWaitAll,const u32 alertable,const i64*const timeOutInNegNS);
__declspec(dllimport) i32 __stdcall NtYieldExecution(void);
__declspec(dllimport) i32 __stdcall RtlQueryPerformanceCounter(u64*const count); // NTDLL, Windows 10; hook it?
__declspec(dllimport) u32 __stdcall GetTickCount(void); // KERNEL32
__declspec(dllimport) u64 __stdcall GetTickCount64(void); // KERNEL32
__declspec(dllimport) i32 __stdcall QueryPerformanceCounter(u64*const count); // KERNEL32; the one to hook
__declspec(dllimport) i32 __stdcall NtCreateThreadEx(u32*const tid,const uxx access,OAT*const oat,uxx const processHandle,NT_THREAD_PROC threadProc,uxx const ctx,const uxx suspend,const uxx opts_0,const uxx stackSize,const uxx stackRes,uxx*const tnf);
__declspec(dllimport) i32 __stdcall NtTerminateThread(u32 const tid,u32 const err);
__declspec(dllimport) i32 __stdcall NtCreateNamedPipeFile(uxx*pipeHandle,
  const u32 access, // 0x40180000 = read (owned), 0x80180000 = write (owned), 0xC0180000 = r/w (owned)
  OAT*const oat, // always \??\Pipe\...
  NTIO*const io, // returns 0,0 or error
  const u32 share, // SHA_RD for read pipe, SHA_WR for write pipe, SHA_RW for r/w pipe
  const u32 disp, // NTCF_CRE
  const u32 opt, // NT_SYNC|NT_THRU
  const u32 pipeType, // 1 = write msg, 2 = no remote
  const u32 readMode, // 1 = read msg
  const u32 completionMode, // must be 0; PIPE_NOWAIT is no good
  const u32 maxInst, // 255 = no limit
  const u32 inbQuota, // typically, 4K
  const u32 outQuota, // typically, 4K
  u64*const timeOut);
__declspec(dllimport) i32 __stdcall NtFsControlFile(uxx const fileHandle,uxx const evt,NT_APC const apc,uxx const ctx,NTIO*const io,const uxx iocc,u8*const ib,const uxx ibl,u8*const ob,const uxx obl);
__declspec(dllimport) i32 __stdcall NtReadFile(uxx const fileHandle,uxx const evt,NT_APC const apc,uxx const ctx,NTIO*const io,u8*const b,const uxx nBytes,const u64*const filePos,const u8*const key_0);
__declspec(dllimport) i32 __stdcall NtWriteFile(uxx const fileHandle,uxx const evt,NT_APC const apc,uxx const ctx,NTIO*const io,u8*const b,const uxx nBytes,const u64*const filePos,const u8*const key_0);
__declspec(dllimport) i32 __stdcall NtClose(uxx const handle);
__declspec(dllimport) i32 __stdcall NtOpenFile(uxx*const fileHandle,const uxx access,OAT*const oat,NTIO*const io,const uxx share,const uxx opts);
__declspec(dllimport) u32 __stdcall GetEnvironmentVariableW(const u16*const name,u16*const buf,const u32 size);
extern i32 __stdcall _DllMainCRTStartup(uxx handle,u32 reason,uxx zPtr);
extern unsigned __int64 __rdtsc(void);
extern __declspec(noreturn) void __fastfail(const unsigned long);
#define hisBAD(_h)                _hisBAD(PU8(_h))
#define hisGOOD(_h)              _hisGOOD(PU8(_h))
static __forceinline u32 __cdecl  _hisBAD(u8*const h){uxx g=UXX(h)+1U;return(!U32(g)||g<=1U);} //  either -1, 0  or 0x00000000FFFFFFFF
static __forceinline u32 __cdecl _hisGOOD(u8*const h){uxx g=UXX(h)+1U;return( U32(g)&&g> 1U);} // neither -1, 0 nor 0x00000000FFFFFFFF
// #define freecopy(_to,_fro,_n) _freecopy(PU16(_to),PU16(_fro),UXX(_n))
// static __forceinline u8* __cdecl _freecopy(u8*const to,const u8*const fro,const uxx n){__movsb(to,fro,n);return to;} // if there is no intrinsic memcpy
extern void* __cdecl memcpy(void*const to,const void*const fro,const uxx n);
#pragma intrinsic(memcpy,__debugbreak,_disable,_enable,__indword,__nop,__readmsr,__readpmc,__ud2,__movsb,__stosb,__stosd,__stosw,_BitScanForward,_BitScanReverse,__popcnt16,__popcnt,_lrotl,_lrotr,_rotl,_rotr,_mm_crc32_u8,_mm_crc32_u16,_mm_crc32_u32,_byteswap_ulong,_interlockedadd,_InterlockedXor,_ReturnAddress,__rdtsc,__fastfail) // now why isn't there a simple "#pragma intrinsic EVERYTHING"???
#define movsb(_to,_x,_n)         __movsb(PU8(_to), PU8(_x),UXX(_n)) // copies n bytes
#define movsw(_to,_x,_n)         __movsw(PU16(_to),PU16(_x),UXX(_n)) // copies n words
#define movsd(_to,_x,_n)         __movsd(PU32(_to),PU32(_x),UXX(_n)) // copies n dwords
#define stosb(_to,_x,_n)         __stosb(PU8(_to),   U8(_x),UXX(_n)) // stamps n bytes
#define stosw(_to,_x,_n)         __stosw(PU16(_to), U16(_x),UXX(_n)) // stamps n words
#define stosd(_to,_x,_n)         __stosd(PU32(_to), U32(_x),UXX(_n)) // stamps n dwords
#define pop32(_x)               __popcnt(U32(_x))
#define rol32(_x,_n)              _lrotl(U32(_x),I32(_n))
#define ror32(_x,_n)              _lrotr(U32(_x),I32(_n))
#define bswap32(x)       _byteswap_ulong(U32(x))

#define  SL0_CNT      0 // value index; 8 bytes follow
#define  PMS_CNT      1 // value index; 8 bytes follow
#define  MWM_CNT      2 // value index; 8 bytes follow
#define  GTC_CNT      3 // value index; 8 bytes follow
#define  QPC_CNT      4 // value index; 8 bytes follow

#define  SL0_REQ      5 // value index; 8 bytes follow
#define  PMS_REQ      6 // value index; 8 bytes follow
#define  MWM_REQ      7 // value index; 8 bytes follow
#define  GTC_REQ      8 // value index; 8 bytes follow
#define  QPC_REQ      9 // value index; 8 bytes follow

#define  SL0_FIX     10 // value index; 8 bytes follow
#define  PMS_FIX     11 // value index; 8 bytes follow
#define  MWM_FIX     12 // value index; 8 bytes follow
#define  GTC_FIX     13 // value index; 8 bytes follow
#define  QPC_FIX     14 // value index; 8 bytes follow

#define  SEND_DATA  255 // send back all the values
#define  DATA_SIZE   78 // 8 qwords + 2 dwords + 6 bytes

#endif
