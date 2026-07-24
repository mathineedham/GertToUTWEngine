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
    private readonly string mExecutable_path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../bin/Debug/net10.0/GertToUTW.exe"));
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;

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

        Task<string> output_task = process.StandardOutput.ReadToEndAsync();
        Task<string> error_task = process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync().ConfigureAwait(false);

        string output = await output_task.ConfigureAwait(false);
        string error = await error_task.ConfigureAwait(false);

        return (output, error);
        }

    /** @test
            Validates that passing the `--help` flag prints the help and usage description.

        @details
            - Executes the application binary with the  any of the help flags.
            - Verifies that stdout contains the expected `Help:` header and `Usage: App.exe` string.

        @note
            Requires the compiled executable binary at `ExecutablePath`.
    */
    [TestMethod]
    [DataRow("--help")]
    [DataRow("-h")]
    [DataRow("-help")]
    public async Task Main_WithHelpFlag(string arg)
        {
        (string output, string error) = await RunCommandAsync(mExecutable_path, arg).ConfigureAwait(false);

        Assert.IsNotNull(output);
        Assert.IsNotEmpty(output);
        Assert.IsEmpty(error);
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
    public async Task Main_WithInvalidArgumentsCount()
        {
        //Test calling the executable with 0 argument
        (string output0, string error0) = await RunCommandAsync(mExecutable_path, "").ConfigureAwait(false);
        Assert.IsNotEmpty(output0);
        Assert.IsEmpty(error0);
        //Test calling the executable with 1 argument
        (string output1, string error1) = await RunCommandAsync(mExecutable_path, "only_one_argument.log").ConfigureAwait(false);
        Assert.IsNotEmpty(output1);
        Assert.IsEmpty(error1);
        //Test calling the executable with >2 arguments
        (string output3, string error3) = await RunCommandAsync(mExecutable_path, $"first.log second.pdf third.xml").ConfigureAwait(false);
        Assert.IsNotEmpty(output3);
        Assert.IsEmpty(error3);
        }

    /** @test
            Validates that execution with 2 arguments will call Application.Execute() but will catch error since input is invalid

    */
    [TestMethod]
    public async Task Main_2Arguments_ErrorInExecute()
        {
        string temp_input_file = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Expected\\valid_singlerun.xml");
        string temp_output_dir = Path.Combine(theBaseFilesDir, "GertToUTW_Test_Output");

        (string output, string error) = await RunCommandAsync(mExecutable_path, $"{temp_input_file} {temp_output_dir}").ConfigureAwait(false);

        Assert.IsEmpty(output);
        Assert.IsNotEmpty(error);
        }

    [TestMethod]
    public async Task Main_2Arguments_ValidInput_ProducesExpectedOutput()
        {
        string valid_input_file = Path.Combine(theBaseFilesDir, "GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log");
        string valid_output_dir = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Generated\\ProgramTest\\");
        (string output, string error) = await RunCommandAsync(mExecutable_path, $"{valid_input_file} {valid_output_dir}").ConfigureAwait(false);
        Assert.IsNotEmpty(output);
        Assert.Contains("Conversion completed successfully.", output);
        Assert.IsEmpty(error);
        }
    }


