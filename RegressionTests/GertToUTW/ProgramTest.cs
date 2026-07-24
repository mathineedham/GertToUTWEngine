/** @file

    @copyright  &copy; 2024–2026, TRIA Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       24.07.2026

    @author
        Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @brief
        Provides regression tests for the GertToUTW CLI executable application entry point.

    @details
        - Executes the compiled `GertToUTW.exe` binary asynchronously in isolated processes.
        - Validates command-line interface output for help options, argument count violations, and valid conversions.
        - Captures standard output and standard error streams concurrently to prevent process deadlocks.
        - Ensures temporary test files and directories are deterministically cleaned up.

    @defgroup REF_GertToUTWEngine_RegressionTests_GertToUTW_Program_Tests Program_Tests
    @{
    @}
*/

using System.Diagnostics;

namespace RegressionTests.GertToUTW;

/** @ingroup REF_GertToUTWEngine_RegressionTests_GertToUTW_Program_Tests
    @class ProgramTests
    @brief
        Contains regression test cases for the command-line interface behavior of GertToUTW.

    @details
        - Tests process execution and standard stream outputs under various command-line arguments.
        - Verifies exit codes and usage messages.
        - Contains no shared mutable state.
*/
[TestClass]
public class ProgramTests
    {
    // Adjust relative path to point to your compiled executable (e.g., bin/Debug/net8.0/GertToUTW.exe)
    private const string EXECUTABLE_PATH = @"../../../bin/Debug/net8.0/GertToUTW.exe";

    /** @brief
            Executes a process asynchronously while capturing stdout and stderr streams.

        @details
            - Configures `ProcessStartInfo` with stream redirection and `UseShellExecute = false`.
            - Reads stdout and stderr concurrently to prevent deadlock conditions.
            - Evaluates the process exit code and throws an exception on non-zero return codes.

        @param[in] command
            Provides the executable path or command name to execute.

        @param[in] arguments
            Provides the command-line arguments passed to the process.

        @return
            Returns a tuple containing `(StandardOutput, StandardError)`.

        @exception InvalidOperationException
            Thrown when the process exits with a non-zero exit code.
    */
    public static async Task<(string, string)> RunCommandAsync( string command, string arguments )
        {
        ProcessStartInfo start_info = new()
            {
            FileName = command,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
            };

        using Process process = new()
            {
            StartInfo = start_info
            };

        _ = process.Start();

        // Read both streams concurrently to avoid deadlocks.
        Task<string> output_task = process.StandardOutput.ReadToEndAsync();
        Task<string> error_task = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        string output = await output_task.ConfigureAwait(false);
        string error = await error_task.ConfigureAwait(false);

        if( process.ExitCode != 0 )
            {
            throw new InvalidOperationException(
                $"Command failed with exit code {process.ExitCode}. Output: {output}. Error: {error}");
            }

        return (output, error);
        }

    /** @test
            Validates that passing the `--help` flag prints the help and usage description.

        @details
            - Executes the application binary with the `--help` flag.
            - Verifies that stdout contains the expected `Help:` header and `Usage: App.exe` string.

        @note
            Requires the compiled executable binary at `ExecutablePath`.
    */
    [TestMethod]
    public async Task Main_WithHelpFlag_PrintsUsageText()
        {
        // Act
        (string output, string error) = await RunCommandAsync(EXECUTABLE_PATH, "--help");

        // Assert
        Assert.Contains("Help:", output);
        Assert.Contains("Usage: App.exe", output);
        }

    /** @test
            Validates that providing an invalid argument count outputs an error and usage message.

        @details
            - Passes a single file argument without providing the output directory argument.
            - Confirms that standard output contains the invalid arguments warning message.

        @note
            Requires the compiled executable binary at `ExecutablePath`.
    */
    [TestMethod]
    public async Task Main_WithInvalidArgumentsCount_PrintsInvalidArgumentsMessage()
        {
        // Act - Passing only 1 argument (which is not help)
        (string output, string error) = await RunCommandAsync(EXECUTABLE_PATH, "only_one_argument.log");

        // Assert
        Assert.Contains("Invalid arguments provided!", output);
        Assert.Contains("Usage: App.exe", output);
        }

    /** @test
            Validates that execution with valid input file and output directory completes successfully.

        @details
            - Creates a temporary input file and temporary output directory.
            - Passes valid paths to the executable.
            - Asserts that standard output reports completion success.
            - Cleans up temporary resources in a `finally` block.

        @note
            Requires write access to system temporary directory.
    */
    [TestMethod]
    public async Task Main_WithValidArguments_PrintsSuccessMessage()
        {
        // Arrange
        string temp_input_file = Path.GetTempFileName();
        string temp_output_dir = Path.Combine(Path.GetTempPath(), "GertToUTW_Test_Output");

        try
            {
            // Act
            (string output, string error) = await RunCommandAsync(EXECUTABLE_PATH, $"\"{temp_input_file}\" \"{temp_output_dir}\"");

            // Assert
            Assert.Contains("Conversion completed successfully.", output);
            }
        finally
            {
            // Cleanup temp test files
            if( File.Exists(temp_input_file) )
                {
                File.Delete(temp_input_file);
                }

            if( Directory.Exists(temp_output_dir) )
                {
                Directory.Delete(temp_output_dir, true);
                }
            }
        }
    }
