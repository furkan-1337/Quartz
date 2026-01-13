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

**Keywords:** `auto`, `int`, `long`, `float`, `double`, `bool`, `string`, `pointer`

```quartz
auto a = 10;            // Integer
int b = 20;             // Explicit Integer
long b2 = 1000000;      // 64-bit Integer
double c = 3.14;        // Float/Double
bool d = true;          // Boolean
string e = "Hello";     // String
auto f = [1, 2, 3];     // Array
float g = 1.23f;        // Explicit Float
```

**Note:** While you can use specific type keywords (`int`, `string`, etc.), the runtime is dynamic.

### Hexadecimal Literals
Quartz supports hexadecimal integer literals using the `0x` prefix.

```quartz
auto hex = 0xFF;        // 255
auto color = 0xAABBCC;  // 11189196
```

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
Quartz supports standard arithmetic, comparison, and logical operators.

#### Arithmetic & Assignment Operators
- `+`, `-`, `*`, `/`: Basic arithmetic.
- `++`, `--`: Increment and decrement (both **Prefix** and **Postfix**).
- `=`, `+=`, `-=`, `*=`, `/=`: Assignment and compound assignment.

```quartz
int i = 0;
i++;         // i is 1
++i;         // i is 2
Console.print(i--); // prints 2, i becomes 1

i += 10;     // i is 11
i -= 5;      // i is 6
i *= 2;      // i is 12
i /= 3;      // i is 4
```

Compound assignments also work on **struct fields**, **array elements**, and **dictionary values**:

```quartz
ts.val += 10;
arr[0] *= 2;
dict["score"] += 100;
```
Console.print(--i); // prints 0, i becomes 0
```

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
| `Marshal.readPtr(ptr)` | Reads a pointer/address from memory. |
| `Marshal.writePtr(ptr, val)` | Writes a pointer/address to memory. |
| `Marshal.stringToPtr(str)` | Allocates and copies ANSI string to memory. Returns `pointer`. |
| `Marshal.stringToPtrUni(str)` | Allocates and copies Unicode string to memory. Returns `pointer`. |
| `Marshal.callPtr(ptr, retType, [argTypes])` | Creates a native function wrapper for a function pointer. |
| `Marshal.readFloat(ptr)` | Reads a 32-bit float. |
| `Marshal.writeFloat(ptr, val)` | Writes a 32-bit float. |
| `Marshal.invoke(func, args)` | Invokes an `ICallable` (function) with a list of arguments. |
| `Marshal.size()` | Returns the size of a pointer (4 or 8 bytes). |

### Introspection <a name="introspection"></a>
Inspect values at runtime using `typeof(val)` and `sizeof(val)`.

```quartz
Console.print(typeof(10));    // "int"
Console.print(sizeof("int")); // 4 (bytes)
```

### Imports & Loading <a name="imports"></a>
Reuse code from other files.

| Function | Description |
| :--- | :--- |
| `import(path)` | Imports a module, executes it in its own environment, and exports its public members. |
| `load(path)` | Executes the specified file in the *current* environment. |

```quartz
// From code or REPL
load("utils.qz");
```

---

## 8. Standard Library Modules <a name="standard-library-modules"></a>

Quartz includes several built-in modules available globally.

### Console <a name="console"></a>

| Function | Description |
| :--- | :--- |
| `print(arg1, arg2, ...)` | Prints values followed by a newline. |
| `writeLine(arg1, arg2, ...)` | Alias for `print`. |
| `write(arg1, arg2, ...)` | Prints values without a newline. |
| `readLine()` | Reads a line of text from standard input. |
| `clear()` | Clears the console screen. |
| `setTitle(title)` | Sets the current console window title. |

### Math <a name="math"></a>

| Property/Function | Description |
| :--- | :--- |
| `PI` | Returns the value of PI (3.14159...). |
| `E` | Returns the value of E (2.71828...). |
| `abs(val)` | Absolute value. |
| `ceil(val)` | Ceiling. |
| `floor(val)` | Floor. |
| `sqrt(val)` | Square root. |
| `sin(val)`, `cos(val)`, `tan(val)` | Trigonometric functions. |
| `pow(base, exp)` | Power function. |
| `round(val)` | Rounds to the nearest integer. |
| `min(a, b)`, `max(a, b)` | Minimum and maximum value. |

### String <a name="string"></a>

| Function | Description |
| :--- | :--- |
| `length(str)` | Returns string length. |
| `upper(str)` | Converts to uppercase. |
| `lower(str)` | Converts to lowercase. |
| `substring(str, start, [len])` | Returns a portion of the string. |
| `replace(str, old, new)` | Replaces occurrences of a substring. |
| `split(str, delimiter)` | Splits string into an array. |
| `trim(str)` | Removes leading and trailing whitespace. |

### IO (File System) <a name="io-file-system"></a>

| Function | Description |
| :--- | :--- |
| `readFile(path)` | Reads the entire content of a file as a string. |
| `writeFile(path, content)` | Writes content to a file (overwrites). |
| `appendFile(path, content)` | Appends content to the end of a file. |
| `fileExists(path)` | Checks if a file exists. |
| `deleteFile(path)` | Deletes a file. |

### Thread <a name="thread"></a>

| Function | Description |
| :--- | :--- |
| `sleep(ms)` | Pauses execution for the specified milliseconds. |
| `create(func)` | Creates a new thread to execute the given function. |
| `getCurrentId()` | Returns the current thread's unique identifier. |

### Process <a name="process"></a>

| Function | Description |
| :--- | :--- |
| `list()` | Returns a list of all running processes. |
| `isRunning(pid_or_name)` | Checks if a process is running. |
| `getModuleAddress(pid, name)` | Gets the base address of a module in a process. |
| `getProcessIdByName(name)` | Retrieves the PID of a process by its name. |
| `getModules(pid)` | Lists modules loaded in a process. |
| `terminate(pid)` | Terminates a process. |
| `getCurrentProcess()` | Returns information about the current process. |
| `getExecutablePath()` | Returns the path to the current executable. |
| `getWorkingPath()` | Returns the current working directory. |

### Network <a name="network"></a>

| Function | Description |
| :--- | :--- |
| `get(url)` | Performs an HTTP GET request. |
| `post(url, data)` | Performs an HTTP POST request. |
| `downloadString(url)` | Downloads a string from a URL. |
| `downloadBytes(url)` | Downloads raw bytes from a URL. |
| `downloadFile(url, path)` | Downloads a file to the specified path. |

### System <a name="system"></a>

| Function | Description |
| :--- | :--- |
| `getOSVersion()` | Returns the OS version string. |
| `getMemoryStats()` | Returns a dictionary with memory usage information. |
| `getCPUUsage()` | Returns current CPU usage percentage. |
| `getMachineName()` | Returns the local machine name. |
| `getUserName()` | Returns the current user's name. |

### Input <a name="input"></a>

| Function | Description |
| :--- | :--- |
| `isKeyDown(vKey)` | Checks if a specific virtual key is held down. |
| `getMousePos()` | Returns a dictionary with `x` and `y` coordinates. |

### Crypto <a name="crypto"></a>

| Function | Description |
| :--- | :--- |
| `sha256(data)` | Computes SHA256 hash. |
| `md5(data)` | Computes MD5 hash. |
| `base64Encode(data)` | Encodes data to Base64. |
| `base64Decode(data)` | Decodes Base64 data. |

### Window <a name="window"></a>

| Function | Description |
| :--- | :--- |
| `find(className, title)` | Finds a window by class or name. Returns a `pointer`. |
| `getForeground()` | Returns a `pointer` to the current foreground window. |
| `setTitle(hwnd, title)` | Sets a window's title. |
| `show(hwnd, cmd)` | Shows or hides a window. |
| `exists(hwnd)` | Checks if a window handle is valid. |
| `setOpacity(hwnd, alpha)` | Sets window transparency (0-255). |
| `getRect(hwnd)` | Returns a dictionary with window coordinates. |
| `setTopmost(hwnd, bool)` | Sets or unsets topmost status. |

### Env <a name="env"></a>

| Function | Description |
| :--- | :--- |
| `get(name)` | Reads an environment variable. |
| `set(name, value)` | Sets an environment variable. |
