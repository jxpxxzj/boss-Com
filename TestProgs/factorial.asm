// compute (cx-1)!, save to ax

mov ax,1
mov bx,1
mov cx,9  // change this number
s1:
    mul bx
    inc bx
loop s1
int 21h