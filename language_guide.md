# Quartz Language Guide

Welcome to the official documentation for the **Quartz** programming language. Quartz is a dynamic, interpreted language designed for flexibility and ease of use, with powerful capabilities for system-level interaction via FFI (Foreign Function Interface) and memory manipulation.

## Table of Contents
1. [Introduction](#introduction)
2. [Basic Syntax](#basic-syntax)
    - [Comments](#comments)
    - [Variables & Types](#variables--types)
    - [String Interpolation](#string-interpolation)
    - [Expressions & Operators](#expressions--operators)
3. [Data Structures](#data-structures)
    - [Arrays](#arrays)
    - [Dictionaries](#dictionaries)
4. [Control Flow](#control-flow)
    - [Conditional Statements (If/Else)](#conditional-statements)
    - [Switch/Case](#switch-case)
    - [Loops (While, For, Foreach)](#loops)
    - [Enums](#enums)
    - [Error Handling (Try/Catch)](#error-handling)
5. [Functions & Lambdas](#functions--lambdas)
    - [Function Definitions](#functions)
    - [Lambda Functions](#lambda-functions)
6. [Object & Type System](#object--type-system)
    - [Classes & Inheritance](#classes--objects)
    - [Structs & Operator Overloading](#structs)
7. [Native & Advanced Features](#native--advanced-features)
    - [Extern (FFI)](#extern-ffi)
    - [Callbacks](#callbacks)
    - [Memory Management (Marshal)](#memory-management)
    - [Introspection (typeof, sizeof)](#introspection)
    - [Imports](#imports)
8. [Standard Library Modules](#standard-library-modules)
    - [Console](#console)
    - [Math](#math)
    - [String](#string)
    - [IO](#io-file-system)
    - [Random](#random)
    - [Converter](#converter)
    - [Process](#process)
    - [System](#system)
    - [Network](#network)
    - [Input](#input)
    - [Crypto](#crypto)
    - [Env](#env)

---

## 1. Introduction <a name="introduction"></a>

Quartz combines the simplicity of scripting languages with low-level access typically found in systems languages. It features a C-like syntax, dynamic typing, and a rich standard library.

---

## 2. Basic Syntax <a name="basic-syntax"></a>

### Comments
Single-line comments start with `//`.

```quartz
// This is a comment
auto x = 10; // Inline comment
```

### Variables & Types
Quartz is dynamically typed, but supports type annotations for clarity. The `auto` keyword is commonly used for type inference.

**Keywords:** `auto`, `int`, `float`, `double`, `bool`, `string`, `pointer`

```quartz
auto a = 10;            // Integer
int b = 20;             // Explicit Integer
double c = 3.14;        // Float/Double
bool d = true;          // Boolean
string e = "Hello";     // String
auto f = [1, 2, 3];     // Array
float g = 1.23f;        // Explicit Float
```

**Note:** While you can use specific type keywords (`int`, `string`, etc.), the runtime is dynamic.

### String Interpolation
Quartz supports string interpolation using the `$"..."` syntax.

```quartz
auto name = "Quartz";
auto version = 1.2;
Console.print($"Welcome to {name} v{version}!");
// Output: Welcome to Quartz v1.2!
```

Expressions inside `{}` are automatically evaluated and converted to strings.

### Expressions & Operators
Quartz supports standard arithmetic and comparison operators, as well as logical operators with short-circuiting.

#### Logical Operators
- `&&` (Logical AND): Returns true if both operands are true. Short-circuits if the first operand is false.
- `||` (Logical OR): Returns true if at least one operand is true. Short-circuits if the first operand is true.

```quartz
if (isTrue() || willNotBeCalled()) {
    Console.print("Short-circuiting works!");
}
```

---

## 3. Data Structures <a name="data-structures"></a>

### Arrays <a name="arrays"></a>
Arrays are dynamic lists that can hold mixed types.

**Creation:**
```quartz
auto list = [10, 20, 30, "Quartz"];
```

**Access & Indexing:**
```quartz
auto val = list[0]; // 10
list[1] = 99;       // Modify
```

**Instance Methods:**
Arrays in Quartz are `QArray` objects and support several instance methods for ease of use.

```quartz
auto list = [1, 2, 3];
list.push(4);          // Adds 4 to the end
auto last = list.pop(); // Removes and returns 3
auto size = list.len(); // Returns 3
```

**Array Module:**
The global `Array` module also provides helper functions (static-style).

| Function | Description | Example |
| :--- | :--- | :--- |
| `Array.length(arr)` | Returns the number of elements. | `Array.length(list)` |
| `Array.push(arr, val)` | Adds an element to the end. | `Array.push(list, 40)` |
| `Array.pop(arr)` | Removes and returns the last element. | `auto x = Array.pop(list)` |
| `Array.insert(arr, i, val)` | Inserts value at index `i`. | `Array.insert(list, 0, 999)` |
| `Array.remove(arr, i)` | Removes element at index `i`. | `Array.remove(list, 0)` |

### Dictionaries <a name="dictionaries"></a>
Dictionaries are collections of key-value pairs.

**Creation:**
```quartz
auto dict = {"name": "Quartz", "version": 1.0, "active": true};
```

**Access:**
```quartz
auto name = dict["name"]; // "Quartz"
dict["version"] = 1.1;     // Modify or Add
```

**Iteration:**
You can use `foreach` to iterate over dictionary keys.
```quartz
foreach (key in dict) {
    Console.print(key, ":", dict[key]);
}
```

---

## 4. Control Flow <a name="control-flow"></a>

### Conditional Statements <a name="conditional-statements"></a>
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

### Switch/Case <a name="switch-case"></a>
Switch statements provide a cleaner way to handle multiple conditional branches.

```quartz
auto day = "Monday";

switch (day) {
    case "Monday":
        Console.print("Start of the week!");
        break;
    case "Friday":
        Console.print("Almost weekend!");
        break;
    default:
        Console.print("Mid-week blues...");
}
```

### Loops <a name="loops"></a>

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

#### Foreach Loop
Used to iterate over collections like arrays and dictionaries.

```quartz
auto colors = ["Red", "Green", "Blue"];
foreach (color in colors) {
    Console.print("Color:", color);
}

auto scores = {"Alice": 90, "Bob": 85};
foreach (name in scores) {
    Console.print(name, "scored", scores[name]);
}
```

### Enums <a name="enums"></a>
Enums are used to define a set of named constants.

```quartz
enum Status {
    Pending,
    Active,
    Inactive
}

auto myStatus = Status.Active;
if (myStatus == Status.Active) {
    Console.print("System is active!");
}
```

### Error Handling (Try/Catch) <a name="error-handling"></a>
Quartz allows you to handle runtime errors gracefully using `try` and `catch` blocks.

```quartz
try {
    auto result = 10 / 0;
} catch (error) {
    Console.print("Caught an error:", error);
}
```

---

## 5. Functions & Lambdas <a name="functions--lambdas"></a>

### Functions <a name="functions"></a>

Functions are defined using the `func` keyword. They can accept arguments and return values using `return`.

```quartz
func add(a, b) {
    return a + b;
}

auto result = add(10, 20);
Console.print("Result:", result);
```

### Lambda Functions <a name="lambda-functions"></a>

Lambda functions are compact, anonymous functions that can be defined as expressions.

**Syntax:** `(params) => expression` or `(params) => { body }`

```quartz
// Simple lambda
auto add = (a, b) => a + b;
Console.print(add(10, 20)); // Output: 30

// Block lambda
auto mul = (a, b) => {
    return a * b;
};

// No arguments
auto hello = () => Console.print("Hello!");
```

---

## 6. Object & Type System <a name="object--type-system"></a>

### Classes & Inheritance <a name="classes--objects"></a>

Quartz supports Object-Oriented Programming (OOP) with classes, including inheritance and constructors.

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

auto rect = Rectangle(10, 5);
Console.print("Area:", rect.area());
```

#### Inheritance & base Keyword

```quartz
class Animal {
    func speak() { Console.print("Animal noise"); }
}

class Dog : Animal {
    func speak() {
        base.speak(); // Call superclass method
        Console.print("Woof!");
    }
}
```

### Structs & Operator Overloading <a name="structs"></a>

Structs are fixed-layout types useful for FFI and memory-sensitive tasks. They support custom constructors (`init`) and operator overloading.

```quartz
struct Vector2 {
    float x;
    float y;

    func init(x, y) {
        this.x = x;
        this.y = y;
    }

    func add(other) {
        return Vector2(this.x + other.x, this.y + other.y);
    }
}

auto v1 = Vector2(10, 20);
auto v2 = Vector2(5, 5);
auto v3 = v1 + v2; // Operator Overloading
```

**Supported Types in Structs:** `int`, `long`, `float`, `double`, `bool`, `pointer`, `string`.

---

## 7. Native & Advanced Features <a name="native--advanced-features"></a>

### Extern (FFI) <a name="extern-ffi"></a>
Call native functions from DLLs.
`extern(libraryName, returnType, functionName, argTypes...)(arguments)`

```quartz
func msgBox(title, text) {
    extern("user32.dll", "int", "MessageBoxA", "int", "string", "string", "int")(0, text, title, 0);
}
```

### Callbacks <a name="callbacks"></a>
Convert Quartz functions to native pointers for use as win32 callbacks.

```quartz
auto cbPtr = Callback.create(onWindowFound);
extern("user32.dll", "bool", "EnumWindows", "pointer", "int")(cbPtr, 0);
```

### Memory Management (Marshal) <a name="memory-management"></a>
Low-level memory access via the `Marshal` module.

| Function | Description |
| :--- | :--- |
| `Marshal.alloc(size)` | Allocates `size` bytes of memory. Returns a `pointer`. |
| `Marshal.free(ptr)` | Frees allocated memory at address. |
| `Marshal.readInt(ptr)` | Reads a 32-bit integer. |
| `Marshal.writeInt(ptr, val)` | Writes a 32-bit integer. |
| `Marshal.readInt16(ptr)` | Reads a 16-bit integer (short). |
| `Marshal.writeInt16(ptr, val)` | Writes a 16-bit integer (short). |
| `Marshal.readInt64(ptr)` | Reads a 64-bit integer (long). |
| `Marshal.writeInt64(ptr, val)` | Writes a 64-bit integer (long). |
| `Marshal.readByte(ptr)` | Reads a single byte. |
| `Marshal.writeByte(ptr, val)` | Writes a single byte. |
| `Marshal.readDouble(ptr)` | Reads a 64-bit float (double). |
| `Marshal.writeDouble(ptr, val)` | Writes a 64-bit float (double). |
| `Marshal.readString(ptr)` | Reads an ANSI string from memory. |
| `Marshal.readStruct(ptr, template)` | Reads a struct instance from memory. |
| `Marshal.writeStruct(ptr, obj)` | Writes a struct instance to memory. |
| `Marshal.structureToPtr(obj, ptr)` | Marshals a class instance to memory. |

### Introspection <a name="introspection"></a>
Inspect values at runtime using `typeof(val)` and `sizeof(val)`.

```quartz
Console.print(typeof(10));    // "int"
Console.print(sizeof("int")); // 4 (bytes)
```

### Imports <a name="imports"></a>
Reuse code from other files.
```quartz
import("math_utils.qz");
auto calc = Calculator();
```

---

## 8. Standard Library Modules <a name="standard-library-modules"></a>

Quartz includes several built-in modules available globally.

### Console <a name="console"></a>
- `Console.print(arg1, arg2, ...)`: Prints values followed by a newline.
- `Console.writeLine(arg1, arg2, ...)`: Alias for `print`.
- `Console.write(arg1, arg2, ...)`: Prints values without a newline.
- `Console.readLine()`: Reads a line of text from standard input.
- `Console.clear()`: Clears the console screen.
- `Console.setTitle(title)`: Sets the current console window title.

### Math <a name="math"></a>
- `Math.PI`: Returns the value of PI (3.14159...).
- `Math.E`: Returns the value of E (2.71828...).
- `Math.abs(val)`: Absolute value.
- `Math.ceil(val)`: Ceiling.
- `Math.floor(val)`: Floor.
- `Math.sqrt(val)`: Square root.
- `Math.sin(val)`, `Math.cos(val)`, `Math.tan(val)`: Trigonometric functions.
- `Math.pow(base, exp)`: Power function.
- `Math.round(val)`: Rounds to the nearest integer.
- `Math.min(a, b)`, `Math.max(a, b)`: Minimum and maximum value.

### String <a name="string"></a>
- `String.length(str)`: Returns string length.
- `String.upper(str)`: Converts to uppercase.
- `String.lower(str)`: Converts to lowercase.
- `String.substring(str, start, [len])`: Returns a portion of the string.
- `String.replace(str, old, new)`: Replaces occurrences of a substring.
- `String.split(str, delimiter)`: Splits string into an array.
- `String.trim(str)`: Removes leading and trailing whitespace.

### IO (File System) <a name="io-file-system"></a>
- `IO.readFile(path)`: Reads the entire content of a file as a string.
- `IO.writeFile(path, content)`: Writes content to a file (overwrites).
- `IO.appendFile(path, content)`: Appends content to the end of a file.
- `IO.fileExists(path)`: Checks if a file exists.
- `IO.deleteFile(path)`: Deletes a file.

### Thread <a name="thread"></a>
- `Thread.sleep(ms)`: Pauses execution for the specified milliseconds.
- `Thread.create(func)`: Creates a new thread to execute the given function.
- `Thread.getCurrentId()`: Returns the current thread's unique identifier.

### Process <a name="process"></a>
- `Process.list()`: Returns a list of all running processes.
- `Process.isRunning(pid_or_name)`: Checks if a process is running.
- `Process.getModuleAddress(pid, name)`: Gets the base address of a module in a process.
- `Process.getProcessIdByName(name)`: Retrieves the PID of a process by its name.
- `Process.getModules(pid)`: Lists modules loaded in a process.
- `Process.terminate(pid)`: Terminates a process.
- `Process.getCurrentProcess()`: Returns information about the current process.
- `Process.getExecutablePath()`: Returns the path to the current executable.
- `Process.getWorkingPath()`: Returns the current working directory.

### Network <a name="network"></a>
- `Network.get(url)`: Performs an HTTP GET request.
- `Network.post(url, data)`: Performs an HTTP POST request.
- `Network.downloadString(url)`: Downloads a string from a URL.
- `Network.downloadBytes(url)`: Downloads raw bytes from a URL.
- `Network.downloadFile(url, path)`: Downloads a file to the specified path.

### System <a name="system"></a>
- `System.getOSVersion()`: Returns the OS version string.
- `System.getMemoryStats()`: Returns a dictionary with memory usage information.
- `System.getCPUUsage()`: Returns current CPU usage percentage.
- `System.getMachineName()`: Returns the local machine name.
- `System.getUserName()`: Returns the current user's name.

### Input <a name="input"></a>
- `Input.isKeyDown(vKey)`, `Input.getMousePos()`

### Crypto <a name="crypto"></a>
- `Crypto.sha256(data)`: Computes SHA256 hash.
- `Crypto.md5(data)`: Computes MD5 hash.
- `Crypto.base64Encode(data)`: Encodes data to Base64.
- `Crypto.base64Decode(data)`: Decodes Base64 data.

### Window <a name="window"></a>
- `Window.find(className, title)`: Finds a window by class or name. Returns a `pointer`.
- `Window.getForeground()`: Returns a `pointer` to the current foreground window.
- `Window.setTitle(hwnd, title)`: Sets a window's title.
- `Window.show(hwnd, cmd)`: Shows or hides a window.
- `Window.exists(hwnd)`: Checks if a window handle is valid.
- `Window.setOpacity(hwnd, alpha)`: Sets window transparency (0-255).
- `Window.getRect(hwnd)`: Returns a dictionary with window coordinates.
- `Window.setTopmost(hwnd, bool)`: Sets or unsets topmost status.
