# boss-asm
boss-asm 语言基本说明.

## 语法
基本接近于 8086 asm, 参考程序见 ../TestProgs.  
支持单行注释, 使用 `//` 起始

## 基本类型
类型 | 格式 | 备注 
---- | --- | ---
Register | ax / bx / cx / dx / si / di / bp / sp / ip / cs / ds / es / ss | 寄存器
Number | 1234 / 1234H | 10 进制 / 16 进制数字
Label | label: / label | 标签, 指向一行指令
MemorySeek | [bx+offset].addition | 指向一个内存地址

## 内存操作
`memory_address := ds * 16 + bx + offset`

每个 CPU 分配 1048576 个 `System.Int32` 作为内存, 即地址空间为 `0x00000` ~ `0xFFFFF`

## 指令信息
目前支持了 mov, xchg, add, mul, sub, div, inc, dec, push, pop, call, loop, int, jmp, je, jne, jb, jnb, ja, jna, cmp, nop 指令.

### mov
语法: `mov op1, op2`  
功能: `op1 := op2`

op1 类型 | op2 类型 | 指令周期 | 备注 
---- | --- | --- | --- 
Register | Register | 2 | -
Register | MemorySeek | 8 | -
MemorySeek | Register | 9 | -
Register | Number | 4 | -
MemorySeek | Number | 10 | -

### xchg
语法: `xchg op1, op2`   
功能: `swap(op1, op2)`

op1 类型 | op2 类型 | 指令周期 | 备注 
---- | --- | --- | --- 
Register | Register | 4 | -

### add, sub
语法: `add op1, op2` / `sub op1, op2`  
功能: `op1 += op2` / `op1 -= op2`

op1 类型 | op2 类型 | 指令周期 | 备注 
---- | --- | --- | --- 
Register | Register | 3 | -
Register | Number | 4 | -

### mul, div
语法: `mul op1` / `div op1`  
功能: `op1 *= op2` / `op1 /= op2`

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Register | 133 / 162 | -
Number | 133 / 162 | -
MemorySeek | 139 / 168 | -

### inc, dec
语法: `inc op1` / `dec op1`  
功能: `op1++` / `op1--`

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Register | 3 | -

### push
语法: `push op1`  
功能: 将 op1 压栈

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Register | 11 | -
T | 16 | 任意类型, 通常为 String 或者 Number, 用于传参

### pop
语法: `pop op1`  
功能: 弹出栈顶元素, 保存至 op1

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Register | 17 | -

### call
语法: `call op1`  
功能: 调用 mscorlib 中的静态方法, 将弹出适量栈内元素作为参数

op1 类型 |  指令周期 | 备注 
---- | --- | --- 
String | 37 | -

示例:
```asm
call System.Console.Clear

push 'ax value is `
call System.Console.Write(System.String)

mov ax, 5050
push ax
call System.Console.WriteLine(System.Int32)

// 输出:
// ax value is 5050
```
### int
语法: `int op1`  
功能: 产生代码为 `op1` 的中断, 可以绑定 Cpu.Interrupt 事件进行处理

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Number | 52 | -

### loop, jmp, je, jne, jb, ja, jna
语法: `loop/jmp/je/jne/jb/ja/jna op1`  
功能: 设置 ip 寄存器, 按特定条件跳转到 `op1` 所在位置的代码

op1 类型 | 指令周期 | 备注 
---- | --- | --- 
Label | 15 (`jmp`) / 16 / 17 (`loop`) | -

跳转条件:
指令 | 条件 | 备注 
---- | --- | --- | --- 
`jmp` | 无条件跳转 | -
`loop` | cx > 0 | 跳转后 cx--
`je` | ZF = 1 | -
`jne` | ZF = 0 | -
`jb` | CF = 1 | -
`jnb` | CF = 0 | -
`ja` | CF = 0 and ZF = 0 | -
`jna` | CF = 1 or ZF = 0 | -

### cmp
语法: `cmp op1, op2`  
功能: 比较 `op1`, `op2` 的大小, 设置标志寄存器

op1 类型 | op2 类型 | 指令周期 | 备注 
---- | --- | --- | --- 
Register | Register | 3 | -
Register | Number | 4 | -
Number | Register | 4 | -

标志位情况:  
value1 == value2 ? ZF = 1 : ZF = 0  
value1 < value2 ? CF = 1 : CF = 0  

### nop
语法: `nop`  
功能: 无操作, 占用 3 个指令周期