# 💎 Quartz Programming Language

Quartz is a powerful, dynamic, and interpreted programming language designed for flexibility, ease of use, and high-performance system interaction. Built on .NET, it combines a familiar C-like syntax with advanced features typically found in systems languages, such as a Foreign Function Interface (FFI) and direct memory manipulation.

## 🚀 Key Features

- **Dynamic Typing**: Enjoy the speed of development with dynamic types and powerful type inference.
- **Foreign Function Interface (FFI)**: Call native functions from DLLs (`user32.dll`, `kernel32.dll`, etc.) directly with the `extern` keyword.
- **Low-Level Control**: Built-in memory management via the `Marshal` module, supporting pointers and raw memory access.
- **Modern OOP**: Full support for Classes, Inheritance, and Structs with operator overloading.
- **Rich Standard Library**: Comprehensive modules for Networking, Cryptography, File I/O, Process Management, and more.
- **First-Class Functions**: Support for standard functions and lambda expressions.
- **Error Handling**: Robust `try/catch` mechanism for graceful error management.

## 🛠️ Quick Start

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/furkan-1337/Quartz.git
   ```
2. Build the project using the .NET SDK:
   ```bash
   dotnet build
   ```

### Running the Interpreter

Launch the Quartz REPL:
```bash
quartz.exe
```

Run a script:
```bash
quartz.exe path/to/script.qz
```

### Hello World Example

```quartz
auto message = "Hello, Quartz!";
Console.print(message);
```

## 🔍 Language Highlights

### Native FFI (Windows API)
Quartz makes it trivial to interact with the Windows API:

```quartz
func msgBox(title, text) {
    extern("user32.dll", "int", "MessageBoxA", "int", "string", "string", "int")(0, text, title, 0);
}

msgBox("Quartz", "Hello from FFI!");
```

### Structs & Operator Overloading
Define compact data structures with custom behavior:

```quartz
struct Vector2 {
    float x;
    float y;

    func add(other) {
        return Vector2(this.x + other.x, this.y + other.y);
    }
}

auto v1 = Vector2(10.0, 20.0);
auto v2 = Vector2(5.0, 5.0);
auto v3 = v1 + v2; // Result: {15.0, 25.0}
```

### Powerful Standard Library
Quickly perform complex tasks like downloading files:

```quartz
auto url = "https://example.com/data.txt";
Network.downloadFile(url, "data.txt");
Console.print("Download complete!");
```

## 📖 Documentation

For a deep dive into the language syntax and all available modules, check out the [Language Guide](language_guide.md).

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.
