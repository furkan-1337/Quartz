# Quartz Programming Language

**Quartz** is a powerful, interpreted programming language built on the .NET ecosystem. It combines the ease of use of dynamic scripting languages with low-level system access capabilities typically reserved for compiled languages like C or C++.

Interpreted by a C# engine, Quartz allows for seamless integration with Windows APIs and native DLLs, making it an excellent choice for system automation, game hacking, learning internals, and rapid prototyping.

## ✨ Key Features

- **Dynamic & Safe**: Supports both dynamic typing (`auto`) and static type declarations (`int`, `bool`, `string`).
- **🔗 Native FFI**: Call functions from native DLLs (like `kernel32.dll`, `user32.dll`) directly using the `extern` keyword.
- **💾 Memory Management**: Allocate, read, write, and free unmanaged memory manually using the `Marshal` module.
- **📦 Comprehensive Standard Library**: Built-in modules for File I/O, Math, Strings, Arrays, Threading, and more.
- **🏗 Object-Oriented**: Support for Classes, Methods, and Objects.
- **C-Like Syntax**: Familiar syntax for developers coming from C, C++, C#, or Java background.

## 📚 Documentation

For a deep dive into the language syntax, modules, and advanced features like FFI and Memory manipulation, please refer to the **[Quartz Language Guide](language_guide.md)**.

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later.

### Building parts
1. Clone the repository.
2. Navigate to the project directory:
   ```bash
   cd Quartz
   ```
3. Build the project:
   ```bash
   dotnet build
   ```

### Running Scripts
You can run a Quartz script (`.qz`) by passing it as an argument to the interpreter:

```bash
# Using dotnet run
dotnet run -- script.qz

# Or using the built executable
./bin/Debug/net9.0/Quartz.exe script.qz
```

## 💡 Example

Here is a simple example showing Quartz's syntax:

```quartz
// Import a standard library module (if needed, though Console is global)
auto name = "Quartz";
auto version = 1.0;

Console.print("Welcome to", name, "v" + version);

// Define a function
func factorial(n) {
    if (n <= 1) return 1;
    return n * factorial(n - 1);
}

Console.print("Factorial of 5 is:", factorial(5));
```


## 🛠 Advanced Usage (FFI)

Quartz shines when you need to interact with the OS. Here is how you can show a Message Box using the Windows API:

```quartz
func msgBox(text) {
    // Call MessageBoxA from user32.dll
    extern("user32.dll", "int", "MessageBoxA", "int", "string", "string", "int")(0, text, "Quartz FFI", 0);
}

msgBox("Hello from Native Windows API!");
```

---
*Built with ❤️ by Furkan*


