# ConsoleGuard — graceful shutdown on Windows

Closing a console window with **X** sends `CTRL_CLOSE` and Windows then kills the process.
That is a problem if the child still has unsaved state.

This wrapper:

1. Starts a child process and copies stdout/stderr.
2. Registers `SetConsoleCtrlHandler`.
3. On close / Ctrl+C / logoff, connects to **127.0.0.1** only and sends a password + command.
4. Waits up to 3 minutes for a clean exit.

Compile (Developer Command Prompt):

```bat
csc /nologo /out:ConsoleGuard.exe ConsoleGuard.cs
```

Do not commit a real password. Pass it on the command line or from a local env file that is gitignored.
