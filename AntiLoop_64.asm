; AntiLoop_64.asm: Sleep(0) error correction DLL (c) 2021 by Sean O'Neil

extrn ntDelayExecution:dword
extrn ntYieldExecution:dword
extrn ntUserPeekMessage:dword
extrn ntUserMsgWaitForMultipleObjectsEx:dword
; extrn ntWaitForMultipleObjects:dword
; extrn ntWaitForSingleObject:dword

public _ntDelayExecution
public _ntYieldExecution
public _ntUserPeekMessage
public _ntUserMsgWaitForMultipleObjectsEx
; public _ntWaitForMultipleObjects
; public _ntWaitForSingleObject

            _text   segment

_ntDelayExecution:
            mov     eax,dword ptr [ntDelayExecution]
            jmp     _sys
_ntYieldExecution:
            mov     eax,dword ptr [ntYieldExecution]
            jmp     _sys
_ntUserPeekMessage:
            mov     eax,dword ptr [ntUserPeekMessage]
_sys:       mov     r10,rcx
            syscall
            ret
_ntUserMsgWaitForMultipleObjectsEx:
            mov     eax,dword ptr [ntUserMsgWaitForMultipleObjectsEx]
            jmp     _sys
; _ntWaitForMultipleObjects:
;           mov     eax,dword ptr [ntWaitForMultipleObjects]
;           jmp     _sys
; _ntWaitForSingleObject:
;           mov     eax,dword ptr [ntWaitForSingleObject]
;           jmp     _sys

            _text   ends
            end
