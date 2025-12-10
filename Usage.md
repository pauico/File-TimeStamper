# 🚀 LogProcessor.exe Usage Guide

`LogProcessor.exe` is a utility designed to execute an external application, capture its output file, apply a timestamp, and append the result to a continuous log file. It handles common issues like the external program's UTF-16 encoding.

## Syntax

The program accepts optional flags followed by three main positional arguments:

```bash
LogProcessor.exe [FLAGS] [OutputLogFile] [CounterExe] [External Arguments...] [TempFile]
```

## Arguments

| Position | Name | Description |
| :--- | :--- | :--- |
| **1** (Positional) | `OutputLogFile` | The path to the final, continuous log file where timestamped entries will be appended (e.g., `metrics.log`). |
| **2** (Positional) | `CounterExe` | The name or path of the external program to execute (e.g., `NetworkCountersWatch.exe`). |
| **3..N-1** (Variable) | `External Arguments...` | All optional flags/arguments required by the external program. |
| **N** (Positional) | `TempFile` | The name of the file the external program writes its output to. **This must be the last argument in the command line.** |

## Flags (Optional)

| Flag | Keyword | Value | Description |
| :--- | :--- | :--- | :--- |
| **`/q`** | `--quiet` | *None* | Suppresses all informational console output (messages about calling the executable, processing, and success). Only fatal errors will be shown. |
| **`/s`** | `--separator` | `tab`, `comma`, `pipe`, `space`, or any custom string. | Specifies the delimiter used to separate the **Timestamp** from the **Data** in the final log file. (Default: `,`). |

## Examples

### 1\. Basic Logging (Default Separator)

Executes `NetworkCountersWatch.exe`, tells it to write to `data.tmp`, and appends the result to `metrics.log` using a **comma (`,`)** as the separator.

```bash
LogProcessor.exe metrics.log NetworkCountersWatch.exe /stabular data.tmp
```

### 2\. Quiet Mode with Tab Separator

Executes silently and ensures the output log (`metrics.log`) is correctly formatted as a standard TSV (Tab-Separated Values) file.

```bash
LogProcessor.exe /q /s tab metrics.log NetworkCountersWatch.exe /stabular data.tmp
```

### 3\. Custom Separator (Pipe)

Uses the long flag names and a pipe character (`|`) as the delimiter.

```bash
LogProcessor.exe --separator pipe final_results.log NetworkCountersWatch.exe /stabular temp_out.log
```

## Output Log Format

Each entry written to the `OutputLogFile` will follow this structure:

```
[YYYY-MM-DD HH:MM:SS][Separator][Data from TempFile]
```

**Example Output (using `/s tab`):**

```text
2025-12-10 22:39:00     Ethernet    219,766,566,588     98,684,398,522
2025-12-10 22:40:00     Ethernet    219,901,450,112     98,710,021,555
```
