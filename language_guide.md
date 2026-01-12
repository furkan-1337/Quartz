# Quartz Language Guide

Welcome to the official documentation for the **Quartz** programming language. Quartz is a dynamic, interpreted language designed for flexibility and ease of use, with powerful capabilities for system-level interaction via FFI (Foreign Function Interface) and memory manipulation.

## Table of Contents
1. [Introduction](#introduction)
2. [Basic Syntax](#basic-syntax)
    - [Comments](#comments)
    - [Variables & Types](#variables--types)
3. [Control Flow](#control-flow)
    - [If/Else](#conditional-statements)
    - [Loops (While, For)](#loops)
4. [Functions](#functions)
5. [Classes & Objects](#classes--objects)
6. [Data Structures](#data-structures)
    - [Arrays](#arrays)
7. [Modules & Standard Library](#modules--standard-library)
    - [Console](#console)
    - [Math](#math)
    - [String](#string)
    - [IO](#io-file-system)
    - [Random](#random)
    - [Converter](#converter)
    - [Process](#process)
8. [Advanced Features](#advanced-features)
    - [Extern (FFI)](#extern-ffi)
    - [Memory Management (Marshal)](#memory-management-marshal)
    - [Imports](#imports)

---

## Introduction

Quartz combines the simplicity of scripting languages with low-level access typically found in systems languages. It features a C-like syntax, dynamic typing, and a rich standard library.

## Basic Syntax

### Comments
Single-line comments start with `//`.

```quartz
// This is a comment
auto x = 10; // Inline comment
```

### Variables & Types
Quartz is dynamically typed, but supports type annotations for clarity. The `auto` keyword is commonly used for type inference.

**Keywords:** `auto`, `int`, `double`, `bool`, `string`, `pointer`

```quartz
auto a = 10;            // Integer
int b = 20;             // Explicit Integer
double c = 3.14;        // Float/Double
bool d = true;          // Boolean
string e = "Hello";     // String
auto f = [1, 2, 3];     // Array
```

**Note:** While you can use specific type keywords (`int`, `string`, etc.), the runtime is dynamic.

---

## Control Flow

### Conditional Statements
Quartz uses standard C-style `if` and `else` blocks.

```quartz
auto score = 85;

if (score >= 90) {
    Console.print("Grade: A");
} else if (score >= 80) {
    Console.print("Grade: B");
} else {
    Console.print("Grade: C");
}
```

### Loops

#### While Loop
```quartz
auto i = 0;
while (i < 5) {
    Console.print("Count:", i);
    i = i + 1;
}
```

#### For Loop
Standard C-style for loops are supported.

```quartz
for (auto i = 0; i < 5; i = i + 1) {
    Console.print("Index:", i);
}
```

---

## Functions

Functions are defined using the `func` keyword. They can accept arguments and return values using `return`.

```quartz
func add(a, b) {
    return a + b;
}

auto result = add(10, 20);
Console.print("Result:", result);
```

---

## Classes & Objects

Quartz supports Object-Oriented Programming (OOP) with classes.

-   Define classes with `class`.
-   Define methods with `func` inside the class.
-   Access instance properties using `this`.

```quartz
class Rectangle {
    // Constructor
    func init(w, h) {
        this.width = w;
        this.height = h;
    }

    func area() {
        return this.width * this.height;
    }
}

// Instantiation with arguments
auto rect = Rectangle(10, 5);
Console.print("Area:", rect.area()); // Output: 50
```

---

## Data Structures

### Arrays
Arrays are dynamic lists that can hold mixed types.

**Creation:**
```quartz
auto list = [10, 20, 30, "Quartz"];
```

**Access:**
```quartz
auto val = list[0]; // 10
list[1] = 99;       // Modify
```

**Array Module:**
The global `Array` module provides helper functions.

| Function | Description | Example |
| :--- | :--- | :--- |
| `Array.length(arr)` | Returns the number of elements. | `Array.length(list)` |
| `Array.push(arr, val)` | Adds an element to the end. | `Array.push(list, 40)` |
| `Array.pop(arr)` | Removes and returns the last element. | `auto x = Array.pop(list)` |
| `Array.insert(arr, i, val)` | Inserts value at index `i`. | `Array.insert(list, 0, 999)` |
| `Array.remove(arr, i)` | Removes element at index `i`. | `Array.remove(list, 0)` |

---

## Modules & Standard Library

Quartz comes with several built-in modules available globally.

### Console
Handles input and output.
-   `Console.print(arg1, arg2, ...)`: Prints values to standard output.
-   `Console.readLine()`: Reads a line of text from input.
-   `Console.clear()`: Clears the console.

### Math
Provides mathematical functions.
-   `Math.pi`: Constant PI (approx).
-   `Math.abs(n)`: Absolute value.
-   `Math.pow(base, exp)`: Power.
-   `Math.sqrt(n)`: Square root.
-   `Math.sin(n)`, `Math.cos(n)`, `Math.tan(n)`: Trigonometry.
-   `Math.floor(n)`, `Math.ceil(n)`, `Math.round(n)`: Rounding.
-   `Math.min(a, b)`, `Math.max(a, b)`: Comparison.

### String
String manipulation utilities.
-   `String.length(str)`: Length of string.
-   `String.upper(str)`: Convert to uppercase.
-   `String.lower(str)`: Convert to lowercase.
-   `String.substring(str, start, len)`: Extract substring.
-   `String.replace(str, old, new)`: Replace occurrences.
-   `String.contains(str, substr)`: Check existence.
-   `String.split(str, delim)`: Split into array.
-   `String.trim(str)`: Remove whitespace.

### IO (File System)
File operations.
-   `IO.readFile(path)`: Reads entire file as string.
-   `IO.writeFile(path, content)`: Writes string to file.
-   `IO.appendFile(path, content)`: Appends string to file.
-   `IO.fileExists(path)`: Checks if file exists.
-   `IO.deleteFile(path)`: Deletes file.

### Random
-   `Random.range(min, max)`: Returns a random integer between min and max.
-   `Random.next()`: Returns a random double between 0.0 and 1.0.

### Converter
-   `Converter.toInt(val)`: Converts string/double to integer.
-   `Converter.toString(val)`: Converts value to string.
-   `Converter.toDouble(val)`: Converts value to double.

### Process
System process management.
- `Process.list()`: Returns an array of strings in format "PID:Name".
- `Process.getModuleAddress(pid, moduleName)`: Returns a `pointer` to the base address of a specified module in a process.
- `Process.isRunning(pid)`: Returns `true` if the process is running.
- `Process.getProcessIdByName(name)`: Returns the PID of the specified process name, or -1 if not found.
- `Process.getModules(pid)`: Returns an array of module names for the specified process.

---

## Advanced Features

### Extern (FFI)
Quartz allows calling native functions from dynamic libraries (DLLs) using the `extern` function.

**Syntax:**
`extern(libraryName, returnType, functionName, argTypes...)(arguments)`

**Supported Types for FFI:** "int", "bool", "string", "pointer", "double", "float".

**Example: MessageBox (User32.dll)**
```quartz
// int MessageBox(HWND hWnd, LPCTSTR lpText, LPCTSTR lpCaption, UINT uType);
func msgBox(title, text) {
    extern("user32.dll", "int", "MessageBoxA", "int", "string", "string", "int")(0, text, title, 0);
}

msgBox("Hello", "This is Quartz Native Call!");
```

### Memory Management (Marshal)
The `Marshal` module works with FFI to handle raw memory and pointers.

| Function | Description |
| :--- | :--- |
| `Marshal.alloc(size)` | Allocates `size` bytes of memory. Returns a `pointer`. |
| `Marshal.free(ptr)` | Frees allocated memory. |
| `Marshal.readInt(ptr)` | Reads an integer from address. |
| `Marshal.writeInt(ptr, val)` | Writes an integer to address. |
| `Marshal.readByte(ptr)` | Reads a byte. |
| `Marshal.writeByte(ptr, val)` | Writes a byte to address. |
| `Marshal.readInt16(ptr)` | Reads a 16-bit integer (short). |
| `Marshal.writeInt16(ptr, val)` | Writes a 16-bit integer (short). |
| `Marshal.readInt(ptr)` | Reads a 32-bit integer (int). |
| `Marshal.writeInt(ptr, val)` | Writes a 32-bit integer (int). |
| `Marshal.readInt64(ptr)` | Reads a 64-bit integer (long). |
| `Marshal.writeInt64(ptr, val)` | Writes a 64-bit integer (long). |
| `Marshal.readDouble(ptr)` | Reads a 64-bit floating point number (double). |
| `Marshal.writeDouble(ptr, val)` | Writes a 64-bit floating point number (double). |
| `Marshal.structureToPtr(obj, ptr)` | Marshals a struct/class to memory. |

**Example: Reading Process Memory**
```quartz
// Open Process (PROCESS_ALL_ACCESS = 0x1F0FFF = 2035711)
auto handle = extern("kernel32.dll", "pointer", "OpenProcess", "int", "bool", "int")(2035711, false, 1234);

if (handle != 0) {
    auto buffer = Marshal.alloc(4);

    // Call ReadProcessMemory to read 4 bytes from address 12345678
    // Arguments: hProcess, lpBaseAddress, lpBuffer, nSize, lpNumberOfBytesRead
    auto success = extern("kernel32.dll", "bool", "ReadProcessMemory", "pointer", "pointer", "pointer", "int", "pointer")(handle, 12345678, buffer, 4, 0);

    if (success) {
        auto val = Marshal.readInt(buffer);
        Console.print("Value read:", val);
    } else {
        Console.print("Failed to read memory.");
    }

    Marshal.free(buffer);
    extern("kernel32.dll", "bool", "CloseHandle", "pointer")(handle);
} else {
    Console.print("Failed to open process.");
}
```

### Imports
You can include other Quartz scripts using the `import` function. This allows you to reuse classes and functions defined in separate files.

**Example:**

Suppose you have a file named `math_utils.qz`:
```quartz
// math_utils.qz
class Calculator {
    func add(a, b) {
        return a + b;
    }
}
```

You can import and use it in your main script:
```quartz
import("math_utils.qz");

// Instantiate the class defined in the imported file
auto calc = Calculator();

// Call a method on the instance
auto result = calc.add(10, 20);
Console.print("Result from imported class:", result);
```
