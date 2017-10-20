// compute 1+...+cx, save to [12345H] and ax

mov ax, 1234H
mov ds, ax

mov cx, 100 // change this number
mov ax, 0

flag:
    add ax, cx
loop flag 

mov bx, 1234H
mov [bx], ax

int 21h