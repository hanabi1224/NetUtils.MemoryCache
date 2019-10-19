using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace NetUtils
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class CustomRetryAttribute : PropertyAttribute, IWrapSetUpTearDown, ICommandWrapper
    {
        public int TryCount { get; }

        /// <summary>
        /// Construct a <see cref="RetryAttribute" />
        /// </summary>
        /// <param name="tryCount">The maximum number of times the test should be run if it fails</param>
        public CustomRetryAttribute(int tryCount = 3) : base(tryCount)
        {
            TryCount = tryCount;
        }

        #region IWrapSetUpTearDown Members

        /// <summary>
        /// Wrap a command and return the result.
        /// </summary>
        /// <param name="command">The command to be wrapped</param>
        /// <returns>The wrapped command</returns>
        public TestCommand Wrap(TestCommand command)
        {
            return new CustomRetryCommand(command, TryCount);
        }

        #endregion

        #region Nested RetryCommand Class

        public class CustomRetryCommand : DelegatingTestCommand
        {
            private readonly int _tryCount;

            /// <summary>
            /// Initializes a new instance of the <see cref="RetryCommand"/> class.
            /// </summary>
            /// <param name="innerCommand">The inner command.</param>
            /// <param name="tryCount">The maximum number of repetitions</param>
            public CustomRetryCommand(TestCommand innerCommand, int tryCount)
                : base(innerCommand)
            {
                _tryCount = tryCount;
            }

            /// <summary>
            /// Runs the test, saving a TestResult in the supplied TestExecutionContext.
            /// </summary>
            /// <param name="context">The context in which the test should run.</param>
            /// <returns>A TestResult</returns>
            public override TestResult Execute(TestExecutionContext context)
            {
                int count = _tryCount;

                while (count-- > 0)
                {
                    try
                    {
                        context.CurrentResult = innerCommand.Execute(context);
                    }
                    // Commands are supposed to catch exceptions, but some don't
                    // and we want to look at restructuring the API in the future.
                    catch (Exception ex)
                    {
                        if (context.CurrentResult == null)
                        {
                            context.CurrentResult = context.CurrentTest.MakeTestResult();
                        }

                        context.CurrentResult.RecordException(ex);
                    }

                    if (context.CurrentResult.ResultState != ResultState.Failure
                        && context.CurrentResult.ResultState != ResultState.Error
                        && context.CurrentResult.ResultState != ResultState.ChildFailure
                        && context.CurrentResult.ResultState != ResultState.SetUpError
                        && context.CurrentResult.ResultState != ResultState.SetUpFailure)
                    {
                        break;
                    }

                    // Clear result for retry
                    if (count > 0)
                    {
                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                        //context.CurrentRepeatCount++; // increment Retry count for next iteration. will only happen if we are guaranteed another iteration
                    }
                }

                return context.CurrentResult;
            }
        }

        #endregion
    }
}
