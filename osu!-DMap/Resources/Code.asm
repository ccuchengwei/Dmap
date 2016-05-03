	option  casemap:none
	.386
	.model  flat,stdcall
include         windows.inc
include         kernel32.inc
include         user32.inc
includelib      kernel32.lib
includelib      user32.lib


__UNICODE__ equ 1




.data
osuName		DB	'osu!',0

hwnd		HWND	?
blnMessage	BOOL	0
blnHook		BOOL	0
WinRect		RECT	<?>
CurPos		POINT	<?>
LinkTitle0	DW	'h','t','t','p',':','/','/',0
LinkTitle1	DW	'h','t','t','p','s',':','/','/',0
WebLink0	DW	'o','s','u','.','p','p','y','.','s','h','/','b','/',0
WebLink1	DW	'o','s','u','.','p','p','y','.','s','h','/','s','/',0
WebLink2	DW	'b','l','o','o','d','c','a','t','.','c','o','m','/','o','s','u','/','m','/',0
WebLink3	DW	'o','s','u','.','p','p','y','.','s','h','/','d','/',0
HttpLink	DW	260 dup(0)

OrgAddr		DWORD	offset OrgFun
ShellExecuteINFO	SHELLEXECUTEINFO	<?,?,?,?,offset exMapLink,?,12345>
exMapLink	DW	'h','t','t','p',':','/','/'
			DW	'o','s','u','.','p','p','y','.','s','h','/','b','/'
			DW	'1','2','3','4','5','6',0
.code
start:
	
	; push	offset	ShellExecuteINFO
	; call	HookSub
	; nop
	; nop
	; nop
	; nop
	; nop
	; nop
	; nop
	; nop
	
	
HookSub:
	push	eax
	push	ebx
	push	ecx
	push	edx
	push	ebp
	mov	ebp,esp
	mov	ebp,dword ptr [ebp+24]
	; invoke  FindWindow, NULL, offset osuName
	; mov	hwnd,eax
	;=====視窗大小=====
	; mov	WinRect.left,0
	; mov	WinRect.top,0
	; mov	WinRect.right,0
	; mov	WinRect.bottom,0
	invoke  GetClientRect, hwnd, offset WinRect	;視窗內框大小
	cmp	WinRect.bottom,0
	je	SUB_END
	;=====滑鼠位址=====
	; mov	CurPos.x,0
	; mov	CurPos.y,0
	invoke  GetCursorPos, offset CurPos
	invoke  ScreenToClient, hwnd, offset CurPos	;螢幕上座標在視窗內部的位置
	;=====滑鼠上限=====
	mov	eax,WinRect.bottom
	mov	ebx,28
	mul	ebx
	mov	ebx,100
	div	ebx
	cmp	CurPos.y,eax
	js	SUB_END
	;=====對話欄=====
	cmp	blnMessage,0
	jne	CheckSTART
	;=====滑鼠下限=====
	mov	eax,WinRect.bottom
	mov	ebx,41
	mul	ebx
	mov	ebx,100
	div	ebx
	cmp	eax,CurPos.y
	js	SUB_END
	;=================
CheckSTART:
	;=====驗證網址=====
CheckTitle0:
	mov	ebx,offset LinkTitle0
	mov	ecx,dword ptr [ebp+16]
CT0loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckLink0
	cmp	ax,word ptr [ecx]
	jne	CheckTitle1
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CT0loop
CheckTitle1:
	mov	ebx,offset LinkTitle1
	mov	ecx,dword ptr [ebp+16]
CT1loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckLink0
	cmp	ax,word ptr [ecx]
	jne	SUB_END
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CT1loop
CheckLink0:
	mov	ebx,offset WebLink0
	mov	edx,ecx
CL0loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckEND
	cmp	ax,word ptr [ecx]
	jne	CheckLink1
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CL0loop
CheckLink1:
	mov	ebx,offset WebLink1
	mov	ecx,edx
CL1loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckEND
	cmp	ax,word ptr [ecx]
	jne	CheckLink2
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CL1loop
CheckLink2:
	mov	ebx,offset WebLink2
	mov	ecx,edx
CL2loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckEND
	cmp	ax,word ptr [ecx]
	jne	CheckLink3
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CL2loop
CheckLink3:
	mov	ebx,offset WebLink3
	mov	ecx,edx
CL3loop:
	mov	ax,word ptr [ebx]
	cmp	ax,0
	je	CheckEND
	cmp	ax,word ptr [ecx]
	jne	SUB_END
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	jmp	short CL3loop
CheckEND:
	;=====上次攔截=====
	cmp	blnHook,0
	jne	SUB_END
	;=====讀取網址連結=====
	mov	ebx,dword ptr [ebp+16]
	mov	ecx,offset HttpLink
ReadLoop:
	mov	ax,word ptr [ebx]
	mov	word ptr [ecx],ax
	inc	ebx
	inc	ebx
	inc	ecx
	inc	ecx
	cmp	ax,0
	jne	ReadLoop
	;=====攔截成功=====
	mov	blnHook,1
	;=================
	pop	ebp
	pop	edx
	pop	ecx
	pop	ebx
	pop	eax
	ret	4
SUB_END:
	pop	ebp
	pop	edx
	pop	ecx
	pop	ebx
	pop	eax
	jmp	dword ptr OrgFun
OrgFun:
	;ret	4
end     start