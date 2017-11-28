start:
    mov bx, 3550h
    mov ax, 2320h     
    call subproc
    
    mov bx, 3550h
    mov ax, 2321h
    call subproc                  
                          
    mov ax, 4c00h
    int 21h
subproc:
       sub bx, ax
       mov cx, 4
       s:
           rol bx, 4
           mov dx, bx
           and dx, fh
           cmp dx, 10  
           ja bigger
           
           add dx, 48  
           jmp print
           
           bigger: 
                add dx, 87
                
           print:
                mov ax, 2
                int 21h
       loop s    
       ret
