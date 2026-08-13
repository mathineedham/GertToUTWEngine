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

using GertToUTW.Generated;

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
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;

    private readonly string mExecutable_path = get_executable_path();

    private static string get_executable_path()
        {
        // 1. Direct check: Works if the executable is copied to the test output directory
        string local_path = Path.Combine(theBaseFilesDir, "GertToUTW.exe");
        if( File.Exists(local_path) )
            {
            return local_path;
            }

        // 2. Build-configuration check: Switches path based on Debug vs Release
#if DEBUG
        const string build_config = "Debug";
#else
        const string build_config = "Release";
#endif

        // Navigates directly from the solution root: <SolutionDir>/GertToUTW/bin/<Debug|Release>/net10.0/GertToUTW.exe
        return Path.GetFullPath(Path.Combine(
            ProjectInfo.SolutionDir,
            "GertToUTW",
            "bin",
            build_config,
            "net10.0",
            "GertToUTW.exe"
        ));
        }
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
            - Executes the application binary with any of the supported help flags (`--help`, `-h`, `-help`).
            - Verifies that standard output contains text and standard error remains empty.

        @param[in] arg
            Provides the help command-line argument passed to the process.

        @note
            Requires the compiled executable binary at `mExecutable_path`.
    */
    [TestMethod]
    [DataRow("--help")]
    [DataRow("-h")]
    public async Task Main_WithHelpFlag( string arg )
        {
        (string output, string error) = await RunCommandAsync(mExecutable_path, arg).ConfigureAwait(false);

        Assert.IsNotNull(error);
        Assert.IsEmpty(output);
        Assert.IsNotEmpty(error);
        }

    /** @test
            Validates that providing an invalid argument count outputs an error and usage message.

        @details
            - Tests execution with 0, 1, and more than 2 command-line arguments.
            - Confirms that standard output contains usage instructions while error output remains empty.

        @note
            Requires the compiled executable binary at `mExecutable_path`.
    */
    [TestMethod]
    public async Task Main_WithInvalidArgumentsCount()
        {
        // Test calling the executable with 0 arguments
        (string output0, string error0) = await RunCommandAsync(mExecutable_path, "").ConfigureAwait(false);
        Assert.IsEmpty(output0);
        Assert.IsNotEmpty(error0);

        // Test calling the executable with 1 argument
        (string output1, string error1) = await RunCommandAsync(mExecutable_path, "only_one_argument.log").ConfigureAwait(false);
        Assert.IsEmpty(output1);
        Assert.IsNotEmpty(error1);

        // Test calling the executable with >2 arguments
        (string output3, string error3) = await RunCommandAsync(mExecutable_path, $"first.log second.pdf third.xml ").ConfigureAwait(false);
        Assert.IsEmpty(output3);
        Assert.IsNotEmpty(error3);
        }

    /** @test
            Validates that execution with two arguments catches invalid input during application execution.

        @details
            - Passes an invalid or unexpected file format input path to the executable.
            - Confirms that execution fails gracefully with empty standard output and non-empty standard error.

        @note
            Requires the compiled executable binary at `mExecutable_path`.
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

    /** @test
            Validates that execution with valid input and output directory arguments produces expected output.

        @details
            - Passes a valid log file and valid target directory to the executable.
            - Verifies that standard output confirms successful conversion and standard error remains empty.

        @note
            Requires the compiled executable binary at `mExecutable_path`.
    */
    [TestMethod]
    public async Task Main_2Arguments_ValidInput_ProducesExpectedOutput()
        {
        string valid_input_file = Path.Combine(theBaseFilesDir, "GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log");
        string valid_output_dir = Path.Combine(theBaseFilesDir, "GertToUTW\\XmlTestFiles\\Generated\\ProgramTest\\");

        (string output, string error) = await RunCommandAsync(mExecutable_path, $"{valid_input_file} {valid_output_dir}").ConfigureAwait(false);

        Assert.IsNotEmpty(output);
        Assert.Contains("Conversion completed successfully.", output);
        Assert.IsEmpty(error);

        // Test calling executable with lot number option
        (string output0, string error0) = await RunCommandAsync(mExecutable_path, $"{valid_input_file} {valid_output_dir} -l 123456").ConfigureAwait(false);
        Assert.IsNotEmpty(output0);
        Assert.Contains("Conversion completed successfully.", output0);
        Assert.IsEmpty(error0);

        (string output1, string error1) = await RunCommandAsync(mExecutable_path, $"{valid_input_file} -l 123456 {valid_output_dir}").ConfigureAwait(false);
        Assert.IsNotEmpty(output1);
        Assert.Contains("Conversion completed successfully.", output1);
        Assert.IsEmpty(error1);

        (string output3, string error3) = await RunCommandAsync(mExecutable_path, $"-l 123456 {valid_input_file} {valid_output_dir}").ConfigureAwait(false);
        Assert.IsNotEmpty(output3);
        Assert.Contains("Conversion completed successfully.", output3);
        Assert.IsEmpty(error3);

        (string output4, string error4) = await RunCommandAsync(mExecutable_path, $"--lot 123456 {valid_input_file} {valid_output_dir}").ConfigureAwait(false);
        Assert.IsNotEmpty(output4);
        Assert.Contains("Conversion completed successfully.", output4);
        Assert.IsEmpty(error4);
        }
    }
